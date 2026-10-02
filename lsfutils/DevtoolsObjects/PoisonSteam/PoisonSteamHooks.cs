using lsfUtils.CWTs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lsfUtils.DevtoolsObjects.PoisonSteam;

public static class PoisonSteamHooks
{
    public static Color poisonSteamOverrideColor = Color.Green;

    public static void ApplyHooks()
    {
        On.Smoke.SteamSmoke.CreateParticle += SteamSmoke_CreateParticle;
    }

    private static Smoke.SmokeSystem.SmokeSystemParticle SteamSmoke_CreateParticle(On.Smoke.SteamSmoke.orig_CreateParticle orig, Smoke.SteamSmoke self)
    {
        Smoke.SmokeSystem.SmokeSystemParticle newParticle = orig(self);
        if (SteamSmokeCWT.TryGetData(self, out var data) && data.isPoisonSmoke)
        {
            if (SteamParticleCWT.TryGetData(newParticle, out var particleData)) particleData.overrideColor = poisonSteamOverrideColor;
        }
        return newParticle;
    }
}