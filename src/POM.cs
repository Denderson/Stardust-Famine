using System;
using UnityEngine;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Pom.Pom;

namespace Stardust
{
    public class ConditionFilterData : ManagedData
    {
        public ConditionFilterData(PlacedObject po) : base(po, new ManagedField[] { })
        {

        }
        [StringField("ConditionalFilter", "Condition-Value", "Processing Condition: ")]
        public string condition;

        [Vector2Field("Radius", defX: 80f, defY: 0f, Vector2Field.VectorReprType.circle)]
        public Vector2 radius;

        public bool Active(ref RainWorldGame game)
        {
            bool? value = WorldLoader.Preprocessing.PreprocessCustomConditions(condition, game);
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

    internal class ConditionFilterUAD : UpdatableAndDeletable
    {
        private ConditionFilterData data;

        public ConditionFilterUAD(PlacedObject placedObject, Room room)
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