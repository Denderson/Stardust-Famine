using lsfUtils.CWTs;
using UnityEngine;
using static lsfUtils.DevtoolsEffects.EvilWater.EvilWater;

namespace lsfUtils.DevtoolsEffects.EvilWater;

public static class EvilWaterHooks
{
    public static void ApplyHooks()
    {
        On.Water.ctor += Water_ctor;
        On.Creature.Update += Creature_Update;
    }

    public static void Water_ctor(On.Water.orig_ctor orig, Water self, Room room, int waterLevel)
    {
        orig(self, room, waterLevel);
        if (room?.roomSettings != null && HasEffect(room) && WaterCWT.TryGetData(self, out var waterdata))
        {
            waterdata.isPoisonous = true;
            if (RegionCWT.TryGetCustomRegionParams(self.room.world.region, out var paramsdata))
            {
                waterdata.evilWaterTimer = paramsdata.EvilWaterTimer;
                waterdata.evilWaterPoisonDelayTimer = paramsdata.EvilWaterPoisonDelayTimer;
                waterdata.evilWaterHealDelayTimer = paramsdata.EvilWaterHealDelayTimer;
            }
        }
    }

    public static void Creature_Update(On.Creature.orig_Update orig, Creature self, bool eu)
    {
        if (!CreatureCWT.TryGetData(self, out var data)) return;
        if (!WaterCWT.TryGetData(self.room?.waterObject, out var waterdata)) return;
        if (!waterdata.isPoisonous) return;

        orig(self, eu);

        if (self.Submersion > 0.5f)
        {
            data.timeInEvilWater++;
            if (data.timeInEvilWater >= waterdata.evilWaterPoisonDelayTimer)
            {
                data.temporaryPoison = Mathf.Min(1f, data.temporaryPoison + 1f / waterdata.evilWaterTimer);
                data.cannotRecoverPoison = 120;
            }
        }
        else if (data.timeInEvilWater > 0) data.timeInEvilWater--;
    }
}
