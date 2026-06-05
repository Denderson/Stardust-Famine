using Fisobs.Core;
using UnityEngine;

namespace lsfUtils.Items.KarmaMask
{
    public class KarmaMaskAbstract : AbstractConsumable
    {
        public KarmaMaskAbstract(World world, WorldCoordinate pos, EntityID ID, int originRoom, int placedObjectIndex, PlacedObject.ConsumableObjectData consumableObjectData) : base(world, AbstractPhysicalObject.AbstractObjectType.VultureMask, null, pos, ID, originRoom, placedObjectIndex, consumableObjectData)
        {
            type = Enums.AbstractPhysicalObjectType.KarmaMask;
            rippleBothSides = true;
        }

        public override void Realize()
        {
            base.Realize();
            realizedObject ??= new KarmaMask(this, world);
        }

        public override string ToString()
        {
            return this.SaveToString("");
        }
    }
}