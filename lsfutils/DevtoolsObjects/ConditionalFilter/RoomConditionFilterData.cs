using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Pom.Pom;

namespace lsfUtils.DevtoolsObjects.ConditionalFilter
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
            bool? value = ConditionalLogic.LSFRoomConditions(condition, game, room);
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
}
