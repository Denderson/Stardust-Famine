using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static lsfUtils.Plugin;

namespace lsfUtils.DevtoolsObjects.EventRectangle
{
    public class EventLogic
    {
        public delegate void TriggeredEvent(Room room, Player player, string eventValue);

        public static Dictionary<string, TriggeredEvent> triggeredEvents = [];

        public static void RegisterBuiltInEvents()
        {
            RegisterEvent("egg", EggEvent);
        }

        public static void RegisterEvent(string eventText,  TriggeredEvent triggeredEvent)
        {
            eventText = eventText.ToLowerInvariant();
            triggeredEvents[eventText] = triggeredEvent;
        }

        public static void TriggerEvent(string eventText, Room room = null, Player triggerer = null, string eventValue = null)
        {
            if (triggeredEvents.TryGetValue(eventText, out TriggeredEvent triggeredEvent))
            {
                triggeredEvent(room, triggerer, eventValue);
            }
            else
            {
                Log.LogMessage("no event matching text found!");
            }
        }

        public static void EggEvent(Room room, Player player, string eventValue)
        {
            if (player == null || room?.game?.cameras == null || room.game.cameras.Length < 1)
            {
                Log.LogMessage("Error in EggEvent!");
                return;
            }
            room.game.cameras[0].hud.textPrompt.AddMessage("Well, there is a man here.", 20, 160, darken: true, hideHud: true);
            AbstractPhysicalObject abstractPhysicalObject = new(room.world, DLCSharedEnums.AbstractObjectType.SingularityBomb, null, room.GetWorldCoordinate(player.mainBodyChunk.pos), room.world.game.GetNewID());
            player.abstractCreature.Room.AddEntity(abstractPhysicalObject);
            abstractPhysicalObject.RealizeInRoom();
            if (player.FreeHand() != -1)
            {
                room.game.cameras[0].hud.textPrompt.AddMessage("He offers you an egg.", 20, 160, darken: true, hideHud: true);
                player.SlugcatGrab(abstractPhysicalObject.realizedObject, player.FreeHand());
            }
            else room.game.cameras[0].hud.textPrompt.AddMessage("He offers you an egg, but your hands are full.", 20, 160, darken: true, hideHud: true);
            return;
        }
    }
}