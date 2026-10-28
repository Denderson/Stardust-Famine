using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static lsfUtils.Enums;

namespace lsfUtils.Items.Spears.CopperSpears
{
    internal class CopperSpearHooks
    {
        public static void ApplyHooks()
        {
            On.SaveState.AbstractPhysicalObjectFromString += SaveState_AbstractPhysicalObjectFromString;
            On.AbstractPhysicalObject.Realize += AbstractPhysicalObject_Realize;
            On.Watcher.LightningMaker.StaticBuildup.GetBestTarget += Watcher_LightningMaker_StaticBuildup_GetBestTarget;
            On.Watcher.LightningMaker.StrikeAOE.ctor += Watcher_LightningMaker_StrikeAOE_ctor;
            On.Watcher.LightningMaker.StrikeAOE.Update += Watcher_LightningMaker_StrikeAOE_Update;
            On.ShelterDoor.DoorClosed += ShelterDoor_DoorClosed;
            On.AbstractSpear.StuckInWallTick += AbstractSpear_StuckInWallTick;
            On.MoreSlugcats.SingularityBomb.Explode += MoreSlugcats_SingularityBomb_Explode;
            On.ElectricDeath.Update += ElectricDeath_Update;
        }


        private static AbstractPhysicalObject SaveState_AbstractPhysicalObjectFromString(On.SaveState.orig_AbstractPhysicalObjectFromString orig, World world, string objString)
        {
            AbstractPhysicalObject obj = orig(world, objString);

            if (obj != null && obj.type == AbstractObjectType.CopperSpear)
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
            if (self.type == AbstractObjectType.CopperSpear)
            {
                if (self.realizedObject != null) return;

                CopperSpearAbstract abstractSpear = self as CopperSpearAbstract;

                if (abstractSpear == null)
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

        private static Vector2 Watcher_LightningMaker_StaticBuildup_GetBestTarget(On.Watcher.LightningMaker.StaticBuildup.orig_GetBestTarget orig, Watcher.LightningMaker.StaticBuildup self)
        {
            Vector2 value = orig(self);

            bool targetSelected = false; //added this because having multiple copper spears in the room would make them cancel each other out, but now instead only the first one in the targets list ever gets chosen
            foreach (PhysicalObject Obj in self.targets)
            {
                if (Obj != null && !self.IsTargetForbidden(Obj) && !self.room.lightningMaker.IsPosProtected(Obj.firstChunk.pos) && Obj is CopperSpear cSpear)
                {
                    if (Vector2.Distance(value, Obj.firstChunk.pos) <= 400 && !targetSelected)
                    {
                        //"if a copper spear is close to the previously chosen striking pos, it's redirected onto the spear"
                        Plugin.Log.LogMessage("copper spear selected for lightning!");
                        targetSelected = true;
                        return Obj.firstChunk.pos;
                    }
                }
            }
            return value;
        }
        private static void Watcher_LightningMaker_StrikeAOE_ctor(On.Watcher.LightningMaker.StrikeAOE.orig_ctor orig, Watcher.LightningMaker.StrikeAOE self, Vector2 pos, float effectRadius, float killRadius, PhysicalObject[] targets, Room room, Color col)
        {
            foreach (PhysicalObject Obj in targets)
            {
                if (Obj is CopperSpear cSpear && Vector2.Distance(pos, cSpear.firstChunk.pos) < 100)
                {
                    //"if close to a copper spear, lightning doesn't kill"
                    orig(self, pos, effectRadius, 0f ,targets, room, col);
                    return;
                }
            }
            orig(self, pos, effectRadius, killRadius, targets, room, col);
        }
        private static void Watcher_LightningMaker_StrikeAOE_Update(On.Watcher.LightningMaker.StrikeAOE.orig_Update orig, Watcher.LightningMaker.StrikeAOE self, bool eu)
        {
            orig(self, eu);
            for (int l = 0; l < self.targets.Length; l++)
            {
                CopperSpear cSpear = self.targets[l] as CopperSpear;
                if (cSpear != null && !cSpear.struck)
                {
                    //charges copper spears based on their proximity to the strike pos. does nothing if too far away, unsure why
                    cSpear.Charge(RWCustom.Custom.LerpMap(Vector2.Distance(self.pos, cSpear.firstChunk.pos), Math.Max(50, self.killRadius / 2), self.effectRadius / 2, 10f, 0.5f));
                    Plugin.Log.LogMessage($"Charged by lightning with intensity {RWCustom.Custom.LerpMap(Vector2.Distance(self.pos, cSpear.firstChunk.pos), Math.Max(50, self.killRadius / 2), self.effectRadius / 2, 10f, 0.5f)} (distance = {Vector2.Distance(self.pos, cSpear.firstChunk.pos)})");
                    cSpear.struck = true;
                }
            }
        }

        private static void ShelterDoor_DoorClosed(On.ShelterDoor.orig_DoorClosed orig, ShelterDoor self)
        {
            for (int i = 0; i < self.room.abstractRoom.entities.Count; i++)
            {
                if (self.room.abstractRoom.entities[i] is CopperSpearAbstract cSpearAbs)
                {
                    //oxidizes copper spears within the current shelter, i don't know how to do it across all shelters
                    (cSpearAbs.realizedObject as CopperSpear).Oxidize();
                    Plugin.Log.LogMessage("Oxidizing in shelter!");
                }
            }
            orig(self);
        }
        private static void AbstractSpear_StuckInWallTick(On.AbstractSpear.orig_StuckInWallTick orig, AbstractSpear self, int ticks)
        {
            if (self is CopperSpearAbstract cSpearAbs)
            {
                //same thing for spears stuck in walls
                if (self.stuckInWallCycles > 0)
                {
                    self.stuckInWallCycles = Math.Max(0, self.stuckInWallCycles - ticks);
                    cSpearAbs.oxidization = Math.Min(cSpearAbs.oxidization++, 6);
                    return;
                }
                if (self.stuckInWallCycles < 0)
                {
                    self.stuckInWallCycles = Math.Min(0, self.stuckInWallCycles + ticks);
                    cSpearAbs.oxidization = Math.Min(cSpearAbs.oxidization++, 6);
                }
            }
            else orig(self, ticks);
        }

        private static void MoreSlugcats_SingularityBomb_Explode(On.MoreSlugcats.SingularityBomb.orig_Explode orig, MoreSlugcats.SingularityBomb self)
        {
            orig(self);
            for (int m = 0; m < self.room.physicalObjects.Length; m++)
            {
                for (int n = 0; n < self.room.physicalObjects[m].Count; n++)
                {
                    if (self.room.physicalObjects[m][n].abstractPhysicalObject.rippleLayer == self.abstractPhysicalObject.rippleLayer || self.room.physicalObjects[m][n].abstractPhysicalObject.rippleBothSides || self.abstractPhysicalObject.rippleBothSides)
                    {
                        if (self.room.physicalObjects[m][n] is CopperSpear)
                        {
                            //charges copper spears based on their proximity to a singularity bomb explosion
                            (self.room.physicalObjects[m][n] as CopperSpear).Charge(RWCustom.Custom.LerpMap(RWCustom.Custom.Dist(self.room.physicalObjects[m][n].firstChunk.pos, self.firstChunk.pos), 350f, 1200f, 15f, 1f));
                            Plugin.Log.LogMessage("charged by singularity bomb");
                        }
                    }
                }
            }
        }

        private static void ElectricDeath_Update(On.ElectricDeath.orig_Update orig, ElectricDeath self, bool eu)
        {
            orig(self, eu);
            if (self.Intensity > 0.5f && UnityEngine.Random.value < RWCustom.Custom.LerpMap(self.Intensity, 0.5f, 1f, 0f, 0.5f))
            {
                for (int j = 0; j < self.room.physicalObjects.Length; j++)
                {
                    for (int k = 0; k < self.room.physicalObjects[j].Count; k++)
                    {
                        for (int l = 0; l < self.room.physicalObjects[j][k].bodyChunks.Length; l++)
                        {
                            if (UnityEngine.Random.value < RWCustom.Custom.LerpMap(self.Intensity, 0.5f, 1f, 0f, 0.5f) && (self.room.physicalObjects[j][k].bodyChunks[l].ContactPoint.x != 0 || self.room.physicalObjects[j][k].bodyChunks[l].ContactPoint.y != 0 || (self.room.GetTile(self.room.physicalObjects[j][k].bodyChunks[l].pos).AnyBeam && (!ModManager.PrecycleModule || self.cycle.preTimer <= 0))))
                            {
                                if (self.room.physicalObjects[j][k] is CopperSpear)
                                {
                                    //randomly charges copper spears while in electric death
                                    (self.room.physicalObjects[j][k] as CopperSpear).Charge(UnityEngine.Random.Range(5f, 10f) * self.Intensity);
                                    Plugin.Log.LogMessage("charged by electric death");
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
