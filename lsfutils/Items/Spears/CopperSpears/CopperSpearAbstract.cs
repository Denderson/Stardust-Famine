using UnityEngine;

namespace lsfUtils.Items.Spears.CopperSpears;

public class CopperSpearAbstract : AbstractSpear
{
    public int oxidization = 0;

    public const int maxOxidation = 6;
    public CopperSpearAbstract(World world, Spear realizedObject, WorldCoordinate pos, EntityID ID, bool explosive, float hue) : base(world, realizedObject, pos, ID, explosive, hue)
    {
        type = Enums.AbstractObjectType.CopperSpear;
        if (world?.game != null && world.game.IsArenaSession) oxidization = Random.Range(0, maxOxidation + 1);
    }

    public CopperSpearAbstract(World world, WorldCoordinate pos, EntityID ID) : base(world, null, pos, ID, false, 0f)
    {
        type = Enums.AbstractObjectType.CopperSpear;
    }

    public override string ToString()
    {
        string baseString = base.ToString();
        return baseString + "<oA>" + oxidization;
    }
}
