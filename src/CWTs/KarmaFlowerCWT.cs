using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace lsfUtils.CWTs
{
    public static class KarmaFlowerCWT
    {

        public static readonly ConditionalWeakTable<KarmaFlower, DataClass> karmaFlowerCWT = new();
        public static bool TryGetData(KarmaFlower key, out DataClass data)
        {
            if (key != null)
            {
                data = karmaFlowerCWT.GetOrCreateValue(key);
            }
            else data = null;

            return data != null;
        }
        public class DataClass
        {
            public bool rippleFlower = false;
            public int rippleLayer = 0;
        }
    }
}