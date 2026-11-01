using MonoMod.RuntimeDetour;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using static lsfUtils.Plugin;
using static lsfUtils.RegionParams.RegionTypeParams;

namespace lsfUtils.RegionParams;

public static class GateParams
{
    public delegate bool CustomRequirementCheck(RegionGate gate, int side);
    public static readonly Dictionary<string, CustomRequirementCheck> CustomChecks = [];

    public static readonly ParamTable<string[]> Table = new();

    public static readonly string ConfigPath = "lsf/customRequirements.txt";

    private static readonly HashSet<string> loadedImages = [];

    // TODO: Images
    private static readonly string imageFolder = null;

    public static void ApplyHooks()
    {
        On.RegionGate.customKarmaGateRequirements += RegionGate_customKarmaGateRequirements;
        new Hook(typeof(RegionGate).GetProperty(nameof(RegionGate.MeetRequirement)).GetGetMethod(), typeof(GateParams).GetMethod(nameof(RegionGate_get_MeetRequirement)));
    }

    public static void LoadParams()
    {
        Table.Clear();
        foreach ((ParamKey key, string value) in ParamFile.Read(ConfigPath))
        {
            string[] sides = ParseSides(value);
            if (sides != null) Table.Add(key, sides);
        }
    }

    private static string[] ParseSides(string value)
    {
        string[] parts = value.Split(',');
        if (parts.Length != 2)
        {
            Log.LogWarning($"Expected Room=Side0,Side1 ! {value}");
            return null;
        }

        string[] sides = [ParseSide(parts[0]), ParseSide(parts[1])];
        if (sides[0] == null && sides[1] == null) return null;
        return sides;
    }

    private static string ParseSide(string raw)
    {
        string code = raw.Trim().ToUpperInvariant();
        if (code.Length == 0)
        {
            return null;
        }
        LoadImage(code);
        return code;
    }

    private static void LoadImage(string code)
    {
        if (imageFolder == null) return;

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

        try
        {
            Futile.atlasManager.LoadImage(path);
            loadedImages.Add(imageName);
        }
        catch (Exception e)
        {
            Log.LogError($"GateParams: failed to load image {path}: {e}");
        }
    }

    private static void RegionGate_customKarmaGateRequirements(On.RegionGate.orig_customKarmaGateRequirements orig, RegionGate self)
    {
        orig(self);

        string[] sides = new string[2];
        foreach (string[] next in Table.Get(self.room.abstractRoom.name))
        {
            for (int side = 0; side < 2; side++)
            {
                if (next[side] != null) sides[side] = next[side];
            }
        }

        for (int side = 0; side < 2; side++)
        {
            if (sides[side] == null) continue;
            self.karmaRequirements[side] = new RegionGate.GateRequirement(sides[side]);
        }
    }

    public static bool RegionGate_get_MeetRequirement(Func<RegionGate, bool> orig, RegionGate self)
    {
        int side = self.letThroughDir ? 0 : 1;
        string requirement = self.karmaRequirements[side]?.value;
        if (requirement != null && CustomChecks.TryGetValue(requirement, out CustomRequirementCheck check)) // for now only uses manually code added checks. Will rework conditional system later on so this includes that too.
        {
            return check(self, side);
        }
        return orig(self);
    }
}