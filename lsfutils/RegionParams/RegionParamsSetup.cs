using lsfUtils.CWTs;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine;
using static lsfUtils.Plugin;

namespace lsfUtils.RegionParams
{
    public class RegionParamsSetup
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

        // Chance for a scav to spawn with a lantern (from 0 to 100)
        public int ScavLanternChance { get; private set; } = 0;

        // Chance for a scav to spawn with an explosive boomerang (from 0 to 100)
        public int ScavExplosiveBoomerangChance { get; private set; } = 0;

        // Chance for a scav to spawn with a singularity boomerang (from 0 to 100)
        public int ScavSingularityBoomerangChance { get; private set; } = 0;

        // Chance for a scav to spawn with a poison dart (from 0 to 100)
        public int ScavPoisonDartChance { get; private set; } = 0;

        // Float Mud color override
        public Color FloatMudColor { get; private set; } = new Color(0.22f, 0.067f, 0.17f);

        public static RegionParamsSetup ParseFromUnrecognized(Dictionary<string, string> unrecognized, string regionName)
        {
            RegionParamsSetup customRegionParams = new();
            if (unrecognized == null || unrecognized.Count == 0) return customRegionParams;

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

                    case "scavLanternChance":
                        customRegionParams.ScavLanternChance = ParseInt(val);
                        break;
                    case "scavExplosiveBoomerangsChance":
                        customRegionParams.ScavExplosiveBoomerangChance = ParseInt(val);
                        break;
                    case "scavSingularityBoomerangsChance":
                        customRegionParams.ScavSingularityBoomerangChance = ParseInt(val);
                        break;
                    case "scavPoisonDartChance":
                        customRegionParams.ScavPoisonDartChance = ParseInt(val);
                        break;
                    case "floatMudColor":
                        {
                            string[] array7 = val.Split(',');
                            if (array7.Length == 3)
                            {
                                customRegionParams.FloatMudColor = new Color(float.Parse(array7[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array7[1], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array7[2], NumberStyles.Any, CultureInfo.InvariantCulture));
                            }
                            else if (array7.Length == 1 && new Regex("[0-9a-fA-F]{6}").IsMatch(array7[0]))
                            {
                                customRegionParams.FloatMudColor = Custom.hexToColor(array7[0]);
                            }
                            break;
                        }
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
            RegionParamsSetup customParams = RegionParamsSetup.ParseFromUnrecognized(self.regionParams.unrecognizedParams, name);
            if (RegionCWT.TryGetData(self, out var data))
            {
                data.customRegionParams = customParams;
            }
        }

    }
}