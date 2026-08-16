using lsfUtils.CWTs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static lsfUtils.DevtoolsEffects.EvilWater.EvilWater;

namespace lsfUtils.DevtoolsEffects.EvilWater
{
    public static class EvilWaterHooks
    {
        public static void ApplyHooks()
        {
            On.Water.ctor += EvilWaterHooks.Water_ctor;
            On.Creature.Update += EvilWaterHooks.Creature_Update;
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
            if (!CreatureCWT.TryGetData(self, out var data))
            {
                orig(self, eu);
                return;
            }
            if (!WaterCWT.TryGetData(self.room?.waterObject, out var waterdata) || !waterdata.isPoisonous)
            {
                orig(self, eu);
                return;
            }

            float oldPoison = self.injectedPoison;
            self.injectedPoison = Mathf.Min(1f, self.injectedPoison + data.temporaryPoison);
            orig(self, eu);
            self.injectedPoison = oldPoison;

            if (self.Submersion > 0.5f)
            {
                if (data.timeInEvilWater >= waterdata.evilWaterPoisonDelayTimer) data.isInEvilWater = true;
                else data.timeInEvilWater++;
            }
            else
            {
                if (data.timeInEvilWater <= 0) data.isInEvilWater = false;
                else data.timeInEvilWater--;
            }

            if (data.isInEvilWater) data.temporaryPoison = Mathf.Min(1f, data.temporaryPoison + 1f / waterdata.evilWaterTimer);
            else data.temporaryPoison = Mathf.Max(0f, data.temporaryPoison - 1f / waterdata.evilWaterTimer);
        }

        /*public static float OverridePoison(Func<Creature, float> orig, Creature self)
        {
            float result = orig(self);
            if (self != null && CreatureCWT.TryGetData(self, out var data))
            {
                return Mathf.Max(result, data.temporaryPoison);
            }
            return result;
        }*/
    }
}
