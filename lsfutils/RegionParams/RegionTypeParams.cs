using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using static lsfUtils.Plugin;

namespace lsfUtils.RegionParams;

public static class RegionTypeParams
{
    public enum RegionTypeParameter { Rotten, VanillaRotten, RotImmune, Daemon, ShatteredTerrace, AncientUrban, WatcherVanilla, Rubicon, NoWarpFatigue } // Someone is going to kill me for making every system act differently

    public static readonly ParamTable<List<RegionTypeParameter>> Table = new();

    public static readonly string ConfigPath = "world/regionMetaParameters.txt";
    public static void CheckFile()
    {
        if (!File.Exists(Custom.RootFolderDirectory() + Path.DirectorySeparatorChar.ToString() + "world/regionMetaParameters.txt"))
        {
            ParamKey.CreateFile("world/regionMetaParameters.txt");
        }
        else Log.LogMessage("File \"world/regionMetaParameters.txt\" already exists");
    }


    public static void ApplyHooks()
    {
        On.Region.IsSentientRotRegion += Region_IsSentientRotRegion;
        On.Region.IsVanillaSentientRotRegion += Region_IsVanillaSentientRotRegion;
        On.Region.HasSentientRotResistance += Region_HasSentientRotResistance;
        On.Region.IsDaemonRegion += Region_IsDaemonRegion;
        On.Region.IsAncientUrbanRegion += Region_IsAncientUrbanRegion;
        On.Region.IsRubiconRegion += Region_IsRubiconRegion;
        On.Region.IsShatteredTerraceRegion += Region_IsShatteredTerraceRegion;
        On.Region.HasWarpFatigueResistance += Region_HasWarpFatigueResistance;
    }

    public static void LoadParams()
    {
        Table.Clear();
        foreach ((ParamKey key, string value) in ParamFile.Read(ConfigPath))
        {
            List<RegionTypeParameter> parameters = ParseParams(value);
            if (parameters.Count > 0) Table.Add(key, parameters);
        }
    }

    public static List<RegionTypeParameter> ParseParams(string value)
    {
        List<RegionTypeParameter> parameters = [];
        foreach (string raw in value.Split(','))
        {
            if (string.IsNullOrWhiteSpace(raw)) continue;
            string trimmed = raw.Trim();
            if (trimmed.Length == 0) continue;

            if (Enum.TryParse(trimmed, true, out RegionTypeParameter parameter)) parameters.Add(parameter);
            else Log.LogWarning($"Unknown parameter {raw}!");
        }
        return parameters;
    }

    public static bool CheckForMetaParam(RegionTypeParameter parameter, string name)
    {
        List<RegionTypeParameter> mergedParams = [];
        RainWorldGame game = null;
        if (Custom.rainWorld.processManager.currentMainLoop is RainWorldGame game1) game = game1;
        foreach (List<RegionTypeParameter> next in Table.Get(name, game)) mergedParams.AddRange(next);
        return mergedParams.Contains(parameter);
    }

    public static bool Region_HasSentientRotResistance(On.Region.orig_HasSentientRotResistance orig, string name)
    {
        bool value = orig(name);
        if (CheckForMetaParam(RegionTypeParameter.RotImmune, name)) return true;
        return value;
    }

    public static bool Region_IsVanillaSentientRotRegion(On.Region.orig_IsVanillaSentientRotRegion orig, string name)
    {
        bool value = orig(name);
        if (CheckForMetaParam(RegionTypeParameter.VanillaRotten, name)) return true;
        return value;
    }

    public static bool Region_IsSentientRotRegion(On.Region.orig_IsSentientRotRegion orig, string name)
    {
        bool value = orig(name);
        if (CheckForMetaParam(RegionTypeParameter.Rotten, name)) return true;
        return value;
    }

    public static bool Region_HasWarpFatigueResistance(On.Region.orig_HasWarpFatigueResistance orig, string name)
    {
        bool value = orig(name);
        if (CheckForMetaParam(RegionTypeParameter.NoWarpFatigue, name)) return true;
        return value;
    }

    public static bool Region_IsShatteredTerraceRegion(On.Region.orig_IsShatteredTerraceRegion orig, string name)
    {
        bool value = orig(name);
        if (CheckForMetaParam(RegionTypeParameter.ShatteredTerrace, name)) return true;
        return value;
    }

    public static bool Region_IsRubiconRegion(On.Region.orig_IsRubiconRegion orig, string name)
    {
        bool value = orig(name);
        if (CheckForMetaParam(RegionTypeParameter.Rubicon, name)) return true;
        return value;
    }

    public static bool Region_IsAncientUrbanRegion(On.Region.orig_IsAncientUrbanRegion orig, string name)
    {
        bool value = orig(name);
        if (CheckForMetaParam(RegionTypeParameter.AncientUrban, name)) return true;
        return value;
    }

    public static bool Region_IsDaemonRegion(On.Region.orig_IsDaemonRegion orig, string name)
    {
        bool value = orig(name);
        if (CheckForMetaParam(RegionTypeParameter.Daemon, name)) return true;
        return value;
    }
}