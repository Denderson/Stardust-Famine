using System;
using UnityEngine;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Pom.Pom;

namespace lsfUtils.DevtoolsObjects.ConditionalFilter
{
    internal class RoomConditionFilterUAD : UpdatableAndDeletable
    {
        private RoomConditionFilterData data;

        public RoomConditionFilterUAD(PlacedObject placedObject, Room room)
        {
            RoomConditionFilterData maybedata = placedObject.data as RoomConditionFilterData;
            if (maybedata == null)
            {
                throw new ArgumentException($"{nameof(PlacedObject)} was null or didn't contain a {nameof(RoomConditionFilterData)} instance");
            }
            data = maybedata;
            this.room = room;
        }
    }
}