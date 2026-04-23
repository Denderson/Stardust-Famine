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
using Stardust.Mechanics;
using Stardust.Slugcats;
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
        public readonly Color exhaustedGateColor = new Color(1f, 0f, 0f);
        public static ManualLogSource Log { get; private set; }

        public static int random;
        public static bool checkFailed;

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

        public static void ResetValues()
        {
            random = RXRandom.Int(100);
        }

        public void OnEnable()
        {
            UnityEngine.Debug.Log("Starting SF");
            try
            {
                Log = base.Logger;
                Logger.LogInfo(Log);

                ResetValues();

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
                    On.Menu.SlugcatSelectMenu.SlugcatPageNewGame.ctor += SlugcatCode.SlugcatPageNewGame_ctor;
                    On.Menu.SlugcatSelectMenu.ContinueStartedGame += Permadeath.GoToThreadsScreenFromMainMenu;
                    On.Menu.SlugcatSelectMenu.StartGame += Permadeath.GoToThreadsScreenFromStartScreen;
                    On.Menu.SlugcatSelectMenu.UpdateStartButtonText += Permadeath.PermadeathStartButton;
                    On.Menu.SlugcatSelectMenu.SlugcatPage.AddImage += SlugcatCode.SlugcatPage_AddImage;
                }

                // bitter code
                {
                    On.Creature.Grasp.ctor += BitterCode.BitterGraspImmunity;
                    On.LocustSystem.Swarm.TryAttach += BitterCode.BitterLocustImmunity;
                    On.SlugcatStats.ctor += BitterCode.OldFoodMeterCode; // NEED CHANGE
                    On.Player.DeathByBiteMultiplier += BitterCode.BitterBiteResistance; // NEEDS CHANGE

                    //On.HUD.FoodMeter.ctor += EmergencyFoodMeter.FoodMeter_ctor;
                }

                // scholar code
                {

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
                }

                // save file code
                {

                }

                // gate code
                {
                    On.DeathPersistentSaveData.CanUseUnlockedGates += GateCode.DeathPersistentSaveData_CanUseUnlockedGates;
                    On.RegionGate.Unlock += GateCode.RegionGate_Unlock;
                    On.RainWorldGame.Win += GateCode.RainWorldGame_Win;
                }

                On.Player.Update += SlugcatCode.Player_Update;
                

                // scav code
                {
                    
                }

                // misc
                {
                    On.LocustSystem.SwarmScore_Creature += LocustSystem_SwarmScore_Creature;
                }
                

                // il hooks
                {
                    Log.LogMessage("IL hook hell starting");
                    IL.Menu.KarmaLadder.ctor_Menu_MenuObject_Vector2_HUD_IntVector2_bool += KarmaCode.KarmaLadder_ctor_Menu_MenuObject_Vector2_HUD_IntVector2_bool;
                    IL.DeathPersistentSaveData.SaveToString += KarmaCode.DeathPersistentSaveData_SaveToString;
                }

                

                On.Menu.SlugcatSelectMenu.SlugcatPageContinue.ctor += Permadeath.PermadeathContinueScreen;

                On.Menu.SlugcatSelectMenu.ctor += Permadeath.AddThreadsCheckbox;
                //On.Menu.MenuScene.BuildScene += MenuScene_BuildScene;

                On.Ghost.FadeOutFinished += GhostCode.CheckpointAfterEcho;

                On.SlugcatStats.SlugcatUnlocked += SlugcatCode.LockScholar;
                On.SlugcatStats.SpearSpawnElectricRandomChance_Timeline += SlugcatCode.SlugcatStats_SpearSpawnElectricRandomChance_Timeline;
                On.SlugcatStats.SpearSpawnExplosiveRandomChance_Timeline += SlugcatCode.SlugcatStats_SpearSpawnExplosiveRandomChance_Timeline;
                On.SlugcatStats.SpearSpawnModifier_Timeline_float += SlugcatCode.SlugcatStats_SpearSpawnModifier_Timeline_float;
                On.SlugcatStats.PearlsGivePassageProgress += SlugcatCode.NoScholarPassage;

                On.ProcessManager.PostSwitchMainProcess += Permadeath.SwitchToThreadsScreen;

                On.Menu.SleepAndDeathScreen.AddPassageButton += SlugcatCode.NoPassageButton;
                IL.Menu.SleepAndDeathScreen.GetDataFromGame += Permadeath.SleepAndDeathScreen_GetDataFromGame;

                On.RainWorldGame.GoToRedsGameOver += Permadeath.ScholarPermadeathTrigger;
                On.RainWorldGame.GameOver += Permadeath.CheckForPermadeath;

                On.SaveState.LoadGame += SaveFileCode.CustomSavedataInit;

                On.RegionGate.KarmaBlinkRed += NoBlinkingKarmaOnExhaustedGates;
                On.RegionGate.ctor += ExhaustGates;

                On.GateKarmaGlyph.Update += GateKarmaGlyph_Update;

                On.HUD.TextPrompt.Update += Permadeath.TextPromptCycleFix;

                On.HUD.Map.CycleLabel.UpdateCycleText += Permadeath.CycleLabelCycleFix;

                On.Menu.MenuScene.BuildScene += MenuScene_BuildScene;

                On.World.LoadWorldForFastTravel_Timeline_List1_Int32Array_Int32Array_Int32Array += World_LoadWorldForFastTravel_Timeline_List1_Int32Array_Int32Array_Int32Array;


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

        private void World_LoadWorldForFastTravel_Timeline_List1_Int32Array_Int32Array_Int32Array(On.World.orig_LoadWorldForFastTravel_Timeline_List1_Int32Array_Int32Array_Int32Array orig, World self, SlugcatStats.Timeline timelinePosition, List<AbstractRoom> abstractRoomsList, int[] swarmRooms, int[] shelters, int[] gates)
        {
            orig(self, timelinePosition, abstractRoomsList, swarmRooms, shelters, gates);
            if (self?.game != null && !self.singleRoomWorld && self.game.session is StoryGameSession && SharedMechanics(self.game.GetStorySession.saveStateNumber)) // you can switch to simply Bitter if we decide to exclude them from Scholar later on
            {
                self.SpawnAnchor();
            }
        }

        public static void MenuScene_BuildScene(On.Menu.MenuScene.orig_BuildScene orig, MenuScene self)
        {
            orig(self);
            if (self.owner is SlugcatSelectMenu.SlugcatPageContinue page && SharedMechanics(page.slugcatNumber))
            {
                SaveState save = Custom.rainWorld.progression.GetOrInitiateSaveState(Enums.SlugcatStatsName.sfscholar, null, self.menu.manager.menuSetup, false);
                if (page.slugcatNumber == Enums.SlugcatStatsName.bitter)
                {
                    if (save.Ripple()) self.sceneID = Enums.MenuSceneIDs.bitterRipple;
                    if (save.HalfwayEchoes()) self.sceneID = Enums.MenuSceneIDs.bitterHalfway;
                    if (save.EchoEncounters() > 0) self.sceneID = Enums.MenuSceneIDs.bitterEcho;
                }
                if (page.slugcatNumber == Enums.SlugcatStatsName.sfscholar)
                {
                    if (save.Ripple()) self.sceneID = Enums.MenuSceneIDs.scholarRipple;
                }
            }
        }

        private void GateKarmaGlyph_Update(On.GateKarmaGlyph.orig_Update orig, GateKarmaGlyph self, bool eu)
        {
            orig(self, eu);
            if (self?.gate != null && CWTs.RegionGateCWT.TryGetData(self.gate, out var data) && data.exhausted)
            {
                self.flicker = Mathf.Max(self.flicker, 0.5f);
                if (UnityEngine.Random.value < 0.02f)
                {
                    self.flicker = Mathf.Max(UnityEngine.Random.value, 0.5f);
                }
            }
        }

        private void ExhaustGates(On.RegionGate.orig_ctor orig, RegionGate self, Room room)
        {
            orig(self, room);
            if (SharedMechanics(room?.game?.StoryCharacter) && room.IsGateLocked())
            {
                if (!CWTs.RegionGateCWT.TryGetData(self, out var data))
                {
                    Log.LogMessage("ERROR: Gate exhausted but cannot use CWT!");
                    return;
                }
                data.exhausted = true;
                foreach(GateKarmaGlyph gateKarmaGlyph in self.karmaGlyphs)
                {
                    gateKarmaGlyph.myDefaultColor = exhaustedGateColor;
                }
            }
        }

        private bool NoBlinkingKarmaOnExhaustedGates(On.RegionGate.orig_KarmaBlinkRed orig, RegionGate self)
        {
            if (CWTs.RegionGateCWT.TryGetData(self, out var data) && data.exhausted)
            {
                return false;
            }
            return orig(self);
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

        public static float LocustSystem_SwarmScore_Creature(On.LocustSystem.orig_SwarmScore_Creature orig, LocustSystem self, Creature crit)
        {
            if (crit != null && crit.Submersion > 0.5)
            {
                return 0f;
            }
            return orig(self, crit);
        }
    }


    
    public class OptionsMenu : OptionInterface
    {
        public OpCheckBox opCheckBox (Configurable<bool> config, int x, int y, bool isUnfinished = false)
        {
            OpCheckBox checkBox = new OpCheckBox(config, x * 160, 503 - y * 80) { description = config.info.description };
            if ( isUnfinished ) checkBox.colorEdge = new Color(0.85f, 0.35f, 0.4f);
            return checkBox;
        }

        public OpLabel opLabel (string text, float x, float y, bool isUnfinished = false)
        {
            OpLabel label = new OpLabel(x * 160 + 30, 500 - y * 80, text);
            if (isUnfinished) label.color = new Color(0.85f, 0.35f, 0.4f);
            return label;
        }

        public OpLabel opBigLabel(string text, float y, bool isUnfinished = false)
        {
            OpLabel label = new OpLabel(410, 480 - y * 80, text, true);
            if (isUnfinished) label.color = new Color(0.85f, 0.35f, 0.4f);
            return label;
        }

        public OpLabel opSliderLabel (string text, int y, bool isUnfinished = false)
        {
            OpLabel label = new OpLabel(110, 460 - y * 80, text) { description = text };
            if (isUnfinished) label.color = new Color(0.85f, 0.35f, 0.4f);
            return label;
        }
        public OptionsMenu(Plugin plugin)
        {
            unlockScholar = config.Bind("stardustfamine_unlockScholar", false, new ConfigurableInfo("Unlocks Scholar"));
            scholarSeenPermadeath = config.Bind("stardustfamine_scholarSeenPermadeath", false, new ConfigurableInfo("Scholar seen permadeath"));
        }

        public override void Initialize()
        {

            base.Initialize();

            Color unfinishedColor = new Color(0.85f, 0.35f, 0.4f);


            this.Tabs = new[] { new OpTab(this, "General options"), new OpTab(this, "Mechanics 1") };

            // Tab 1
            UIelement[] UIArrayElements = new UIelement[]
            {
                new OpLabel(0, 550, "General options", true), new OpLabel(160, 550, "(red means not implemeted yet)", true){color = unfinishedColor}
            };
            Tabs[0].AddItems(UIArrayElements);


            // Tab 2
            UIArrayElements = new UIelement[]
            {
                new OpLabel(0, 550, "Mechanics", true), new OpLabel(110, 550, "(red means not implemented yet)", true){color = unfinishedColor},

                opLabel("Unlock Scholar", 0, 0),
                opCheckBox(unlockScholar, 0, 0),

                opLabel("Seen Scholar permadeath", 0, 1),
                opCheckBox(scholarSeenPermadeath, 0, 1)
            };
            Tabs[1].AddItems(UIArrayElements);
        }

        public static Configurable<bool> unlockScholar, scholarSeenPermadeath;
    }
}


