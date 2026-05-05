using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using Watcher;

namespace lsfUtils.Creatures.Lizards.PoisonLizard
{
    internal class PoisonLizardHooks
    {
        public static CreatureTemplate.Relationship LizardAI_IUseARelationshipTracker_UpdateDynamicRelationship(On.LizardAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, LizardAI self, RelationshipTracker.DynamicRelationship dRelation)
        {
            CreatureTemplate.Relationship value = orig(self, dRelation);
            if (self?.lizard is PoisonLizard)
            {
                if (dRelation?.trackerRep?.representedCreature == self.lizard.abstractCreature)
                {
                    return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f);
                }
                if (self.lizard.grasps != null && self.lizard.grasps.Length > 0 && self.lizard.grasps[0] != null && self.lizard.grasps[0].grabbed is Creature grabbedCreature)
                {
                    if (dRelation.trackerRep?.representedCreature?.realizedCreature != grabbedCreature)
                    {
                        return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f);
                    }
                    return value;
                }
                if (dRelation?.state != null && dRelation.trackerRep?.representedCreature?.realizedCreature != null && dRelation.trackerRep.representedCreature == (self.lizard as PoisonLizard).trackedCreature)
                {
                    AbstractCreature rep = dRelation.trackerRep.representedCreature;
                    float distance = Custom.Dist(self.lizard.mainBodyChunk.pos, rep.realizedCreature.mainBodyChunk.pos);
                    int poison = (int)(rep.realizedCreature.injectedPoison * 4);

                    self.lizard.JawOpen = math.clamp(self.lizard.JawOpen, 0f, 0.2f);

                    if (rep.realizedCreature.injectedPoison > 0.9)
                    {
                        return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1);
                    }
                    else if (distance <= 250f - poison * 20)
                    {
                        return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, (250 - distance) / 250);
                    }
                    else if (distance <= 350f - poison * 20f)
                    {
                        return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0);
                    }
                }
            }
            return value;
        }

        public static void Lizard_Bite(On.Lizard.orig_Bite orig, Lizard self, BodyChunk chunk)
        {
            orig(self, chunk);
            if (self?.AI != null && self.Template?.type == Enums.CreatureTemplateType.PoisonLizard && chunk != null && chunk.owner is Creature creature && creature.abstractCreature != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    Vector2 vector = Custom.RNV();
                    self.room.AddObject(new Spark(self.firstChunk.pos + vector * 40f, vector * Mathf.Lerp(4f, 30f, UnityEngine.Random.value), self.effectColor, null, 8, 24));
                }
                if (creature.dead) return;

                creature.InjectPoison(0.1f, self.effectColor);
                creature.Stun(40);

                (self as PoisonLizard).trackedCreature = creature.abstractCreature;
                (self as PoisonLizard).trackTimeRemaining = 40000;
                self.room.AddObject(new PoisonInjecter(creature, 1f, 100f, self.effectColor));
            }
        }

        public static void LizardAI_Update(On.LizardAI.orig_Update orig, LizardAI self)
        {
            orig(self);
            if (self?.lizard == null) return;
            if (self.lizard is PoisonLizard lizard)
            {
                if (lizard.trackTimeRemaining > 0)
                {
                    lizard.trackTimeRemaining--;
                }
                else
                {
                    lizard.trackedCreature = null;
                }
                if (lizard.trackedCreature?.Room != null && lizard.abstractCreature?.Room != null && lizard.trackedCreature.Room == lizard.room.abstractRoom)
                {
                    self.tracker.SeeCreature(lizard.trackedCreature);
                    if (lizard.trackedCreature.realizedCreature != null && lizard.trackedCreature.realizedCreature.injectedPoison <= 0)
                    {
                        lizard.trackedCreature = null;
                        lizard.trackTimeRemaining = 0;
                    }
                }
            }
        }

        public static void LizardTongue_Impact(On.LizardTongue.orig_Impact orig, LizardTongue self)
        {
            orig(self);
            if (self?.lizard?.Template?.type != null && self.lizard.Template.type == Enums.CreatureTemplateType.PoisonLizard && self.attached.owner is Creature creature)
            {
                creature.Stun(20);
                (creature as Player).lungsExhausted = true;
            }
        }
    }
}
