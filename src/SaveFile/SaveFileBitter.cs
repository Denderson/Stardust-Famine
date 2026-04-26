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

        public static float ArmorFromSave(int armor)
        {
            return armor * 0.01f;
        }

        public static int ArmorToSave(float armor)
        {
            return (int)(armor * 150);
        }

        public static void ShelterDoor_Close(On.ShelterDoor.orig_Close orig, ShelterDoor self)
        {
            if (self?.room == null)
            {
                Log.LogMessage("Room is null in ShelterDoor_Close!");
                return;
            }
            if (self.room.game?.StoryCharacter != Enums.SlugcatStatsName.bitter)
            {
                return;
            }
            if (self.room.PlayersInRoom == null || self.room.PlayersInRoom.Count <= 0)
            {
                Log.LogMessage("No players in ShelterDoor_Close!");
                return;
            }
            float armor = 0f;
            bool anyoneHasArmor = false;
            foreach (Player player in self.room.PlayersInRoom)
            {
                if (player != null && player.SlugCatClass == Enums.SlugcatStatsName.bitter && PlayerCWT.TryGetData(player, out var data))
                {
                    anyoneHasArmor = true;
                    armor = math.max(armor, data.armorHealth);
                }
            }
            if (anyoneHasArmor)
            {
                self.room.game.GetStorySession.saveState.SetInt(bitterArmorRemaining, ArmorToSave(armor));
            }
            orig(self);
        }
    }
}
