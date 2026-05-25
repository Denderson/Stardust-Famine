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
            public int timeInEvilWater;
            public float temporaryPoison;
        }
    }
}
