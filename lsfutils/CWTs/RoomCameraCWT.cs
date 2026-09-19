using lsfUtils.ScreenVariants;
using System.Runtime.CompilerServices;

namespace lsfUtils.CWTs;

public static class RoomCameraCWT
{

    public static readonly ConditionalWeakTable<RoomCamera, DataClass> cameraCWT = new();
    public static bool TryGetData(RoomCamera key, out DataClass data)
    {
        if (key != null)
        {
            data = cameraCWT.GetOrCreateValue(key);
        }
        else data = null;

        return data != null;
    }
    public class DataClass
    {
        public ScreenVariantController screenVariantController;
    }
}
