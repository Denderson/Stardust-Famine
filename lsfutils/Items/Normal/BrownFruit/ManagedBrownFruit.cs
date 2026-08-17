using DevInterface;
using static lsfUtils.Plugin;
using static Pom.Pom;

namespace lsfUtils.Items.BrownFruit
{
    public class ManagedBrownFruit : ManagedObjectType
    {
        public class BrownFruitData : PlacedObject.ConsumableObjectData
        {
            public BrownFruitData(PlacedObject po) : base(po) { }
        }

        public ManagedBrownFruit() : base("BrownFruit", "lsfUtils", typeof(BrownFruit), typeof(PlacedObject.ConsumableObjectData), typeof(ConsumableRepresentation)) { }

        public override UpdatableAndDeletable MakeObject(PlacedObject placedObject, Room room)
        {
            int pobjIndex = room.roomSettings.placedObjects.IndexOf(placedObject);
            if (room.game.GetStorySession?.saveState.ItemConsumed(room.world, false, room.abstractRoom.index, pobjIndex) == false && room.abstractRoom.firstTimeRealized)
            {
                BrownFruitAbstract BrownFruitAbstract = new(room.world, room.GetWorldCoordinate(placedObject.pos), room.game.GetNewID(), room.abstractRoom.index, pobjIndex, placedObject.data as PlacedObject.ConsumableObjectData)
                {
                    isConsumed = false
                };
                room.abstractRoom.AddEntity(BrownFruitAbstract);
                BrownFruitAbstract.placedObjectOrigin = room.SetAbstractRoomAndPlacedObjectNumber(room.abstractRoom.name, pobjIndex);
                Log.LogMessage("Making BrownFruit!");
            }
            return null;
        }
    }
}