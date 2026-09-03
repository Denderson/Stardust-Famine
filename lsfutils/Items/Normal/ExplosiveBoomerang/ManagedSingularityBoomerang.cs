using DevInterface;
using static lsfUtils.Plugin;
using static Pom.Pom;

namespace lsfUtils.Items.Normal.ExplosiveBoomerang
{
    public class ManagedSingularityBoomerang : ManagedObjectType
    {
        public class SingularityBoomerangData : PlacedObject.ConsumableObjectData
        {
            public SingularityBoomerangData(PlacedObject po) : base(po) { }
        }

        public ManagedSingularityBoomerang() : base("SingularityBoomerang", "lsfUtils", typeof(ExplosiveBoomerang), typeof(PlacedObject.ConsumableObjectData), typeof(ConsumableRepresentation)) { }

        public override UpdatableAndDeletable MakeObject(PlacedObject placedObject, Room room)
        {
            int pobjIndex = room.roomSettings.placedObjects.IndexOf(placedObject);
            if (room.game.GetStorySession?.saveState.ItemConsumed(room.world, false, room.abstractRoom.index, pobjIndex) == false && room.abstractRoom.firstTimeRealized)
            {
                AbstractExplosiveBoomerang SingularityBoomerangAbstract = new(room.world, room.GetWorldCoordinate(placedObject.pos), room.game.GetNewID())
                {
                    isSingularity = true
                };
                room.abstractRoom.AddEntity(SingularityBoomerangAbstract);
                SingularityBoomerangAbstract.placedObjectOrigin = room.SetAbstractRoomAndPlacedObjectNumber(room.abstractRoom.name, pobjIndex);
            }
            return null;
        }
    }
}