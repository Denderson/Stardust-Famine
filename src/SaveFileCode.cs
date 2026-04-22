using BepInEx;
using BepInEx.Logging;
using Fisobs.Core;
using Menu;
using Menu.Remix.MixedUI;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MoreSlugcats;
using Music;
using Newtonsoft.Json.Linq;
using RWCustom;
using SlugBase;
using SlugBase.Features;
using SlugBase.SaveData;
using Stardust.Anchors;
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
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Playables;
using Watcher;
using static SlugBase.Features.FeatureTypes;
using static SlugBase.SaveData.SlugBaseSaveData;
using static Stardust.Plugin;

namespace Stardust
{
    public static class SaveFileCode
    {

        public const int maxEchoes = 6;
        public const string prefix = "SF";

        public const string scholarPermadeath = prefix + "ScholarPermadeath";
        public const string rippleDone = prefix + "Ripple";
        public const string backup = prefix + "Backup";
        public const string saveInit = prefix + "SaveInit";
        public const string gates = prefix + "Gates";
        public const string backupToUse = prefix + "BackupToUse";
        public const string anchors = prefix + "Anchors";

        public static void CustomSavedataInit(On.SaveState.orig_LoadGame orig, SaveState self, string str, RainWorldGame game)
        {
            orig(self, str, game);
            if (self.saveStateNumber == Enums.SlugcatStatsName.sfscholar && self.deathPersistentSaveData.GetSlugBaseData().TryGet<int>(SaveFileCode.backupToUse, out int backup) && backup >= 0)
            {
                Log.LogMessage("Loading backup in loadgame: " + backup);
                string saveToLoad = self.deathPersistentSaveData.GetBackup(backup);
                orig(self, saveToLoad, game);
            }
            if (Plugin.SharedMechanics(self.saveStateNumber) && self.cycleNumber <= 1)
            {
                SaveFileCode.InitialSaveSetup(self);
            }
        }

        public static void InitialSaveSetup(this SaveState save)
        {
            if (!save.deathPersistentSaveData.GetSlugBaseData().TryGet(saveInit, out bool value) || !value)
            {
                Log.LogMessage("RESETTING SF SAVE DATA");
                save.deathPersistentSaveData.GetSlugBaseData().Set(saveInit, true);
                save.deathPersistentSaveData.SetBool(rippleDone, false);
                save.SetString(gates, "");
                save.ClearBackupSaves();
                save.deathPersistentSaveData.SetBool(scholarPermadeath, false);
                save.deathPersistentSaveData.SetString(anchors, "");
            }
        }

        public static bool GetBool(this SaveState save, string name)
        {
            if (save.miscWorldSaveData.GetSlugBaseData().TryGet<bool>(name, out bool b)) return b;
            Plugin.Log.LogMessage("Failed to get " + name);
            return false;
        }
        public static void SetBool(this SaveState save, string name, bool value) => save.miscWorldSaveData.GetSlugBaseData().Set<bool>(name, value);
        public static string GetString(this SaveState save, string name)
        {
            if (save.miscWorldSaveData.GetSlugBaseData().TryGet<string>(name, out string value)) return value;
            Plugin.Log.LogMessage("Failed to get " + name);
            return null;
        }
        public static void SetString(this SaveState save, string name, string value) => save.miscWorldSaveData.GetSlugBaseData().Set<string>(name, value);
        public static int GetInt(this SaveState save, string name)
        {
            if (save.miscWorldSaveData.GetSlugBaseData().TryGet<int>(name, out int value)) return value;
            Plugin.Log.LogMessage("Failed to get " + name);
            return -1;
        }
        public static void SetInt(this SaveState save, string name, int value) => save.miscWorldSaveData.GetSlugBaseData().Set<int>(name, value);
        public static int GetInt(this DeathPersistentSaveData data, string name)
        {
            if (data.GetSlugBaseData().TryGet<int>(name, out int value)) return value;
            Plugin.Log.LogMessage("Failed to get " + name);
            return -1;
        }
        public static void SetInt(this DeathPersistentSaveData data, string name, int value) => data.GetSlugBaseData().Set<int>(name, value);
        public static void SetBool(this DeathPersistentSaveData save, string name, bool value) => save.GetSlugBaseData().Set<bool>(name, value);
        public static bool GetBool(this DeathPersistentSaveData save, string name)
        {
            if (save.GetSlugBaseData().TryGet<bool>(name, out bool b)) return b;
            Plugin.Log.LogMessage("Failed to get " + name);
            return false;
        }
        public static string GetString(this DeathPersistentSaveData save, string name)
        {
            if (save.GetSlugBaseData().TryGet<string>(name, out string value)) return value;
            Plugin.Log.LogMessage("Failed to get " + name);
            return null;
        }
        public static void SetString(this DeathPersistentSaveData save, string name, string value) => save.GetSlugBaseData().Set<string>(name, value);
        public static string GetBackup(this DeathPersistentSaveData save, int backupNumber)
        {
            if (save.GetSlugBaseData().TryGet<string>(backup + backupNumber, out string saveString))
            {
                string result = saveString;
                Log.LogMessage("Getting backup: " + result);
                return result;
            }
            Plugin.Log.LogMessage("Failed to get " + backup + backupNumber);
            return null;
        }
        public static void SetBackup(this DeathPersistentSaveData save, string name, ref SaveState value)
        {
            string result = value.SaveToString();
            Log.LogMessage("Setting backup: " + result);
            save.GetSlugBaseData().Set<string>(name, result);
        }

