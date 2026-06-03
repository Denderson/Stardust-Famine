using System;
using System.Collections.Generic;
using System.IO;
using static lsfUtils.Plugin;

namespace lsfUtils.RegionParams
{
    public static class RegionTypeParams
    {
        public enum RegionMetaParameter { Rotten, VanillaRotten, RotImmune, Daemon, ShatteredTerrace, AncientUrban, WatcherVanilla, Rubicon, NoWarpFatigue }

        public struct RegionMeta
        {
            public List<RegionMetaParameter> Parameters;

            public RegionMeta()
            {
                Parameters = [];
            }
        }

        public struct RegionKey
        {
            public string RegionTimeline;
            public SlugcatStats.Timeline? Timeline;

            public RegionKey(string regionTimeline, SlugcatStats.Timeline? timeline)
            {
                RegionTimeline = regionTimeline.ToUpperInvariant();
                Timeline = timeline;
            }
        }

        public static readonly Dictionary<RegionKey, RegionMeta> regionMetaParameters = [];

        public static RegionMeta? GetClosestMeta(string regionTimeline, SlugcatStats.Timeline? timeline)
        {
            RegionKey exactKey = new RegionKey(regionTimeline, timeline);
            if (regionMetaParameters.TryGetValue(exactKey, out RegionMeta exactMeta))
                return exactMeta;

            RegionKey fallbackKey = new RegionKey(regionTimeline, null);
            if (regionMetaParameters.TryGetValue(fallbackKey, out RegionMeta fallbackMeta))
                return fallbackMeta;

            return null;
        }

        public static void Load()
        {
            regionMetaParameters.Clear();
            string path = AssetManager.ResolveFilePath("lsfUtils/regionMetaParameters.txt");
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                Log.LogWarning("regionMetaParameters.txt not found.");
                return;
            }

            foreach (string line in File.ReadAllLines(path))
            {
                if (string.IsNullOrEmpty(line)) continue;

                string[] parts = line.Trim().Split('=');
                if (parts.Length != 2)
                {
                    Log.LogWarning($"Skipping malformed line: {line}");
                    continue;
                }

                string[] keyParts = parts[0].Trim().Split(',');
                string regionTimeline = keyParts[0].Trim().ToUpperInvariant();
                SlugcatStats.Timeline? timeline = keyParts.Length > 1 ? new SlugcatStats.Timeline(keyParts[1].Trim(), false) : null;

                string[] values = parts[1].Trim().Split(',');
                RegionMeta meta = new RegionMeta();

                foreach (string value in values)
                {
                    string trimmed = value.Trim();
                    if (Enum.TryParse<RegionMetaParameter>(trimmed, ignoreCase: true, out RegionMetaParameter param))
                    {
                        meta.Parameters.Add(param);
                        Log.LogMessage($"{regionTimeline} ({timeline?.ToString() ?? "any"}) -> {param}");
                    }
                    else
                    {
                        Log.LogWarning($"Unknown parameter '{trimmed}' for region {regionTimeline}");
                    }
                }

                RegionKey key = new RegionKey(regionTimeline, timeline);

                if (regionMetaParameters.TryGetValue(key, out RegionMeta existing))
                {
                    foreach (RegionMetaParameter param in meta.Parameters)
                    {
                        if (!existing.Parameters.Contains(param))
                            existing.Parameters.Add(param);
                    }
                    regionMetaParameters[key] = existing;
                    Log.LogMessage($"Merged duplicate key ({regionTimeline}, {timeline?.ToString() ?? "any"}).");
                }
                else
                {
                    regionMetaParameters[key] = meta;
                }
            }
        }

        public static bool CheckForMetaParam(RegionMetaParameter parameter, string name)
        {
            if (GetTimelineFromAnywhere(out var timeline))
            {
                RegionMeta? meta = GetClosestMeta(name, timeline);
                if (meta.HasValue)
                {
                    if (meta.Value.Parameters.Contains(RegionMetaParameter.RotImmune)) return true;
                }
            }
            return false;
        }

        public static bool Region_HasSentientRotResistance(On.Region.orig_HasSentientRotResistance orig, string name)
        {
            bool value = orig(name);
            if (CheckForMetaParam(RegionMetaParameter.RotImmune, name)) return true;
            return value;
        }

        public static bool Region_IsVanillaSentientRotRegion(On.Region.orig_IsVanillaSentientRotRegion orig, string name)
        {
            bool value = orig(name);
            if (CheckForMetaParam(RegionMetaParameter.VanillaRotten, name)) return true;
            return value;
        }

        public static bool Region_IsSentientRotRegion(On.Region.orig_IsSentientRotRegion orig, string name)
        {
            bool value = orig(name);
            if (CheckForMetaParam(RegionMetaParameter.Rotten, name)) return true;
            return value;
        }

        public static bool Region_HasWarpFatigueResistance(On.Region.orig_HasWarpFatigueResistance orig, string name)
        {
            bool value = orig(name);
            if (CheckForMetaParam(RegionMetaParameter.NoWarpFatigue, name)) return true;
            return value;
        }

        public static bool Region_IsShatteredTerraceRegion(On.Region.orig_IsShatteredTerraceRegion orig, string name)
        {
            bool value = orig(name);
            if (CheckForMetaParam(RegionMetaParameter.ShatteredTerrace, name)) return true;
            return value;
        }

        public static bool Region_IsRubiconRegion(On.Region.orig_IsRubiconRegion orig, string name)
        {
            bool value = orig(name);
            if (CheckForMetaParam(RegionMetaParameter.Rubicon, name)) return true;
            return value;
        }

        public static bool Region_IsAncientUrbanRegion(On.Region.orig_IsAncientUrbanRegion orig, string name)
        {
            bool value = orig(name);
            if (CheckForMetaParam(RegionMetaParameter.AncientUrban, name)) return true;
            return value;
        }

        public static bool Region_IsDaemonRegion(On.Region.orig_IsDaemonRegion orig, string name)
        {
            bool value = orig(name);
            if (CheckForMetaParam(RegionMetaParameter.Daemon, name)) return true;
            return value;
        }
    }
}