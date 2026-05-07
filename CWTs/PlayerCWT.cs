using lsfUtils.Items.Dart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace lsfUtils.CWTs
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
            public bool rippleMode = false;
            public bool startingRipple = false;
            public int rippleTimer = -1;
            public int activationTimer = 0;
            public Dart pullingOutThisDart = null;
        }
    }
}