        public static bool IsGateLocked(this Room room)
        {
            string regions = room.game.GetStorySession.saveState.GetString(gates);
            bool value = regions != null && regions.Contains(room.abstractRoom.name);
            return value;
        }
        public static void LockGate(this Room room)
        {
            Log.LogMessage("Locking gate: " + room.abstractRoom.name);
            string regions = room.game.GetStorySession.saveState.GetString(gates);
            if (regions != null) room.game.GetStorySession.saveState.SetString(gates, regions + "+" + room.abstractRoom.name + "/3");
            else room.game.GetStorySession.saveState.SetString(gates, room.abstractRoom.name + "/3");
            Log.LogMessage("Gates locked: " + room.game.GetStorySession.saveState.GetString(gates));
        }

        public static bool HalfwayEchoes(this SaveState data)
        {
            return (EchoEncounters(data) > (int)(maxEchoes / 2));
        }

        public static int EchoEncounters(this DeathPersistentSaveData data)
        {
            if (data.maximumRippleLevel >= 1f)
            {
                return maxEchoes;
            }
            return math.clamp(data.karmaCap - 5, 0, maxEchoes);
        }

        public static int EchoEncounters(this SaveState save)
        {
            if (save?.deathPersistentSaveData != null && Plugin.SharedMechanics(save.saveStateNumber))
            {
                return save.deathPersistentSaveData.EchoEncounters();
            }
            return 0;
        }

        public static bool Ripple(this SaveState data)
        {
            if (data?.deathPersistentSaveData != null)
            {
                return data.deathPersistentSaveData.GetBool(rippleDone);
            }
            return false;
        }

        public static int MinKarma(this DeathPersistentSaveData data)
        {
            int echo = data.EchoEncounters();
            return (1 + echo) / 2;
        }
        public static int MinKarma(this SaveState data)
        {
            int echo = data.EchoEncounters();
            return (1 + echo) / 2;
        }

        public static bool ScholarPermadeath(this DeathPersistentSaveData data)
        {
            return data.GetBool(scholarPermadeath);
        }
        public static int MinKarma(this Menu.Menu menu)
        {
            if (menu is KarmaLadderScreen)
            {
                return (menu as KarmaLadderScreen).saveState.MinKarma();
            }
            if (menu is SleepAndDeathScreen)
            {
                return (menu as SleepAndDeathScreen).saveState.deathPersistentSaveData.MinKarma();
            }    
            return 0;
        }

        public static void SetBackupSave(this SaveState mainSave, ref SaveState backupSave, int backupNumber)
        {
            backupSave.ClearBackupSaves();
            mainSave.deathPersistentSaveData.SetBackup(backup + backupNumber, ref backupSave);
        }

        public static void ClearBackupSaves(this SaveState save)
        {
            if (save?.deathPersistentSaveData?.GetSlugBaseData() != null)
            {
                for (int i = 0; i < maxEchoes; i++)
                {
                    save.deathPersistentSaveData.GetSlugBaseData().Remove(backup + i);
                }
            }
            
        }

        public static void CopyBackupSaves(this SaveState saveToCopyTo, ref SaveState saveToCopyFrom)
        {
            for (int i = 0; i < maxEchoes; i++)
            {
                if (saveToCopyFrom.deathPersistentSaveData.GetSlugBaseData().TryGet<string>(backup + i, out string saveString))
                {
                    saveToCopyTo.deathPersistentSaveData.GetSlugBaseData().Set<string>(backup + i, saveString);
                }
            }
        }

        public static void SetAnchorMeeting(this DeathPersistentSaveData data, AnchorEnums.AnchorID anchorType)
        {
            string anchorTypeString = anchorType.ToString()?.ToLowerInvariant();
            string anchorData = data.GetString(anchors)?.ToLowerInvariant();

            if (anchorData != null && anchorData.Length > 0)
            {
                Log.LogMessage("Adding anchor meeting!");
                anchorData = anchorData + "+" + anchorTypeString;
            }
            else
            {
                Log.LogMessage("Adding first anchor meeting!");
                anchorData = anchorTypeString;
            }
            data.SetString(anchors, anchorData);
        }

        public static bool GetAnchorMeeting(this DeathPersistentSaveData data, AnchorEnums.AnchorID anchorType)
        {
            string anchorTypeString = anchorType.ToString()?.ToLowerInvariant();
            string anchorData = data.GetString(anchors)?.ToLowerInvariant();

            if (anchorData == null || anchorData.Length <= 0)
            {
                Log.LogMessage("No anchor data!");
                return false;
            }

            if (anchorData.Contains(anchorTypeString))
            {
                Log.LogMessage("Met this anchor before: " + anchorTypeString);
                return true;
            }
            Log.LogMessage("Didnt meet this anchor before: " + anchorTypeString);
            return false;
        }
    }
}
