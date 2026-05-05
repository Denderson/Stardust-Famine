using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using Watcher;
using static Stardust.Plugin;
using Menu;
using Mono.Cecil.Cil;
using UnityEngine.Rendering;
using Stardust.Slugcats.Scholar.ThreadsSequence;
using Stardust.SaveFile;

namespace Stardust.Slugcats.Scholar.Permadeath
{
    public static class Permadeath
    {
        public const int maxCycles = 20;

        public static int CyclesRemaining(int currentCycle) => maxCycles - currentCycle;
        public static bool ShouldPermadie(int currentCycle) => CyclesRemaining(currentCycle) <= 0;

        public static void TextPromptCycleFix(On.HUD.TextPrompt.orig_Update orig, HUD.TextPrompt self)
        {
            orig(self);
            if (self?.hud?.owner != null && self.hud.owner is Player && (self.hud.owner as Player).room?.game?.StoryCharacter == Enums.SlugcatStatsName.sfscholar && !self.gameOverMode && self.cycleTick > -1)
                self.label.text = self.hud.rainWorld.inGameTranslator.Translate("Cycle") + " " + CyclesRemaining((self.hud.owner as Player).room.game.GetStorySession.saveState.cycleNumber);
        }

        public static void CycleLabelCycleFix(On.HUD.Map.CycleLabel.orig_UpdateCycleText orig, HUD.Map.CycleLabel self)
        {
            orig(self);
            var player = self.owner.hud.owner as Player;
            if (player.abstractCreature.world.game.StoryCharacter == Enums.SlugcatStatsName.sfscholar)
            {
                int cyclesRemaining = CyclesRemaining(player.abstractCreature.world.game.GetStorySession.saveState.cycleNumber);
                self.red = (cyclesRemaining <= 0) ? 1 : -1;
                self.label.text = self.owner.hud.rainWorld.inGameTranslator.Translate("Cycle") + $" {cyclesRemaining}";
            }
        }

        public static void PermadeathContinueScreen(On.Menu.SlugcatSelectMenu.SlugcatPageContinue.orig_ctor orig, SlugcatSelectMenu.SlugcatPageContinue self, Menu.Menu menu, MenuObject owner, int pageIndex, SlugcatStats.Name slugcatNumber)
        {
            orig(self, menu, owner, pageIndex, slugcatNumber);
            if (slugcatNumber == Enums.SlugcatStatsName.sfscholar)
            {
                if (menu.manager.rainWorld.progression.GetOrInitiateSaveState(Enums.SlugcatStatsName.sfscholar, null, menu.manager.menuSetup, false) is SaveState save && save.deathPersistentSaveData.ScholarPermadeath())
                {
                    Log.LogMessage("Log 2");
                    var hud = self.hud;
                    List<FNode> thingsToNotRender = [hud.karmaMeter.darkFade, hud.karmaMeter.karmaSprite, hud.karmaMeter.glowSprite, hud.foodMeter.darkFade, hud.foodMeter.lineSprite];

                    hud.foodMeter.circles.ForEach(circle =>
                    {
                        thingsToNotRender.Add(circle.gradient);
                        thingsToNotRender.Add(circle.circles[0].sprite);
                        thingsToNotRender.Add(circle.circles[1].sprite);
                    });
                    thingsToNotRender.ForEach(thingNotToRender => hud.fContainers[1].RemoveChild(thingNotToRender));
                }
                else if (self.saveGameData.shelterName != null && self.saveGameData.shelterName.Length > 2)
                {
                    string text = Region.GetRegionFullName(self.saveGameData.shelterName.Substring(0, self.saveGameData.shelterName.IndexOf("_")), slugcatNumber);
                    if (text.Length > 0)
                    {
                        text = menu.Translate(text);
                        text = text + " - " + menu.Translate("Cycle") + $" {self.saveGameData.cycle}";
                        self.regionLabel.text = text;
                    }
                }
            }
        }

        public static void SupernovaMenuScene(On.Menu.MenuScene.orig_BuildScene orig, MenuScene self)
        {
            orig(self);
            if (self.sceneID == Enums.MenuSceneIDs.threadsScene)
            {
                self.sceneFolder = $"Scenes{Path.DirectorySeparatorChar}ScholarSupernova";
                if (true) //self.flatMode)
                {
                    self.AddIllustration(new MenuIllustration(self.menu, self, self.sceneFolder, "ScholarSupernova - Flat", new Vector2(683f, 384f), crispPixels: false, anchorCenter: true));
                }
                else
                {
                    // TODO: the actual scene
                }
            }
        }
        public static void PermadeathStartButton(On.Menu.SlugcatSelectMenu.orig_UpdateStartButtonText orig, SlugcatSelectMenu self)
        {
            orig(self);
            if (self.slugcatPages[self.slugcatPageIndex].slugcatNumber == Enums.SlugcatStatsName.sfscholar && self.manager.rainWorld.progression.GetOrInitiateSaveState(Enums.SlugcatStatsName.sfscholar, null, self.manager.menuSetup, false) is SaveState save && save.deathPersistentSaveData.ScholarPermadeath())
            {
                self.startButton.menuLabel.text = self.Translate("CHOOSE THREAD").Replace(" ", "\r\n");
            }
        }

