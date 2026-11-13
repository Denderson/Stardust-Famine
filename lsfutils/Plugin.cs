using BepInEx;
using BepInEx.Logging;
using lsfUtils.Creatures;
using lsfUtils.Creatures.Lizards;
using lsfUtils.Creatures.Lizards.AirplaneLizard;
using lsfUtils.Creatures.Lizards.FlameLizard;
using lsfUtils.Creatures.Lizards.MonitorLizard;
using lsfUtils.Creatures.Lizards.PoisonLizard;
using lsfUtils.Creatures.Lizards.RaspberryLizard;
using lsfUtils.Creatures.Lizards.StarNosedLizard;
using lsfUtils.Creatures.Lizards.WeaverLizard;
using lsfUtils.Creatures.Misc.ClimbGrub;
using lsfUtils.Creatures.Misc.ToxicSponge;
using lsfUtils.Creatures.Scavs;
using lsfUtils.Creatures.Scavs.ScavFlank;
using lsfUtils.Creatures.Scavs.ScavMessenger;
using lsfUtils.Creatures.Scavs.ScavSeer;
using lsfUtils.Creatures.Spawn;
using lsfUtils.Creatures.Spiders.PoisonSpider;
using lsfUtils.CreatureTags;
using lsfUtils.CWTs;
using lsfUtils.DevtoolsEffects.CreepingDarkness;
using lsfUtils.DevtoolsEffects.EvilWater;
using lsfUtils.DevtoolsEffects.StardustRain;
using lsfUtils.DevtoolsObjects.BackgroundMud;
using lsfUtils.DevtoolsObjects.Bumper;
using lsfUtils.DevtoolsObjects.ConditionalFilter;
using lsfUtils.DevtoolsObjects.CreatureFlagZone;
using lsfUtils.DevtoolsObjects.CustomPushback;
using lsfUtils.DevtoolsObjects.EventRectangle;
using lsfUtils.DevtoolsObjects.FloatMud;
using lsfUtils.DevtoolsObjects.LocalGravity;
using lsfUtils.DevtoolsObjects.MudBonePile;
using lsfUtils.DevtoolsObjects.PoisonSteam;
using lsfUtils.DevtoolsObjects.RippleTunnel;
using lsfUtils.DevtoolsObjects.RippleZone;
using lsfUtils.DevtoolsObjects.WaveLight;
using lsfUtils.Items;
using lsfUtils.Items.Artefacts;
//using lsfUtils.Items.Artefacts.KarmaMask;
using lsfUtils.Items.Artifacts.BasementKey;
using lsfUtils.Items.BrownFruit;
using lsfUtils.Items.ExplosiveBoomerang;
using lsfUtils.Items.KarmaMask;
using lsfUtils.Items.KnotSpawnV2;
using lsfUtils.Items.RippleFlower;
using lsfUtils.Items.Spears;
using lsfUtils.Items.Spears.CopperSpears;
using lsfUtils.Items.Spears.TorchSpears;
using lsfUtils.Mechanics;
using lsfUtils.RegionParams;
using lsfUtils.SaveFile;
using lsfUtils.ScreenVariants;
using lsfUtils.ScreenVariants.Deeperspace;
using RWCustom;
using Stardust.PlacedObjects;
using System;
using System.IO;
using System.Security.Permissions;
using UnityEngine;
using static Pom.Pom;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace lsfUtils;

[BepInPlugin("lsfUtils", "LSF Utils", "0.1")]

public class Plugin : BaseUnityPlugin
{
    public static ManualLogSource Log { get; private set; }

    public static readonly int StaticRandom = RXRandom.Int(100);

    public bool initialized;
    public bool isInit;

    public const string templarMaskIcon = "atlases/templarMaskIcon";

    public static readonly EntityID SpecialId = new(-1, -20);

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
            On.RainWorld.PostModsInit += RainWorld_PostModsInit;

            // save file code
            {
                On.SaveState.LoadGame += SaveFileMain.CustomSavedataInit;
                On.RainWorldGame.Win += SaveFileMain.RainWorldGame_Win;
            }

