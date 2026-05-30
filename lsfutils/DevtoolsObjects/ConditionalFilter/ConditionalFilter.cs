using RWCustom;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Pom.Pom;

namespace lsfUtils.DevtoolsObjects.ConditionalFilter
{
    internal class ConditionFilter : UpdatableAndDeletable
    {
        private ConditionFilterData data;

        public ConditionFilter(PlacedObject placedObject, Room room)
        {
            ConditionFilterData maybedata = placedObject.data as ConditionFilterData;
            if (maybedata == null)
            {
                throw new ArgumentException($"{nameof(PlacedObject)} was null or didn't contain a {nameof(ConditionFilterData)} instance");
            }
            data = maybedata;
            this.room = room;
        }
    }
}