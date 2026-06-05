using BepInEx;
using BepInEx.Logging;
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
using lsfUtils.Creatures.Spiders;
using lsfUtils.Creatures.Spiders.PoisonSpider;
using lsfUtils.CreatureTags;
using lsfUtils.CWTs;
using lsfUtils.DevtoolsObjects.ConditionalFilter;
using lsfUtils.DevtoolsObjects.EventRectangle;
using lsfUtils.DevtoolsObjects.LocalGravity;
using lsfUtils.DevtoolsObjects.RippleZone;
using lsfUtils.Effects;
using lsfUtils.Items.Darts.Dart;
using lsfUtils.Items.Darts.PoisonDart;
using lsfUtils.Items.RippleFlower;
using lsfUtils.Items.KarmaMask;
using lsfUtils.RegionParams;
using lsfUtils.Ripplespace;
using Menu.Remix.MixedUI;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Newtonsoft.Json.Linq;
using RWCustom;
using SlugBase.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Security.Permissions;
using UnityEngine;
using Watcher;
using static lsfUtils.RegionParams.RegionTypeParams;
using static Pom.Pom;
using static SlugBase.Features.FeatureTypes;
using System.IO;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace lsfUtils
{
    [BepInDependency("slime-cubed.slugbase")]
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

                    Content.Register(new RippleFlowerFisob());
                    Content.Register(new PoisonDartFisob());
                    Content.Register(new KarmaMaskFisob());

                    Log.LogMessage("Done with Fisobs!");
                }

                // creatures
                {
                    // lizards
                    {
                        On.LizardBreeds.BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate += LizardCode.On_LizardBreeds_BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate;
                        On.LizardVoice.GetMyVoiceTrigger += LizardCode.On_LizardVoice_GetMyVoiceTrigger;
                        On.LizardAI.ctor += LizardCode.LizardAI_ctor;
                        On.LizardTongue.ctor += LizardCode.LizardTongue_ctor;
                        On.LizardGraphics.InitiateSprites += LizardCode.LizardGraphics_InitiateSprites;
                        new Hook(typeof(Lizard).GetProperty(nameof(Lizard.Swimmer)).GetGetMethod(), typeof(LizardCode).GetMethod(nameof(LizardCode.Lizard_Swimmer)));

                        // airplane lizard
                        {
                            On.Lizard.EnterAnimation += AirplaneLizardHooks.Lizard_EnterAnimation;
                            On.LizardGraphics.BodyColor += AirplaneLizardHooks.LizardGraphics_BodyColor;
                            On.LizardGraphics.ColorBody += AirplaneLizardHooks.LizardGraphics_ColorBody;
                            On.LizardCosmetics.SpineSpikes.ctor += AirplaneLizardHooks.SpineSpikes_ctor;
                            On.LizardAI.AggressiveBehavior += AirplaneLizardHooks.LizardAI_AggressiveBehavior;
                        }

                        // flame lizard
                        {

                        }

                        // raspberry lizard
                        {
                            //IL.LizardCosmetics.Antennae.ctor += Creatures.RaspberryLizard.RaspberryLizardHooks.Antennae_ctor;
                            On.LizardAI.ctor += RaspberryLizardHooks.LizardAI_ctor;
                            On.LizardAI.TravelPreference += RaspberryLizardHooks.LizardAI_TravelPreference;
                            On.LizardPather.HeuristicForCell += RaspberryLizardHooks.LizardPather_HeuristicForCell;
                        }

                        // weaver lizard
                        {
                            On.LizardGraphics.InitiateSprites += WeaverLizardHooks.LizardGraphics_InitiateSprites;
                            On.LizardCosmetics.LongShoulderScales.ctor += WeaverLizardHooks.LongShoulderScales_ctor;
                            On.LizardCosmetics.TailTuft.ctor += WeaverLizardHooks.TailTuft_ctor;

                        }

                        // poison lizard
                        {
                            On.LizardAI.Update += PoisonLizardHooks.LizardAI_Update;
                            On.Lizard.Bite += PoisonLizardHooks.Lizard_Bite;
                            On.LizardTongue.Impact += PoisonLizardHooks.LizardTongue_Impact;
                            On.LizardAI.IUseARelationshipTracker_UpdateDynamicRelationship += PoisonLizardHooks.LizardAI_IUseARelationshipTracker_UpdateDynamicRelationship;
                        }

                        // monitor lizard
                        {
                            On.LizardGraphics.ColorBody += MonitorLizardHooks.LizardGraphics_ColorBody;
                            On.LizardGraphics.BodyColor += MonitorLizardHooks.LizardGraphics_BodyColor;
                            On.Water.Update += MonitorLizardHooks.Water_Update;
                            On.MudPit.ChunkSlowdown += MonitorLizardHooks.MudPit_ChunkSlowdown;
                        }

                        // starnosed lizard
                        {
                            On.SuperHearing.Update += StarNosedLizardHooks.SuperHearing_Update;
                            IL.LizardCosmetics.Whiskers.ctor += NoseTendrils.Whiskers_ctor;
                            new Hook(typeof(LizardGraphics).GetProperty(nameof(LizardGraphics.effectColor)).GetGetMethod(), typeof(StarNosedLizardHooks).GetMethod(nameof(StarNosedLizardHooks.Lizard_effectColor)));
                            new Hook(typeof(LizardGraphics).GetProperty(nameof(LizardGraphics.HeadLightsUpFromNoise)).GetGetMethod(), typeof(StarNosedLizardHooks).GetMethod(nameof(StarNosedLizardHooks.Lizard_HeadLight)));
                            On.LizardAI.Update += StarNosedLizardHooks.LizardAI_Update;
                        }
                    }

                    // scavs
                    {
                        On.Scavenger.SetUpCombatSkills += ScavCode.Scavenger_SetUpCombatSkills;
                        On.Scavenger.Throw += ScavCode.Scavenger_Throw;
                        On.ScavengerAI.WantToThrowSpearAtCreature += ScavCode.ScavengerAI_WantToThrowSpearAtCreature;
                        On.ScavengerAI.DecideBehavior += ScavCode.ScavengerAI_DecideBehavior;
                        On.ScavengersWorldAI.Trader.ScavScore += ScavCode.Trader_ScavScore;

                        // scav flank
                        {
                            On.Scavenger.ctor += ScavFlankHooks.Scavenger_ctor;
                            On.Scavenger.Update += ScavFlankHooks.Scavenger_Update;
                            On.Scavenger.PlaceInRoom += ScavFlankHooks.Scavenger_PlaceInRoom;
                            On.Scavenger.Violence += ScavFlankHooks.Scavenger_Violence;

                            new Hook(typeof(Scavenger).GetProperty(nameof(Scavenger.KarmicArmorProtected)).GetGetMethod(), typeof(ScavFlankHooks).GetMethod(nameof(ScavFlankHooks.KarmicArmor_Protected)));
                        }

                        // scav seer
                        {
                            On.ScavengerGraphics.ShockReaction += ScavSeerHooks.ScavengerGraphics_ShockReaction;
                            On.ScavengerAbstractAI.InOffscreenDen += ScavFlankHooks.ScavengerAbstractAI_InOffscreenDen;
                            On.ScavengerAI.ScavPlayerRelationChange += ScavMessengerHooks.ScavengerAI_ScavPlayerRelationChange;
                            On.ScavengerAI.WantToStayInDenUntilEndOfCycle += ScavMessengerHooks.ScavengerAI_WantToStayInDenUntilEndOfCycle;
                            On.ScavengerGraphics.ctor += ScavSeerHooks.ScavengerGraphics_ctor;
                            On.ScavengerAbstractAI.ScavengerSquad.UpdateLeader += ScavSeerHooks.ScavengerSquad_UpdateLeader;
                            On.ScavengerAI.ReactToNoise += ScavSeerHooks.ScavengerAI_ReactToNoise;
                            On.ScavengerAI.Update += ScavSeerHooks.ScavengerAI_Update;
                        }

                        // scav messenger
                        {
                            On.ScavengerAI.SocialEvent += ScavMessengerHooks.ScavengerAI_SocialEvent;
                            On.ScavengerAbstractAI.GoHome += ScavMessengerHooks.ScavengerAbstractAI_GoHome;
                            On.ScavengerAbstractAI.ReGearInDen += ScavMessengerHooks.ScavengerAbstractAI_ReGearInDen;
                            On.ScavengerAI.WeaponScore += ScavMessengerHooks.ScavengerAI_WeaponScore;
                            On.ScavengerAI.CollectScore_PhysicalObject_bool += ScavMessengerHooks.ScavengerAI_CollectScore_PhysicalObject_bool;
                        }


                    }

                    // spiders
                    {
                        On.DartMaggot.ChangeMode += SpiderCode.DartMaggot_ChangeMode;
                        On.BigSpiderAI.SpiderSpitModule.CanSpit += SpiderCode.SpiderSpitModule_CanSpit;
                        On.DartMaggot.Shoot += SpiderCode.DartMaggot_Shoot;
                        On.DartMaggot.Update += SpiderCode.DartMaggot_Update;
                        On.BigSpiderAI.SpiderSpitModule.SpiderHasSpit += SpiderCode.SpiderSpitModule_SpiderHasSpit;
                    }
                }

                // items
                {
                    // ripple flower
                    {
                        Log.LogMessage("Loading ripple flower code!");
                        On.Player.Update += RippleFlower.Player_Update;
                        On.KarmaFlower.BitByPlayer += RippleFlower.KarmaFlower_BitByPlayer;
                        On.KarmaFlower.DrawSprites += RippleFlower.KarmaFlower_DrawSprites;
                        On.KarmaFlower.ApplyPalette += RippleFlower.KarmaFlower_ApplyPalette;
                        On.KarmaFlower.InitiateSprites += RippleFlower.KarmaFlower_InitiateSprites;
                        On.PlayerGraphics.InitiateSprites += RippleFlower.PlayerGraphics_InitiateSprites;


                        On.PhysicalObject.GetLocalGravity += PhysicalObject_GetLocalGravity;

                        new Hook(typeof(Player).GetProperty(nameof(Player.rippleLevel)).GetGetMethod(), typeof(RippleFlower).GetMethod(nameof(RippleFlower.PlayerRippleLevel)));
                        new Hook(typeof(Player).GetProperty(nameof(Player.maxRippleLevel)).GetGetMethod(), typeof(RippleFlower).GetMethod(nameof(RippleFlower.PlayerMaxRippleLevel)));
                        Log.LogMessage("Exiting ripple flower code!");
                    }

                    // darts
                    {
                        On.Player.GrabUpdate += DartHooks.Player_GrabUpdate;
                    }

                    // karma mask
                    {
                        new Hook(typeof(RegionGate).GetProperty(nameof(RegionGate.MeetRequirement))!.GetGetMethod(), typeof(KarmaMaskHooks).GetMethod(nameof(KarmaMaskHooks.Meet_Requirement)));
                        On.HUD.KarmaMeter.Update += KarmaMaskHooks.KarmaMeter_Update;
                        On.Player.Update += KarmaMaskHooks.Player_Update;
                        On.MoreSlugcats.VultureMaskGraphics.ctor_PhysicalObject_MaskType_int_string += KarmaMaskHooks.VultureMaskGraphics_ctor_PhysicalObject_MaskType_int_string;
                        On.MoreSlugcats.VultureMaskGraphics.DrawSprites += KarmaMaskHooks.VultureMaskGraphics_DrawSprites;

                    }
                }

                // devtools objects
                {
                    // local gravity
                    {
                        Log.LogMessage("Gravity override rework!");
                        On.PhysicalObject.Update += LocalGravity.PhysicalObject_Update;
                        On.Player.Update += LocalGravity.Player_Update_CorrectGravityField;
                        On.Player.UpdateBodyMode += LocalGravity.Player_UpdateBodyMode;
                        On.Player.UpdateAnimation += LocalGravity.Player_UpdateAnimation;
                        On.Player.Update += LocalGravity.Player_Update;
                        new Hook(typeof(PhysicalObject).GetProperty(nameof(PhysicalObject.EffectiveRoomGravity)).GetGetMethod(), typeof(LocalGravity).GetMethod(nameof(LocalGravity.EffectiveRoomGravity)));
                        new Hook(typeof(Player).GetProperty(nameof(Player.EffectiveRoomGravity)).GetGetMethod(), typeof(LocalGravity).GetMethod(nameof(LocalGravity.EffectiveRoomGravityForPlayer)));
                    }

                    // conditional filter
                    {
                        On.RoomSettings.LoadPlacedObjects_StringArray_Timeline += ConditionalLogic.RoomSettings_LoadPlacedObjects_StringArray_Timeline;
                    }

                    // event zone
                    {

                    }
                }

                // devtools effects
                {
                    // creeping darkness
                    {
                        On.RoomCamera.Update += CreepingDarkness.RoomCamera_Update;
                        On.LightSource.Update += CreepingDarkness.LightSource_Update;
                        On.Lantern.Update += CreepingDarkness.Lantern_Update;
                        On.Player.Update += CreepingDarkness.Player_Update;
                        On.RainWorldGame.Update += CreepingDarkness.RainWorldGame_Update;
                    }

                    // evilwater
                    {
                        On.Water.ctor += EvilWater.InitialiseEvilWater;
                        On.Creature.Update += EvilWater.EvilWaterLogic;
                    }
                }

                // creature flags
                {
                    On.AbstractCreature.setCustomFlags += CreatureFlagSetup.AbstractCreature_setCustomFlags;

                    // ripple hybrid
                    {
                        On.PhysicalObject.InitiateGraphicsModule += RippleHybrid.PhysicalObject_InitiateGraphicsModule;
                        On.RoomCamera.SpriteLeaser.ctor += RippleHybrid.SpriteLeaser_ctor;
                    }

                    // poison immune
                    {
                        On.Creature.InjectPoison += PoisonImmune.Creature_InjectPoison;
                        On.Creature.Update += PoisonImmune.Creature_Update;
                    }

                    // ghost immune
                    {
                        IL.GhostCreatureSedater.Update += EchoImmune.GhostCreatureSedater_Update;
                    }
                }

                // region parameters
                {
                    On.Region.ctor_string_int_int_RainWorldGame_Timeline += CustomRegionParams.Region_ctor_string_int_int_RainWorldGame_Timeline;

                    // scavenger params
                    {
                        On.ScavengerAbstractAI.InitGearUp += ScavengerParams.ScavengerAbstractAI_InitGearUp;
                    }

                    // sentient rot params
                    {
                        On.Region.IsSentientRotRegion += Region_IsSentientRotRegion;
                        On.Region.IsVanillaSentientRotRegion += Region_IsVanillaSentientRotRegion;
                        On.Region.HasSentientRotResistance += Region_HasSentientRotResistance;
                        On.Region.IsDaemonRegion += Region_IsDaemonRegion;
                        On.Region.IsAncientUrbanRegion += Region_IsAncientUrbanRegion;
                        On.Region.IsRubiconRegion += Region_IsRubiconRegion;
                        On.Region.IsShatteredTerraceRegion += Region_IsShatteredTerraceRegion;
                        On.Region.HasWarpFatigueResistance += Region_HasWarpFatigueResistance;
                    }
                }

                EvilWater.RegisterEvilWater();
                CreepingDarkness.RegisterCreepingDarkness();

                On.RainWorld.Start += RainWorld_Start;

                Log.LogMessage("Checking isInit!");

                if (isInit) return;
                isInit = true;

                // processing conditions
                {
                    WorldLoader.Preprocessing.preprocessorConditions.Add(ConditionalLogic.LSFConditions);
                }

                Log.LogMessage("Registering lsfUtils stuff...");

                RegisterManagedObject<ConditionFilter, ConditionFilterData, ManagedRepresentation>("ConditionalFilter", "lsfUtils");
                RegisterManagedObject<RoomConditionFilterUAD, RoomConditionFilterData, ManagedRepresentation>("RoomConditionalFilter", "lsfUtils");
                RegisterManagedObject<LocalGravity, LocalGravityData, ManagedRepresentation>("LocalGravity", "lsfUtils");
                RegisterManagedObject<RippleZone, RippleZoneData, ManagedRepresentation>("RippleZone", "lsfUtils");
                RegisterManagedObject<EventRect, EventRectData, ManagedRepresentation>("EventRect", "lsfUtils");

                RegisterManagedObject(new ManagedRippleFlower());
                RegisterManagedObject(new ManagedKarmaMask());

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
                Log.LogWarning($"RegionTypeParams.Load: AssetManager not ready or path resolution failed: {ex.Message}");
                return;
            }

            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                Log.LogWarning("regionMetaParameters.txt not found.");
                return;
            }

            RegionTypeParams.Load();
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
            else
            {
                Log.LogMessage("Error in GetNameFromAnywhere!");
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
            else
            {
                Log.LogMessage("Error in GetTimelineFromAnywhere!");
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

            Futile.atlasManager.LoadAtlas("atlases/lsfLizardStuff");

            Futile.atlasManager.LoadImage(templarMaskIcon);
        }

        private float PhysicalObject_GetLocalGravity(On.PhysicalObject.orig_GetLocalGravity orig, PhysicalObject self)
        {
            if (self != null && PhysicalObjectCWT.TryGetData(self, out var data) && data.shouldOverrideGravity)
            {
                return data.overrideGravity;
            }
            return orig(self);
        }

        public void OnDisable()
        {
            if (!isInit) return;
            isInit = false;

            WorldLoader.Preprocessing.preprocessorConditions.Remove(ConditionalLogic.LSFConditions);
        }
    }
}


