using lsfUtils.CWTs;
using MonoMod.RuntimeDetour;
using MoreSlugcats;
using RWCustom;
using System;
using UnityEngine;
using static lsfUtils.Plugin;

namespace lsfUtils.Items.BrownFruit
{
    public static class BrownFruitHooks
    {
        public static void ApplyHooks()
        {
            On.SlugcatStats.NourishmentOfObjectEaten += SlugcatStats_NourishmentOfObjectEaten;
            On.DangleFruit.ApplyPalette += DangleFruit_ApplyPalette;
        }

        public static Color brownFruitColor = new(0.6f, 0.17f, 0.13f);

        public static void DangleFruit_ApplyPalette(On.DangleFruit.orig_ApplyPalette orig, DangleFruit self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig(self, sLeaser, rCam, palette);
            if (self is BrownFruit) self.color = brownFruitColor;
        }

        public static int SlugcatStats_NourishmentOfObjectEaten(On.SlugcatStats.orig_NourishmentOfObjectEaten orig, SlugcatStats.Name slugcatIndex, IPlayerEdible eatenobject)
        {
            if (eatenobject is BrownFruit)
            {
                return 2;
            }
            return orig(slugcatIndex, eatenobject);
        }
    }
}