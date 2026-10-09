using lsfUtils.CWTs;
using UnityEngine;
using static lsfUtils.DevtoolsEffects.EvilWater.EvilWater;
using static lsfUtils.Plugin;

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
                Log.LogMessage($"Evil Water Timer: {waterdata.evilWaterTimer}");
                waterdata.evilWaterPoisonDelayTimer = paramsdata.EvilWaterPoisonDelayTimer;
                Log.LogMessage($"Evil Water Poison Delay Timer: {waterdata.evilWaterPoisonDelayTimer}");
                waterdata.evilWaterHealDelayTimer = paramsdata.EvilWaterHealDelayTimer;
                Log.LogMessage($"Evil Water Heal Delay Timer: {waterdata.evilWaterHealDelayTimer}");
            }
        }
    }

    public static void Creature_Update(On.Creature.orig_Update orig, Creature self, bool eu)
    {
        if (!CreatureCWT.TryGetData(self, out var data) || !WaterCWT.TryGetData(self.room?.waterObject, out var waterdata) || !waterdata.isPoisonous)
        {
            orig(self, eu);
            return;
        }

        orig(self, eu);

        if (self.Submersion > 0.5f)
        {
            data.timeInEvilWater++;
            if (data.timeInEvilWater >= waterdata.evilWaterPoisonDelayTimer)
            {
                data.temporaryPoison = Mathf.Min(1f, data.temporaryPoison + 1f / waterdata.evilWaterTimer);
                data.cannotRecoverPoison = waterdata.evilWaterHealDelayTimer;
            }
        }
        else if (data.timeInEvilWater > 0) data.timeInEvilWater--;
    }
}
