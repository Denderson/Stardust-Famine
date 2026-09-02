using MonoMod.RuntimeDetour;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using static lsfUtils.Plugin;

public static class ConditionalGates
{
    public class ConditionalGateEntry
    {
        public string roomName;
        public string[] sides = new string[2];
    }
    
    public delegate bool CustomRequirementCheck(RegionGate gate, int side);
    public static readonly Dictionary<string, CustomRequirementCheck> CustomChecks = [];

    public static Dictionary<string, ConditionalGateEntry> rawConfig = [];

    private static readonly HashSet<string> loadedImages = [];

    public static readonly string ConfigPath = AssetManager.ResolveFilePath("lsf/customRequirements.txt");

    public static void ApplyHooks()
    {
        LoadConfig(ConfigPath);
        On.RegionGate.customKarmaGateRequirements += RegionGate_customKarmaGateRequirements;
        new Hook(typeof(RegionGate).GetProperty(nameof(RegionGate.MeetRequirement)).GetGetMethod(), typeof(ConditionalGates).GetMethod(nameof(RegionGate_get_MeetRequirement)));
    }

    public static void LoadConfig(string path)
    {
        rawConfig.Clear();
        // TODO: Images
        string imageFolder = null;
        return;
        if (!File.Exists(path))
        {
            Log.LogMessage("Custom gate lock config not found!, " + path);
            return;
        }
        foreach (string rawLine in File.ReadAllLines(path))
        {
            string line = rawLine.Split('/')[0].Trim();
            if (line.Length == 0)
            {
                continue;
            }
            string[] parts = line.Split([" : "], System.StringSplitOptions.None);
            if (parts.Length != 3)
            {
                Log.LogMessage("Expected 'Room : Side0 : Side1'!, " + rawLine);
                continue;
            }
            string roomName = parts[0].Trim().ToUpperInvariant();
            if (roomName.Length == 0)
            {
                Log.LogMessage("Malformed room name!, " + rawLine);
                continue;
            }
            ConditionalGateEntry entry = new ConditionalGateEntry { roomName = roomName };
            entry.sides[0] = ParseSide(parts[1], imageFolder);
            entry.sides[1] = ParseSide(parts[2], imageFolder);
            if (entry.sides[0] == null && entry.sides[1] == null)
            {
                Log.LogMessage("Neither side set anything!, " + rawLine);
                continue;
            }
            rawConfig[roomName] = entry;
        }
        Log.LogMessage("Loaded " + rawConfig.Count + " custom gate lock(s) from " + path);
    }

    private static string ParseSide(string raw, string imageFolder)
    {
        string code = raw.Trim().ToUpperInvariant();
        if (code.Length == 0)
        {
            return null;
        }
        EnsureImageLoaded(imageFolder, code);
        return code;
    }

    private static void EnsureImageLoaded(string imageFolder, string code)
    {
        string imageName = "gateSymbol" + code;
        if (loadedImages.Contains(imageName))
        {
            return;
        }
        string path = AssetManager.ResolveFilePath(Path.Combine(imageFolder, imageName));
        if (!File.Exists(path + ".png"))
        {
            Log.LogMessage(path + ".png image doesnt exist!");
            return;
        }
        Futile.atlasManager.LoadImage(path);
        loadedImages.Add(imageName);
    }

    private static void RegionGate_customKarmaGateRequirements(On.RegionGate.orig_customKarmaGateRequirements orig, RegionGate self)
    {
        orig(self);

        if (!rawConfig.TryGetValue(self.room.abstractRoom.name, out ConditionalGateEntry entry)) return;
        for (int side = 0; side < 2; side++)
        {
            if (entry.sides[side] == null)
            {
                continue;
            }
            self.karmaRequirements[side] = new RegionGate.GateRequirement(entry.sides[side]);
        }
    }

    public static bool RegionGate_get_MeetRequirement(Func<RegionGate, bool> orig, RegionGate self)
    {
        int side = self.letThroughDir ? 0 : 1;
        string code = self.karmaRequirements[side]?.value;
        if (code != null && CustomChecks.TryGetValue(code, out CustomRequirementCheck check))
        {
            return check(self, side);
        }
        return orig(self);
    }
}
