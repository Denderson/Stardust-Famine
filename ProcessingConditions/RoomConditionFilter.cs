using System;
using UnityEngine;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using lsfUtils.ProcessingConditions;
using static Pom.Pom;

namespace lsfUtils.ProcessingConditions
{
    public class RoomConditionFilterData : ManagedData
    {
        public RoomConditionFilterData(PlacedObject po) : base(po, new ManagedField[] { })
        {

        }
        [StringField("RoomConditionalFilter", "Condition-Value", "Room Processing Condition: ")]
        public string condition;

        [Vector2Field("Radius", defX: 80f, defY: 0f, Vector2Field.VectorReprType.circle)]
        public Vector2 radius;

        public bool Active(ref RainWorldGame game, Room room)
        {
            bool? value = lsfUtils.ProcessingConditions.ConditionalLogic.LSFRoomConditions(condition, game, room) ;
            if (value != null && value == true)
            {
                return true;
            }
            return false;
        }

        public virtual void DeactivatePlacedObject(PlacedObject pObj)
        {
            pObj.active = false;
        }
    }

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