            // fisobs
            {
                ItemRegistryCore.ApplyHooks();
                ItemRegistry.RegisterAll();

                CreatureRegistryCore.ApplyHooks();
                CreatureRegistry.RegisterAll();
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
                ToxicSpongeHooks.ApplyHooks();
            }

            // items
            {
                ArtifactHooks.Apply();

                RippleFlowerHooks.ApplyHooks();
                DartHooks.ApplyHooks();
                KarmaMaskHooks.ApplyHooks();
                ExplosiveBoomerangHooks.ApplyHooks();
                SpearHooks.ApplyHooks();
                TorchSpearHooks.ApplyHooks();
                CopperSpearHooks.ApplyHooks();
                BasementKeyHooks.ApplyHooks();
            }

            // devtools objects
            {
                LocalGravityHooks.ApplyHooks();
                ConditionalFilterHooks.ApplyHooks();
                CreatureFlagZoneHooks.ApplyHooks();
                FloatMudHooks.ApplyHooks();
                BackgroundMudHooks.ApplyHooks();
                WaveLightHooks.ApplyHooks();
                PoisonSteamHooks.ApplyHooks();
            }

            // devtools effects
            {
                CreepingDarkness.RegisterCreepingDarkness();
                CreepingDarknessHooks.ApplyHooks();

                EvilWater.RegisterEvilWater();
                EvilWaterHooks.ApplyHooks();

                StardustEnums.RegisterValues();
                StardustRainHooks.Apply();
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
                RegionTypeParams.ApplyHooks();
                ShelterParams.ApplyHooks();
                ShortcutParams.ApplyHooks();
                GateParams.ApplyHooks();
                ScavengerGearParams.ApplyHooks();
                On.Region.ctor_string_int_int_RainWorldGame_Timeline += CustomRegionParams.SetupParams;
            }

            // mechanics
            {
                TemporaryPoison.ApplyHooks();
            }

            //region params
            {
                ParamKey.ApplyHooks();
                ConditionalWorld.ApplyHooks();
            }
            // screen variants
            {
                ScreenVariantHooks.ApplyHooks();
            }

            On.RainWorld.Start += RainWorld_Start;

            Log.LogMessage("Checking isInit!");

            if (isInit) return;
            isInit = true;

            ConditionalLogic.LoadConditions();
            WorldLoader.Preprocessing.preprocessorConditions.Add(ConditionalLogic.LSFConditions);

            Log.LogMessage("Registering lsfUtils stuff...");

            RegisterManagedObject<ConditionFilter, ConditionFilterData, ManagedRepresentation>("ConditionalFilter", "lsfUtils");
            RegisterManagedObject<RoomConditionFilterUAD, RoomConditionFilterData, ManagedRepresentation>("RoomConditionalFilter", "lsfUtils");
            RegisterManagedObject<CreatureFlagZone, CreatureFlagZoneData, ManagedRepresentation>("CreatureFlagZone", "lsfUtils");
            RegisterManagedObject<LocalGravity, LocalGravityData, ManagedRepresentation>("LocalGravity", "lsfUtils");
            RegisterManagedObject<RippleZone, RippleZoneData, ManagedRepresentation>("RippleZone", "lsfUtils");
            RegisterManagedObject<EventRect, EventRectData, ManagedRepresentation>("EventRect", "lsfUtils");
            RegisterManagedObject<CustomPushback, CustomPushbackData, ManagedRepresentation>("CustomPushback", "lsfUtils");
            RegisterManagedObject<WaveLight, WaveLightData, WaveLightRepresentation>("WaveLight", "lsfUtils");
            //RegisterManagedObject<Bumper, BumperData, ManagedRepresentation>("Bumper", "lsfUtils");

            RegisterManagedObject(new ManagedObjectType("MudBonePile", "lsfUtils", typeof(MudBonePile), typeof(MudBonePileData), typeof(MudBonePileRepresentation)));

