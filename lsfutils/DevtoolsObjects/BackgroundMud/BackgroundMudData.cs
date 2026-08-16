using DevInterface;
using lsfUtils.Items.KarmaMask;
using static lsfUtils.Plugin;
using static Pom.Pom;

namespace lsfUtils.DevtoolsObjects.BackgroundMud
{
    public class ManagedBackgroundMud : ManagedObjectType
    {
        public class BackgroundMudData : MudPit.MudPitData
        {
            public BackgroundMudData(PlacedObject po) : base(po) { }
        }

        public ManagedBackgroundMud() : base("BackgroundMud", "lsfUtils", typeof(BackgroundMud), typeof(BackgroundMudData), typeof(MudPitRepresentation)) { }

        public override UpdatableAndDeletable MakeObject(PlacedObject placedObject, Room room)
        {
            BackgroundMud mud = new(placedObject)
            {
                room = room
            };
            return mud;
        }
    }
}