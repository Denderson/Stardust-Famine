using SlugBase.SaveData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Stardust.Plugin;
using static Stardust.SaveFile.SaveFileMain;

namespace Stardust.SaveFile
{
    public static class SaveFileScholar
    {
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
                if (saveToCopyFrom.deathPersistentSaveData.GetSlugBaseData().TryGet(backup + i, out string saveString))
                {
                    saveToCopyTo.deathPersistentSaveData.GetSlugBaseData().Set(backup + i, saveString);
                }
            }
        }

        public static bool ScholarPermadeath(this DeathPersistentSaveData data)
        {
            return data.GetBool(scholarPermadeath);
        }
    }
}
