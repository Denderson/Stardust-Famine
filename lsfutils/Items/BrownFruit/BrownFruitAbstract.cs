using Fisobs.Core;
using UnityEngine;
using static lsfUtils.Plugin;

namespace lsfUtils.Items.BrownFruit
{
    public class BrownFruitAbstract : AbstractConsumable
    {
        public BrownFruitAbstract(World world, WorldCoordinate pos, EntityID ID, int originRoom, int placedObjectIndex, PlacedObject.ConsumableObjectData consumableObjectData)
            : base(world, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null, pos, ID, originRoom, placedObjectIndex, consumableObjectData)
        {
            Log.LogMessage("Spawning BrownFruitAbstract!");
            type = Enums.AbstractObjectType.BrownFruit;
            if (world is null) Log.LogMessage("World was null!");
        }

        public override void Realize()
        {
            Log.LogMessage("Realising!");
            base.Realize();
            realizedObject ??= new BrownFruit(this, world);
            Log.LogMessage("Spawning brown fruit!");
        }

        public override string ToString()
        {
            return this.SaveToString("");
        }
    }
}