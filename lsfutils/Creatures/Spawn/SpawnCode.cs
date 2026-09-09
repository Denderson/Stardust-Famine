

namespace lsfUtils.Creatures.Spawn;

public static class SpawnCode
{
    public static void VoidSpawnEgg_Pop(On.VoidSpawnEgg.orig_Pop orig, VoidSpawnEgg self)
    {
        self.room.game.session.creatureCommunities.InfluenceLikeOfPlayer(Enums.CreatureCommunityID.StarSpawn, self.room.world.RegionNumber, 0, 0.1f, 0.2f, 0.1f);
        orig(self);
    }

    public static void Player_Grabbed(On.Player.orig_Grabbed orig, Player self, Creature.Grasp grasp)
    {
        orig(self, grasp);
        if (grasp.grabber is StarSpawn && self.dangerGrasp == null)
        {
            self.dangerGraspTime = 0;
            self.dangerGrasp = grasp;
        }
    }
}
