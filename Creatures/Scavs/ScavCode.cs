using lsfUtils.Creatures.Scavs.ScavFlank;
using lsfUtils.Creatures.Scavs.ScavMessenger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static lsfUtils.Plugin;

namespace lsfUtils.Creatures.Scavs
{
    public static class ScavCode
    {
        public static bool IsSeer(this AbstractCreature creature)
        {
            return creature.creatureTemplate.type == Enums.CreatureTemplateType.ScavSeer;
        }

        public static bool IsFlank(this AbstractCreature creature)
        {
            return creature.creatureTemplate.type == Enums.CreatureTemplateType.ScavFlank;
        }

        public static bool IsMessenger(this AbstractCreature creature)
        {
            return creature.creatureTemplate.type == Enums.CreatureTemplateType.ScavMessenger;
        }

        public static bool IsNotSpecialScav(this AbstractCreature creature)
        {
            return creature.creatureTemplate.type != Enums.CreatureTemplateType.ScavSeer && creature.creatureTemplate.type != Enums.CreatureTemplateType.ScavMessenger && creature.creatureTemplate.type != Enums.CreatureTemplateType.ScavFlank;
        }
        public static bool AvoidsCombat(this AbstractCreature creature)
        {
            return creature.IsSeer() || creature.IsMessenger();
        }

        public static void ScavengerAI_DecideBehavior(On.ScavengerAI.orig_DecideBehavior orig, ScavengerAI self)
        {
            // TODO
            orig(self);
            AIModule aIModule = self.utilityComparer.HighestUtilityModule();
            if (aIModule != null && self?.scavenger?.abstractCreature != null)
            {
                if (self.scavenger.abstractCreature.IsFlank())
                {
                    if (aIModule is ThreatTracker && self.scavenger.karmaLevels > 0)
                    {
                        self.behavior = ScavengerAI.Behavior.Attack;
                    }

                }
                if (self.scavenger.abstractCreature.IsSeer())
                {
                    // possibly change some behaviour?
                    if (false) // add losing condition
                    {
                        //self.behavior = ScavengerAI.Behavior.Injured;
                    }

                }
            }
        }

        

        public static void Scavenger_SetUpCombatSkills(On.Scavenger.orig_SetUpCombatSkills orig, Scavenger self)
        {
            orig(self);
            if (self?.abstractCreature != null)
            {
                if (self.abstractCreature.IsFlank())
                {
                    self.dodgeSkill /= 2;
                    self.meleeSkill = 1f;
                }
                if (self.abstractCreature.IsMessenger())
                {
                    self.dodgeSkill = Mathf.Clamp(self.dodgeSkill * 1.5f, 0f, 0.8f);
                    self.reactionSkill = Mathf.Clamp(self.reactionSkill * 1.3f, 0f, 0.8f);
                }
            }
        }

        public static float ScavengerAI_WantToThrowSpearAtCreature(On.ScavengerAI.orig_WantToThrowSpearAtCreature orig, ScavengerAI self, Tracker.CreatureRepresentation rep)
        {
            float value = orig(self, rep);
            if (self?.scavenger?.abstractCreature != null && self.scavenger.abstractCreature.AvoidsCombat())
            {
                //value *= 0.5f;
            }
            return value;
        }

        public static float Trader_ScavScore(On.ScavengersWorldAI.Trader.orig_ScavScore orig, ScavengersWorldAI.Trader self, ScavengerAbstractAI testScav)
        {
            float value = orig(self, testScav);
            if (testScav.parent.IsNotSpecialScav())
            {
                return 0f;
            }
            return value;
        }
        public static void Scavenger_Throw(On.Scavenger.orig_Throw orig, Scavenger self, Vector2 throwDir)
        {
            if (self.abstractCreature != null && self.abstractCreature.AvoidsCombat() && self.grasps != null && self.grasps[0].grabbed is Weapon && UnityEngine.Random.value < 0.30f)
            {
                return;
            }
            orig(self, throwDir);
        }
    }
}
