using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static lsfUtils.Plugin;

namespace lsfUtils.RegionParams
{
    public static class RegionTypeParams
    {
        public enum RegionMetaParameter { Rotten, VanillaRotten, RotImmune, Daemon, ShatteredTerrace, AncientUrban, WatcherVanilla, Rubicon }

        public static readonly Dictionary<string, RegionMetaParameter> regionMetaParameters = [];

        public static void Load()
        {
            regionMetaParameters.Clear();

            var path = AssetManager.ResolveFilePath("lsfUtils/regionMetaParameters.txt");

            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                Log.LogWarning("RegionList: regions.txt not found.");
                return;
            }

            foreach (string line in File.ReadAllLines(path))
            {
                if (string.IsNullOrEmpty(line)) continue;

                var parts = line.Trim().Split('=');
                if (parts.Length != 2)
                {
                    Log.LogWarning($"RegionTypeParams: Skipping malformed line: {line}");
                    continue;
                }

                var regionName = parts[0].Trim().ToUpperInvariant();
                var paramName = parts[1].Trim();

                if (!Enum.TryParse<RegionMetaParameter>(paramName, ignoreCase: true, out var param))
                {
                    Log.LogWarning($"RegionTypeParams: Unknown parameter '{paramName}' for region {regionName}");
                    continue;
                }

                regionMetaParameters[regionName] = param;
                Log.LogMessage($"RegionTypeParams: {regionName} -> {param}");
            }
        }
    }
}
