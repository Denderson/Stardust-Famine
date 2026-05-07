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
using lsfUtils.CWTs;
using lsfUtils.Items;
using lsfUtils.Items.PoisonDart;
using lsfUtils.Items.RippleFlower;
using lsfUtils.ProcessingConditions;
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
using static Pom.Pom;
using static SlugBase.Features.FeatureTypes;
using static lsfUtils.Ripplespace.RippleHybridHooks;
using lsfUtils.Ripplespace;
using lsfUtils.Effects.EvilWater;
using lsfUtils.Items.Dart;

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

        public static readonly EntityID SpecialId = new(1, -20);
        public static readonly float secondsUntilFullPoisonFromEvilWater = 5;

        public static readonly float secondsUntilPoisonStartsFallingOffAfterExitingPoisonWater = 3;
        public static readonly float secondsUntilPoisonBuildupAfterEnteringPoisonWater = 1.5f;

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

                    Log.LogMessage("Done with Fisobs!");
                }

                // lizards
                {
                    On.LizardBreeds.BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate += LizardCode.On_LizardBreeds_BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate;
                    On.LizardVoice.GetMyVoiceTrigger += LizardCode.On_LizardVoice_GetMyVoiceTrigger;
                    On.LizardAI.ctor += LizardCode.LizardAI_ctor;
                    On.LizardTongue.ctor += LizardCode.LizardTongue_ctor;
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

                // items
                {
                    // ripple flower
                    {
                        On.Player.Update += RippleFlower.Player_Update;
                        On.KarmaFlower.BitByPlayer += RippleFlower.KarmaFlower_BitByPlayer;
                        On.KarmaFlower.DrawSprites += RippleFlower.KarmaFlower_DrawSprites;
                        On.KarmaFlower.ApplyPalette += RippleFlower.KarmaFlower_ApplyPalette;
                        On.KarmaFlower.InitiateSprites += RippleFlower.KarmaFlower_InitiateSprites;
                        On.PlayerGraphics.InitiateSprites += RippleFlower.PlayerGraphics_InitiateSprites;

                        On.PhysicalObject.GetLocalGravity += PhysicalObject_GetLocalGravity;

                        new Hook(typeof(Player).GetProperty(nameof(Player.rippleLevel)).GetGetMethod(), typeof(RippleFlower).GetMethod(nameof(RippleFlower.PlayerRippleLevel)));
                        new Hook(typeof(Player).GetProperty(nameof(Player.maxRippleLevel)).GetGetMethod(), typeof(RippleFlower).GetMethod(nameof(RippleFlower.PlayerMaxRippleLevel)));
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

                // gravity override
                {
                    On.PhysicalObject.Update += LocalGravity.PhysicalObject_Update;
                    On.Player.Update += LocalGravity.Player_Update;
                    new Hook(typeof(PhysicalObject).GetProperty(nameof(PhysicalObject.EffectiveRoomGravity)).GetGetMethod(), typeof(LocalGravity).GetMethod(nameof(LocalGravity.EffectiveRoomGravity)));
                    new Hook(typeof(Player).GetProperty(nameof(Player.EffectiveRoomGravity)).GetGetMethod(), typeof(LocalGravity).GetMethod(nameof(LocalGravity.EffectiveRoomGravityForPlayer)));
                }

                // misc
                {
                    On.PhysicalObject.InitiateGraphicsModule += PhysicalObject_InitiateGraphicsModule;
                    On.RoomCamera.SpriteLeaser.ctor += SpriteLeaser_ctor;
                    On.AbstractCreature.setCustomFlags += AbstractCreature_setCustomFlags;

                    On.Water.ctor += EvilWater.InitialiseEvilWater;
                    On.Creature.Update += EvilWater.EvilWaterLogic;
                }

                On.RoomSettings.LoadPlacedObjects_StringArray_Timeline += ConditionalLogic.RoomSettings_LoadPlacedObjects_StringArray_Timeline;
                On.Player.GrabUpdate += Player_GrabUpdate;


                if (isInit) return;
                isInit = true;


                // processing conditions
                {
                    
                    WorldLoader.Preprocessing.preprocessorConditions.Add(ConditionalLogic.LSFConditions);
                }

                RegisterManagedObject(new ManagedRippleFlower());
                RegisterManagedObject<ConditionFilterUAD, ConditionFilterData, ManagedRepresentation>("ConditionalFilter", "lsfUtils");
                RegisterManagedObject<RoomConditionFilterUAD, RoomConditionFilterData, ManagedRepresentation>("RoomConditionalFilter", "lsfUtils");
                RegisterManagedObject<LocalGravity, LocalGravityData, ManagedRepresentation>("LocalGravity", "lsfUtils");
                RegisterManagedObject<RippleZoneUAD, RippleZoneData, ManagedRepresentation>("RippleZone", "lsfUtils");
                EvilWater.RegisterEvilWater();

                Logger.LogMessage("LSF Utils success!");
            }
            catch (Exception e)
            {
                Logger.LogMessage("LSF Utils failure!!!");
                Logger.LogError(e);
            }
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
        }

        private float PhysicalObject_GetLocalGravity(On.PhysicalObject.orig_GetLocalGravity orig, PhysicalObject self)
        {
            if (self != null && PhysicalObjectCWT.TryGetData(self, out var data) && data.shouldOverrideGravity)
            {
                return data.overrideGravity;
            }
            return orig(self);
        }

        private void LizardAI_ctor(On.LizardAI.orig_ctor orig, LizardAI self, AbstractCreature creature, World world)
        {
            orig(self, creature, world);
            CreatureTemplate.Type type = creature?.creatureTemplate?.type;
            if (type == Enums.CreatureTemplateType.AirplaneLizard  || type == Enums.CreatureTemplateType.WeaverLizard || type == Enums.CreatureTemplateType.FlameLizard || type == Enums.CreatureTemplateType.RaspberryLizard)
            {
                self.usedToVultureMask = 9999;
            }
        }

        public void OnDisable()
        {
            if (!isInit) return;
            isInit = false;

            WorldLoader.Preprocessing.preprocessorConditions.Remove(ConditionalLogic.LSFConditions);
        }
    }
}


