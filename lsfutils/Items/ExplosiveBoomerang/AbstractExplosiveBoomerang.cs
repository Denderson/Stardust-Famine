using Fisobs.Core;
using static lsfUtils.Plugin;

namespace lsfUtils.Items.ExplosiveBoomerang
{
    public class AbstractExplosiveBoomerang : AbstractPhysicalObject
    {
        public AbstractExplosiveBoomerang(World world, WorldCoordinate pos, EntityID id) : base(world, Enums.AbstractPhysicalObjectType.ExplosiveBoomerang, null, pos, id)
        {
        }

        public override void Realize()
        {
            base.Realize();
            realizedObject ??= new ExplosiveBoomerang(this, world);
        }

        public override string ToString() => this.SaveToString("");
    }
}