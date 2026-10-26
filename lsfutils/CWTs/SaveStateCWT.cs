using System.Runtime.CompilerServices;
using static lsfUtils.Enums;

namespace lsfUtils.CWTs;

public static class SaveStateCWT
{

    public static readonly ConditionalWeakTable<SaveState, DataClass> saveStateCWT = new();
    public static bool TryGetData(SaveState key, out DataClass data)
    {
        if (key != null)
        {
            data = saveStateCWT.GetOrCreateValue(key);
        }
        else data = null;

        return data != null;
    }
    public class DataClass
    {
        public CloakColor gownColor = CloakColor.Default;
        public CloakType gownType = CloakType.Default;
    }
}
