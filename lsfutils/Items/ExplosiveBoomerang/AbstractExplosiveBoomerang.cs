

namespace lsfUtils.Items.ExplosiveBoomerang;

public class AbstractExplosiveBoomerang : AbstractPhysicalObject
{
    public bool isSingularity = false;
    public AbstractExplosiveBoomerang(World world, WorldCoordinate pos, EntityID id, bool isSingularity = false) : base(world, Enums.AbstractObjectType.ExplosiveBoomerang, null, pos, id)
    {
        this.isSingularity = isSingularity;
    }

    public override void Realize()
    {
        base.Realize();
        realizedObject ??= new ExplosiveBoomerang(this, world, isSingularity);
    }
}