using System.Runtime.CompilerServices;
using Watcher;

namespace lsfUtils.CWTs;

public static class WarpRegionIconCWT
{
    public static readonly ConditionalWeakTable<WarpMap.WarpRegionIcon, DataClass> warpRegionIconCWT = new();
    public static bool TryGetData(WarpMap.WarpRegionIcon key, out DataClass data)
    {
        if (key != null)
        {
            data = warpRegionIconCWT.GetOrCreateValue(key);
        }
        else data = null;

        return data != null;
    }
    public class DataClass
    {
        public WarpMap map = null;
    }
}
