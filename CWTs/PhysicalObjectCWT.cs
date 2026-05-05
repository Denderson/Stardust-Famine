using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace lsfUtils.CWTs
{
    public static class PhysicalObjectCWT
    {

        public static readonly ConditionalWeakTable<PhysicalObject, DataClass> physicalObjectCWT = new();
        public static bool TryGetData(PhysicalObject key, out DataClass data)
        {
            if (key != null)
            {
                data = physicalObjectCWT.GetOrCreateValue(key);
            }
            else data = null;

            return data != null;
        }
        public class DataClass
        {
            public bool shouldOverrideGravity;
            public float overrideGravity;
        }
    }
}
