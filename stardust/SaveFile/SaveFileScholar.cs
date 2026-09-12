using SlugBase.SaveData;
using System.Collections.Generic;
using static Stardust.Plugin;
using static Stardust.SaveFile.SaveFileMain;

namespace Stardust.SaveFile
{
    public static class SaveFileScholar
    {
        public static Dictionary<int, string> GetAllBackups(this SaveState save)
        {
            var result = new Dictionary<int, string>();
            for (int i = 0; i < maxEchoes; i++)
            {
                string s = save.deathPersistentSaveData.GetBackup(i);
                if (s != null) result[i] = s;
            }
            return result;
        }
        public static Dictionary<int, string> GetAllPreviousBackups(this SaveState save, int backupNumber)
        {
            var result = new Dictionary<int, string>();
            for (int i = 0; i < backupNumber; i++)
            {
                string s = save.deathPersistentSaveData.GetBackup(i);
                if (s != null) result[i] = s;
            }
            return result;
        }
        
        public static void SetBackupsSaveRaw(this SaveState save, Dictionary<int, string> backups)
        {
            foreach (var kvp in backups)
            {
                save.deathPersistentSaveData.SetBackup(backup + kvp.Key, kvp.Value);
            }
        }
        
        public static void SetBackupSave(this SaveState mainSave, SaveState backupSave, int backupNumber)
        {
            bool sameSave = ReferenceEquals(mainSave, backupSave);
            Dictionary<int, string> preserved = sameSave ? backupSave.GetAllBackups() : null;
            
            backupSave.ClearBackupSaves();
            string snapshot = backupSave.SaveToString();

            if (sameSave) mainSave.SetBackupsSaveRaw(preserved);

            mainSave.deathPersistentSaveData.SetBackup(backup + backupNumber, snapshot);
            Log.LogMessage($"Set backup {backupNumber}");
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
                if (saveToCopyFrom.deathPersistentSaveData.GetSlugBaseData().TryGet(backup + i, out string saveString))
                {
                    saveToCopyTo.deathPersistentSaveData.GetSlugBaseData().Set(backup + i, saveString);
                }
            }
        }
        
        public static void LoadBackupAsMain(this SaveState save, int backupNumber)
        {
            string saveToLoad = save.deathPersistentSaveData.GetBackup(backupNumber);
            if (saveToLoad == null)
            {
                Log.LogMessage($"Backup {backupNumber} doesnt exist!");
                return;
            }

            Dictionary<int, string> olderBackups = save.GetAllPreviousBackups(backupNumber);

            save.LoadGame(saveToLoad, null);save.ClearBackupSaves();
            save.SetBackupsSaveRaw(olderBackups);

            Log.LogMessage($"Loaded {backupNumber} as main save, {olderBackups.Count} backups remain");
        }

        public static bool ScholarPermadeath(this DeathPersistentSaveData data)
        {
            return data.GetBool(scholarPermadeath);
        }
    }
}