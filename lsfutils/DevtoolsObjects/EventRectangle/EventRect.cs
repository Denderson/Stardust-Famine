using RWCustom;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Pom.Pom;
using static lsfUtils.Plugin;
using BepInEx;

namespace lsfUtils.DevtoolsObjects.EventRectangle
{
    public class EventRect : UpdatableAndDeletable
    {
        private EventRectData data;
        private PlacedObject placedObject;
        public FloatRect rect;
        public EventRect(PlacedObject placedObject, Room room)
        {
            EventRectData maybedata = placedObject.data as EventRectData;
            if (maybedata == null)
            {
                throw new ArgumentException($"{nameof(PlacedObject)} was null or didn't contain a {nameof(EventRectData)} instance");
            }
            data = maybedata;
            this.placedObject = placedObject;
            this.room = room;
            Vector2 corner1 = placedObject.pos;
            Vector2 corner2 = placedObject.pos + data.p2;
            this.rect = new FloatRect(
                Mathf.Min(corner1.x, corner2.x),
                Mathf.Min(corner1.y, corner2.y),
                Mathf.Max(corner1.x, corner2.x),
                Mathf.Max(corner1.y, corner2.y)
            );
        }
        public override void Update(bool eu)
        {
            base.Update(eu);
            for (int i = 0; i < room.physicalObjects.Length; i++)
            {
                for (int j = 0; j < room.physicalObjects[i].Count; j++)
                {
                    for (int k = 0; k < room.physicalObjects[i][j].bodyChunks.Length; k++)
                    {
                        Vector2 vector = room.physicalObjects[i][j].bodyChunks[k].ContactPoint.ToVector2();
                        Vector2 pos = room.physicalObjects[i][j].bodyChunks[k].pos + vector * (room.physicalObjects[i][j].bodyChunks[k].rad + 30f);
                        if (rect.Vector2Inside(pos) && room.physicalObjects[i][j] is Player player)
                        {
                            if (data?.condition != null && !data.condition.IsNullOrWhiteSpace())
                            {
                                string eventText = data.condition.Trim().ToLowerInvariant();
                                if (eventText.Contains("event: ")) eventText = eventText.Replace("event: ", "").Trim();

                                EventLogic.TriggerEvent(eventText, room, player);
                            }
                            else
                            {
                                Log.LogMessage("No eventText!");
                            }
                            this.Destroy();
                        }
                    }
                }
            }
        }
    }
}