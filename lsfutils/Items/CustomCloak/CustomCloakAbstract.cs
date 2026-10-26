using UnityEngine;
using static lsfUtils.Enums;
using static lsfUtils.Plugin;

namespace lsfUtils.Items.CustomCloak;

public class CustomCloakAbstract : AbstractConsumable
{
    public CloakType cloakType = CloakType.Default;
    public CloakColor cloakColor = CloakColor.Default;

    public Color? cloakColorOverride = null;

    public CustomCloakAbstract(World world, WorldCoordinate pos, EntityID ID, int originRoom, int placedObjectIndex, PlacedObject.ConsumableObjectData consumableObjectData) : base(world, MoreSlugcats.MoreSlugcatsEnums.AbstractObjectType.MoonCloak, null, pos, ID, originRoom, placedObjectIndex, consumableObjectData)
    {
        Log.LogMessage("Spawning customcloakabstract!");
        type = Enums.AbstractObjectType.CustomCloak;
        if (world is null) Log.LogMessage("World was null!");
    }

    public override void Realize()
    {
        Log.LogMessage("Realising!");
        base.Realize();
        realizedObject ??= new CustomCloak(this, world);
        Log.LogMessage("Spawning custom cloak!");
    }
}