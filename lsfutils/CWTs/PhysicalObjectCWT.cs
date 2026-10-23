using System.Runtime.CompilerServices;

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

            public bool inHazerInk = false;
            public bool onFire = false;
            public int fireDuration = 0;
            public float permanentFireAmount = 0f;
            public float flammability = 2f;
            public bool canSpreadFire = false;
            public int fireLoss = 0;
        }
    }
}
