using lsfUtils.Mechanics;
using MonoMod.RuntimeDetour;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using static lsfUtils.Mechanics.ConditionalLogic;
using static lsfUtils.Plugin;
using static lsfUtils.RegionParams.RegionTypeParams;

namespace lsfUtils.RegionParams;

public static class GateParams
{
    public delegate bool CustomRequirementCheck(RegionGate gate, int side);
    public static readonly Dictionary<string, CustomRequirementCheck> CustomChecks = [];

    public static readonly ParamTable<string[]> Table = new();

    public static readonly string ConfigPath = "world/customRequirements.txt";
    public static void CheckFile()
    {
        if (!File.Exists(Custom.RootFolderDirectory() + Path.DirectorySeparatorChar.ToString() + "world/customRequirements.txt"))
        {
            ParamKey.CreateFile("world/customRequirements.txt");
        }
        else Log.LogMessage("File \"world/customRequirements.txt\" already exists");
    }

    public static readonly HashSet<string> loadedImages = [];

    // TODO: Images
    public static readonly string imageFolder = null;

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

    public static string[] ParseSides(string value)
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

    public static string ParseSide(string raw)
    {
        string side = raw.Trim().ToLowerInvariant();
        if (side.Length == 0) return null;
        LoadImage(side);
        return side;
    }

    public static void LoadImage(string name)
    {
        if (imageFolder == null) return;

        string imageName = "gateSymbol" + name;
        if (loadedImages.Contains(imageName)) return;
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

    public static void RegionGate_customKarmaGateRequirements(On.RegionGate.orig_customKarmaGateRequirements orig, RegionGate self)
    {
        orig(self);

        string[] sides = new string[2];
        foreach (string[] next in Table.Get(self.room.abstractRoom.name, self.room.game))
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
        RainWorldGame game = self?.room?.game;
        int side = self.letThroughDir ? 0 : 1;
        string requirement = self.karmaRequirements[side]?.value;
        if (requirement == null) return orig(self);

        if (game != null && LSFConditions(requirement, game) == true) return true;
        if (game != null && LSFRoomConditions(requirement, game, self.room) == true) return true;
        return orig(self);
    }
}