using Stardust.Slugcats.Bitter.BitterGraphics;
using Stardust.Slugcats.Scholar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Stardust.CWTs
{
    public static class BitterGraphicsCWT
    {
        public static readonly ConditionalWeakTable<PlayerGraphics, BitterData> bitterCWT = new();
        public static bool TryGetData(PlayerGraphics key, out BitterData data)
        {
            if (key != null && key?.player?.SlugCatClass == Enums.SlugcatStatsName.bitter)
            {
                data = bitterCWT.GetOrCreateValue(key);
            }
            else data = null;

            return data != null;
        }
    }
}