using lsfUtils.CWTs;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using static lsfUtils.Plugin;

namespace lsfUtils.CreatureTags
{
    public static class StarveFlagHooks
    {
        public static void ApplyHooks()
        {
            On.RelationshipTracker.DynamicRelationship.Update += StarveFlagHooks.DynamicRelationship_Update;
            On.Creature.Update += StarveFlagHooks.Creature_Update;
            On.SlugcatStats.NourishmentOfObjectEaten += StarveFlagHooks.SlugcatStats_NourishmentOfObjectEaten;
            On.LizardAI.ctor += StarveFlagHooks.LizardAI_ctor;
        }
        public static void SetupStarveTag(this AbstractCreature abstractCreature)
        {
            if (abstractCreature == null)
            {
                return;
            }
            if (!AbstractCreatureCWT.TryGetData(abstractCreature, out var data))
            {
                Log.LogMessage("Couldnt get AbstractCreatureCWT!");
                return;
            }
            data.starveTag = true;
        }

        public static bool IsStarving(this Creature creature)
        {
            if (creature?.abstractCreature == null) return false;

            if (!AbstractCreatureCWT.TryGetData(creature.abstractCreature, out var data)) return false;

            return data.starveTag;
        }

        public static bool IsStarving(this AbstractCreature creature)
        {
            if (!AbstractCreatureCWT.TryGetData(creature, out var data)) return false;

            return data.starveTag;
        }

        public static void DynamicRelationship_Update(On.RelationshipTracker.DynamicRelationship.orig_Update orig, RelationshipTracker.DynamicRelationship self)
        {
            orig(self);

            Creature critter = self.rt?.AI?.creature?.realizedCreature;

            if (critter != null && critter.IsStarving() && self.currentRelationship.type == CreatureTemplate.Relationship.Type.Eats)
            {
                self.currentRelationship.intensity = math.clamp(self.currentRelationship.intensity * 1.5f, 0f, 1f);
                self.trackerRep.priority = self.currentRelationship.intensity * self.trackedByModuleWeigth;
            }
        }

        public static void Creature_Update(On.Creature.orig_Update orig, Creature self, bool eu)
        {
            orig(self, eu);
            if (!self.IsStarving()) return;
            if (!CreatureCWT.TryGetData(self, out var data)) return;

            if (self.bodyChunks != null && !data.starveInit)
            {
                for (int i = 0; i < self.bodyChunks.Length; i++)
                {
                    self.bodyChunks[i].mass *= 0.75f;
                }
                data.starveInit = true;
            }

            data.starveStunTimer++;
            if (data.starveStunTimer >= 120)
            {
                data.starveStunTimer = 0;
                if (UnityEngine.Random.value < 0.1f)
                {
                    self.Stun(80);
                }
            }
        }
        public static int SlugcatStats_NourishmentOfObjectEaten(On.SlugcatStats.orig_NourishmentOfObjectEaten orig, SlugcatStats.Name slugcatIndex, IPlayerEdible eatenobject)
        {
            int value = orig(slugcatIndex, eatenobject);
            if (eatenobject != null && eatenobject is Creature creature && creature.IsStarving())
            {
                value /= 2;
            }
            return value;
        }

        public static void LizardAI_ctor(On.LizardAI.orig_ctor orig, LizardAI self, AbstractCreature creature, World world)
        {
            orig(self, creature, world);
            if (creature.IsStarving())
            {
                self.friendTracker.tamingDifficlty *= 0.5f;
            }
        }
    }
}