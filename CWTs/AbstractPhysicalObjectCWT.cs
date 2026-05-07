using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace lsfUtils.CWTs
{
    public static class AbstractPhysicalObjectCWT
    {

        public static readonly ConditionalWeakTable<AbstractPhysicalObject, DataClass> abstractPhysicalObjectCWT = new();
        public static bool TryGetData(AbstractPhysicalObject key, out DataClass data)
        {
            if (key != null)
            {
                data = abstractPhysicalObjectCWT.GetOrCreateValue(key);
            }
            else data = null;

            return data != null;
        }
        public class DataClass
        {
            public bool isripplehybrid;
        }
    }
}