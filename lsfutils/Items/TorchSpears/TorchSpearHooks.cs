using lsfUtils.CWTs;
using RWCustom;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Watcher;
using static lsfUtils.Enums;
using lsfUtils.Mechanics;
using lsfUtils.Mechanics.Fire;

namespace lsfUtils.Items.TorchSpears;

public static class TorchSpearHooks
{
    public static void ApplyHooks()
    {
        On.AbstractPhysicalObject.Realize += AbstractPhysicalObject_Realize;
        On.Spear.HitSomething += Spear_HitSomething;
        On.SaveState.AbstractPhysicalObjectFromString += SaveState_AbstractPhysicalObjectFromString;
        On.Watcher.FlameJet.UpdateDamage += Watcher_FlameJet_UpdateDamage;
        On.Smoke.BlackHaze.BigBlackHaze.BlackHazeVisionObscurer.Update += Smoke_BlackHaze_Update_BigBlackHaze_BlackHazeVisionObscurer;
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
        return obj;
    }

    private static void AbstractPhysicalObject_Realize(On.AbstractPhysicalObject.orig_Realize orig, AbstractPhysicalObject self)
    {
        if (self.type == AbstractObjectType.TorchSpear)
        {
            if (self.realizedObject != null) return;

            TorchSpearAbstract abstractSpear = self as TorchSpearAbstract;

            if (abstractSpear == null)
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
        orig(self);
    }

    private static bool Spear_HitSomething(On.Spear.orig_HitSomething orig, Spear self, SharedPhysics.CollisionResult result, bool eu)
    {
        bool hitResult = orig(self, result, eu);

        if (result.obj is Creature targetCreature && self is TorchSpear torch && torch.isLit && targetCreature.SpearStick(self, self.spearDamageBonus, result.chunk, null, self.firstChunk.vel))
        {
            if (self.room != null)
            {
                if (!torch.weakFlame) self.room.AddObject(new FlameEffect(targetCreature, result.chunk, result.onAppendagePos, 1600));
                else self.room.AddObject(new FlameEffect(targetCreature, result.chunk, result.onAppendagePos, Math.Min(1500 + torch.clothLife * 100, 800)));
                Vector2 hitPos = result.collisionPoint;
                self.room.AddObject(new Spark(hitPos, Custom.RNV() * 5f, new Color(1f, 0.4f, 0.1f), null, 10, 20));
            }

            targetCreature.Violence(
                source: self.firstChunk,
                directionAndMomentum: null,
                hitChunk: result.chunk ?? targetCreature.mainBodyChunk,
                hitAppendage: result.onAppendagePos,
                type: Creature.DamageType.Explosion,
                damage: 0.05f,
                stunBonus: 0f
            );
        }

        return hitResult;
    }
    
    private static void PhysicalObject_Collide(On.PhysicalObject.orig_Collide orig, PhysicalObject self, PhysicalObject other, int myChunk, int otherChunk)
    {
        orig(self, other, myChunk, otherChunk);
        if (self?.bodyChunks == null || other?.bodyChunks == null || self.bodyChunks.Count() <= 0 || other.bodyChunks.Count() <= 0) return;
        if (self.room != other.room || self.abstractPhysicalObject?.rippleLayer != other.abstractPhysicalObject.rippleLayer) return;
        if (!Custom.DistLess(self.bodyChunks[myChunk].pos, other.bodyChunks[otherChunk].pos, 20 + self.bodyChunks[myChunk].rad + other.bodyChunks[otherChunk].rad)) return;

        if (self is TorchSpear torchSpear && !torchSpear.isLit && other is TorchSpear otherTorch && otherTorch.isLit)
        {
            torchSpear.Relight(1);
        }
        else if (self is Creature creature1 && other is Creature creature2)
        {
            if (CreatureCWT.TryGetData(creature1, out var target) && CreatureCWT.TryGetData(creature2, out var burner))
            {
                if (target.canSpreadFire || burner.canSpreadFire)
                {
                    if (UnityEngine.Random.value < (target.flammability + burner.flammability + 1) / 9)
                    {
                        Plugin.Log.LogMessage($"{target.flammability}, {burner.flammability}. succeded with {(target.flammability + burner.flammability + 1) / 9 * 100}% chance");
                        if (!target.onFire && burner.onFire)
                        {
                            self.room.AddObject(new FlameEffect(creature1, self.bodyChunks[myChunk], null, burner.fireDuration));
                            Plugin.Log.LogMessage($"burned {self} for {burner.fireDuration / 1600f * 40} seconds");
                        }
                        else if (target.onFire && !burner.onFire)
                        {
                            self.room.AddObject(new FlameEffect(creature2, other.bodyChunks[otherChunk], null, target.fireDuration));
                            Plugin.Log.LogMessage($"burned {other} for {target.fireDuration / 1600f * 40} seconds");

                        }
                    }
                    else Plugin.Log.LogMessage($"{target.flammability}, {burner.flammability}.failed ({(target.flammability + burner.flammability + 1) / 9 * 100}% chance)");
                }
            }
        }
    }
    private static void Watcher_FlameJet_UpdateDamage(On.Watcher.FlameJet.orig_UpdateDamage orig, FlameJet self)
    {
        orig(self);
        for (int i = 0; i < self.room.physicalObjects.Length; i++)
        {
            foreach (PhysicalObject physicalObject in self.room.physicalObjects[i])
            {
                if (physicalObject is TorchSpear torchSpear && !torchSpear.isLit)
                {
                    Vector2 vector = torchSpear.clothPos[torchSpear.clothSegments - 1] - self.pos;
                    if (self.bounds.Vector2Inside(vector))
                    {
                        for (int j = 0; j < self.jetPos.Length - 1; j++)
                        {
                            float t = (float)(Time.frameCount % 10) * 0.1f;
                            Vector2 b = Vector2.Lerp(self.jetPos[j], self.jetPos[j + 1], t);
                            float num = Mathf.Lerp(self.jetScale[j], self.jetScale[j + 1], t);
                            float num2 = Mathf.Clamp((float)j / 3f, 0.5f, 1f);
                            num2 *= Mathf.Clamp((float)(self.jetPos.Length - 1 - j) / 5f, 0.5f, 1f);
                            float num3 = Vector2.Distance(vector, b);
                            float num4 = num * 0.5f * num2;
                            if (num3 <= num4)
                            {
                                torchSpear.Relight(1);
                            }
                        }
                    }
                }
            }
        }
    }

    private static void Smoke_BlackHaze_Update_BigBlackHaze_BlackHazeVisionObscurer(On.Smoke.BlackHaze.BigBlackHaze.BlackHazeVisionObscurer.orig_Update orig, Smoke.BlackHaze.BigBlackHaze.BlackHazeVisionObscurer self, bool eu)
    {
        orig(self, eu);
        self.obscureFac = Mathf.InverseLerp(0.1f, 0.8f, self.bigBlackHaze.alpha);
        self.rad = self.bigBlackHaze.Rad(1f) * 0.55f;
        self.pos = self.bigBlackHaze.pos;
        if (UnityEngine.Random.value < self.bigBlackHaze.alpha)
        {
            for (int i = 0; i < self.room.physicalObjects.Length; i++)
            {
                foreach (PhysicalObject physicalObject in self.room.physicalObjects[i])
                {
                    if (physicalObject is TorchSpear torchSpear && torchSpear.isLit && (physicalObject.abstractPhysicalObject.rippleLayer == self.bigBlackHaze.smoke.rippleLayer || physicalObject.abstractPhysicalObject.rippleBothSides) && Custom.DistLess(torchSpear.clothPos[torchSpear.clothSegments - 1], self.pos, self.rad * Mathf.Pow(self.bigBlackHaze.alpha, 0.6f) / 2f)) torchSpear.Extinguish(true);
                }
            }
            if (self.room.abstractRoom.creatures.Count > 0)
            {
                AbstractCreature abstractCreature = self.room.abstractRoom.creatures[UnityEngine.Random.Range(0, self.room.abstractRoom.creatures.Count)];
                if (CreatureCWT.TryGetData(abstractCreature.realizedCreature, out var data))
                { 
                    if (abstractCreature.realizedCreature != null && (abstractCreature.rippleLayer == self.bigBlackHaze.smoke.rippleLayer || abstractCreature.rippleBothSides) && Custom.DistLess(abstractCreature.realizedCreature.mainBodyChunk.pos, self.pos, self.rad * Mathf.Pow(self.bigBlackHaze.alpha, 0.6f) / 2f))
                    {
                        data.inHazerInk = true;
                    }
                    else data.inHazerInk = false;
                }
            }
        }
    }
}