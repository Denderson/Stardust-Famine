using System.Runtime.CompilerServices;

namespace lsfUtils.CWTs
{
    public static class AbstractCreatureCWT
    {

        public static readonly ConditionalWeakTable<AbstractCreature, DataClass> abstractCreatureCWT = new();
        public static bool TryGetData(AbstractCreature key, out DataClass data)
        {
            if (key != null)
            {
                data = abstractCreatureCWT.GetOrCreateValue(key);
            }
            else data = null;

            return data != null;
        }
        public class DataClass
        {
            public int defaultRippleLayer;
            public bool isRippleHybrid;
            public bool isEchoImmune;
            public bool isPoisonImmune;
        }
    }
}
