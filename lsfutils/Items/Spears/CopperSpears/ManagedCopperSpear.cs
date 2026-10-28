using DevInterface;
using static lsfUtils.Plugin;
using static Pom.Pom;

namespace lsfUtils.Items.Spears.CopperSpears;

public class ManagedCopperSpear : ManagedObjectType
{
    public class CopperSpearData : PlacedObject.ConsumableObjectData
    {
        public CopperSpearData(PlacedObject po) : base(po)
        {

        }
    }

    public ManagedCopperSpear() : base("CopperSpear", "lsfUtils", typeof(CopperSpear), typeof(PlacedObject.ConsumableObjectData), typeof(ConsumableRepresentation))
    {

    }
    public override UpdatableAndDeletable MakeObject(PlacedObject placedObject, Room room)
    {
        int pobjIndex = room.roomSettings.placedObjects.IndexOf(placedObject);
        if (room.abstractRoom.firstTimeRealized)
        {
            CopperSpearAbstract copperSpearAbstract = new(room.world, room.GetWorldCoordinate(placedObject.pos), room.game.GetNewID());
            room.abstractRoom.AddEntity(copperSpearAbstract);
            copperSpearAbstract.placedObjectOrigin = room.SetAbstractRoomAndPlacedObjectNumber(room.abstractRoom.name, pobjIndex);
            copperSpearAbstract.Realize();
            Log.LogMessage("Making copper spear!");
            return null;
        }
        return null;
    }
}