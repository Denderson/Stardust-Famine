using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Looker.CWTs
{
    public static class ARKillRectCWT
    {

        public static readonly ConditionalWeakTable<ARKillRect, DataClass> killrectCWT = new();
        public static bool TryGetData(ARKillRect key, out DataClass data)
        {
            if (key != null)
            {
                data = killrectCWT.GetOrCreateValue(key);
            }
            else data = null;

            return data != null;
        }
        public class DataClass
        {
            public bool active = true;
        }
    }
}
