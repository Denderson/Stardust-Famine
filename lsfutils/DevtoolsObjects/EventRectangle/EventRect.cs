using System;
using UnityEngine;
using static Pom.Pom;
using static lsfUtils.Plugin;
using BepInEx;
using static lsfUtils.DevtoolsObjects.EventRectangle.EventLogic;
using lsfUtils.SaveFile;

namespace lsfUtils.DevtoolsObjects.EventRectangle;

public class EventRect : UpdatableAndDeletable
{
    private EventRectData data;
    private PlacedObject placedObject;

    public FloatRect rect; // the range of the event rectangle

    public int timer; // for events that are not instant, tell it how many frames the event lasts, counts to zero

    public bool triggered; // prevents an event rectangle from activating multiple times per cycle

    public string eventType; // represents what event should trigger. may change to an enum later on for convenience

    public string eventValue; // represents the custom info about that event, can be null and still work if the event allows it

    public Player triggerer;
    public EventRect(PlacedObject placedObject, Room room)
    {
        EventRectData maybedata = placedObject.data as EventRectData ?? throw new ArgumentException($"{nameof(PlacedObject)} was null or didn't contain a {nameof(EventRectData)} instance");
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
        timer = -1;
        triggered = false;
        if (data != null)
        {
            if (data.eventType != null && !data.eventType.IsNullOrWhiteSpace())
            {
                eventType = data.eventType.Trim().ToLowerInvariant();
                if (eventType.Contains("event: ")) eventType = eventType.Replace("event: ", "").Trim();
            }
            else
            {
                Log.LogMessage("eventValue doesnt exist!");
                eventType = null;
            }

            if (data.eventValue != null && !data.eventValue.IsNullOrWhiteSpace())
            {
                eventValue = data.eventValue.Trim().ToLowerInvariant();
                if (eventValue.Contains("event: ")) eventValue = eventValue.Replace("event: ", "").Trim();
            }
            else
            {
                eventValue = null;
            }
        }

    }
    public override void Update(bool eu)
    {

        base.Update(eu);
        if (!triggered && room != null && room.PlayersInRoom != null && room.PlayersInRoom.Count > 0)
        {
            foreach (Player player in room.PlayersInRoom)
            {
                if (player == null || player.bodyChunks == null || player.bodyChunks.Length < 1)
                {
                    continue;
                }
                Vector2 vector = player.mainBodyChunk.ContactPoint.ToVector2();
                Vector2 pos = player.mainBodyChunk.pos + vector * (player.mainBodyChunk.rad + 30f);
                if (rect.Vector2Inside(pos))
                {
                    triggered = true;
                    triggerer = player;
                }
            }
        }

        if (triggered)
        {
            EventLogic.TriggerEvent(this, eventType, room, triggerer, eventValue);
            if (timer <= 0)
            {
                if (data.singleUse && room?.game?.GetStorySession?.saveState != null)
                {
                    string eventTriggerName = "triggeredEvent" + room.abstractRoom.name + eventType + eventValue;
                    if (room.game.GetStorySession.saveState.GetBool(eventTriggerName)) return;
                    else room.game.GetStorySession.saveState.Set(eventTriggerName, true);
                }
                Destroy();
            }
        }
    }
}