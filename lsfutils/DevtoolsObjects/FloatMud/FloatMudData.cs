using DevInterface;
using lsfUtils.Items.KarmaMask;
using static lsfUtils.Plugin;
using static Pom.Pom;

namespace lsfUtils.DevtoolsObjects.FloatMud
{
    public class ManagedFloatMud : ManagedObjectType
    {
        public class FloatMudData : MudPit.MudPitData
        {
            public FloatMudData(PlacedObject po) : base(po) { }
        }

        public ManagedFloatMud() : base("FloatMud", "lsfUtils", typeof(FloatMud), typeof(FloatMudData), typeof(MudPitRepresentation)) { }

        public override UpdatableAndDeletable MakeObject(PlacedObject placedObject, Room room)
        {
            FloatMud mud = new(placedObject)
            {
                room = room
            };
            return mud;
        }
    }
}