using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Pom.Pom;
namespace lsfUtils.DevtoolsObjects.EventRectangle
{
    public class EventRectData(PlacedObject po) : ManagedData(po, [ new Vector2Field("01p2", new Vector2(160f, 80f), Vector2Field.VectorReprType.rect) ])
    {
        [BackedByField("01p2")]
        public Vector2 p2;
        [StringField("EventType", "Event-type", "type: ")]
        public string eventType;

        [StringField("EventValue", "Event-Value", "Value: ")]
        public string eventValue; // TODO

        [BooleanField("OncePerSavefile", false, ManagedFieldWithPanel.ControlType.button, "Once per savefile: ")]
        public bool singleUse; // TODO
    }
}