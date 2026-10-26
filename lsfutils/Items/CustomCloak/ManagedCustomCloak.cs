using DevInterface;
using static lsfUtils.Plugin;
using static Pom.Pom;

namespace lsfUtils.Items.CustomCloak;

public class ManagedCustomCloak : ManagedObjectType
{
    public class CustomCloakData : PlacedObject.ConsumableObjectData
    {
        public CustomCloakData(PlacedObject po) : base(po) { }
    }

    public ManagedCustomCloak() : base("CustomCloak", "lsfUtils", typeof(CustomCloak), typeof(PlacedObject.ConsumableObjectData), typeof(ConsumableRepresentation)) { }

    public override UpdatableAndDeletable MakeObject(PlacedObject placedObject, Room room)
    {
        int pobjIndex = room.roomSettings.placedObjects.IndexOf(placedObject);
        if (room.game.GetStorySession?.saveState.ItemConsumed(room.world, false, room.abstractRoom.index, pobjIndex) == false && room.abstractRoom.firstTimeRealized)
        {
            CustomCloakAbstract customCloakAbstract = new(room.world, room.GetWorldCoordinate(placedObject.pos), room.game.GetNewID(), room.abstractRoom.index, pobjIndex, placedObject.data as PlacedObject.ConsumableObjectData)
            {
                isConsumed = false
            };
            room.abstractRoom.AddEntity(customCloakAbstract);
            customCloakAbstract.placedObjectOrigin = room.SetAbstractRoomAndPlacedObjectNumber(room.abstractRoom.name, pobjIndex);
            Log.LogMessage("Making custom cloak!");
        }
        return null;
    }
}