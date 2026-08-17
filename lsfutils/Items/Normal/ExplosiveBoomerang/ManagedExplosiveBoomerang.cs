using DevInterface;
using static lsfUtils.Plugin;
using static Pom.Pom;

namespace lsfUtils.Items.ExplosiveBoomerang
{
    public class ManagedExplosiveBoomerang : ManagedObjectType
    {
        public class ExplosiveBoomerangData : PlacedObject.ConsumableObjectData
        {
            public ExplosiveBoomerangData(PlacedObject po) : base(po) { }
        }

        public ManagedExplosiveBoomerang() : base("ExplosiveBoomerang", "lsfUtils", typeof(ExplosiveBoomerang), typeof(PlacedObject.ConsumableObjectData), typeof(ConsumableRepresentation)) { }

        public override UpdatableAndDeletable MakeObject(PlacedObject placedObject, Room room)
        {
            int pobjIndex = room.roomSettings.placedObjects.IndexOf(placedObject);
            if (room.game.GetStorySession?.saveState.ItemConsumed(room.world, false, room.abstractRoom.index, pobjIndex) == false && room.abstractRoom.firstTimeRealized)
            {
                AbstractExplosiveBoomerang ExplosiveBoomerangAbstract = new(room.world, room.GetWorldCoordinate(placedObject.pos), room.game.GetNewID())
                {
                    isSingularity = false
                };
                room.abstractRoom.AddEntity(ExplosiveBoomerangAbstract);
                ExplosiveBoomerangAbstract.placedObjectOrigin = room.SetAbstractRoomAndPlacedObjectNumber(room.abstractRoom.name, pobjIndex);
                Log.LogMessage("Making karma mask!");
            }
            return null;
        }
    }
}