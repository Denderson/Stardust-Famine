using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace lsfUtils.Creatures.Scavs.ScavFlank
{
    public static class ScavFlankHooks
    {
        public static void Scavenger_Violence(On.Scavenger.orig_Violence orig, Scavenger self, BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppendage, Creature.DamageType type, float damage, float stunBonus)
        {
            bool wasTriggered = self.karmicArmorTriggered;
            orig(self, source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
            if (self?.abstractCreature != null && self.abstractCreature.IsFlank() && self.karmicArmorTriggered)
            {
                if (wasTriggered && (self as ScavFlank).reinforcedShield)
                {
                    self.karmicArmorResetTime /= 3;
                    (self as ScavFlank).reinforcedShield = false;
                    if (self.karmicArmor != null)
                    {
                        self.karmicArmor.rad /= 1.2f;
                        self.room.AddObject(new KarmicShockwave(self, self.karmicArmor.pos, 20, 15f, 300f));
                    }
                }
                else self.karmicArmorResetTime *= 3;
            }
        }

        public static void ScavengerAbstractAI_InOffscreenDen(On.ScavengerAbstractAI.orig_InOffscreenDen orig, ScavengerAbstractAI self)
        {
            orig(self);
            if (self?.parent?.creatureTemplate?.type == Enums.CreatureTemplateType.ScavFlank)
            {
                if (self.freeze > 0)
                {
                    self.freeze = 100;
                }
            }
        }

        public static void Scavenger_ctor(On.Scavenger.orig_ctor orig, Scavenger self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            if (abstractCreature != null && abstractCreature.IsFlank())
            {
                self.karmaLevels = UnityEngine.Random.Range(4, 6);
            }
        }

        public static bool KarmicArmor_Protected(Func<Scavenger, bool> orig, Scavenger self)
        {
            return orig(self) || self?.abstractCreature != null && self.karmaLevels > 0 && self.abstractCreature.IsFlank();
        }

        public static void Scavenger_Update(On.Scavenger.orig_Update orig, Scavenger self, bool eu)
        {
            orig(self, eu);
            if (self?.abstractCreature != null && !self.inShortcut && self.abstractCreature.IsFlank())
            {
                if (self.karmicArmor != null)
                {
                    self.karmicArmor.subtleMode = false;
                    if (self.karmicArmor.slatedForDeletetion || self.karmicArmor.room != self.room)
                    {
                        self.karmicArmor.Destroy();
                        self.karmicArmor = null;
                    }
                }
                if (self.karmaLevels > 0 && self.karmicArmor == null)
                {
                    self.karmicArmor = new KarmicArmor(self, self.karmaLevels);
                    self.room.AddObject(self.karmicArmor);
                }
            }
        }

        public static void Scavenger_PlaceInRoom(On.Scavenger.orig_PlaceInRoom orig, Scavenger self, Room placeRoom)
        {
            orig(self, placeRoom);
            if (self?.abstractCreature != null && self.abstractCreature.IsFlank())
            {
                self.karmaLevels = UnityEngine.Random.Range(4, 6);
            }
        }
    }
}
