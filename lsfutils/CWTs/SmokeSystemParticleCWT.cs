using System.Drawing;
using System.Runtime.CompilerServices;

namespace lsfUtils.CWTs;

public static class SteamParticleCWT
{

    public static readonly ConditionalWeakTable<Smoke.SmokeSystem.SmokeSystemParticle, DataClass> steamParticleCWT = new();
    public static bool TryGetData(Smoke.SmokeSystem.SmokeSystemParticle key, out DataClass data)
    {
        if (key != null)
        {
            data = steamParticleCWT.GetOrCreateValue(key);
        }
        else data = null;

        return data != null;
    }
    public class DataClass
    {
        public Color? overrideColor = null;
    }
}
