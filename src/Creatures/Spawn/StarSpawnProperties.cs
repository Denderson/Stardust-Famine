using Fisobs.Properties;
using MoreSlugcats;

namespace lsfUtils.Creatures.Spawn;

public class StarSpawnProperties : ItemProperties
{
    private readonly StarSpawn StarSpawn;

    public StarSpawnProperties(StarSpawn StarSpawn)
    {
        this.StarSpawn = StarSpawn;
    }

    public override void Grabability(Player player, ref Player.ObjectGrabability grabability)
    {
        if (!StarSpawn.dead && !StarSpawn.IsJelly())
        {
            grabability = Player.ObjectGrabability.CantGrab;
        }
        else if (StarSpawn.SmallerThan(player.TotalMass))
        {
            grabability = Player.ObjectGrabability.OneHand;
        }
        else
        {
            grabability = Player.ObjectGrabability.Drag;
        }
    }

    public override void Nourishment(Player player, ref int quarterPips)
    {
        if (player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint)
        {
            quarterPips = -1;
        }
        else
        {
            quarterPips = 4 * StarSpawn.FoodPoints;
        }
    }
}