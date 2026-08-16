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