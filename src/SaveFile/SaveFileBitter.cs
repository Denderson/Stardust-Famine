using Stardust.CWTs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using static Stardust.Plugin;
using static Stardust.SaveFile.SaveFileMain;

namespace Stardust.SaveFile
{
    public static class SaveFileBitter
    {
        public static int maxArmor = 200;
        public static int armorPerHibernation = 100;
        public static int armorPerStarve = 50;

        public static void TickArmor(RainWorldGame self, bool malnourished)
        {
            int armorRemaining = self.GetStorySession.saveState.GetInt(bitterArmorRemaining);
            Log.LogMessage("Before change: " + armorRemaining);
            if (!malnourished) armorRemaining += armorPerHibernation;
            else armorRemaining += armorPerStarve;
            if (armorRemaining > maxArmor) armorRemaining = maxArmor;
            self.GetStorySession.saveState.SetInt(bitterArmorRemaining, armorRemaining);
        }

        public static int ArmorFromSave(SaveState save)
        {
            int armor = save.GetInt(bitterArmorRemaining);
            if (armor > 0)
            {
                return armor;
            }
            Log.LogMessage("Armor not found, or broken!");
            return 0;
        }

        public static float ArmorIntToFloat(int armor)
        {
            return (float)(armor * (1f / maxArmor));
        }

        public static int ArmorFloatToInt(float armor)
        {
            return (int)(armor * maxArmor);
        }


    }
}
