using System.Collections.Generic;
using System.IO;
using RWCustom;
using UnityEngine;
using static lsfUtils.Plugin;

public static class ShortcutLinks
{
    public class ShortcutLinkEntry
    {
        public string sourceRoom;
        public int sourceNode;
        public List<(string room, int node)> candidates = [];
    }

    public static List<ShortcutLinkEntry> rawConfig;

    public static string ConfigPath = "lsf/shortcutLinks.txt";

    public static void Load()
    {
        rawConfig = [];
        string path = AssetManager.ResolveFilePath(ConfigPath);
        if (!File.Exists(path))
        {
            Log.LogMessage("Config file not found!, " + path);
            return;
        }

        foreach (string rawLine in File.ReadAllLines(path))
        {
            string line = rawLine.Split('/')[0].Trim();
            if (line.Length == 0)
            {
                continue;
            }

            string[] sides = line.Split('=');
            if (sides.Length != 2)
            {
                Log.LogMessage("No = found!, " + rawLine);
                continue;
            }

            string[] srcParts = sides[0].Trim().Split(':');
            if (srcParts.Length != 2 || !int.TryParse(srcParts[1].Trim(), out int srcNode))
            {
                Log.LogMessage("Malformed source! (want RoomName:nodeIndex), " + rawLine);
                continue;
            }

            ShortcutLinkEntry entry = new ShortcutLinkEntry
            {
                sourceRoom = srcParts[0].Trim().ToUpperInvariant(),
                sourceNode = srcNode
            };

            foreach (string candRaw in sides[1].Split(','))
            {
                string[] candParts = candRaw.Trim().Split(':');
                if (candParts.Length != 2 || !int.TryParse(candParts[1].Trim(), out int candNode))
                {
                    Log.LogMessage("Malformed candidate (want RoomName:nodeIndex), " + candRaw);
                    continue;
                }
                entry.candidates.Add((candParts[0].Trim().ToUpperInvariant(), candNode));
            }

            if (entry.candidates.Count > 0)
            {
                rawConfig.Add(entry);
            }
        }
    }

    public static World resolvedForWorld;
    public static Dictionary<(int room, int node), List<(int room, int node)>> resolved;

    public static void EnsureResolved(World world)
    {
        if (resolvedForWorld == world && resolved != null) return;

        resolved = [];
        resolvedForWorld = world;

        if (rawConfig == null) return;

        foreach (ShortcutLinkEntry entry in rawConfig)
        {
            AbstractRoom srcRoom = world.GetAbstractRoom(entry.sourceRoom);
            if (srcRoom == null)
            {
                Log.LogMessage("Unknown source room: " + entry.sourceRoom);
                continue;
            }

            List<(int, int)> candidates = [];
            foreach ((string roomName, int node) in entry.candidates)
            {
                AbstractRoom destRoom = world.GetAbstractRoom(roomName);
                if (destRoom == null)
                {
                    Log.LogMessage("Unknown destination room: " + roomName);
                    continue;
                }
                candidates.Add((destRoom.index, node));
            }

            if (candidates.Count > 0)
            {
                resolved[(srcRoom.index, entry.sourceNode)] = candidates;
            }
        }
    }

    public static void ApplyHooks()
    {
        On.ShortcutHandler.Update += ShortcutHandler_Update;
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

        for (int i = 0; i < self.transportVessels.Count; i++)
        {
            ShortcutHandler.ShortCutVessel vessel = self.transportVessels[i];
            
            if (vessel.wait > 0) continue;

            Room realizedRoom = vessel.room.realizedRoom;
            if (realizedRoom == null) continue;

            EnsureResolved(vessel.room.world);
            
            IntVector2 nextPos = ShortcutHandler.NextShortcutPosition(vessel.pos, vessel.lastPos, realizedRoom);
            if (nextPos == vessel.pos) continue;
            if (realizedRoom.GetTile(nextPos).Terrain == Room.Tile.TerrainType.ShortcutEntrance) continue;
            if (realizedRoom.GetTile(nextPos).shortCut != 2) continue; // 2 == RoomExit

            int node = realizedRoom.exitAndDenIndex.IndexfOf(nextPos);
            if (!resolved.TryGetValue((vessel.room.index, node), out List<(int room, int node)> candidates)) continue;

            (int room, int node) key = (vessel.room.index, node);
            if (chosenThisTick.ContainsKey(key)) continue;

            (int destRoomIndex, int destNodeIndex) = candidates[Random.Range(0, candidates.Count)];
            AbstractRoom destRoom = vessel.room.world.GetAbstractRoom(destRoomIndex);
            if (destRoom == null) continue;

            chosenThisTick[key] = (destRoomIndex, destNodeIndex);
            
            activeOverrides.Add(new ConnectionOverride(vessel.room, node, vessel.room.connections[node]));
            vessel.room.connections[node] = destRoomIndex;
            activeOverrides.Add(new ConnectionOverride(destRoom, destNodeIndex, destRoom.connections[destNodeIndex]));
            destRoom.connections[destNodeIndex] = vessel.room.index;
        }
        Log.LogMessage("Before orig in shortcut override!");
        orig(self);
        Log.LogMessage("Past orig in shortcut override!");
        foreach (ConnectionOverride ov in activeOverrides)
        {
            ov.room.connections[ov.overrideNode] = ov.previousNode;
        }
    }
}