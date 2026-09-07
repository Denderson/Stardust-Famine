using Fisobs.Core;
using UnityEngine;
using static lsfUtils.Plugin;
using lsfUtils.Items.KnotSpawnV2;


namespace lsfUtils.Items.KnotSpawnV2;

public class KnotSpawnV2Abstract : AbstractConsumable
{
    public KnotSpawnV2Abstract(World world, WorldCoordinate pos, EntityID ID, int originRoom, int placedObjectIndex, PlacedObject.ConsumableObjectData consumableObjectData)
        : base(world, AbstractObjectType.DangleFruit, null, pos, ID, originRoom, placedObjectIndex, consumableObjectData)
    {
        Log.LogMessage("Spawning KnotSpawnV2Abstract!");
        type = Enums.AbstractObjectType.KnotSpawnV2;
        if (world is null) Log.LogMessage("World was null!");
    }

    public override void Realize()
    {
        Log.LogMessage("Realising!");
        base.Realize();
        realizedObject ??= new KnotSpawnV2(this);
        Log.LogMessage("Spawning knot spawn!");
    }

    public override string ToString()
    {
        return this.SaveToString("");
    }
}