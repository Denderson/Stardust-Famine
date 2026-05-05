using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace lsfUtils.CWTs
{
    public static class WaterCWT
    {

        public static readonly ConditionalWeakTable<Water, DataClass> waterCWT = new();
        public static bool TryGetData(Water key, out DataClass data)
        {
            if (key != null)
            {
                data = waterCWT.GetOrCreateValue(key);
            }
            else data = null;

            return data != null;
        }
        public class DataClass
        {
            public bool isPoisonous;
        }
    }
}
