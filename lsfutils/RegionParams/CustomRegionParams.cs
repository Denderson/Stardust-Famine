using lsfUtils.CWTs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using UnityEngine;
using static lsfUtils.Plugin;

namespace lsfUtils.RegionParams
{
    public class CustomRegionParams
    {
        // Which looker mechanic is active in this region. Null = no override
        public string LookerMechanicOverride { get; private set; } = null;

        // Whenever the region can be infected by sentient rot (not counted for Prince quest if uninfectable)
        public bool RotImmune { get; private set; } = false;

        // How long until EvilWater accumulates 100% poison
        public int EvilWaterTimer { get; private set; } = 400;

        // How long until EvilWater starts being poisonous
        public int EvilWaterPoisonDelayTimer { get; private set; } = 60;

        // How long outside of EvilWater until you start cleaning poison
        public int EvilWaterHealDelayTimer { get; private set; } = 120;

        // How long until CreepingDarkness fully expands
        public int CreepingDarknessExpandTimer { get; private set; } = 600;

        // How long until CreepingDarkness fully retracts
        public int CreepingDarknessRetractTimer { get; private set; } = 200;

        // How long CreepingDarkness stays idle after fully expanding
        public int CreepingDarknessExpandIdleTimer { get; private set; } = 80;

        // How long CreepingDarkness stays idle after fully retracting
        public int CreepingDarknessRetractIdleTimer { get; private set; } = 120;

        // Is CreepingDarkness using the "complex" (retract/expand based on a timer) or "simple" (expand until player reaches a lightsource, then dissapear) version
        public bool CreepingDarknessSimpleVersion { get; private set; } = false;

        // If true, makes all scavengers have lanterns
        public bool CreepingDarknessScavLantern { get; private set; } = false;

        public static CustomRegionParams ParseFromUnrecognized(Dictionary<string, string> unrecognized, string regionName)
        {
            CustomRegionParams customRegionParams = new();
            if (unrecognized == null || unrecognized.Count == 0)
                return customRegionParams;

            Log.LogMessage($"Reading custom parameters for region '{regionName}'...");

            foreach (var keyvalue in unrecognized)
            {
                string key = keyvalue.Key.Trim();
                string val = keyvalue.Value.Trim();

                switch (key)
                {
                    case "lookerMechanicOverride":
                        customRegionParams.LookerMechanicOverride = val;
                        break;

                    case "rotImmune":
                        customRegionParams.RotImmune = ParseBool(val);
                        break;

                    case "evilWaterTimer":
                        customRegionParams.EvilWaterTimer = ParseInt(val);
                        break;

                    case "evilWaterPoisonDelayTimer":
                        customRegionParams.EvilWaterPoisonDelayTimer = ParseInt(val);
                        break;

                    case "evilWaterHealDelayTimer":
                        customRegionParams.EvilWaterHealDelayTimer = ParseInt(val);
                        break;

                    case "creepingDarknessExpandTimer":
                        customRegionParams.CreepingDarknessExpandTimer = ParseInt(val);
                        break;

                    case "creepingDarknessRetractTimer":
                        customRegionParams.CreepingDarknessRetractTimer = ParseInt(val);
                        break;

                    case "creepingDarknessExpandIdleTimer":
                        customRegionParams.CreepingDarknessExpandIdleTimer = ParseInt(val);
                        break;

                    case "creepingDarknessRetractIdleTimer":
                        customRegionParams.CreepingDarknessRetractIdleTimer = ParseInt(val);
                        break;

                    case "creepingDarknessSimpleVersion":
                        customRegionParams.CreepingDarknessSimpleVersion = ParseBool(val);
                        break;

                    case "creepingDarknessScavLantern":
                        customRegionParams.CreepingDarknessScavLantern = ParseBool(val);
                        break;
                }
            }

            return customRegionParams;
        }

        private static int ParseInt(string s) => int.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture);

        private static bool ParseBool(string s) => s.Trim().ToLowerInvariant() == "true";

        public static void Region_ctor_string_int_int_RainWorldGame_Timeline(On.Region.orig_ctor_string_int_int_RainWorldGame_Timeline orig, Region self, string name, int firstRoomIndex, int regionNumber, RainWorldGame game, SlugcatStats.Timeline timelineIndex)
        {
            orig(self, name, firstRoomIndex, regionNumber, game, timelineIndex);
            if (self?.regionParams == null)
            {
                return;
            }
            CustomRegionParams customParams = CustomRegionParams.ParseFromUnrecognized(self.regionParams.unrecognizedParams, name);
            if (RegionCWT.TryGetData(self, out var data))
            {
                data.customRegionParams = customParams;
            }
        }

    }
}