using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Stardust.Plugin;

namespace Stardust.Slugcats.Bitter
{
    public static class EmergencyFoodMeter
    {
        public const int maxEmergencyFood = 3;
        public static bool recursionPrevention = false;

        public static void TrySpawnEmergencyBar(this HUD.FoodMeter self)
        {
            Log.LogMessage("Checking to make SFM");

            if (recursionPrevention) return;
            if (self?.hud?.owner == null) return;
            if (self.hud.owner is Player)
            {
                if ((self.hud.owner as Player).room?.game == null) return;
                if (!(self.hud.owner as Player).room.game.IsStorySession) return;
            }
            if (!CWTs.FoodMeterCWT.TryGetData(self, out var data)) return;
            if (data.isEFM || data.hasEFM || data.EFM != null) return;

            recursionPrevention = true;

            Log.LogMessage("Adding EFM");

            Vector2 position = new Vector2(self.pos.x, self.pos.y + 30f);

            data.hasEFM = true;
            data.EFM = new HUD.FoodMeter(self.hud, maxEmergencyFood, maxEmergencyFood, null)
            {
                pos = position,
                lastPos = position
            };
            if (CWTs.FoodMeterCWT.TryGetData(self, out var EFMData))
            {
                Plugin.Log.LogMessage("EFM init success!!");
                EFMData.isEFM = true;
                EFMData.hasEFM = false;
                EFMData.normalFoodMeter = self;
                EFMData.EFM = null;
            }
            self.hud.AddPart(data.EFM);
        }

        public static void FoodMeter_ctor(On.HUD.FoodMeter.orig_ctor orig, HUD.FoodMeter self, HUD.HUD hud, int maxFood, int survivalLimit, Player associatedPup, int pupNumber)
        {
            orig(self, hud, maxFood, survivalLimit, associatedPup, pupNumber);
            bool shouldHaveEmergencyFoodMeter = false;
            if (CWTs.FoodMeterCWT.TryGetData(self, out var data))
            {
                if (!data.isEFM && !data.hasEFM)
                {
                    shouldHaveEmergencyFoodMeter = true;
                }
            }

            if (shouldHaveEmergencyFoodMeter)
            {
                self.TrySpawnEmergencyBar();
            }
            recursionPrevention = false;
        }
    }
}
