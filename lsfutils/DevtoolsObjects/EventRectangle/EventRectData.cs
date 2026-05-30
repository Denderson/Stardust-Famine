using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Pom.Pom;
namespace lsfUtils.DevtoolsObjects.EventRectangle
{
    public class EventRectData : ManagedData
    {
        [BackedByField("01p2")]
        public Vector2 p2;

        public EventRectData(PlacedObject po) : base(po, new ManagedField[]
        {
            new Vector2Field("01p2", new Vector2(160f, 80f), Vector2Field.VectorReprType.rect)
        })
        {
        }

        [StringField("EventRect", "Event-Value", "Event: ")]
        public string condition;

        [BooleanField("SingleUse", false, ManagedFieldWithPanel.ControlType.button, "Single-Use: ")]
        public bool singleUse;

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
}