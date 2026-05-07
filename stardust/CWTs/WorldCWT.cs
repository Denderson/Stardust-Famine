using Stardust.Anchors;
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
    public static class WorldCWT
    {

        public static readonly ConditionalWeakTable<World, DataClass> worldCWT = new();
        public static bool TryGetData(World key, out DataClass data)
        {
            if (key != null)
            {
                data = worldCWT.GetOrCreateValue(key);
            }
            else data = null;

            return data != null;
        }
        public class DataClass
        {
            public AnchorWorldPresence anchorWorldPresence;
            public bool hasAnchorWorldPresence;
        }
    }
}
