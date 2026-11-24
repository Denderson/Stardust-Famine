using lsfUtils.CreatureTags;
using lsfUtils.CWTs;
using lsfUtils.Items.Spears.TorchSpears;
using lsfUtils.Mechanics.Fire;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering;

namespace lsfUtils.DevtoolsEffects.Sunburn
{
    internal class SunburnHooks
    {
        public static void ApplyHooks()
        {
            On.PhysicalObject.Update += PhysicalObject_Update;
        }

        private static void PhysicalObject_Update(On.PhysicalObject.orig_Update orig, PhysicalObject self, bool eu)
        {
            orig(self, eu);
            if (!RegionCWT.TryGetCustomRegionParams(self.room.world.region, out var regionData)) return;
            if (Sunburn.InSunlight(self))
            {
                float burnSpeed = regionData.SunburnBurnTimer > 0 ? 1f / regionData.SunburnBurnTimer : 1f / 400;
                float floatFix = 0.0001f;
                if (self is Creature creature && CreatureCWT.TryGetData(creature, out var creatureData) && !creatureData.onFire && creatureData.fireBuildupAmount < 1f && creatureData.flammability > 0f)
                {
                    if (creature.Hypothermia > 0f)
                    {
                        creature.Hypothermia -= burnSpeed / 2 * creatureData.flammability;
                        creatureData.fireBuildupAmount += burnSpeed / 4 * creatureData.flammability;
                    }
                    else creatureData.fireBuildupAmount += burnSpeed / 2 * creatureData.flammability;

                    if (creatureData.fireBuildupAmount >= 1f)
                    {
                        self.room.AddObject(new FlameEffect(creature, creature.RandomChunk, null, 160));
                        self.room.PlaySound(SoundID.Firecracker_Burn, creature.firstChunk.pos, 0.7f, 1.2f);
                    }
                    else if (Math.Abs(creatureData.fireBuildupAmount - 0.5f) < floatFix || Math.Abs(creatureData.fireBuildupAmount - 0.6f) < floatFix || Math.Abs(creatureData.fireBuildupAmount - 0.7f) < floatFix || Math.Abs(creatureData.fireBuildupAmount - 0.8f) < floatFix || Math.Abs(creatureData.fireBuildupAmount - 0.9f) < floatFix)
                    {
                        self.room.AddObject(new Smoke.Smolder(self.room, creature.abstractCreature.pos.Vec2(), creature.RandomChunk, null));
                        self.room.PlaySound(SoundID.Fire_Spear_Ignite, self.firstChunk.pos, 0.7f, 1.2f);
                    }
                    if (creature is Player player && creatureData.fireBuildupAmount >= 0.7f)
                    {
                        player.Blink(8);
                    }

                    //Plugin.Log.LogMessage($"burn timer for {creature}: {creatureData.fireBuildupAmount}");
                }
                else if (self is not Creature && PhysicalObjectCWT.TryGetData(self, out var objectData) && !objectData.onFire && objectData.fireBuildupAmount < 1f && objectData.flammability > 0f)
                {
                    objectData.fireBuildupAmount += burnSpeed / 2 * objectData.flammability;
                    if (self is TorchSpear tSpear && objectData.fireBuildupAmount >= 1f)
                    {
                        tSpear.Relight(1);
                        objectData.fireBuildupAmount = 0f;
                    }
                    else if (objectData.fireBuildupAmount >= 1f)
                    {
                        self.room.AddObject(new FlameEffect(self, self.RandomChunk, null, 160));
                    }
                    else if (Math.Abs(objectData.fireBuildupAmount - 0.5f) < floatFix || Math.Abs(objectData.fireBuildupAmount - 0.6f) < floatFix || Math.Abs(objectData.fireBuildupAmount - 0.7f) < floatFix || Math.Abs(objectData.fireBuildupAmount - 0.8f) < floatFix || Math.Abs(objectData.fireBuildupAmount - 0.9f) < floatFix)
                    {
                        self.room.AddObject(new Smoke.Smolder(self.room, self.abstractPhysicalObject.pos.Vec2(), self.RandomChunk, null));
                    }
                    //Plugin.Log.LogMessage($"burn timer for {self}: {objectData.fireBuildupAmount}");
                }
            }
            else
            {
                float recoverySpeed = regionData.SunburnRecoveryTimer > 0 ? 1f / regionData.SunburnRecoveryTimer : 1f / 600f;
                if (self is Creature creature && CreatureCWT.TryGetData(creature, out var creatureData) && creatureData.fireBuildupAmount > 0f)
                {
                    creatureData.fireBuildupAmount = Math.Max(creatureData.fireBuildupAmount - recoverySpeed, 0f);
                    if (creature.Submersion > 0.2f) creatureData.fireBuildupAmount = 0f;
                }
                else if (PhysicalObjectCWT.TryGetData(self, out var objectData) && objectData.fireBuildupAmount > 0f)
                {
                    objectData.fireBuildupAmount = Math.Max(objectData.fireBuildupAmount - recoverySpeed, 0f);
                    if (self.Submersion > 0.2f) objectData.fireBuildupAmount = 0f;
                }
            }
        }
    }
}
