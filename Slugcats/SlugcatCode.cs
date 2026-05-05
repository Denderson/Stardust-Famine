using Menu;
using RWCustom;
using Stardust.SaveFile;
using Stardust.Slugcats.Scholar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Stardust.Plugin;

namespace Stardust.Slugcats
{
    public static class SlugcatCode
    {
        public static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);
            if (self == null || self.inShortcut)
            {
                return;
            }
            if (!CWTs.PlayerCWT.TryGetData(self, out var data)) return;
            if (self.SlugCatClass == Enums.SlugcatStatsName.bitter)
            {
                self.dangerGraspTime = 0;
                if (self.dangerGrasp != null)
                {
                    self.standing = false;
                }
            }
            if (self.SlugCatClass == Enums.SlugcatStatsName.sfscholar)
            {
                if (data.teleportCooldown > 0)
                {
                    data.teleportCooldown--;
                }
                else if (self.Consious && self.input[0].spec)
                {
                    data.teleportCooldown = 40;
                    ScholarCode.Teleport(self);
                }
            }
        }

        public static void SlugcatPage_AddImage(On.Menu.SlugcatSelectMenu.SlugcatPage.orig_AddImage orig, SlugcatSelectMenu.SlugcatPage self, bool ascended)
        {
            orig(self, ascended);

        }

        public static bool HasMark(Func<Menu.SlugcatSelectMenu.SlugcatPageContinue, bool> orig, Menu.SlugcatSelectMenu.SlugcatPageContinue self)
        {
            bool value = orig(self);
            if (SharedMechanics(self.slugcatNumber))
            {
                return false;
            }
            return value;
        }

        public static bool HasGlow(Func<Menu.SlugcatSelectMenu.SlugcatPageContinue, bool> orig, Menu.SlugcatSelectMenu.SlugcatPageContinue self)
        {
            bool value = orig(self);
            if (self?.slugcatNumber == Enums.SlugcatStatsName.sfscholar && self.menu.manager.rainWorld.progression.GetOrInitiateSaveState(Enums.SlugcatStatsName.sfscholar, null, self.menu.manager.menuSetup, false) is SaveState save && save.deathPersistentSaveData.ScholarPermadeath())
            {
                return false;
            }
            return value;
        }

        public static bool NoScholarPassage(On.SlugcatStats.orig_PearlsGivePassageProgress orig, StoryGameSession session)
        {
            bool value = orig(session);
            if (SharedMechanics(session.saveStateNumber))
            {
                return false;
            }
            return value;
        }

        public static float SlugcatStats_SpearSpawnModifier_Timeline_float(On.SlugcatStats.orig_SpearSpawnModifier_Timeline_float orig, SlugcatStats.Timeline index, float originalSpearChance)
        {
            float value = orig(index, originalSpearChance);
            if (SharedMechanics(index))
            {
                return Mathf.Pow(originalSpearChance, 0.9f);
            }
            return value;
        }

        public static float SlugcatStats_SpearSpawnExplosiveRandomChance_Timeline(On.SlugcatStats.orig_SpearSpawnExplosiveRandomChance_Timeline orig, SlugcatStats.Timeline index)
        {
            float value = orig(index);
            if (SharedMechanics(index))
            {
                return 0.02f;
            }
            return value;
        }

        public static float SlugcatStats_SpearSpawnElectricRandomChance_Timeline(On.SlugcatStats.orig_SpearSpawnElectricRandomChance_Timeline orig, SlugcatStats.Timeline index)
        {
            float value = orig(index);
            if (SharedMechanics(index))
            {
                return 0.02f;
            }
            return value;
        }

        public static bool LockScholar(On.SlugcatStats.orig_SlugcatUnlocked orig, SlugcatStats.Name i, RainWorld rainWorld)
        {
            bool value = orig(i, rainWorld);
            if (i == Enums.SlugcatStatsName.sfscholar)
            {
                return OptionsMenu.unlockScholar.Value;
            }
            return value;
        }

        public static void SlugcatPageNewGame_ctor(On.Menu.SlugcatSelectMenu.SlugcatPageNewGame.orig_ctor orig, SlugcatSelectMenu.SlugcatPageNewGame self, Menu.Menu menu, MenuObject owner, int pageIndex, SlugcatStats.Name slugcatNumber)
        {
            orig(self, menu, owner, pageIndex, slugcatNumber);
            if (slugcatNumber == Enums.SlugcatStatsName.sfscholar)
            {
                string s = (menu as SlugcatSelectMenu).SlugcatUnlocked(slugcatNumber) ? "Scholar desc goes here" : "Clear the game as Bitter to unlock.";
                if (SlugBase.SlugBaseCharacter.TryGet(slugcatNumber, out var character))
                {
                    character.Description = s;
                }
                orig(self, menu, owner, pageIndex, slugcatNumber);
            }
        }

        public static void NoPassageButton(On.Menu.SleepAndDeathScreen.orig_AddPassageButton orig, SleepAndDeathScreen self, bool buttonBlack)
        {
            if (SharedMechanics(self?.saveState?.saveStateNumber)) return;
            orig(self, buttonBlack);
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
    }
}
