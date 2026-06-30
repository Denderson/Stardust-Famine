using lsfUtils.Effects;
using System.Runtime.CompilerServices;
using static lsfUtils.Enums;

namespace lsfUtils.CWTs
{
    public static class WorldCWT
    {

        public static readonly ConditionalWeakTable<World, DataClass> worldCWT = new();
        public static bool TryGetData(World key, out DataClass data)
        {
            if (key != null)
            {
                data = worldCWT.GetOrCreateValue(key);
            }
            else data = null;

            return data != null;
        }
        public class DataClass
        {
            public CreepingDarknessUAD creepingDarkness;
            public int regionState;
            public int regionStateSwitchTimer;
        }
    }
}