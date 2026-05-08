using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Looker.CWTs
{
    public static class LanternCWT
    {

        public static readonly ConditionalWeakTable<Lantern, DataClass> lanternCWT = new();
        public static bool TryGetData(Lantern key, out DataClass data)
        {
            if (key != null)
            {
                data = lanternCWT.GetOrCreateValue(key);
            }
            else data = null;

            return data != null;
        }
        public class DataClass
        {
            public int health = 200;
        }
    }
}
