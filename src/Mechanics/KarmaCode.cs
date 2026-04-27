using BepInEx;
using BepInEx.Logging;
using Fisobs.Core;
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
using Stardust.SaveFile;
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
using static Stardust.Plugin;

namespace Stardust
{
    public static class KarmaCode
    {
        public static bool Outside_Watcher(Func<Player, bool> orig, Player self)
        {
            return orig(self) || (SharedMechanics(self?.slugcatStats?.name));
        }

        public static float Ripple_Level(Func<Player, float> orig, Player self)
        {
            if (SharedMechanics(self?.room?.game?.StoryCharacter))
            {
                if (self.room.game.GetStorySession.saveState.deathPersistentSaveData.GetBool(SaveFileMain.rippleSequenceDone)) return 5f;
            }
            return orig(self);
        }

        public static void KarmaMeter_UpdateGraphic_int_int(On.HUD.KarmaMeter.orig_UpdateGraphic_int_int orig, HUD.KarmaMeter self, int karma, int cap)
        {
            orig(self, karma, cap);
            if (self.hud.owner is Player player && SharedMechanics(player?.room?.game?.StoryCharacter))
            {
                if (player.maxRippleLevel < 1f)
                {
                    if (karma > 9)
                    {
                        self.karmaSprite.element = Futile.atlasManager.GetElementWithName("atlases/sfsmallkarma" + karma);
                    }
                    self.baseColor = Color.Lerp(Color.white, RainWorld.SaturatedGold, player.room.game.GetStorySession.saveState.EchoEncounters() * 0.07f);
                }
                self.karmaSprite.color = self.baseColor;
                self.glowSprite.color = self.baseColor;
            }
            
        }

        public static void KarmaMeter_UpdateGraphic(On.HUD.KarmaMeter.orig_UpdateGraphic orig, HUD.KarmaMeter self)
        {
            orig(self);
            if (self.hud.owner is Player player && SharedMechanics(player?.room?.game?.StoryCharacter))
            {
                if (player.maxRippleLevel < 1f)
                {
                    if (self.displayKarma.x > 9)
                    {
                        self.karmaSprite.element = Futile.atlasManager.GetElementWithName("atlases/sfsmallkarma" + self.displayKarma.x);
                    }
                    self.baseColor = Color.Lerp(Color.white, RainWorld.SaturatedGold, player.room.game.GetStorySession.saveState.EchoEncounters() * 0.07f);
                }
                self.karmaSprite.color = self.baseColor;
                self.glowSprite.color = self.baseColor;
            }

        }

        public static void KarmaMeter_ctor(On.HUD.KarmaMeter.orig_ctor orig, HUD.KarmaMeter self, HUD.HUD hud, FContainer fContainer, IntVector2 displayKarma, bool showAsReinforced)
        {
            orig(self, hud, fContainer, displayKarma, showAsReinforced);
            if (hud.owner is Player player && SharedMechanics(player?.room?.game?.StoryCharacter))
            {
                if (player.maxRippleLevel < 1f)
                {
                    if (self.displayKarma.x > 9)
                    {
                        self.karmaSprite.element = Futile.atlasManager.GetElementWithName("atlases/sfsmallkarma" + self.displayKarma.x);
                    }
                    self.baseColor = Color.Lerp(Color.white, RainWorld.SaturatedGold, player.room.game.GetStorySession.saveState.EchoEncounters() * 0.07f);
                }
                self.karmaSprite.color = self.baseColor;
                self.glowSprite.color = self.baseColor;
            }
            
        }

        public static void KarmaSymbol_GrafUpdate(On.Menu.KarmaLadder.KarmaSymbol.orig_GrafUpdate orig, Menu.KarmaLadder.KarmaSymbol self, float timeStacker)
        {
            orig(self, timeStacker);
            if (self?.parent is not null && self.parent is Menu.KarmaLadder && (self.parent as Menu.KarmaLadder).menu is Menu.KarmaLadderScreen karmaLadderScreen && SharedMechanics((karmaLadderScreen.saveState?.saveStateNumber)))
            {
                if (!self.rippleMode)
                {
                    float num6 = Mathf.Clamp(Mathf.Abs((float)self.displayKarma.x - Mathf.Lerp(self.ladder.lastScroll, self.ladder.scroll, timeStacker)) / 0.75f, 0f, 1f);
                    float num12 = Mathf.Lerp(self.lastEnergy, self.energy, timeStacker);
                    Color color2 = Color.Lerp(Color.black, Color.Lerp(Color.Lerp(Color.white, RainWorld.SaturatedGold, karmaLadderScreen.saveState.EchoEncounters() * 0.07f), Menu.Menu.MenuColor(Menu.Menu.MenuColors.DarkGrey).rgb, num6), num12);
                    self.sprites[self.KarmaSprite].color = color2;
                    self.sprites[self.RingSprite].color = color2;
                }
            }
        }

