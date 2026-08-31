using BepInEx;
using BepInEx.Logging;
using DevInterface;
using Fisobs.Core;
using LizardCosmetics;
using lsfUtils.Creatures.Lizards;
using lsfUtils.Creatures.Lizards.AirplaneLizard;
using lsfUtils.Creatures.Lizards.FlameLizard;
using lsfUtils.Creatures.Lizards.MonitorLizard;
using lsfUtils.Creatures.Lizards.PoisonLizard;
using lsfUtils.Creatures.Lizards.RaspberryLizard;
using lsfUtils.Creatures.Lizards.StarNosedLizard;
using lsfUtils.Creatures.Lizards.WeaverLizard;
using lsfUtils.Creatures.Scavs;
using lsfUtils.Creatures.Scavs.ScavFlank;
using lsfUtils.Creatures.Scavs.ScavMessenger;
using lsfUtils.Creatures.Scavs.ScavSeer;
using lsfUtils.Creatures.Spawn;
using lsfUtils.Creatures.Spiders.PoisonSpider;
using lsfUtils.Creatures.Worm;
using lsfUtils.CreatureTags;
using lsfUtils.CWTs;
using lsfUtils.DevtoolsEffects.CreepingDarkness;
using lsfUtils.DevtoolsEffects.EvilWater;
using lsfUtils.DevtoolsObjects.BackgroundMud;
using lsfUtils.DevtoolsObjects.Bumper;
using lsfUtils.DevtoolsObjects.ConditionalFilter;
using lsfUtils.DevtoolsObjects.CustomPushback;
using lsfUtils.DevtoolsObjects.EventRectangle;
using lsfUtils.DevtoolsObjects.FloatMud;
using lsfUtils.DevtoolsObjects.LocalGravity;
using lsfUtils.DevtoolsObjects.MudBonePile;
using lsfUtils.DevtoolsObjects.RippleTunnel;
using lsfUtils.DevtoolsObjects.RippleZone;
using lsfUtils.DevtoolsObjects.WaveLight;
using lsfUtils.Items;
using lsfUtils.Items.BrownFruit;
using lsfUtils.Items.Darts.Dart;
using lsfUtils.Items.Darts.PoisonDart;
using lsfUtils.Items.ExplosiveBoomerang;
using lsfUtils.Items.KarmaMask;
using lsfUtils.Items.Normal.TorchSpears;
using lsfUtils.Items.RippleFlower;
using lsfUtils.RegionParams;
using Menu.Remix.MixedUI;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Newtonsoft.Json.Linq;
using RWCustom;
using Stardust.PlacedObjects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using System.Security.Permissions;
using UnityEngine;
using Watcher;
using static lsfUtils.RegionParams.TypeParamsHooks;
using static Pom.Pom;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace lsfUtils
{
    [BepInDependency("io.github.dual.fisobs")]
    [BepInPlugin("lsfUtils", "LSF Utils", "0.1")]

    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource Log { get; private set; }

        public static readonly int StaticRandom = RXRandom.Int(100);

        public bool initialized;
        public bool isInit;

        public const string templarMaskIcon = "atlases/templarMaskIcon";

        public static readonly EntityID SpecialId = new(1, -20);

        private void LoadResources(RainWorld rainWorld)
        {

        }


        public void OnEnable()
        {
            Debug.Log("Starting LSF Utils");
            try
            {
                Log = Logger;

                On.RainWorld.OnModsInit += RainWorld_OnModsInit;

                // fisobs
                {
                    Content.Register(new WeaverLizardCritsob());
                    Content.Register(new FlameLizardCritsob());
                    Content.Register(new AirplaneLizardCritsob());
                    Content.Register(new RaspberryLizardCritsob());
                    Content.Register(new MonitorLizardCritob());
                    Content.Register(new StarNosedLizardCritob());
                    Content.Register(new PoisonLizardCritob());
                    Content.Register(new ScavSeerCritob());
                    Content.Register(new ScavMessengerCritob());
                    Content.Register(new ScavFlankCritob());
                    Content.Register(new StarSpawnCritob());
                    Content.Register(new StarNoodlesCritob());
                    Content.Register(new StarJellyCritob());
                    Content.Register(new PoisonSpiderCritob());
                    Content.Register(new ClimbGrubCritob());

                    Content.Register(new RippleFlowerFisob());
                    Content.Register(new PoisonDartFisob());
                    Content.Register(new KarmaMaskFisob());
                    Content.Register(new ExplosiveBoomerangFisob());
                    Content.Register(new SingularityBoomerangFisob());
                    Content.Register(new TorchSpearFisob());

                    Log.LogMessage("Done with Fisobs!");
                }

                // creatures
                {
                    LizardHooks.ApplyHooks();

                    AirplaneLizardHooks.ApplyHooks();
                    RaspberryLizardHooks.ApplyHooks();
                    WeaverLizardHooks.ApplyHooks();
                    PoisonLizardHooks.ApplyHooks();
                    MonitorLizardHooks.ApplyHooks();
                    StarNosedLizardHooks.ApplyHooks();

                    ScavHooks.ApplyHooks();

                    ScavFlankHooks.ApplyHooks();
                    ScavSeerHooks.ApplyHooks();
                    ScavMessengerHooks.ApplyHooks();

                    PoisonSpiderHooks.ApplyHooks();

                    ClimbGrubHooks.ApplyHooks();
                }

                // items
                {
                    RippleFlowerHooks.ApplyHooks();
                    DartHooks.ApplyHooks();
                    KarmaMaskHooks.ApplyHooks();
                    ExplosiveBoomerangHooks.ApplyHooks();
                    TorchSpearHooks.ApplyHooks();
                }

                // devtools objects
                {
                    LocalGravityHooks.ApplyHooks();
                    ConditionalFilterHooks.ApplyHooks();
                    FloatMudHooks.ApplyHooks();
                    BackgroundMudHooks.ApplyHooks();
                    WaveLightHooks.ApplyHooks();
                }

                // devtools effects
                {
                    CreepingDarkness.RegisterCreepingDarkness();
                    CreepingDarknessHooks.ApplyHooks();

                    EvilWater.RegisterEvilWater();
                    EvilWaterHooks.ApplyHooks();
                }

                // creature flags
                {
                    CreatureFlagHooks.ApplyHooks();

                    EchoImmuneHooks.ApplyHooks();
                    RippleHybridHooks.ApplyHooks();
                    PoisonImmuneHooks.ApplyHooks();

                    FactionHooks.ApplyHooks();
                    StarveFlagHooks.ApplyHooks();
                }

                // region parameters
                {
                    On.Region.ctor_string_int_int_RainWorldGame_Timeline += RegionParams.RegionParamsSetup.SetupParams;

                    ScavParamsHooks.ApplyHooks();
                    TypeParamsHooks.ApplyHooks();
                }

                On.RainWorld.Start += RainWorld_Start;

                Log.LogMessage("Checking isInit!");

                if (isInit) return;
                isInit = true;

                WorldLoader.Preprocessing.preprocessorConditions.Add(ConditionalLogic.LSFConditions);

                Log.LogMessage("Registering lsfUtils stuff...");

                RegisterManagedObject<ConditionFilter, ConditionFilterData, ManagedRepresentation>("ConditionalFilter", "lsfUtils");
                RegisterManagedObject<RoomConditionFilterUAD, RoomConditionFilterData, ManagedRepresentation>("RoomConditionalFilter", "lsfUtils");
                RegisterManagedObject<LocalGravity, LocalGravityData, ManagedRepresentation>("LocalGravity", "lsfUtils");
                RegisterManagedObject<RippleZone, RippleZoneData, ManagedRepresentation>("RippleZone", "lsfUtils");
                RegisterManagedObject<EventRect, EventRectData, ManagedRepresentation>("EventRect", "lsfUtils");
                RegisterManagedObject<CustomPushback, CustomPushbackData, ManagedRepresentation>("CustomPushback", "lsfUtils");
                RegisterManagedObject<WaveLight, WaveLightData, WaveLightRepresentation>("WaveLight", "lsfUtils");
                RegisterManagedObject<Bumper, BumperData, ManagedRepresentation>("Bumper", "lsfUtils");

                RegisterManagedObject(new ManagedObjectType("MudBonePile", "lsfUtils", typeof(MudBonePile), typeof(MudBonePileData), typeof(MudBonePileRepresentation)));

                RegisterManagedObject<RippleTunnel, RippleTunnelData, RippleTunnelRepresentation>("RippleTunnel", "lsfUtils");

                RegisterManagedObject(new ManagedFloatMud());
                RegisterManagedObject(new ManagedBackgroundMud());
                RegisterManagedObject(new ManagedRippleFlower());
                RegisterManagedObject(new ManagedKarmaMask());
                RegisterManagedObject(new ManagedBrownFruit());
                RegisterManagedObject(new ManagedTorchSpear());

                //hi
                //hello

                EventLogic.RegisterBuiltInEvents();

                Logger.LogMessage("LSF Utils success!");
            }
            catch (Exception e)
            {
                Logger.LogMessage("LSF Utils failure!!!");
                Logger.LogError(e);
            }
        }

        public static void RainWorld_Start(On.RainWorld.orig_Start orig, RainWorld self)
        {
            orig(self);
            // load here stuff that requires AssetManager here (like ResolveFilePath)

            string path;
            try
            {
                path = AssetManager.ResolveFilePath("lsfUtils/regionMetaParameters.txt");
            }
            catch (Exception ex)
            {
                Log.LogWarning($"TypeParamsHooks.Load: AssetManager not ready or path resolution failed: {ex.Message}");
                return;
            }

            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                Log.LogWarning("regionMetaParameters.txt not found.");
                return;
            }

            TypeParamsHooks.Load();
        }

        public static bool GetNameFromAnywhere(out SlugcatStats.Name name)
        {
            name = null;
            if (RWCustom.Custom.rainWorld.processManager?.currentMainLoop is RainWorldGame game)
            {
                if (game.IsStorySession)
                {
                    name = game.StoryCharacter;
                    return true;
                }
            }
            return false;
        }

        public static bool GetTimelineFromAnywhere(out SlugcatStats.Timeline timeline)
        {
            timeline = null;
            if (RWCustom.Custom.rainWorld.processManager?.currentMainLoop is RainWorldGame game && game.IsStorySession)
            {
                if (game.IsStorySession)
                {
                    timeline = game.TimelinePoint;
                    return true;
                }
            }
            return false;
        }


        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            if (ModManager.MSC)
            {
                On.MoreSlugcats.GourmandCombos.InitCraftingLibrary += ItemCrafting.On_GourmandCombos_InitCraftingLibrary;
            }

            if (initialized)
            {
                return;
            }
            initialized = true;
            Futile.atlasManager.LoadImage("atlases/Kill_MonitorLizard");
            Futile.atlasManager.LoadImage("atlases/Kill_StarNosedLizard");

            Futile.atlasManager.LoadImage("atlases/Symbol_Dart");

            Futile.atlasManager.LoadImage("atlases/Dart");
            Futile.atlasManager.LoadImage("atlases/PoisonDart");

            Futile.atlasManager.LoadImage("atlases/Bumper1");
            Futile.atlasManager.LoadImage("atlases/Bumper2");

            Futile.atlasManager.LoadAtlas("atlases/lsfLizardStuff");
            Futile.atlasManager.LoadAtlas("atlases/corrodedskylines");

            Futile.atlasManager.LoadImage(templarMaskIcon);

            Futile.atlasManager.LoadAtlas("atlases/SeerMaskSprites");

            string bundlePath = AssetManager.ResolveFilePath("shaders/lsfutils");
            Log.LogMessage($"Resolved bundle path: {bundlePath}");
            if (System.IO.File.Exists(bundlePath))
            {
                Log.LogMessage($"Bundle last write: {System.IO.File.GetLastWriteTime(bundlePath)}");
            }
            else
            {
                Log.LogMessage("File does not exist at resolved path!");
            }

            AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
            if (bundle != null)
            {
                Log.LogMessage("Bundle loaded. Assets inside: " + string.Join(", ", bundle.GetAllAssetNames()));
                RegisterShader(self, bundle, "Assets/Shaders/RippleSpawnBodyGreen.shader", "RippleSpawnBodyGreen");
                RegisterShader(self, bundle, "Assets/Shaders/RippleGlowGreen.shader", "RippleGlowGreen");
                RegisterShader(self, bundle, "Assets/Shaders/SeerHalo.shader", "SeerHalo");
                RegisterShader(self, bundle, "Assets/Shaders/WaveLightOverlay.shader", "WaveLightOverlay");
                RegisterShader(self, bundle, "Assets/Shaders/ClothBurnShader.shader", "ClothBurnShader");
                RegisterShader(self, bundle, "Assets/Shaders/ProceduralFireShader.shader", "ProceduralFireShader");
                RegisterShader(self, bundle, "Assets/Shaders/ScorchBurnShader.shader", "ScorchBurnShader");

                bundle.Unload(false);
            }
            else
            {
                Log.LogMessage("Failed to load lsfUtils shader bundle!");
            }

            Factions.LoadAndRegisterAllFactions();

            Log.LogMessage("Sprites and shaders loaded!");
        }

        private static void RegisterShader(RainWorld rainWorld, AssetBundle bundle, string assetPath, string shaderKey)
        {
            Shader unityShader = bundle.LoadAsset<Shader>(assetPath);
            if (unityShader == null)
            {
                Log.LogMessage($"Shader at {assetPath} not found in bundle!");
                return;
            }
            rainWorld.Shaders[shaderKey] = FShader.CreateShader(shaderKey, unityShader);
        }

        public void OnDisable()
        {
            if (!isInit) return;
            isInit = false;

            WorldLoader.Preprocessing.preprocessorConditions.Remove(ConditionalLogic.LSFConditions);
        }
    }
}