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
    public static class PlayerThreatTrackerCWT
    {

        public static readonly ConditionalWeakTable<Music.PlayerThreatTracker, DataClass> playerThreatTrackerCWT = new();
        public static bool TryGetData(Music.PlayerThreatTracker key, out DataClass data)
        {
            if (key != null)
            {
                data = playerThreatTrackerCWT.GetOrCreateValue(key);
            }
            else data = null;

            return data != null;
        }
        public class DataClass
        {
            public bool anchorMode;
            public float anchorIntensity;
        }
    }
}
