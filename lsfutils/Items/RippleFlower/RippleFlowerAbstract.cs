using Fisobs.Core;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace lsfUtils.Items.RippleFlower
{
    public class RippleFlowerAbstract : AbstractConsumable
    {
        public int flowerRippleLayer = 0;
        public RippleFlowerAbstract(World world, WorldCoordinate pos, EntityID ID, int originRoom, int placedObjectIndex, PlacedObject.ConsumableObjectData consumableObjectData) : base(world, Enums.AbstractPhysicalObjectType.RippleFlower, null, pos, ID, originRoom, placedObjectIndex, consumableObjectData)
        {
            type = Enums.AbstractPhysicalObjectType.RippleFlower;
        }

        public override void Realize()
        {
            base.Realize();
            realizedObject ??= new RippleFlower(this);
        }

        public override string ToString()
        {
            return this.SaveToString($"{flowerRippleLayer}");
        }
    }

}
