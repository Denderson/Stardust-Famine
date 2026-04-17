using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace lsfUtils.Creatures.Scavs.ScavMessenger
{
    public static class ScavMessengerHooks
    {
        public static int ScavengerAI_CollectScore_PhysicalObject_bool(On.ScavengerAI.orig_CollectScore_PhysicalObject_bool orig, ScavengerAI self, PhysicalObject obj, bool weaponFiltered)
        {
            int value = orig(self, obj, weaponFiltered);
            if (self?.scavenger?.abstractCreature != null && self.scavenger.abstractCreature.IsMessenger() && obj is Weapon weapon && weapon.HeavyWeapon)
            {
                return 1;
            }
            return value;
            // unfinished
        }

        public static int ScavengerAI_WeaponScore(On.ScavengerAI.orig_WeaponScore orig, ScavengerAI self, PhysicalObject obj, bool pickupDropInsteadOfWeaponSelection, bool reallyWantsSpear)
        {
            int value = orig(self, obj, pickupDropInsteadOfWeaponSelection, reallyWantsSpear);
            if (self?.scavenger?.abstractCreature != null && self.scavenger.abstractCreature.IsMessenger() && obj is Weapon weapon && weapon.HeavyWeapon)
            {
                return 1;
            }
            return value;
            // unfinished
        }

        public static void ScavengerAI_SocialEvent(On.ScavengerAI.orig_SocialEvent orig, ScavengerAI self, SocialEventRecognizer.EventID ID, Creature subjectCrit, Creature objectCrit, PhysicalObject involvedItem)
        {
            orig(self, ID, subjectCrit, objectCrit, involvedItem);
            if (self?.scavenger?.abstractCreature != null && self.scavenger.abstractCreature.IsMessenger())
            {
                if (ID == SocialEventRecognizer.EventID.LethalAttack)
                {
                    (self.scavenger as ScavMessenger).retreatCounter += 30;
                }
                else if (ID == SocialEventRecognizer.EventID.Killing)
                {
                    (self.scavenger as ScavMessenger).retreatCounter += 60;
                }
            }
        }

        public static void ScavengerAI_ScavPlayerRelationChange(On.ScavengerAI.orig_ScavPlayerRelationChange orig, ScavengerAI self, float change, AbstractCreature player)
        {
            orig(self, change, player);
            if (self?.scavenger?.abstractCreature != null && self.scavenger.abstractCreature.IsMessenger())
            {
                (self.scavenger as ScavMessenger).message += Mathf.Clamp(change, -0.3f, 0.3f);
            }
        }

        public static void ScavengerAbstractAI_ReGearInDen(On.ScavengerAbstractAI.orig_ReGearInDen orig, ScavengerAbstractAI self)
        {
            orig(self);
            if (self?.parent?.realizedCreature != null && self.parent.realizedCreature is ScavMessenger messenger)
            {
                messenger.retreatCounter = 0;
                self.world.game.session.creatureCommunities.InfluenceLikeOfPlayer(CreatureCommunities.CommunityID.Scavengers, self.world.RegionNumber, 0, messenger.message, 0.2f, 0.3f);
                messenger.message = 0f;
            }
        }

        public static bool ScavengerAbstractAI_GoHome(On.ScavengerAbstractAI.orig_GoHome orig, ScavengerAbstractAI self)
        {
            bool value = orig(self);
            if (self?.parent?.realizedCreature != null && self.parent.realizedCreature is ScavMessenger messenger && messenger.WantsToRetreat())
            {
                return true;
            }
            return value;
        }

        public static bool ScavengerAI_WantToStayInDenUntilEndOfCycle(On.ScavengerAI.orig_WantToStayInDenUntilEndOfCycle orig, ScavengerAI self)
        {
            bool value = orig(self);
            if (self?.scavenger != null && self.scavenger is ScavMessenger messenger && messenger.WantsToRetreat())
            {
                return true;
            }
            return value;
        }
    }
}