        public static void GoToThreadsScreenFromMainMenu(On.Menu.SlugcatSelectMenu.orig_ContinueStartedGame orig, SlugcatSelectMenu self, SlugcatStats.Name storyGameCharacter)
        {
            if (storyGameCharacter == Enums.SlugcatStatsName.sfscholar)
            {
                bool threadsProcessDueToPermadeath = self.manager.rainWorld.progression.GetOrInitiateSaveState(Enums.SlugcatStatsName.sfscholar, null, self.manager.menuSetup, false) is SaveState save && save.deathPersistentSaveData.ScholarPermadeath();
                ThreadsCheckbox checkbox = self.pages[0].subObjects.OfType<ThreadsCheckbox>().FirstOrDefault();
                bool threadsProcessDueToCheckbox = checkbox != null && checkbox.Checked && self.manager.rainWorld.progression.IsThereASavedGame(storyGameCharacter);
                if (threadsProcessDueToCheckbox || threadsProcessDueToPermadeath)
                {
                    self.manager.RequestMainProcessSwitch(Enums.ProcessIDs.threadsProcess);
                    self.PlaySound(SoundID.MENU_Switch_Page_Out);
                    return;
                }
            }
            orig(self, storyGameCharacter);
        }

        public static void AddThreadsCheckbox(On.Menu.SlugcatSelectMenu.orig_ctor orig, SlugcatSelectMenu self, ProcessManager manager)
        {
            orig(self, manager);
            self.pages[0].subObjects.Add(new ThreadsCheckbox(self, self.startButton.pos.x - 200f - SlugcatSelectMenu.GetRestartTextOffset(self.CurrLang)));
        }

        public static void GoToThreadsScreenFromStartScreen(On.Menu.SlugcatSelectMenu.orig_StartGame orig, SlugcatSelectMenu self, SlugcatStats.Name storyGameCharacter)
        {
            if (storyGameCharacter == Enums.SlugcatStatsName.sfscholar)
            {
                bool threadsProcessDueToPermadeath = self.manager.rainWorld.progression.GetOrInitiateSaveState(Enums.SlugcatStatsName.sfscholar, null, self.manager.menuSetup, false) is SaveState save && save.deathPersistentSaveData.ScholarPermadeath();
                ThreadsCheckbox checkbox = self.pages[0].subObjects.OfType<ThreadsCheckbox>().FirstOrDefault();
                bool threadsProcessDueToCheckbox = checkbox != null && checkbox.Checked && self.manager.rainWorld.progression.IsThereASavedGame(storyGameCharacter);
                if (threadsProcessDueToCheckbox || threadsProcessDueToPermadeath)
                {
                    self.manager.RequestMainProcessSwitch(Enums.ProcessIDs.threadsProcess);
                    self.PlaySound(SoundID.MENU_Switch_Page_Out);
                    return;
                }
            }
            orig(self, storyGameCharacter);
        }

        public static void SwitchToThreadsScreen(On.ProcessManager.orig_PostSwitchMainProcess orig, ProcessManager self, ProcessManager.ProcessID ID)
        {
            if (ID == Enums.ProcessIDs.threadsProcess)
                self.currentMainLoop = new ThreadsScreen(self);
            orig(self, ID);
        }

        public static void CheckForPermadeath(On.RainWorldGame.orig_GameOver orig, RainWorldGame self, Creature.Grasp dependentOnGrasp)
        {
            if (self.IsStorySession && self.GetStorySession.saveStateNumber == Enums.SlugcatStatsName.sfscholar)
            {
                bool causePermadeath = (self.session as StoryGameSession).saveState.cycleNumber > 5;
                // just a placeholder
                if (causePermadeath)
                {
                    self.GoToRedsGameOver();
                    return;
                }
            }
            orig(self, dependentOnGrasp);
        }

        public static void ScholarPermadeathTrigger(On.RainWorldGame.orig_GoToRedsGameOver orig, RainWorldGame self)
        {
            if (self?.StoryCharacter != Enums.SlugcatStatsName.sfscholar)
            {
                orig(self);
                return;
            }
            if (self.manager.upcomingProcess == null)
            {
                if (self.manager.musicPlayer != null)
                    self.manager.musicPlayer.FadeOutAllSongs(20f);
                self.GetStorySession.saveState.deathPersistentSaveData.Set<bool>(SaveFileMain.scholarPermadeath, true);
                self.manager.RequestMainProcessSwitch(Enums.ProcessIDs.threadsProcess, 5f);
            }
        }


        public static void SleepAndDeathScreen_GetDataFromGame(ILContext il)
        {
            // code by VN, with permission
            var c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.After, x => x.MatchLdsfld<ModManager>(nameof(ModManager.MMF))))
            {
                c.Emit(OpCodes.Ldarg_1);
                c.EmitDelegate<Func<bool, KarmaLadderScreen.SleepDeathScreenDataPackage, bool>>(static (orig, package) => orig && package.characterStats.name != Enums.SlugcatStatsName.sfscholar);
            }
            if (c.TryGotoNext(MoveType.After, q => q.MatchCall("ExtEnum`1<SlugcatStats/Name>", "op_Inequality")))
            {
                c.Emit(OpCodes.Ldarg_1);
                c.EmitDelegate<Func<bool, KarmaLadderScreen.SleepDeathScreenDataPackage, bool>>(static (orig, package) => orig && package.characterStats.name != Enums.SlugcatStatsName.sfscholar);
            }
        }
    }
}
