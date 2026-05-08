using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Looker.CWTs
{
    public static class VultureMaskCWT
    {

        public static readonly ConditionalWeakTable<VultureMask, DataClass> vultureMaskCWT = new();
        public static bool TryGetData(VultureMask key, out DataClass data)
        {
            if (key != null)
            {
                data = vultureMaskCWT.GetOrCreateValue(key);
            }
            else data = null;

            return data != null;
        }
        public class DataClass
        {
            public bool isKarmaMask = false;
            public LightSource lightSource = null;
        }
    }
}
