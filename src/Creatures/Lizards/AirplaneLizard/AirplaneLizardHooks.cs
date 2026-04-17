using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace lsfUtils.Creatures.Lizards.AirplaneLizard
{
    public static class AirplaneLizardHooks
    {
        public static void LizardGraphics_ColorBody(On.LizardGraphics.orig_ColorBody orig, LizardGraphics self, RoomCamera.SpriteLeaser sLeaser, Color col)
        {
            if (self?.lizard?.Template?.type == Enums.CreatureTemplateType.AirplaneLizard)
            {
                col = Color.white;
            }
            orig(self, sLeaser, col);
        }

        public static Color LizardGraphics_BodyColor(On.LizardGraphics.orig_BodyColor orig, LizardGraphics self, float f)
        {
            if (self?.lizard?.Template?.type == Enums.CreatureTemplateType.AirplaneLizard)
            {
                return Color.Lerp(Color.white, self.effectColor, Mathf.Clamp(Mathf.InverseLerp(self.lizard.lizardParams.tailColorationStart, 0.95f, Mathf.InverseLerp(self.bodyLength / self.BodyAndTailLength, 1f, f)), 0f, 1f));
            }
            return orig(self, f);
        }

        public static void SpineSpikes_ctor(On.LizardCosmetics.SpineSpikes.orig_ctor orig, LizardCosmetics.SpineSpikes self, LizardGraphics lGraphics, int startSprite)
        {
            orig(self, lGraphics, startSprite);
            if (lGraphics?.lizard?.Template?.type == Enums.CreatureTemplateType.AirplaneLizard)
            {
                self.colored = 1;
                self.sizeRangeMin = 1f;
                self.sizeRangeMax = 3f;
                self.numberOfSprites = self.bumps * 2;
            }
        }

        public static void Lizard_EnterAnimation(On.Lizard.orig_EnterAnimation orig, Lizard self, Lizard.Animation anim, bool forceAnimationChange)
        {
            orig(self, anim, forceAnimationChange);
            if (self is AirplaneLizard)
            {
                if (self.animation == Lizard.Animation.PrepareToLounge)
                {
                    if (self.AI?.focusCreature?.representedCreature?.realizedCreature != null && self.AI.focusCreature.representedCreature.realizedCreature.mainBodyChunk != null)
                    {
                        self.loungeDir = Custom.DirVec(self.mainBodyChunk.pos, self.AI.focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos);
                    }
                    else self.loungeDir = Custom.DirVec(self.bodyChunks[1].pos, self.bodyChunks[0].pos);
                }
            }
        }

        public static void LizardAI_AggressiveBehavior(On.LizardAI.orig_AggressiveBehavior orig, LizardAI self, Tracker.CreatureRepresentation target, float tongueChance)
        {
            orig(self, target, tongueChance);
            if (self?.lizard is AirplaneLizard && target != null && self.lizard.loungeDelay < 1 && UnityEngine.Random.value < 0.01f && self.lizard.LegsGripping > 2)
            {
                self.lizard.EnterAnimation(Lizard.Animation.PrepareToLounge, forceAnimationChange: false);
            }
        }
    }
}
