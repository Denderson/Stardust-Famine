using lsfUtils.Items.Darts.Dart;
using System.Runtime.CompilerServices;

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
            public bool pendingRippleExit;
            public Dart pullingOutThisDart = null;
            public int rippleExitTimeout;
            public int darknessImmunity = 0;

            public bool karmaMode = false;
            public bool previousKarmaMode = false;
        }
    }
}