        public static void KarmaSymbol_UpdateDisplayKarma(On.Menu.KarmaLadder.KarmaSymbol.orig_UpdateDisplayKarma orig, Menu.KarmaLadder.KarmaSymbol self, IntVector2 dpKarma)
        {
            orig(self, dpKarma);
            if (self?.parent is not null && self.parent is Menu.KarmaLadder && (self.parent as Menu.KarmaLadder).menu is Menu.KarmaLadderScreen karmaLadderScreen && SharedMechanics((karmaLadderScreen.saveState?.saveStateNumber)))
            {
                if (!self.rippleMode && self.displayKarma.x > 9)
                {
                    self.sprites[self.KarmaSprite].element = Futile.atlasManager.GetElementWithName("atlases/sfkarma" + self.displayKarma.x);
                }
            }
        }

        public static void KarmaSymbol_ctor(On.Menu.KarmaLadder.KarmaSymbol.orig_ctor orig, Menu.KarmaLadder.KarmaSymbol self, Menu.Menu menu, Menu.MenuObject owner, Vector2 pos, FContainer container, FContainer foregroundContainer, IntVector2 displayKarma, bool rippleMode)
        {
            orig(self, menu, owner, pos, container, foregroundContainer, displayKarma, rippleMode);
            if (self?.parent is not null && self.parent is Menu.KarmaLadder && (self.parent as Menu.KarmaLadder).menu is Menu.KarmaLadderScreen karmaLadderScreen && SharedMechanics((karmaLadderScreen.saveState?.saveStateNumber)))
            {
                if (!self.rippleMode && self.displayKarma.x > 9)
                {
                    self.sprites[self.KarmaSprite].element = Futile.atlasManager.GetElementWithName("atlases/sfkarma" + self.displayKarma.x);
                }
            }
        }

        public static void GetTriggersAfterEcho(On.SaveState.orig_GhostEncounter orig, SaveState self, GhostWorldPresence.GhostID ghost, RainWorld rainWorld)
        {
            orig(self, ghost, rainWorld);
            Log.LogMessage("Ghost encounter reached");
            if (SharedMechanics(self.saveStateNumber))
            {
                int i = 0;
                foreach (KeyValuePair<GhostWorldPresence.GhostID, int> item in self.deathPersistentSaveData.ghostsTalkedTo)
                {
                    if (item.Value > 1)
                    {
                        i++;
                    }
                }
                Log.LogMessage("no of Echo: " + i);
                if (i == 0)
                {
                    return;
                }
                Log.LogMessage("Echo met");
                self.deathPersistentSaveData.karmaCap = i + 5;
                self.theGlow = true;
                Debug.Log("Current karma: " + self.deathPersistentSaveData.karmaCap);
                if (self.saveStateNumber == Enums.SlugcatStatsName.sfscholar)
                {
                    SaveState saveCopy = self;
                    self.SetBackupSave(ref saveCopy, Math.Max(i - 1, 0));
                }
                rainWorld.progression.SaveProgressionAndDeathPersistentDataOfCurrentState(saveAsDeath: false, saveAsQuit: false);
            }
        }

        public static void DeathPersistentSaveData_SaveToString(ILContext il)
        {
            ILCursor c = new(il);
            ILLabel skipSubtract = c.DefineLabel();

            // code by __nv_ on discord, used with permission
            if (c.TryGotoNext(MoveType.After, x => x.MatchLdstr("REINFORCEDKARMA<dpB>0<dpA>"))
                && c.TryGotoNext(MoveType.Before, x => x.MatchLdcI4(1)))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Predicate<DeathPersistentSaveData>>((DeathPersistentSaveData saveData) =>
                {
                    return saveData.karma <= saveData.MinKarma();
                });
                c.Emit(OpCodes.Brtrue_S, skipSubtract);
                c.Index += 2;
                c.MarkLabel(skipSubtract);
            }
            else
            {
                Log.LogMessage("failed to find karma decrease in saving data. The karma will go down if you die on minimum karma");
            }
        }

        public static void ChangeDestinationKarma(On.Menu.KarmaLadder.orig_GoToKarma orig, Menu.KarmaLadder self, int newGoalKarma, bool displayMetersOnRest)
        {
            if (self.menu is Menu.KarmaLadderScreen && newGoalKarma <= (self.menu as Menu.KarmaLadderScreen).MinKarma())
            {
                self.cappedMovement = true;
                newGoalKarma = (self.menu as Menu.KarmaLadderScreen).MinKarma();
            }
            orig(self, newGoalKarma, displayMetersOnRest);
        }


        public static void KarmaLadder_ctor_Menu_MenuObject_Vector2_HUD_IntVector2_bool(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (!c.TryGotoNext(
                MoveType.Before,
                instr => instr.MatchLdcI4(0),
                instr => instr.MatchStloc(out _)))
            {
                Debug.Log("KarmaLadder IL hook failed");
                return;
            }

            c.Remove();

            c.Emit(OpCodes.Ldarg_1);

            c.Emit(OpCodes.Call,
                il.Import(
                    typeof(SaveFileMain).GetMethod(
                        "MinKarma",
                        new[] { typeof(Menu.Menu) }
                    )
                )
            );
        }
    }
}
