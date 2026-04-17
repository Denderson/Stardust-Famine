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
    public static class RegionGateCWT
    {

        public static readonly ConditionalWeakTable<RegionGate, DataClass> gateCWT = new();
        public static bool TryGetData(RegionGate key, out DataClass data)
        {
            if (key != null)
            {
                data = gateCWT.GetOrCreateValue(key);
            }
            else data = null;

            return data != null;
        }
        public class DataClass
        {
            public bool exhausted = false;
        }
    }
}
