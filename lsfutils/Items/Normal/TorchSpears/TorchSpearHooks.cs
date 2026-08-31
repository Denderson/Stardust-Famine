using lsfUtils;
using lsfUtils.Items.Normal.TorchSpears;
using RWCustom;
using UnityEngine;
using static lsfUtils.Enums;

namespace lsfUtils.Items.Normal.TorchSpears
{
    public static class TorchSpearHooks
    {
        public static void ApplyHooks()
        {
            On.AbstractPhysicalObject.Realize += AbstractPhysicalObject_Realize;
            On.Spear.HitSomething += Spear_HitSomething;
            On.SaveState.AbstractPhysicalObjectFromString += SaveState_AbstractPhysicalObjectFromString;
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
            if (self.type == Enums.AbstractObjectType.TorchSpear)
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

            if (result.obj is Creature targetCreature && self is TorchSpear torch && torch.isLit)
            {
                if (self.room != null)
                {
                    self.room.AddObject(new TorchBurnEffect(targetCreature, result.chunk, result.onAppendagePos));

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
    }
}