using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using RWCustom;
using UnityEngine;
using static lsfUtils.Plugin;

public static class ShortcutLinks
{
    public class PipeConfigEntry
    {
        public string sourceRoom;
        public int sourceNode;
        public List<(string room, int node)> candidates = [];
    }

    public static List<PipeConfigEntry> rawConfig;

    public static readonly string ConfigPath = AssetManager.ResolveFilePath("lsf/shortcutLinks.txt");

    public static void LoadConfig(string path)
    {
        rawConfig = [];

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

            PipeConfigEntry entry = new PipeConfigEntry
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
    public static readonly Dictionary<(int destRoom, int sourceRoom), int> activeExitOverrides = [];

    public static void EnsureResolved(World world)
    {
        if (resolvedForWorld == world && resolved != null) return;

        resolved = [];
        resolvedForWorld = world;
        activeExitOverrides.Clear();

        if (rawConfig == null) return;

        foreach (PipeConfigEntry entry in rawConfig)
        {
            AbstractRoom srcRoom = world.GetAbstractRoom(entry.sourceRoom);
            if (srcRoom == null)
            {
                Log.LogMessage("ShortcutLinks: unknown source room: " + entry.sourceRoom);
                continue;
            }

            List<(int, int)> candidates = [];
            foreach ((string roomName, int node) in entry.candidates)
            {
                AbstractRoom destRoom = world.GetAbstractRoom(roomName);
                if (destRoom == null)
                {
                    Custom.LogWarning("ShortcutLinks: unknown destination room", roomName);
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

    public class PendingDest
    {
        public int sourceNode;
        public int destRoomIndex;
        public int destNodeIndex;
    }

    public static readonly ConditionalWeakTable<ShortcutHandler.ShortCutVessel, PendingDest> pending = new ConditionalWeakTable<ShortcutHandler.ShortCutVessel, PendingDest>();

    public static void ApplyHooks()
    {
        LoadConfig(ConfigPath);
        On.ShortcutHandler.SuckInCreature += ShortcutHandler_SuckInCreature;
        On.ShortcutHandler.Update += ShortcutHandler_Update;
        On.AbstractRoom.ExitIndex += AbstractRoom_ExitIndex;
    }

    public static int AbstractRoom_ExitIndex(On.AbstractRoom.orig_ExitIndex orig, AbstractRoom self, int targetRoom)
    {
        if (activeExitOverrides.TryGetValue((self.index, targetRoom), out int node)) return node;
        return orig(self, targetRoom);
    }

    public static void SetReverseConnection(AbstractRoom destRoom, int destNodeIndex, int sourceRoomIndex)
    {
        destRoom.connections[destNodeIndex] = sourceRoomIndex;
        activeExitOverrides[(destRoom.index, sourceRoomIndex)] = destNodeIndex;
    }

    public static void ShortcutHandler_SuckInCreature(On.ShortcutHandler.orig_SuckInCreature orig, ShortcutHandler self, Creature creature, Room room, ShortcutData shortCut)
    {
        if (shortCut.shortCutType != ShortcutData.Type.RoomExit)
        {
            orig(self, creature, room, shortCut);
            return;
        }

        EnsureResolved(room.world);

        if (!resolved.TryGetValue((room.abstractRoom.index, shortCut.destNode), out List<(int room, int node)> candidates))
        {
            orig(self, creature, room, shortCut);
            return;
        }

        (int destRoomIndex, int destNodeIndex) = candidates[UnityEngine.Random.Range(0, candidates.Count)];

        room.abstractRoom.connections[shortCut.destNode] = destRoomIndex;
        AbstractRoom destRoomForPreload = room.world.GetAbstractRoom(destRoomIndex);
        SetReverseConnection(destRoomForPreload, destNodeIndex, room.abstractRoom.index);

        orig(self, creature, room, shortCut);

        if (self.transportVessels.Count == 0)
        {
            return;
        }
        ShortcutHandler.ShortCutVessel vessel = self.transportVessels[self.transportVessels.Count - 1];
        if (vessel.creature != creature) return;

        pending.Remove(vessel);
        pending.Add(vessel, new PendingDest
        {
            sourceNode = shortCut.destNode,
            destRoomIndex = destRoomIndex,
            destNodeIndex = destNodeIndex
        });
    }

    public static void ShortcutHandler_Update(On.ShortcutHandler.orig_Update orig, ShortcutHandler self)
    {
        HashSet<(int room, int node)> claimedThisTick = [];

        for (int i = 0; i < self.transportVessels.Count; i++)
        {
            ShortcutHandler.ShortCutVessel vessel = self.transportVessels[i];

            if (!pending.TryGetValue(vessel, out PendingDest dest))
            {
                continue;
            }
            if (vessel.wait > 0)
            {
                continue;
            }

            Room realizedRoom = vessel.room.realizedRoom;
            if (realizedRoom == null)
            {
                continue;
            }

            IntVector2 nextPos = ShortcutHandler.NextShortcutPosition(vessel.pos, vessel.lastPos, realizedRoom);

            if (nextPos == vessel.pos) continue;
            if (realizedRoom.GetTile(nextPos).Terrain == Room.Tile.TerrainType.ShortcutEntrance) continue;
            if (realizedRoom.GetTile(nextPos).shortCut != 2) continue;

            int num2 = realizedRoom.exitAndDenIndex.IndexfOf(nextPos);
            if (num2 != dest.sourceNode) continue;

            (int room, int node) key = (vessel.room.index, num2);
            if (!claimedThisTick.Add(key))
            {
                vessel.wait = 1;
                continue;
            }

            vessel.room.connections[num2] = dest.destRoomIndex;
            AbstractRoom destRoom = vessel.room.world.GetAbstractRoom(dest.destRoomIndex);
            SetReverseConnection(destRoom, dest.destNodeIndex, vessel.room.index);

            pending.Remove(vessel);
        }
        orig(self);
    }
}