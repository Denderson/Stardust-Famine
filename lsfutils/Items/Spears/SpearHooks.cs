using lsfUtils.CWTs;
using lsfUtils.Items.Spears.CopperSpears;
using lsfUtils.Items.Spears.TorchSpears;
using lsfUtils.Mechanics.Fire;
using RWCustom;
using MoreSlugcats;
using System;
using System.Linq;
using UnityEngine;
using static lsfUtils.Enums;
using static lsfUtils.Plugin;

namespace lsfUtils.Items.Spears;

internal class SpearHooks
{
    public static void ApplyHooks()
    {
        On.SaveState.AbstractPhysicalObjectFromString += SaveState_AbstractPhysicalObjectFromString;
        On.AbstractPhysicalObject.Realize += AbstractPhysicalObject_Realize;
        On.ShelterDoor.DoorClosed += ShelterDoor_DoorClosed;
        On.PhysicalObject.Collide += PhysicalObject_Collide;

    }
    private static AbstractPhysicalObject SaveState_AbstractPhysicalObjectFromString(On.SaveState.orig_AbstractPhysicalObjectFromString orig, World world, string objString)
    {
        AbstractPhysicalObject obj = orig(world, objString);

        if (obj != null && obj.type == AbstractObjectType.TorchSpear)
        {
            if (objString.Contains("<BaseA>"))
            {
                objString = objString.Replace("<BaseA>", "<oA>");
            }

            string[] array = System.Text.RegularExpressions.Regex.Split(objString, "<oA>");

            bool explosive = false;
            if (array.Length > 4) explosive = array[4] == "1";

            float hue = 0f;
            if (array.Length > 5)
            {
                float.TryParse(array[5], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out hue);
            }

            TorchSpearAbstract torchSpear = new TorchSpearAbstract(world, null, obj.pos, obj.ID, explosive, hue);

            if (array.Length > 3)
            {
                int.TryParse(array[3], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out torchSpear.stuckInWallCycles);
            }

            if (array.Length > 6)
            {
                torchSpear.isLit = array[array.Length - 1] == "1";
            }

            return torchSpear;
        }
        else if (obj != null && obj.type == AbstractObjectType.CopperSpear)
        {
            if (objString.Contains("<BaseA>"))
            {
                objString = objString.Replace("<BaseA>", "<oA>");
            }

            string[] array = System.Text.RegularExpressions.Regex.Split(objString, "<oA>");

            bool explosive = false;
            if (array.Length > 4) explosive = array[4] == "1";

            float hue = 0f;
            if (array.Length > 5)
            {
                float.TryParse(array[5], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out hue);
            }

            CopperSpearAbstract copperSpear = new CopperSpearAbstract(world, null, obj.pos, obj.ID, explosive, hue);

            if (array.Length > 3)
            {
                int.TryParse(array[3], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out copperSpear.stuckInWallCycles);
            }

            if (array.Length > 6)
            {
                int.TryParse(array[array.Length - 1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out copperSpear.oxidization);
            }
            return copperSpear;
        }
        return obj;
    }

    private static void AbstractPhysicalObject_Realize(On.AbstractPhysicalObject.orig_Realize orig, AbstractPhysicalObject self)
    {
        if (self.type == AbstractObjectType.TorchSpear)
        {
            if (self.realizedObject != null) return;
            if (self is not TorchSpearAbstract abstractSpear)
            {
                abstractSpear = new TorchSpearAbstract(self.world, self.pos, self.ID);
                AbstractRoom absRoom = self.world.GetAbstractRoom(self.pos);
                if (absRoom != null)
                {
                    for (int i = 0; i < absRoom.entities.Count; i++)
                    {
                        if (absRoom.entities[i] == self)
                        {
                            absRoom.entities[i] = abstractSpear;
                            break;
                        }
                    }
                }
            }
            self.realizedObject = new TorchSpear(abstractSpear, self.world);
            return;
        }
        else if (self?.type == AbstractObjectType.CopperSpear)
        {
            if (self.realizedObject != null) return;
            if (self is not CopperSpearAbstract abstractSpear)
            {
                abstractSpear = new CopperSpearAbstract(self.world, self.pos, self.ID);
                AbstractRoom absRoom = self.world.GetAbstractRoom(self.pos);
                if (absRoom != null)
                {
                    for (int i = 0; i < absRoom.entities.Count; i++)
                    {
                        if (absRoom.entities[i] == self)
                        {
                            absRoom.entities[i] = abstractSpear;
                            break;
                        }
                    }
                }
            }
            self.realizedObject = new CopperSpear(abstractSpear, self.world);
            return;
        }
        orig(self);
    }

    private static void ShelterDoor_DoorClosed(On.ShelterDoor.orig_DoorClosed orig, ShelterDoor self)
    {
        for (int i = 0; i < self.room.abstractRoom.entities.Count; i++)
        {
            if (self.room.abstractRoom.entities[i] is CopperSpearAbstract tSpearAbs)
            {
                (tSpearAbs.realizedObject as TorchSpear).Extinguish(false);
                Plugin.Log.LogMessage("Extinguishing in shelter!");
            }
            else if (self.room.abstractRoom.entities[i] is CopperSpearAbstract cSpearAbs)
            {
                //oxidizes copper spears within the current shelter, i don't know how to do it across all shelters
                (cSpearAbs.realizedObject as CopperSpear).Oxidize();
                Log.LogMessage("Oxidizing in shelter!");
            }
        }
        orig(self);
    }

    private static void PhysicalObject_Collide(On.PhysicalObject.orig_Collide orig, PhysicalObject self, PhysicalObject other, int myChunk, int otherChunk)
    {
        orig(self, other, myChunk, otherChunk);
        if (self?.bodyChunks == null || other?.bodyChunks == null || self.bodyChunks.Count() <= 0 || other.bodyChunks.Count() <= 0) return;
        if (self.room != other.room || self.abstractPhysicalObject?.rippleLayer != other.abstractPhysicalObject.rippleLayer) return;
        if (!Custom.DistLess(self.bodyChunks[myChunk].pos, other.bodyChunks[otherChunk].pos, 30 + self.bodyChunks[myChunk].rad + other.bodyChunks[otherChunk].rad)) return;
        if (self is CopperSpear cSpear && other is ElectricSpear elSpear)
        {
            if (elSpear.abstractSpear.electricCharge > 0)
            {
                cSpear.Charge(elSpear.abstractSpear.electricCharge / (float)(2 * (cSpear.charges + 1)));
            }
            else if (cSpear.intensity > 0f)
            {
                elSpear.Recharge();
            }
        }
        if (self is TorchSpear selfTorch)
        {
            if (other is not Creature && other is PhysicalObject otherObject)
            {
                if ((!selfTorch.isLit || selfTorch.fireLife < 3) && other is TorchSpear otherTorch && otherTorch.isLit)
                {
                    selfTorch.Relight(1);
                }
                else if (selfTorch.isLit && PhysicalObjectCWT.TryGetData(otherObject, out var target) && !target.onFire && otherObject.collisionLayer != 1)
                {
                    self.room.AddObject(new FlameEffect(otherObject, other.bodyChunks[otherChunk], null, Math.Min(Math.Max(1500 + selfTorch.clothLife * 100, 800), 1600) + (200 * selfTorch.fireLife - 600)));
                    if (otherObject is not Spear) selfTorch.Damage(1);
                }
            }
        }
        else if (self is Creature creature1 && CreatureCWT.TryGetData(creature1, out var targetCreature) && !targetCreature.onFire)
        {
            if (other is Creature creature2 && CreatureCWT.TryGetData(creature2, out var burnerCreature) && burnerCreature.onFire)
            {
                if (burnerCreature.canSpreadFire && UnityEngine.Random.value < (targetCreature.flammability + burnerCreature.flammability + 1) / 10)
                {
                    self.room.AddObject(new FlameEffect(creature1, self.bodyChunks[myChunk], null, burnerCreature.fireDuration));
                    Plugin.Log.LogMessage($"burned {self} for {burnerCreature.fireDuration / 1600f * 40} seconds");

                }
            }
            else if (other is PhysicalObject physicalObject && other is not TorchSpear && PhysicalObjectCWT.TryGetData(physicalObject, out var burnerObject) && burnerObject.onFire)
            {
                if (burnerObject.canSpreadFire && UnityEngine.Random.value < (targetCreature.flammability + burnerObject.flammability + 1) / 10)
                {
                    self.room.AddObject(new FlameEffect(creature1, self.bodyChunks[myChunk], null, 2 * burnerObject.fireDuration / 3));
                    Plugin.Log.LogMessage($"burned {self} for {burnerObject.fireDuration / 2400f * 40} seconds");
                }

            }
        }
        else if (self is PhysicalObject object1 && PhysicalObjectCWT.TryGetData(object1, out var targetObject) && !targetObject.onFire)
        {
            if (other is Creature creature && CreatureCWT.TryGetData(creature, out var burnerCreature) && burnerCreature.onFire)
            {
                if (burnerCreature.canSpreadFire && UnityEngine.Random.value < (targetObject.flammability + burnerCreature.flammability + 1) / 10)
                {
                    self.room.AddObject(new FlameEffect(object1, self.bodyChunks[myChunk], null, 2 * burnerCreature.fireDuration / 3));
                    Plugin.Log.LogMessage($"burned {self} for {burnerCreature.fireDuration / 2400f * 40} seconds");

                }
            }
            else if (other is PhysicalObject object2 && other is not TorchSpear && PhysicalObjectCWT.TryGetData(object2, out var burnerObject) && burnerObject.onFire)
            {
                if (burnerObject.canSpreadFire && UnityEngine.Random.value < (targetObject.flammability + burnerObject.flammability + 1) / 10)
                {
                    self.room.AddObject(new FlameEffect(object2, self.bodyChunks[myChunk], null, burnerObject.fireDuration));
                    Plugin.Log.LogMessage($"burned {self} for {burnerObject.fireDuration / 1600f * 40} seconds");

                }

            }
        }
    }

}
