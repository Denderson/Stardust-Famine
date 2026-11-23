using System.Runtime.CompilerServices;

namespace lsfUtils.CWTs;

public static class GameCWT
{

    public static readonly ConditionalWeakTable<RainWorldGame, DataClass> rainWorldGameCWT = new();
    public static bool TryGetData(RainWorldGame key, out DataClass data)
    {
        if (key != null)
        {
            data = rainWorldGameCWT.GetOrCreateValue(key);
        }
        else data = null;

        return data != null;
    }
    public class DataClass
    {
        public bool switchedLocks = false;
    }
}
