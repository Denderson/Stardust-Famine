using System.Runtime.CompilerServices;

namespace lsfUtils.CWTs
{
    public static class RegionCWT
    {

        public static readonly ConditionalWeakTable<Region, DataClass> regionCWT = new();
        public static bool TryGetData(Region key, out DataClass data)
        {
            if (key != null)
            {
                data = regionCWT.GetOrCreateValue(key);
            }
            else data = null;

            return data != null;
        }

        public static bool TryGetCustomRegionParams(Region key, out CustomRegionParams customRegionParams)
        {
            customRegionParams = null;
            if (TryGetData(key, out var data) && data?.customRegionParams != null)
            {
                customRegionParams = data.customRegionParams;
                return true;
            }
            return false;
        }

        public class DataClass
        {
            public CustomRegionParams customRegionParams;
        }
    }
}
