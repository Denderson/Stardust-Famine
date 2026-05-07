using Stardust.Slugcats.Scholar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Stardust.CWTs
{
    public static class FoodMeterCWT
    {

        public static readonly ConditionalWeakTable<HUD.FoodMeter, DataClass> foodCWT = new();
        public static bool TryGetData(HUD.FoodMeter key, out DataClass data)
        {
            if (key != null)
            {
                data = foodCWT.GetOrCreateValue(key);
            }
            else data = null;

            return data != null;
        }
        public class DataClass
        {
            public bool hasEFM = false;
            public bool isEFM = false;
            public HUD.FoodMeter EFM = null;
            public HUD.FoodMeter normalFoodMeter = null;
        }
    }
}
