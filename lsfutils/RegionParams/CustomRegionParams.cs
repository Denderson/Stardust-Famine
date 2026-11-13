using lsfUtils.CWTs;
using lsfUtils.Mechanics.Fire;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine;
using static lsfUtils.Plugin;

namespace lsfUtils.RegionParams;

public class CustomRegionParams
{
    public readonly ProceduralFireUniforms uniforms = new();


    // Which looker mechanic is active in this region. Null = no override
    public string LookerMechanicOverride { get; set; } = null;

    // How long until EvilWater accumulates 100% poison
    public int EvilWaterTimer { get; set; } = 400;

    // How long until EvilWater starts being poisonous
    public int EvilWaterPoisonDelayTimer { get; set; } = 60;

    // How long outside of EvilWater until you start cleaning poison
    public int EvilWaterHealDelayTimer { get; set; } = 120;

    // How long until CreepingDarkness fully expands
    public int CreepingDarknessExpandTimer { get; set; } = 600;

    // How long until CreepingDarkness fully retracts
    public int CreepingDarknessRetractTimer { get; set; } = 200;

    // How long CreepingDarkness stays idle after fully expanding
    public int CreepingDarknessExpandIdleTimer { get; set; } = 80;

    // How long CreepingDarkness stays idle after fully retracting
    public int CreepingDarknessRetractIdleTimer { get; set; } = 120;

    // Is CreepingDarkness using the "complex" (retract/expand based on a timer) or "simple" (expand until player reaches a lightsource, then dissapear) version
    public bool CreepingDarknessSimpleVersion { get; set; } = false;

    //Chance for a Dart to replace a spear
    public int DartFrequency { get; set; } = 10;

    // Float Mud color override
    public Color FloatMudColor { get; set; } = new Color(0.22f, 0.067f, 0.17f);

    // Torch spear fire color override
    public Color FireCoreColor { get; set; } = new(1f, 0.4f, 0.1f);

    public Color FireEdgeColor { get; set; } = new(1f, 0.15f, 0f);

    public int InvedSlugpupChance { get; set; } = 0;

    public static CustomRegionParams ParseFromUnrecognized(Dictionary<string, string> unrecognized, string regionName, SlugcatStats.Timeline timeline)
    {
        CustomRegionParams customRegionParams = new();
        if (unrecognized == null || unrecognized.Count == 0) return customRegionParams;

        ParamTable<string> table = new();
        foreach (KeyValuePair<string, string> keyvalue in unrecognized)
        {
            if (ParamKey.TryParse(keyvalue.Key, out ParamKey? key)) table.Add(key.Value, keyvalue.Value.Trim());
        }

        if (TryGet(table, "lookerMechanicOverride", timeline, out string val)) customRegionParams.LookerMechanicOverride = val;

        if (TryGet(table, "evilWaterTimer", timeline, out val) && TryParseInt(val, out int number)) customRegionParams.EvilWaterTimer = number;
        if (TryGet(table, "evilWaterPoisonDelayTimer", timeline, out val) && TryParseInt(val, out number)) customRegionParams.EvilWaterPoisonDelayTimer = number;
        if (TryGet(table, "evilWaterHealDelayTimer", timeline, out val) && TryParseInt(val, out number)) customRegionParams.EvilWaterHealDelayTimer = number;

        if (TryGet(table, "creepingDarknessExpandTimer", timeline, out val) && TryParseInt(val, out number)) customRegionParams.CreepingDarknessExpandTimer = number;
        if (TryGet(table, "creepingDarknessRetractTimer", timeline, out val) && TryParseInt(val, out number)) customRegionParams.CreepingDarknessRetractTimer = number;
        if (TryGet(table, "creepingDarknessExpandIdleTimer", timeline, out val) && TryParseInt(val, out number)) customRegionParams.CreepingDarknessExpandIdleTimer = number;
        if (TryGet(table, "creepingDarknessRetractIdleTimer", timeline, out val) && TryParseInt(val, out number)) customRegionParams.CreepingDarknessRetractIdleTimer = number;
        if (TryGet(table, "creepingDarknessSimpleVersion", timeline, out val)) customRegionParams.CreepingDarknessSimpleVersion = ParseBool(val);

        if (TryGet(table, "dartFrequency", timeline, out val) && TryParseInt(val, out number)) customRegionParams.DartFrequency = number;

        if (TryGet(table, "floatMudColor", timeline, out val) && TryParseColor(val, out Color color)) customRegionParams.FloatMudColor = color;
        if (TryGet(table, "fireCoreColor", timeline, out val) && TryParseColor(val, out color)) customRegionParams.FireCoreColor = color;
        if (TryGet(table, "fireEdgeColor", timeline, out val) && TryParseColor(val, out color)) customRegionParams.FireEdgeColor = color;
        if (ModManager.MSC && TryGet(table, "invedSlugpupChance", timeline, out val) && TryParseInt(val, out number)) customRegionParams.InvedSlugpupChance = number;

        return customRegionParams;
    }

    public static bool TryGet(ParamTable<string> table, string name, SlugcatStats.Timeline timeline, out string value)
    {
        RainWorldGame game = null;
        if (Custom.rainWorld.processManager.currentMainLoop is RainWorldGame game1) game = game1;
        List<string> found = table.Get(name, game, timeline);
        value = found.Count > 0 ? found[found.Count - 1] : null;
        return found.Count > 0;
    }

    public static bool TryParseColor(string val, out Color color)
    {
        color = default;
        string[] parts = val.Split(',');
        if (parts.Length == 3)
        {
            if (float.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out float r)
                && float.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out float g)
                && float.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out float b))
            {
                color = new Color(r, g, b);
                return true;
            }
            Log.LogWarning($"Invalid color value! {val}");
            return false;
        }
        if (parts.Length == 1 && new Regex("[0-9a-fA-F]{6}").IsMatch(parts[0]))
        {
            color = Custom.hexToColor(parts[0]);
            return true;
        }
        Log.LogWarning($"Invalid color value! {val}");
        return false;
    }

    public static bool TryParseInt(string s, out int result)
    {
        if (int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out result)) return true;
        Log.LogWarning($"Invalid integer value! {s}");
        return false;
    }

    public static bool ParseBool(string s) => s.Trim().ToLowerInvariant() == "true";

    public static void SetupParams(On.Region.orig_ctor_string_int_int_RainWorldGame_Timeline orig, Region self, string name, int firstRoomIndex, int regionNumber, RainWorldGame game, SlugcatStats.Timeline timelineIndex)
    {
        orig(self, name, firstRoomIndex, regionNumber, game, timelineIndex);
        if (self?.regionParams == null) return;
        CustomRegionParams customParams = ParseFromUnrecognized(self.regionParams.unrecognizedParams, name, timelineIndex);
        if (RegionCWT.TryGetData(self, out var data))
        {
            data.customRegionParams = customParams;

            data.customRegionParams.uniforms.SetColors(data.customRegionParams.FireCoreColor, data.customRegionParams.FireEdgeColor);
            data.customRegionParams.uniforms.PushGlobals();
        }
    }
}