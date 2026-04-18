using BepInEx;
using BepInEx.Logging;
using Fisobs.Core;
using IL;
using IL.LizardCosmetics;
using IL.Menu;
using Menu.Remix.MixedUI;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MoreSlugcats;
using Music;
using RWCustom;
using SlugBase;
using SlugBase.Features;
using SlugBase.SaveData;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Contexts;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Threading;
using UnityEngine;
using UnityEngine.Playables;
using Watcher;
using static SlugBase.Features.FeatureTypes;
using static Stardust.Plugin;

namespace Stardust
{
    public static class BitterCode
    {
        public static void BitterGraspImmunity(On.Creature.Grasp.orig_ctor orig, Creature.Grasp self, Creature grabber, PhysicalObject grabbed, int graspUsed, int chunkGrabbed, Creature.Grasp.Shareability shareability, float dominance, bool pacifying)
        {
            orig(self, grabber, grabbed, graspUsed, chunkGrabbed, shareability, dominance, pacifying);
            if (grabbed != null && grabbed is Player player && player?.SlugCatClass != null && player.SlugCatClass == Enums.SlugcatStatsName.bitter)
            {
                self.pacifying = false;
                player.Stun(20);
            }
        }

        public static bool BitterLocustImmunity(On.LocustSystem.Swarm.orig_TryAttach orig, LocustSystem.Swarm self)
        {
            bool success = orig(self);
            if (success && self.target is Player && (self.target as Player).SlugCatClass == Enums.SlugcatStatsName.bitter)
            {
                self.attachCooldown += 40;
            }
            return success;

        }

        public static void OldFoodMeterCode(On.SlugcatStats.orig_ctor orig, SlugcatStats self, SlugcatStats.Name slugcat, bool malnourished)
        {
            orig(self, slugcat, malnourished);
            if (slugcat == Enums.SlugcatStatsName.bitter)
            {
                IntVector2 intVector = SlugcatStats.SlugcatFoodMeter(slugcat);
                self.foodToHibernate = intVector.y;
            }
        }

        public static float BitterBiteResistance(On.Player.orig_DeathByBiteMultiplier orig, Player self)
        {
            if (self?.SlugCatClass != null && self.SlugCatClass == Enums.SlugcatStatsName.bitter)
            {
                return orig(self) * 0.50f;
            }
            return orig(self);

        }
    }
}
