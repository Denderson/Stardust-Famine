using System.Runtime.CompilerServices;

namespace lsfUtils.CWTs
{
    public static class CreatureCWT
    {

        public static readonly ConditionalWeakTable<Creature, DataClass> creatureCWT = new();
        public static bool TryGetData(Creature key, out DataClass data)
        {
            if (key != null)
            {
                data = creatureCWT.GetOrCreateValue(key);
            }
            else data = null;

            return data != null;
        }
        public class DataClass
        {
            public int timeInEvilWater = 0;
            public float temporaryPoison = 0f;
            public bool isInEvilWater = false;

            public int rippleTunnelTimer = 0;
            public int rippleTunnelCooldown = 0;

            public int starveStunTimer = 0;
            public bool starveInit = false;
        }
    }
}
