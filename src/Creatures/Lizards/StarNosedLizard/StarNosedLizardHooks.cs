using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace lsfUtils.Creatures.Lizards.StarNosedLizard
{
    internal class StarNosedLizardHooks
    {
        public static bool Lizard_HeadLight(Func<LizardGraphics, bool> orig, LizardGraphics self)
        {
            return orig(self) || self.lizard.Template.type == Enums.CreatureTemplateType.StarNosedLizard;
        }

        public static Color Lizard_effectColor(Func<LizardGraphics, Color> orig, LizardGraphics self)
        {
            if (self.lizard.Template.type == Enums.CreatureTemplateType.StarNosedLizard)
            {
                return Color.Lerp(self.palette.blackColor, new Color(0.22f, 0.22f, 0.25f), 0.5f);
            }
            return orig(self);
        }

        public static void LizardAI_Update(On.LizardAI.orig_Update orig, LizardAI self)
        {
            orig(self);
            if (self?.lizard == null) return;
            if (self.lizard.Template?.type == Enums.CreatureTemplateType.StarNosedLizard && self.noiseTracker?.hearingSkill != null)
            {
                self.noiseTracker.hearingSkill = 2f;
            }
        }

        public static void SuperHearing_Update(On.SuperHearing.orig_Update orig, SuperHearing self)
        {
            if (self?.AI?.creature?.type != null && self.AI is LizardAI lizardAI && lizardAI?.lizard?.Template?.type != null && lizardAI.lizard.Template.type == Enums.CreatureTemplateType.StarNosedLizard)
            {
                float superHearingOrig = self.superHearingSkill;
                if (lizardAI.lizard.mainBodyChunk?.vel.x != null && Math.Abs(lizardAI.lizard.mainBodyChunk.vel.x) > 3)
                {
                    self.superHearingSkill /= Math.Abs(lizardAI.lizard.mainBodyChunk.vel.x) - 2;
                }

                for (int i = 0; i < self.room.physicalObjects.Length; i++)
                {
                    for (int j = 0; j < self.room.physicalObjects[i].Count; j++)
                    {
                        if (self.room.physicalObjects[i][j] is Creature creature)
                        {
                            if (creature == self.AI.creature.realizedCreature || creature.abstractCreature.rippleLayer != self.AI.creature.rippleLayer && !creature.abstractCreature.rippleBothSides && !self.AI.creature.rippleBothSides)
                            {
                                continue;
                            }
                            if (creature is Player player && player.sporeParticleTicks > 0)
                            {
                                continue;
                            }
                            if (creature.muddy > 0)
                            {
                                continue;
                            }
                            for (int k = 0; k < creature.bodyChunks.Length; k++)
                            {
                                BodyChunk bodyChunk = creature.bodyChunks[k];
                                if (Custom.DistLess(bodyChunk.pos, self.AI.creature.realizedCreature.mainBodyChunk.pos, 200))
                                {
                                    self.tracker.SeeCreature(creature.abstractCreature);
                                    (lizardAI.lizard as StarNosedLizard).smellPoint = bodyChunk.pos;
                                    (lizardAI.lizard as StarNosedLizard).smellRemaining = 240;
                                }
                            }
                        }
                    }
                }
                orig(self);
                self.superHearingSkill = superHearingOrig;
            }
            else orig(self);
        }


    }
}
