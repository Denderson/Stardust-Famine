using RWCustom;
using UnityEngine;

namespace lsfUtils.Creatures.Misc.ClimbGrub;

public class ClimbGrubAI : ArtificialIntelligence
{
    public int destCounter;

    public ClimbGrub grub;

    public ClimbGrubAI(AbstractCreature creature, World world) : base(creature, world)
    {
        grub = creature?.realizedCreature as ClimbGrub;
        grub.AI = this;
        AddModule(new StandardPather(this, world, creature));
        AddModule(new Tracker(this, 10, 10, 600, 0.5f, 5, 5, 20));
    }

    public override void NewRoom(Room room)
    {
        base.NewRoom(room);
        destCounter = 0;
    }

    public override void Update()
    {
        base.Update();
        destCounter--;
        if (destCounter < 1 || Custom.ManhattanDistance(creature.pos, pathFinder.GetDestination) < 6)
        {
            RandomDest();
        }
    }

    public void RandomDest()
    {
        WorldCoordinate coord = new(grub.room.abstractRoom.index, Random.Range(0, grub.room.TileWidth), Random.Range(0, grub.room.TileHeight), -1);
        if (pathFinder.CoordinateReachableAndGetbackable(coord))
        {
            creature.abstractAI.SetDestination(coord);
            destCounter = Random.Range(400, 1200);
        }
    }
}