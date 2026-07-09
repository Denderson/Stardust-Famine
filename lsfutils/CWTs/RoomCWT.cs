using lsfUtils.DevtoolsObjects.LocalGravity;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace lsfUtils.CWTs
{
    public static class RoomCWT
    {

        public static readonly ConditionalWeakTable<Room, DataClass> roomCWT = new();
        public static bool TryGetData(Room key, out DataClass data)
        {
            if (key != null)
            {
                data = roomCWT.GetOrCreateValue(key);
            }
            else data = null;

            return data != null;
        }
        public class DataClass
        {
            public List<LocalGravity> localGravities = [];
        }
    }
}