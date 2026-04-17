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
    public static class PlayerCWT
    {

        public static readonly ConditionalWeakTable<Player, DataClass> playerCWT = new();
        public static bool TryGetData(Player key, out DataClass data)
        {
            if (key != null)
            {
                data = playerCWT.GetOrCreateValue(key);
            }
            else data = null;

            return data != null;
        }
        public class DataClass
        {
            public float rippleAttack = 0f;

            public float temporaryPoison = 0f;

            public int pullingOutFrames = 0;
            public int pullingOutAttampts = 0;

            public int teleportCooldown = 60;

            public int emergencyFood = 0;
        }
    }
}
