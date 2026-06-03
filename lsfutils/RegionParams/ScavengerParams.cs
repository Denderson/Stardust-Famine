using lsfUtils.CWTs;

namespace lsfUtils.RegionParams
{
    public static class ScavengerParams
    {
        public static void ScavengerAbstractAI_InitGearUp(On.ScavengerAbstractAI.orig_InitGearUp orig, ScavengerAbstractAI self)
        {
            orig(self);

            if (!RegionCWT.TryGetCustomRegionParams(self?.world?.region, out var customRegionParams))
                return;

            if (customRegionParams.CreepingDarknessScavLantern)
            {
                int highestUsedSlot = -1;
                foreach (AbstractPhysicalObject.AbstractObjectStick abstractObjectStick in self.parent.stuckObjects)
                {
                    if (abstractObjectStick is AbstractPhysicalObject.CreatureGripStick grip && grip.A == self.parent)
                    {
                        highestUsedSlot = System.Math.Max(highestUsedSlot, grip.grasp);
                    }
                }

                int nextSlot = highestUsedSlot + 1;

                if (nextSlot > 3)
                    return;

                AbstractPhysicalObject lantern = new(self.world, AbstractPhysicalObject.AbstractObjectType.Lantern, null, self.parent.pos, self.world.game.GetNewID());
                self.world.GetAbstractRoom(self.parent.pos).AddEntity(lantern);
                new AbstractPhysicalObject.CreatureGripStick(self.parent, lantern, nextSlot, carry: true);
            }
        }
    }
}