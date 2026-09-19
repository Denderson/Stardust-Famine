using System.Runtime.CompilerServices;

namespace Stardust.CWTs;

public static class RoomCameraCWT
{

    public static readonly ConditionalWeakTable<RoomCamera, DataClass> roomCameraCWT = new();
    public static bool TryGetData(RoomCamera key, out DataClass data)
    {
        if (key != null)
        {
            data = roomCameraCWT.GetOrCreateValue(key);
        }
        else data = null;

        return data != null;
    }
    public class DataClass
    {
        //public DeeperspaceData deeperspaceData;
    }
}
