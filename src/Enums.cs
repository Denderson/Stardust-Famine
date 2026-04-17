using BepInEx;
using BepInEx.Logging;
using DevInterface;
using Fisobs.Core;
using Menu;
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
using Stardust.Mechanics;
using Stardust.Slugcats;
using Stardust.Slugcats.Bitter;
using Stardust.Slugcats.Scholar;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Contexts;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Playables;
using Watcher;
using static Pom.Pom;
using static SlugBase.Features.FeatureTypes;
using static Watcher.RippleHybridVFX;

namespace Stardust
{
    public static class Enums
    {
        public class Colors
        {
            public static Color PoisonColor = new(0.31f, 0.46f, 0.10f);
        }

        public class SlugcatStatsName
        {
            public static SlugcatStats.Name bitter = new(nameof(bitter));
            public static SlugcatStats.Name sfscholar = new(nameof(sfscholar));
        }

        public class SlugcatStatsTimeline
        {
            public static SlugcatStats.Timeline bitterTimeline = new(nameof(bitterTimeline));
            public static SlugcatStats.Timeline sfscholarTimeline = new(nameof(sfscholarTimeline));
        }

        public class ProcessIDs
        {
            public static ProcessManager.ProcessID threadsProcess = new(nameof(threadsProcess));
        }

        public class MenuSceneIDs
        {
            public static MenuScene.SceneID bitterRipple = new(nameof(bitterRipple));
            public static MenuScene.SceneID bitterHalfway = new(nameof(bitterHalfway));
            public static MenuScene.SceneID bitterEcho = new(nameof(bitterEcho));

            public static MenuScene.SceneID scholarRipple = new(nameof(scholarRipple));
            public static MenuScene.SceneID scholarPermadeath = new(nameof(scholarPermadeath));

            public static MenuScene.SceneID threadsScene = new(nameof(threadsScene));
        }
        
        public class CreatureTemplateTypes
        {
            

            public static CreatureTemplate.Type PoisonSpider = new(nameof(PoisonSpider), true);
            public static CreatureTemplate.Type MantisSpider = new(nameof(MantisSpider), true);

            public static CreatureTemplate.Type MudJetfish = new(nameof(MudJetfish), true);

            

            public static CreatureTemplate.Type Porcupine = new(nameof(Porcupine), true);
            public static CreatureTemplate.Type Ratel = new(nameof(Ratel), true);
            public static CreatureTemplate.Type Muskfox = new(nameof(Muskfox), true);

        }

        public class AbstractPhysicalObjects
        {
            public static AbstractPhysicalObject.AbstractObjectType PoisonDart = new(nameof(PoisonDart), true);

        }
        
        public class SandboxUnlockIDs
        {

            public static MultiplayerUnlocks.SandboxUnlockID PoisonSpider = new(nameof(PoisonSpider), true);
            public static MultiplayerUnlocks.SandboxUnlockID PoisonDart = new(nameof(PoisonDart), true);

            public static MultiplayerUnlocks.SandboxUnlockID MantisSpider = new(nameof(MantisSpider), true);

            public static MultiplayerUnlocks.SandboxUnlockID MudJetfish = new(nameof(MudJetfish), true);

            public static MultiplayerUnlocks.SandboxUnlockID Porcupine = new(nameof(Porcupine), true);
            public static MultiplayerUnlocks.SandboxUnlockID Ratel = new(nameof(Ratel), true);
            public static MultiplayerUnlocks.SandboxUnlockID Muskfox = new(nameof(Muskfox), true);
        }
    }
}
