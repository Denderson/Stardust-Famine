using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lsfUtils.DevtoolsObjects.CreatureFlagZone
{
    public class CreatureFlagZone : UpdatableAndDeletable
    {
        private CreatureFlagZoneData data;

        public CreatureFlagZone(PlacedObject placedObject, Room room)
        {
            CreatureFlagZoneData flagdata = placedObject.data as CreatureFlagZoneData;
            if (flagdata == null)
            {
                throw new ArgumentException($"{nameof(PlacedObject)} was null or didn't contain a {nameof(CreatureFlagZoneData)} instance");
            }
            data = flagdata;
            this.room = room;
        }
    }
}