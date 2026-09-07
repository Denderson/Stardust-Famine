using DevInterface;
using static lsfUtils.Plugin;
using static Pom.Pom;

namespace lsfUtils.Items.KarmaMask
{
    public class ManagedKarmaMask : ManagedObjectType
    {
        public class KarmaMaskData : PlacedObject.ConsumableObjectData
        {
            public KarmaMaskData(PlacedObject po) : base(po) { }
        }

        public ManagedKarmaMask() : base("KarmaMask", "lsfUtils", typeof(KarmaMask), typeof(PlacedObject.ConsumableObjectData), typeof(ConsumableRepresentation)) { }

        public override UpdatableAndDeletable MakeObject(PlacedObject placedObject, Room room)
        {
            int pobjIndex = room.roomSettings.placedObjects.IndexOf(placedObject);
            if (room.game.GetStorySession?.saveState.ItemConsumed(room.world, false, room.abstractRoom.index, pobjIndex) == false && room.abstractRoom.firstTimeRealized)
            {
                KarmaMaskAbstract karmaMaskAbstract = new(room.world, room.GetWorldCoordinate(placedObject.pos), room.game.GetNewID(), room.abstractRoom.index, pobjIndex, placedObject.data as PlacedObject.ConsumableObjectData)
                {
                    isConsumed = false
                };
                room.abstractRoom.AddEntity(karmaMaskAbstract);
                karmaMaskAbstract.placedObjectOrigin = room.SetAbstractRoomAndPlacedObjectNumber(room.abstractRoom.name, pobjIndex);
                Log.LogMessage("Making karma mask!");
            }
            return null;
        }
    }
}