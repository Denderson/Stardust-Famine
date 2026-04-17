using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lsfUtils.Creatures.Lizards.WeaverLizard
{
    public static class WeaverLizardHooks
    {
        public static void TailTuft_ctor(On.LizardCosmetics.TailTuft.orig_ctor orig, LizardCosmetics.TailTuft self, LizardGraphics lGraphics, int startSprite)
        {
            if (lGraphics?.lizard?.Template?.type == Enums.CreatureTemplateType.WeaverLizard)
            {
                CreatureTemplate.Type type = lGraphics.lizard.Template.type;
                lGraphics.lizard.Template.type = CreatureTemplate.Type.RedLizard;
                orig(self, lGraphics, startSprite);
                lGraphics.lizard.Template.type = type;
                return;
            }
            orig(self, lGraphics, startSprite);
        }

        public static void LongShoulderScales_ctor(On.LizardCosmetics.LongShoulderScales.orig_ctor orig, LizardCosmetics.LongShoulderScales self, LizardGraphics lGraphics, int startSprite)
        {

            if (lGraphics?.lizard?.Template?.type == Enums.CreatureTemplateType.WeaverLizard)
            {
                CreatureTemplate.Type type = lGraphics.lizard.Template.type;
                lGraphics.lizard.Template.type = CreatureTemplate.Type.RedLizard;
                orig(self, lGraphics, startSprite);
                lGraphics.lizard.Template.type = type;
                return;
            }
            orig(self, lGraphics, startSprite);
        }

        public static void LizardGraphics_InitiateSprites(On.LizardGraphics.orig_InitiateSprites orig, LizardGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            orig(self, sLeaser, rCam);
            if (self?.lizard?.Template?.type == Enums.CreatureTemplateType.WeaverLizard)
            {
                for (int i = 1; i < sLeaser.sprites.Count() - 1; i++)
                {
                    if (sLeaser.sprites[i].shader == Custom.rainWorld.Shaders["Basic"] || sLeaser.sprites[i].shader == Custom.rainWorld.Shaders["RippleBasic"])
                    {
                        sLeaser.sprites[i].shader = Custom.rainWorld.Shaders["RippleBasicBothSides"];
                    }
                }
            }
        }
    }
}
