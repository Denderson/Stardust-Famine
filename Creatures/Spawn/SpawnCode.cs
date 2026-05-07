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


namespace lsfUtils.Creatures.Spawn
{
    public static class SpawnCode
    {
        public static void VoidSpawnEgg_Pop(On.VoidSpawnEgg.orig_Pop orig, VoidSpawnEgg self)
        {
            self.room.game.session.creatureCommunities.InfluenceLikeOfPlayer(Enums.CreatureCommunityID.StarSpawn, self.room.world.RegionNumber, 0, 0.1f, 0.2f, 0.1f);
            orig(self);
        }

        public static void Player_Grabbed(On.Player.orig_Grabbed orig, Player self, Creature.Grasp grasp)
        {
            orig(self, grasp);
            if (grasp.grabber is StarSpawn && self.dangerGrasp == null)
            {
                self.dangerGraspTime = 0;
                self.dangerGrasp = grasp;
            }
        }
    }
}
