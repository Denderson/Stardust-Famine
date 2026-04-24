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

namespace Stardust.Mechanics
{
    public static class GateCode
    {
        public static readonly Color exhaustedGateColor = new Color(1f, 0f, 0f);

        public static bool No_Energy(Func<GateKarmaGlyph, bool> orig, GateKarmaGlyph self)
        {
            return orig(self) || (self?.gate != null && CWTs.RegionGateCWT.TryGetData(self.gate, out var data) && data.exhausted);
        }
        public static void RainWorldGame_Win(On.RainWorldGame.orig_Win orig, RainWorldGame self, bool malnourished, bool fromWarpPoint)
        {
            if (self.manager.upcomingProcess != null)
            {
                orig(self, malnourished, fromWarpPoint);
                return;
            }
            if (!malnourished && self.IsStorySession)
            {
                string gates = self.GetStorySession.saveState.GetString(SaveFileCode.gates);
                if (!string.IsNullOrEmpty(gates))
                {
                    Log.LogMessage("Before change: " + gates);
                    string newGates = string.Empty;

                    string[] arrayGates;
                    if (gates != null && gates.Length > 2 && gates.Contains('+'))
                    {
                        arrayGates = gates.Split('+');
                    }
                    else arrayGates = [gates];
                    for (int i = 0; i < arrayGates.Length; i++)
                    {
                        string newValue = arrayGates[i];
                        if (newValue != null && newValue.Length > 1 && newValue.Contains('/') && newValue.Split('/').Length > 1 && Int32.TryParse(newValue.Split('/')[1], out int cyclesUntilOpen))
                        {
                            cyclesUntilOpen--;
                            if (cyclesUntilOpen > 0)
                            {
                                if (newGates != string.Empty) newGates += "+";
                                newGates += newValue.Split('/')[0] + "/" + cyclesUntilOpen;
                            }
                        }
                    }
                    Log.LogMessage("After change: " + newGates);
                    self.GetStorySession.saveState.SetString(SaveFileCode.gates, newGates);
                }
            }
            orig(self, malnourished, fromWarpPoint);

        }

        public static void RegionGate_Unlock(On.RegionGate.orig_Unlock orig, RegionGate self)
        {
            if (SharedMechanics(self.room?.game?.StoryCharacter))
            {
                Log.LogMessage("Unlocking region gate + " + self.room.abstractRoom.name);
                if (self.unlocked)
                {
                    return;
                }
                self.unlocked = true;
                self.room.LockGate();
            }
            orig(self);
        }

        public static bool DeathPersistentSaveData_CanUseUnlockedGates(On.DeathPersistentSaveData.orig_CanUseUnlockedGates orig, DeathPersistentSaveData self, SlugcatStats.Name slugcat)
        {
            return orig(self, slugcat) || (SharedMechanics(slugcat));
        }

        public static bool Meet_Requirement(Func<RegionGate, bool> orig, RegionGate self)
        {
            if (SharedMechanics(self?.room?.game?.StoryCharacter))
            {
                if (CWTs.RegionGateCWT.TryGetData(self, out var data) && data.exhausted)
                {
                    return false;
                }
                if (self.room.game.GetStorySession.saveState.deathPersistentSaveData.GetBool(SaveFileCode.rippleDone))
                {
                    return false;
                }
            }
            return orig(self);
        }

        public static void GateKarmaGlyph_Update(On.GateKarmaGlyph.orig_Update orig, GateKarmaGlyph self, bool eu)
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

        public static void ExhaustGates(On.RegionGate.orig_ctor orig, RegionGate self, Room room)
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
                foreach (GateKarmaGlyph gateKarmaGlyph in self.karmaGlyphs)
                {
                    gateKarmaGlyph.myDefaultColor = exhaustedGateColor;
                }
            }
        }

        public static bool NoBlinkingKarmaOnExhaustedGates(On.RegionGate.orig_KarmaBlinkRed orig, RegionGate self)
        {
            if (CWTs.RegionGateCWT.TryGetData(self, out var data) && data.exhausted)
            {
                return false;
            }
            return orig(self);
        }
    }
}
