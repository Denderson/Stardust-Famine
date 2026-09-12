using SlugBase.SaveData;
using static Stardust.Plugin;
using static Stardust.Enums;
using static Stardust.Enums.SlugcatStatsName;

namespace Stardust.SaveFile
{
    public static class SaveFileMain
    {
        public const int maxEchoes = 6;

        public const string prefix = "SF";
        public const string saveInit = prefix + "SaveInit";

        public const string gates = prefix + "Gates";
        public const string rippleSequenceDone = prefix + "RippleSequenceDone";
        public const string anchors = prefix + "Anchors";

        public const string bitterTutorialDone = prefix + "BitterTutorialDone";
        public const string bitterSeenSlugtreeSequence = prefix + "BitterSeenSlugtreeSequence";
        public const string bitterArmorRemaining = prefix + "ArmorHealthRemaining";

        public const string scholarPermadeath = prefix + "ScholarPermadeath";
        public const string backup = prefix + "Backup";
        public const string backupToUse = prefix + "BackupToUse";


        public static void CustomSavedataInit(On.SaveState.orig_LoadGame orig, SaveState self, string str, RainWorldGame game)
        {
            orig(self, str, game);
            if (self?.saveStateNumber == sfscholar && self.deathPersistentSaveData.GetSlugBaseData().TryGet(backupToUse, out int backup) && backup > -1)
            {
                Log.LogMessage($"Loading backup in loadgame: {backup}");
                self.LoadBackupAsMain(backup);
            }
            if (self.cycleNumber <= 1)
            {
                if (self.saveStateNumber == bitter) self.InitialSaveSetupBitter();
                else if (self.saveStateNumber == sfscholar) self.InitialSaveSetupScholar();
            }
        }

        public static void InitialSaveSetup(this SaveState save)
        {
            Log.LogMessage("RESETTING SF SAVE DATA");
            save.deathPersistentSaveData.GetSlugBaseData().Set(saveInit, true);

            save.deathPersistentSaveData.Set<bool>(rippleSequenceDone, false);
            save.Set<string>(gates, "");
            save.deathPersistentSaveData.Set<string>(anchors, "");
        }

        public static void InitialSaveSetupBitter(this SaveState save)
        {
            if (!save.deathPersistentSaveData.GetSlugBaseData().TryGet(saveInit, out bool value) || !value)
            {
                save.InitialSaveSetup();
                save.Set<bool>(bitterTutorialDone, false);
                save.deathPersistentSaveData.Set<bool>(bitterSeenSlugtreeSequence, false);
                save.Set<int>(bitterArmorRemaining, 150);
            }
        }

        public static void InitialSaveSetupScholar(this SaveState save)
        {
            if (!save.deathPersistentSaveData.GetSlugBaseData().TryGet(saveInit, out bool value) || !value)
            {
                save.InitialSaveSetup();
                save.ClearBackupSaves();
                save.deathPersistentSaveData.Set<bool>(scholarPermadeath, false);
            }
        }

        public static void RainWorldGame_Win(On.RainWorldGame.orig_Win orig, RainWorldGame self, bool malnourished, bool fromWarpPoint)
        {
            if (self.manager.upcomingProcess != null || !self.IsStorySession)
            {
                orig(self, malnourished, fromWarpPoint);
                return;
            }
            SaveFileGates.TickGates(self, malnourished);
            SaveFileBitter.TickArmor(self, malnourished);

            orig(self, malnourished, fromWarpPoint);
        }

        public static bool GetBool(this SaveState save, string name)
        {
            if (save.miscWorldSaveData.GetSlugBaseData().TryGet(name, out bool b))
                return b;
            Log.LogMessage($"Failed to get {name}");
            return new bool();
        }

        public static void Set<T>(this SaveState save, string name, T value) => save.miscWorldSaveData.GetSlugBaseData().Set(name, value);
        public static void Set<T>(this DeathPersistentSaveData data, string name, T value) => data.GetSlugBaseData().Set(name, value);

        public static string GetString(this SaveState save, string name)
        {
            if (save.miscWorldSaveData.GetSlugBaseData().TryGet(name, out string value)) return value;
            Log.LogMessage($"Failed to get {name}");
            return null;
        }
        public static int GetInt(this SaveState save, string name)
        {
            if (save.miscWorldSaveData.GetSlugBaseData().TryGet(name, out int value)) return value;
            Log.LogMessage($"Failed to get {name}");
            return -1;
        }
        public static int GetInt(this DeathPersistentSaveData data, string name)
        {
            if (data.GetSlugBaseData().TryGet(name, out int value)) return value;
            Log.LogMessage($"Failed to get {name}");
            return -1;
        }

        public static bool GetBool(this DeathPersistentSaveData save, string name)
        {
            if (save.GetSlugBaseData().TryGet(name, out bool b)) return b;
            Log.LogMessage($"Failed to get {name}");
            return false;
        }
        
        public static string GetString(this DeathPersistentSaveData save, string name)
        {
            if (save.GetSlugBaseData().TryGet(name, out string value)) return value;
            Log.LogMessage($"Failed to get {name}");
            return null;
        }

        public static string GetBackup(this DeathPersistentSaveData save, int backupNumber)
        {
            if (save.GetSlugBaseData().TryGet(backup + backupNumber, out string saveString))
            {
                string result = saveString;
                Log.LogMessage($"Getting backup: {result}");
                return result;
            }
            Log.LogMessage($"Failed to get {backup}{backupNumber}");
            return null;
        }
        public static void SetBackup(this DeathPersistentSaveData save, string name, ref SaveState value)
        {
            string result = value.SaveToString();
            Log.LogMessage($"Setting backup: {result}");
            save.GetSlugBaseData().Set(name, result);
        }
        
        public static void SetBackup(this DeathPersistentSaveData save, string name, string value)
        {
            Log.LogMessage($"Setting backup: {value}");
            save.GetSlugBaseData().Set(name, value);
        }
    }
}
