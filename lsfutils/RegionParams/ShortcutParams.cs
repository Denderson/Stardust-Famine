using RWCustom;
using System.Collections.Generic;
using UnityEngine;
using static lsfUtils.Plugin;

namespace lsfUtils.RegionParams;

public static class ShortcutParams
{
    public static readonly ParamTable<List<(string room, int node)>> Table = new();

    public static readonly string ConfigPath = "lsf/shortcutLinks.txt";

    private static readonly HashSet<string> invalidDestinations = [];

    public static void ApplyHooks()
    {
        On.ShortcutHandler.Update += ShortcutHandler_Update;
    }

    public static void LoadParams()
    {
        Table.Clear();
        invalidDestinations.Clear();
        foreach ((ParamKey key, string value) in ParamFile.Read(ConfigPath))
        {
            List<(string room, int node)> candidates = ParseCandidates(value);
            if (candidates.Count > 0) Table.Add(key, candidates);
        }
    }

    private static List<(string room, int node)> ParseCandidates(string value)
    {
        List<(string room, int node)> candidates = [];
        foreach (string raw in value.Split(','))
        {
            string[] parts = raw.Split(':');
            if (parts.Length != 2 || !int.TryParse(parts[1], out int node))
            {
                Log.LogWarning($"Expected RoomName:nodeIndex! {raw}");
                continue;
            }
            candidates.Add((parts[0].Trim().ToUpperInvariant(), node));
        }
        return candidates;
    }

    private static List<(string room, int node)> GetCandidates(string exit)
    {
        List<(string room, int node)> candidates = [];
        foreach (List<(string room, int node)> next in Table.Get(exit))
        {
            foreach ((string room, int node) candidate in next)
            {
                if (!candidates.Contains(candidate)) candidates.Add(candidate);
            }
        }
        return candidates;
    }

    public struct ConnectionOverride(AbstractRoom room, int overrideNode, int previousNode)
    {
        public AbstractRoom room = room;
        public int overrideNode = overrideNode;
        public int previousNode = previousNode;
    }

    public static void ShortcutHandler_Update(On.ShortcutHandler.orig_Update orig, ShortcutHandler self)
    {
        List<ConnectionOverride> activeOverrides = [];
        Dictionary<(int room, int node), (int destRoomIndex, int destNodeIndex)> chosenThisTick = [];

        try
        {
            for (int i = 0; i < self.transportVessels.Count; i++)
            {
                ShortcutHandler.ShortCutVessel vessel = self.transportVessels[i];

                if (vessel.wait > 0) continue;

                Room realizedRoom = vessel.room.realizedRoom;
                if (realizedRoom == null) continue;

                IntVector2 nextPos = ShortcutHandler.NextShortcutPosition(vessel.pos, vessel.lastPos, realizedRoom);
                if (nextPos == vessel.pos) continue;
                if (realizedRoom.GetTile(nextPos).Terrain == Room.Tile.TerrainType.ShortcutEntrance) continue;
                if (realizedRoom.GetTile(nextPos).shortCut != 2) continue; // 2 == RoomExit

                int node = realizedRoom.exitAndDenIndex.IndexfOf(nextPos);
                List<(string room, int node)> candidates = GetCandidates(vessel.room.name + ":" + node);
                if (candidates.Count == 0) continue;

                (int room, int node) key = (vessel.room.index, node);
                if (chosenThisTick.ContainsKey(key)) continue;

                (string destRoomName, int destNodeIndex) = candidates[Random.Range(0, candidates.Count)];
                AbstractRoom destRoom = vessel.room.world.GetAbstractRoom(destRoomName);
                if (destRoom == null)
                {
                    Log.LogWarning($"Unknown destination room! { destRoomName}");
                    continue;
                }

                if (destNodeIndex < 0 || destNodeIndex >= destRoom.connections.Length)
                {
                    if (invalidDestinations.Add(destRoomName + ":" + destNodeIndex))
                    {
                        Log.LogWarning($"Invalid destination node {destNodeIndex} for room {destRoomName}! (it has {destRoom.connections.Length} connections)");
                    }
                    continue;
                }

                chosenThisTick[key] = (destRoom.index, destNodeIndex);

                activeOverrides.Add(new ConnectionOverride(vessel.room, node, vessel.room.connections[node]));
                vessel.room.connections[node] = destRoom.index;
                activeOverrides.Add(new ConnectionOverride(destRoom, destNodeIndex, destRoom.connections[destNodeIndex]));
                destRoom.connections[destNodeIndex] = vessel.room.index;
            }
            orig(self);
        }
        finally
        {
            for (int o = activeOverrides.Count - 1; o >= 0; o--)
            {
                ConnectionOverride ov = activeOverrides[o];
                ov.room.connections[ov.overrideNode] = ov.previousNode;
            }
        }
    }
}