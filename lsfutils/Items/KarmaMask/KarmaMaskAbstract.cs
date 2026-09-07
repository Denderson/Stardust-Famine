using Fisobs.Core;
using UnityEngine;
using static lsfUtils.Plugin;

namespace lsfUtils.Items.KarmaMask
{
    public class KarmaMaskAbstract : AbstractConsumable
    {
        public KarmaMaskAbstract(World world, WorldCoordinate pos, EntityID ID, int originRoom, int placedObjectIndex, PlacedObject.ConsumableObjectData consumableObjectData)
            : base(world, AbstractPhysicalObject.AbstractObjectType.VultureMask, null, pos, ID, originRoom, placedObjectIndex, consumableObjectData)
        {
            Log.LogMessage("Spawning karmamaskabstract!");
            type = Enums.AbstractObjectType.KarmaMask;
            rippleBothSides = true;
            if (world is null) Log.LogMessage("World was null!");
        }

        public override void Realize()
        {
            Log.LogMessage("Realising!");
            base.Realize();
            realizedObject ??= new KarmaMask(this, world);
            Log.LogMessage("Spawning karma mask!");
        }

        public override string ToString()
        {
            return this.SaveToString("");
        }
    }
}