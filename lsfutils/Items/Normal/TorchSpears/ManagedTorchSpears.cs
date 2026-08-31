using DevInterface;
using static lsfUtils.Plugin;
using static Pom.Pom;

namespace lsfUtils.Items.Normal.TorchSpears;

public class ManagedTorchSpear : ManagedObjectType
{
    public class TorchSpearData : PlacedObject.ConsumableObjectData
    {
        public TorchSpearData(PlacedObject po) : base(po)
        {

        }
    }

    public ManagedTorchSpear() : base("TorchSpear", "lsfUtils", typeof(TorchSpear), typeof(PlacedObject.ConsumableObjectData), typeof(ConsumableRepresentation))
    {

    }
    public override UpdatableAndDeletable MakeObject(PlacedObject placedObject, Room room)
    {
        int pobjIndex = room.roomSettings.placedObjects.IndexOf(placedObject);
        if (room.abstractRoom.firstTimeRealized)
        {
            TorchSpearAbstract torchSpearAbstract = new(room.world, room.GetWorldCoordinate(placedObject.pos), room.game.GetNewID());
            room.abstractRoom.AddEntity(torchSpearAbstract);
            torchSpearAbstract.placedObjectOrigin = room.SetAbstractRoomAndPlacedObjectNumber(room.abstractRoom.name, pobjIndex);
            torchSpearAbstract.Realize();
            Log.LogMessage("Making torch spear!");
            return null;
        }
        return null;
    }
}