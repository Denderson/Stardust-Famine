using DevInterface;
using static lsfUtils.Plugin;
using static Pom.Pom;

namespace lsfUtils.Items.RippleFlower;

public class ManagedRippleFlower : Pom.Pom.ManagedObjectType
{
    public class RippleFlowerData : PlacedObject.ConsumableObjectData
    {
        public RippleFlowerData(PlacedObject po) : base(po)
        {

        }
        [IntegerField("FlowerRippleLayer", -1, 9, 0, ManagedFieldWithPanel.ControlType.button, "Layer: ")]
        public int flowerRippleLayer;
    }

    public ManagedRippleFlower() : base("RippleFlower", "lsfUtils", typeof(RippleFlower), typeof(PlacedObject.ConsumableObjectData), typeof(ConsumableRepresentation))
    {

    }
    public override UpdatableAndDeletable MakeObject(PlacedObject placedObject, Room room)
    {
        int pobjIndex = room.roomSettings.placedObjects.IndexOf(placedObject);
        if (room.game.GetStorySession?.saveState.ItemConsumed(room.world, false, room.abstractRoom.index, pobjIndex) == false)
        {
            RippleFlowerAbstract RippleFlowerAbstract = new(room.world, room.GetWorldCoordinate(placedObject.pos), room.game.GetNewID(), room.abstractRoom.index, pobjIndex, placedObject.data as PlacedObject.ConsumableObjectData)
            {
                isConsumed = false
            };
            room.abstractRoom.AddEntity(RippleFlowerAbstract);
            RippleFlowerAbstract.placedObjectOrigin =  room.SetAbstractRoomAndPlacedObjectNumber(room.abstractRoom.name, pobjIndex);
            Log.LogMessage("Making ripple flower!");
            return null;
        }
        return null;
    }
}