using DevInterface;
using static lsfUtils.Plugin;
using static Pom.Pom;

namespace lsfUtils.Items.KnotSpawnV2;

public class ManagedKnotSpawnV2 : ManagedObjectType
{
    public class KnotSpawnV2Data : PlacedObject.ConsumableObjectData
    {
        public KnotSpawnV2Data(PlacedObject po) : base(po) { }
    }

    public ManagedKnotSpawnV2() : base("KnotSpawnV2", "lsfUtils", typeof(KnotSpawnV2), typeof(PlacedObject.ConsumableObjectData), typeof(ConsumableRepresentation)) { }

    public override UpdatableAndDeletable MakeObject(PlacedObject placedObject, Room room)
    {
        int pobjIndex = room.roomSettings.placedObjects.IndexOf(placedObject);
        if (room.abstractRoom.firstTimeRealized)
        {
            KnotSpawnV2Abstract KnotSpawnV2Abstract = new(room.world, room.GetWorldCoordinate(placedObject.pos), room.game.GetNewID(), room.abstractRoom.index, pobjIndex, placedObject.data as PlacedObject.ConsumableObjectData)
            {
                isConsumed = false
            };
            room.abstractRoom.AddEntity(KnotSpawnV2Abstract);
            KnotSpawnV2Abstract.placedObjectOrigin = room.SetAbstractRoomAndPlacedObjectNumber(room.abstractRoom.name, pobjIndex);
            Log.LogMessage("Making KnotSpawnV2!");
        }
        return null;
    }
}