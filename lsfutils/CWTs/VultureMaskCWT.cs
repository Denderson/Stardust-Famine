using System.Runtime.CompilerServices;

namespace lsfUtils.CWTs
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
