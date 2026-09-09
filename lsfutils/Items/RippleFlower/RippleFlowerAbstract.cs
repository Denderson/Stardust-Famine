

namespace lsfUtils.Items.RippleFlower;

public class RippleFlowerAbstract : AbstractConsumable
{
    public int flowerRippleLayer = 0;
    public RippleFlowerAbstract(World world, WorldCoordinate pos, EntityID ID, int originRoom, int placedObjectIndex, PlacedObject.ConsumableObjectData consumableObjectData) : base(world, Enums.AbstractObjectType.RippleFlower, null, pos, ID, originRoom, placedObjectIndex, consumableObjectData)
    {
        type = Enums.AbstractObjectType.RippleFlower;
    }

    public override void Realize()
    {
        base.Realize();
        realizedObject ??= new RippleFlower(this);
    }
}
