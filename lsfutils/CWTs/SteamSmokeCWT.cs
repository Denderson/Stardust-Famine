using System.Drawing;
using System.Runtime.CompilerServices;

namespace lsfUtils.CWTs;

public static class SteamSmokeCWT
{

    public static readonly ConditionalWeakTable<Smoke.SteamSmoke, DataClass> steamCWT = new();
    public static bool TryGetData(Smoke.SteamSmoke key, out DataClass data)
    {
        if (key != null)
        {
            data = steamCWT.GetOrCreateValue(key);
        }
        else data = null;

        return data != null;
    }
    public class DataClass
    {
        public bool isPoisonSmoke = false;
    }
}