            RegisterManagedObject<RippleTunnel, RippleTunnelData, RippleTunnelRepresentation>("RippleTunnel", "lsfUtils");

            RegisterManagedObject(new ManagedFloatMud());
            RegisterManagedObject(new ManagedPoisonSteam());
            RegisterManagedObject(new ManagedBackgroundMud());
            RegisterManagedObject(new ManagedRippleFlower());
            RegisterManagedObject(new ManagedKarmaMask());
            RegisterManagedObject(new ManagedBrownFruit());
            RegisterManagedObject(new ManagedTorchSpear());
            RegisterManagedObject(new ManagedCopperSpear());
            RegisterManagedObject(new ManagedKnotSpawnV2());

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

        Log.LogMessage("Loading params...");

        RegionTypeParams.CheckFile();
        RegionTypeParams.LoadParams();
        ShelterParams.CheckFile();
        ShelterParams.LoadParams();
        ShortcutParams.CheckFile();
        ShortcutParams.LoadParams();
        GateParams.CheckFile();
        GateParams.LoadParams();
        ScavengerGearParams.CheckFile();
        ScavengerGearParams.LoadParams();

        Log.LogMessage("Finished loading params!");
    }

    public static bool TryAddItem(AbstractCreature creature, string itemName, int graspIndex, bool addToRoom, out AbstractPhysicalObject result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(itemName)) return false;

        string strItem = itemName.Trim().ToLowerInvariant();

        if (strItem == "spear" || strItem == "explosivespear" || strItem == "electricspear" || strItem == "firespear" || strItem == "poisonspear")
        {
            bool explosive = strItem == "explosivespear";
            bool electric = strItem == "electricspear";
            bool fire = strItem == "firespear";
            bool poison = strItem == "poisonspear";
            Log.LogMessage($"explosive: {explosive}, electric: {electric}, fire: {fire}, poison: {poison}");

            float hue = 0f;
            if (fire) hue = Mathf.Lerp(UnityEngine.Random.value, 1f, 0.001f);
            AbstractSpear spear = new AbstractSpear(creature.world, null, creature.pos, creature.world.game.GetNewID(), explosive, electric);
            AbstractSpear spearf = new AbstractSpear(creature.world, null, creature.pos, creature.world.game.GetNewID(), explosive, hue);
            if (poison)
            {
                spear.poison = 1f;
                spear.poisonHue = UnityEngine.Random.value;
            }
            if (hue != 0f)
            {
                if (addToRoom) creature.world.GetAbstractRoom(creature.pos).AddEntity(spearf);
                if (graspIndex > -1) new AbstractPhysicalObject.CreatureGripStick(creature, spearf, graspIndex, true);
            }
            else
            {
                if (addToRoom) creature.world.GetAbstractRoom(creature.pos).AddEntity(spear);
                if (graspIndex > -1) new AbstractPhysicalObject.CreatureGripStick(creature, spear, graspIndex, true);
            }
            result = spear;
            return true;
        }

        AbstractPhysicalObject.AbstractObjectType item = new(itemName.Trim(), false);
        if (item.Index == -1)
        {
            Log.LogWarning($"Unknown item! {itemName}");
            return false;
        }

        AbstractPhysicalObject abstractPhysicalObject = null;

        if (ItemRegistryTemplate.TryGet(item, out var lsfItem))
        {
            abstractPhysicalObject = lsfItem.Ctor(creature.world, creature.pos, creature.world.game.GetNewID());
        }
        else if (item == AbstractPhysicalObject.AbstractObjectType.GraffitiBomb)
        {
            abstractPhysicalObject = new GraffitiBomb.AbstractGraffitiBomb(creature.world, null, creature.pos, creature.world.game.GetNewID(), -1, -1, null);
        }
        else if (item == AbstractPhysicalObject.AbstractObjectType.DataPearl)
        {
            abstractPhysicalObject = new DataPearl.AbstractDataPearl(creature.world, item, null, creature.pos, creature.world.game.GetNewID(), -1, -1, null, DataPearl.AbstractDataPearl.DataPearlType.Misc);
        }
        else if (AbstractConsumable.IsTypeConsumable(item))
        {
            abstractPhysicalObject = new AbstractConsumable(creature.world, item, null, creature.pos, creature.world.game.GetNewID(), -1, -1, null);
        }
        else
        {
            abstractPhysicalObject = new AbstractPhysicalObject(creature.world, item, null, creature.pos, creature.world.game.GetNewID());
        }

        if (addToRoom) creature.world.GetAbstractRoom(creature.pos).AddEntity(abstractPhysicalObject);
        if (graspIndex > -1) new AbstractPhysicalObject.CreatureGripStick(creature, abstractPhysicalObject, graspIndex, true);
        result = abstractPhysicalObject;
        return true;
    }

    public static bool GetNameFromAnywhere(out SlugcatStats.Name name)
    {
        name = null;
        if (Custom.rainWorld.processManager?.currentMainLoop is RainWorldGame game && game.IsStorySession)
        {
            name = game.StoryCharacter;
            return true;
        }
        return false;
    }

    public static bool GetTimelineFromAnywhere(out SlugcatStats.Timeline timeline)
    {
        timeline = null;
        if (Custom.rainWorld.processManager?.currentMainLoop is RainWorldGame game && game.IsStorySession)
        {
            timeline = game.TimelinePoint;
            return true;
        }
        return false;
    }


    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

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

        Futile.atlasManager.LoadImage("atlases/egg");

        Futile.atlasManager.LoadAtlas("atlases/lsfLizardStuff");
        Futile.atlasManager.LoadAtlas("atlases/corrodedskylines");

        Futile.atlasManager.LoadImage(templarMaskIcon);

        Futile.atlasManager.LoadAtlas("atlases/SeerMaskSprites");

        string bundlePath = AssetManager.ResolveFilePath("shaders/lsfutils");
        Log.LogMessage($"Resolved bundle path: {bundlePath}");
        if (File.Exists(bundlePath))
        {
            Log.LogMessage($"Bundle last write: {File.GetLastWriteTime(bundlePath)}");
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
            RegisterShader(self, bundle, "Assets/Shaders/CustomStardust.shader", "CustomStardust");

            bundle.Unload(false);
        }
        else
        {
            Log.LogMessage("Failed to load lsfUtils shader bundle!");
        }

        Factions.LoadAndRegisterAllFactions();

        Log.LogMessage("Sprites and shaders loaded!");
    }

    private void RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
    {
        orig(self);
        if (ModManager.MSC)
        {
            ItemCrafting.ResizeGourmandCombos();
            ItemCrafting.InitGourmandCombos();
            On.MoreSlugcats.GourmandCombos.InitCraftingLibrary += ItemCrafting.On_GourmandCombos_InitCraftingLibrary;
            On.Player.CraftingResults += ItemCrafting.On_Player_CraftingResults;
            On.Player.SpitUpCraftedObject += ItemCrafting.On_Player_SpitUpCraftedObject;
            On.Player.Die += ItemCrafting.On_Player_Die;
            On.PlayerGraphics.DefaultFaceSprite_float_int += ItemCrafting.On_PlayerGraphics_DefaultFaceSprite_float_int;
            On.PlayerGraphics.DefaultFaceSprite_float += ItemCrafting.On_PlayerGraphics_DefaultFaceSprite_float;
            On.PlayerGraphics.DrawSprites += ItemCrafting.On_PlayerGraphics_DrawSprites;
            On.PlayerGraphics.ApplyPalette += ItemCrafting.On_PlayerGraphics_ApplyPalette;
            On.AbstractRoom.RealizeRoom += ItemCrafting.On_AbstractRoom_RealizeRoom;
            On.World.SpawnPupNPCs += ItemCrafting.On_World_SpawnPupNPCs;
            On.MoreSlugcats.GourmandCombos.CraftingResults += ItemRegistryCore.GourmandCombos_CraftingResults;
        }
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