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
using Stardust.Anchors;
using Stardust.Conditionals;
using Stardust.CWTs;
using Stardust.Mechanics;
using Stardust.SaveFile;
using Stardust.Slugcats;
using Stardust.Slugcats.Bitter;
using Stardust.Slugcats.Scholar.Permadeath;
using Stardust.Slugcats.Bitter.BitterGraphics;
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
using System.Text.RegularExpressions;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Playables;
using Watcher;
using static Pom.Pom;
using static SlugBase.Features.FeatureTypes;
using static Watcher.RippleHybridVFX;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace Stardust
{
    [BepInDependency("lsfUtils", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("slime-cubed.slugbase", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("io.github.dual.fisobs", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("stardustFamine", "Stardust Famine", "0.1")]

    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource Log { get; private set; }

        public static OptionsMenu optionsMenuInstance;
        public bool initialized;
        public bool isInit;

        public static bool SharedMechanics(SlugcatStats.Name name)
        {
            return name == Enums.SlugcatStatsName.bitter || name == Enums.SlugcatStatsName.sfscholar;
        }

        public static bool SharedMechanics(SlugcatStats.Timeline name)
        {
            return name == Enums.SlugcatStatsTimeline.bitterTimeline || name == Enums.SlugcatStatsTimeline.sfscholarTimeline;
        }

        public static bool CanCommunicate(SlugcatStats.Name name)
        {
            return name == WatcherEnums.SlugcatStatsName.Watcher || name == Enums.SlugcatStatsName.bitter || name == Enums.SlugcatStatsName.sfscholar;
        }
        private void LoadResources(RainWorld rainWorld)
        {

        }

        public void OnEnable()
        {
            UnityEngine.Debug.Log("Starting SF");
            try
            {
                Log = base.Logger;
                Logger.LogInfo(Log);

                On.RainWorld.OnModsInit += RainWorld_OnModsInit;

                // conditionals code
                {
                    On.SaveState.SaveToString += ConditionalsCode.DeleteRespawnList;
                    On.WorldLoader.GeneratePopulation += ConditionalsCode.RefreshSpawns;
                }

                // spawn code
                {
                    On.Player.Update += SpawnCode.StarspawnKillCode;
                }

                // slugcat code
                {
                    On.Player.Update += SlugcatCode.Player_Update;

                    On.Menu.SlugcatSelectMenu.SlugcatPageNewGame.ctor += SlugcatCode.SlugcatPageNewGame_ctor;
                    On.Menu.SlugcatSelectMenu.SlugcatPage.AddImage += SlugcatCode.SlugcatPage_AddImage;
                    On.Menu.MenuScene.BuildScene += SlugcatCode.MenuScene_BuildScene;

                    On.SlugcatStats.SlugcatUnlocked += SlugcatCode.LockScholar;
                    On.SlugcatStats.SpearSpawnElectricRandomChance_Timeline += SlugcatCode.SlugcatStats_SpearSpawnElectricRandomChance_Timeline;
                    On.SlugcatStats.SpearSpawnExplosiveRandomChance_Timeline += SlugcatCode.SlugcatStats_SpearSpawnExplosiveRandomChance_Timeline;
                    On.SlugcatStats.SpearSpawnModifier_Timeline_float += SlugcatCode.SlugcatStats_SpearSpawnModifier_Timeline_float;
                    On.SlugcatStats.PearlsGivePassageProgress += SlugcatCode.NoScholarPassage;
                    On.Menu.SleepAndDeathScreen.AddPassageButton += SlugcatCode.NoPassageButton;
                }

                // bitter code
                {
                    // graphics
                    {
                        On.PlayerGraphics.InitiateSprites += BitterGraphics.PlayerGraphics_InitiateSprites;
                        On.PlayerGraphics.AddToContainer += BitterGraphics.PlayerGraphics_AddToContainer;
                        On.PlayerGraphics.DrawSprites += BitterGraphics.PlayerGraphics_DrawSprites;
                        On.PlayerGraphics.ApplyPalette += BitterGraphics.PlayerGraphics_ApplyPalette;
                        On.PlayerGraphics.DefaultFaceSprite_float_int += BitterGraphics.PlayerGraphics_DefaultFaceSprite_float_int;
                        On.PlayerGraphics.MuddableSprite += BitterGraphics.PlayerGraphics_MuddableSprite;
                    }


                    // armor code
                    {
                        On.Player.ctor += ArmorCode.SetArmorOnPlayerCreation;
                        On.ShelterDoor.Close += ArmorCode.SaveArmorOnHibernation;
                        On.Creature.Violence += ArmorCode.Creature_Violence;
                        On.Player.SpearStick += ArmorCode.Player_SpearStick;
                    }

                    On.Creature.Grasp.ctor += BitterCode.BitterGraspImmunity;
                    On.LocustSystem.Swarm.TryAttach += BitterCode.BitterLocustImmunity;
                    On.SlugcatStats.ctor += BitterCode.OldFoodMeterCode; // NEED CHANGE
                    On.Player.DeathByBiteMultiplier += BitterCode.BitterBiteResistance; // NEEDS CHANGE

                    //On.HUD.FoodMeter.ctor += EmergencyFoodMeter.FoodMeter_ctor;
                }

                // scholar code
                {

                }

                // permadeath code
                {
                    On.HUD.TextPrompt.Update += Permadeath.TextPromptCycleFix;
                    On.HUD.Map.CycleLabel.UpdateCycleText += Permadeath.CycleLabelCycleFix;
                    On.RainWorldGame.GoToRedsGameOver += Permadeath.ScholarPermadeathTrigger;
                    On.RainWorldGame.GameOver += Permadeath.CheckForPermadeath;
                    On.Menu.SlugcatSelectMenu.SlugcatPageContinue.ctor += Permadeath.PermadeathContinueScreen;
                    On.Menu.SlugcatSelectMenu.ctor += Permadeath.AddThreadsCheckbox;
                    On.ProcessManager.PostSwitchMainProcess += Permadeath.SwitchToThreadsScreen;
                    On.Menu.SlugcatSelectMenu.ContinueStartedGame += Permadeath.GoToThreadsScreenFromMainMenu;
                    On.Menu.SlugcatSelectMenu.StartGame += Permadeath.GoToThreadsScreenFromStartScreen;
                    On.Menu.SlugcatSelectMenu.UpdateStartButtonText += Permadeath.PermadeathStartButton;
                }

                // karma code
                {
                    
                    On.SaveState.GhostEncounter += KarmaCode.GetTriggersAfterEcho;
                    On.Menu.KarmaLadder.GoToKarma += KarmaCode.ChangeDestinationKarma;

                    // all of these just edit the karma meter the same way in different triggers
                    On.HUD.KarmaMeter.ctor += KarmaCode.KarmaMeter_ctor;
                    On.HUD.KarmaMeter.UpdateGraphic += KarmaCode.KarmaMeter_UpdateGraphic;
                    On.HUD.KarmaMeter.UpdateGraphic_int_int += KarmaCode.KarmaMeter_UpdateGraphic_int_int;
                    On.Menu.KarmaLadder.KarmaSymbol.ctor += KarmaCode.KarmaSymbol_ctor;
                    On.Menu.KarmaLadder.KarmaSymbol.UpdateDisplayKarma += KarmaCode.KarmaSymbol_UpdateDisplayKarma;
                    On.Menu.KarmaLadder.KarmaSymbol.GrafUpdate += KarmaCode.KarmaSymbol_GrafUpdate;
                }

                // ghost code
                {
                    On.GoldFlakes.NumberOfFlakes += GhostCode.DynamicNumberOfFlakes;
                    On.Menu.GhostEncounterScreen.GetDataFromGame += GhostCode.MinKarmaOnEchoScreen;
                    On.GhostWorldPresence.SpawnGhost += GhostCode.NoEchoPriming;
                    On.Room.Loaded += GhostCode.RippleDepthsNearEchoes;
                    On.Ghost.FadeOutFinished += GhostCode.CheckpointAfterEcho;
                }

                // save file code
                {
                    On.SaveState.LoadGame += SaveFileMain.CustomSavedataInit;
                    On.RainWorldGame.Win += SaveFileMain.RainWorldGame_Win;
                }

                // gate code
                {
                    On.DeathPersistentSaveData.CanUseUnlockedGates += GateCode.DeathPersistentSaveData_CanUseUnlockedGates;
                    On.RegionGate.Unlock += GateCode.RegionGate_Unlock;
                    On.RegionGate.KarmaBlinkRed += GateCode.NoBlinkingKarmaOnExhaustedGates;
                    On.RegionGate.ctor += GateCode.ExhaustGates;
                    On.GateKarmaGlyph.Update += GateCode.GateKarmaGlyph_Update;
                }

                // misc
                {
                    On.LocustSystem.SwarmScore_Creature += MiscCode.LocustSystem_SwarmScore_Creature;
                }

                // anchor
                {
                    On.World.LoadWorldForFastTravel_Timeline_List1_Int32Array_Int32Array_Int32Array += AnchorHooks.World_LoadWorldForFastTravel_Timeline_List1_Int32Array_Int32Array_Int32Array;
                    On.Music.PlayerThreatTracker.Update += AnchorHooks.PlayerThreatTracker_Update;
                    On.Music.MusicPlayer.GameRequestsSong += AnchorHooks.MusicPlayer_GameRequestsSong;
                    On.Music.GhostSong.Update += AnchorHooks.GhostSong_Update;
                }

                // il hooks
                {
                    Log.LogMessage("IL hook hell starting");
                    IL.Menu.KarmaLadder.ctor_Menu_MenuObject_Vector2_HUD_IntVector2_bool += KarmaCode.KarmaLadder_ctor_Menu_MenuObject_Vector2_HUD_IntVector2_bool;
                    IL.DeathPersistentSaveData.SaveToString += KarmaCode.DeathPersistentSaveData_SaveToString;
                    IL.Menu.SleepAndDeathScreen.GetDataFromGame += Permadeath.SleepAndDeathScreen_GetDataFromGame;
                }

                // manual hooks
                {
                    Log.LogMessage("Manual hook hell starting");
                    new Hook(typeof(Player).GetProperty(nameof(Player.OutsideWatcherCampaign)).GetGetMethod(), typeof(KarmaCode).GetMethod(nameof(KarmaCode.Outside_Watcher)));
                    new Hook(typeof(Player).GetProperty(nameof(Player.rippleLevel)).GetGetMethod(), typeof(KarmaCode).GetMethod(nameof(KarmaCode.Ripple_Level)));

                    new Hook(typeof(RegionGate).GetProperty(nameof(RegionGate.MeetRequirement))!.GetGetMethod(), typeof(GateCode).GetMethod(nameof(GateCode.Meet_Requirement)));

                    new Hook(typeof(SaveState).GetProperty(nameof(SaveState.CanSeeVoidSpawn)).GetGetMethod(), typeof(SpawnCode).GetMethod(nameof(SpawnCode.CanSee_VoidSpawn)));
                    new Hook(typeof(VoidSpawnMigrationStream).GetProperty(nameof(VoidSpawnMigrationStream.MaxCapacity)).GetGetMethod(), typeof(SpawnCode).GetMethod(nameof(SpawnCode.Migration_MaxCapacity)));

                    new Hook(typeof(GateKarmaGlyph).GetProperty(nameof(GateKarmaGlyph.PlayNoEnergyAnimation)).GetGetMethod(), typeof(GateCode).GetMethod(nameof(GateCode.No_Energy)));

                    new Hook(typeof(Menu.SlugcatSelectMenu.SlugcatPageContinue).GetProperty(nameof(Menu.SlugcatSelectMenu.SlugcatPageContinue.HasGlow)).GetGetMethod(), typeof(SlugcatCode).GetMethod(nameof(SlugcatCode.HasGlow)));
                    new Hook(typeof(Menu.SlugcatSelectMenu.SlugcatPageContinue).GetProperty(nameof(Menu.SlugcatSelectMenu.SlugcatPageContinue.HasMark)).GetGetMethod(), typeof(SlugcatCode).GetMethod(nameof(SlugcatCode.HasMark)));
                }

                if (!isInit)
                {
                    isInit = true;

                    Pom.Pom.RegisterManagedObject<Anchor, AnchorData, ManagedRepresentation>("AnchorSpot", "Stardust Famine");
                    Logger.LogMessage("Filter registered!!!!");

                    WorldLoader.Preprocessing.preprocessorConditions.Add(ConditionalsCode.StardustConditions);
                    Logger.LogMessage("Hooking success!!!");
                }
            }
            catch (Exception e)
            {
                Logger.LogMessage("Stardust Famine hooks failed!!!");
                Logger.LogError(e);
            }
        }

        

        public void OnDisable()
        {
            if (!isInit)
                return;
            isInit = false;
        }
        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            if (initialized)
            {
                return;
            }
            initialized = true;
            Futile.atlasManager.LoadImage("atlases/sfkarma10");
            Futile.atlasManager.LoadImage("atlases/sfkarma11");
            Futile.atlasManager.LoadImage("atlases/sfsmallkarma10");
            Futile.atlasManager.LoadImage("atlases/sfsmallkarma11");
            optionsMenuInstance = new OptionsMenu(this);
            try
            {
                MachineConnector.SetRegisteredOI("stardustFamine", optionsMenuInstance);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log($"Stardust Famine: Hook_OnModsInit options failed init error {optionsMenuInstance}{ex}");
                Logger.LogError(ex);
            }
        }
    }
}


