using System.Collections.Generic;
using System.IO;
using RWCustom;
using UnityEngine;
using static lsfUtils.Plugin;

public static class ShelterLinks
{
    public class ShelterLinkEntry
    {
        public string sourceShelter;
        public List<string> candidates = [];
    }
    
    public static List<ShelterLinkEntry> rawConfig = [];
    
    public static readonly Dictionary<string, ShelterLinkEntry> byShelter = [];

    public static readonly string ConfigPath = AssetManager.ResolveFilePath("lsf/shelterLinkstxt");

    public static void ApplyHooks()
    {
        LoadConfig(ConfigPath);
        On.SaveState.BringUpToDate += SaveState_BringUpToDate;
    }
    
    public static void LoadConfig(string path)
    {
        rawConfig.Clear();
        byShelter.Clear();
        if (!File.Exists(path))
        {
            Log.LogMessage("Shelter link config not found!, " + path);
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
            string sourceShelter = sides[0].Trim().ToUpperInvariant();
            if (sourceShelter.Length == 0)
            {
                Log.LogMessage("Malformed source shelter!, " + rawLine);
                continue;
            }
            ShelterLinkEntry entry = new() { sourceShelter = sourceShelter };
            foreach (string candRaw in sides[1].Split(','))
            {
                string candidate = candRaw.Trim().ToUpperInvariant();
                if (candidate.Length == 0)
                {
                    continue;
                }
                entry.candidates.Add(candidate);
            }
            if (entry.candidates.Count > 0)
            {
                rawConfig.Add(entry);
            }
            else
            {
                Log.LogMessage("No valid candidates!, " + rawLine);
            }
        }
        foreach (ShelterLinkEntry entry in rawConfig)
        {
            byShelter[entry.sourceShelter] = entry;
        }
        Log.LogMessage("Loaded " + rawConfig.Count + " shelter link(s) from " + path);
    }

    public static void SaveState_BringUpToDate(On.SaveState.orig_BringUpToDate orig, SaveState self, RainWorldGame game)
    {
        orig(self, game);

        string fromShelter = self.denPosition;
        if (fromShelter == null || !byShelter.TryGetValue(fromShelter, out ShelterLinkEntry entry)) return;

        string toShelter = SelectTarget(entry, self, game);
        if (toShelter == null || !ShouldRedirect(self, game, fromShelter, toShelter)) return;

        RedirectDen(self, game, fromShelter, toShelter);
    }
    
    public static string SelectTarget(ShelterLinkEntry entry, SaveState save, RainWorldGame game)
    {
        return entry.candidates.Count > 0 ? entry.candidates[0] : null;
    }
    
    public static bool ShouldRedirect(SaveState save, RainWorldGame game, string fromShelter, string toShelter)
    {
        return true;
    }

    public static void RedirectDen(SaveState save, RainWorldGame game, string fromShelter, string toShelter)
    {
        save.denPosition = toShelter;
        save.TrySetVanillaDen(toShelter);
        
        AbstractRoom fromRoom = game.world.GetAbstractRoom(fromShelter);
        if (fromRoom == null) return;

        for (int i = fromRoom.entities.Count - 1; i >= 0; i--)
        {
            if (fromRoom.entities[i] is not AbstractPhysicalObject obj) continue;
            if (obj is AbstractCreature crit && crit.creatureTemplate.type == CreatureTemplate.Type.Slugcat)  continue;
            if (HeldByPlayer(obj, game)) continue;

            save.pendingObjects.Add(obj is AbstractCreature critter ? SaveState.AbstractCreatureToStringStoryWorld(critter) : obj.ToString());
        }
    }

    public static bool HeldByPlayer(AbstractPhysicalObject obj, RainWorldGame game)
    {
        for (int p = 0; p < game.session.Players.Count; p++)
        {
            if (game.session.Players[p].realizedCreature is not Player player) continue;

            for (int g = 0; g < player.grasps.Length; g++)
            {
                if (player.grasps[g]?.grabbed?.abstractPhysicalObject == obj)
                {
                    return true;
                }
            }
        }
        return false;
    }
}