using System.Runtime.CompilerServices;

namespace lsfUtils.CWTs;

public static class AbstractRoomCWT
{

    public static readonly ConditionalWeakTable<AbstractRoom, DataClass> abstractRoomCWT = new();
    public static bool TryGetData(AbstractRoom key, out DataClass data)
    {
        if (key != null)
        {
            data = abstractRoomCWT.GetOrCreateValue(key);
        }
        else data = null;

        return data != null;
    }
    public class DataClass
    {
        public int[] originalConnections = null;
    }
}
