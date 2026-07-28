using lsfUtils.CWTs;
using System;
using static lsfUtils.Enums;

namespace lsfUtils.RegionParams
{
    public static class ScavengerParams
    {
        public static void ScavengerAbstractAI_InitGearUp(On.ScavengerAbstractAI.orig_InitGearUp orig, ScavengerAbstractAI self)
        {
            orig(self);

            if (!RegionCWT.TryGetCustomRegionParams(self?.world?.region, out var customRegionParams)) return;
            if (self?.parent is not AbstractCreature scav) return;

            if ((int)UnityEngine.Random.value * 100 < customRegionParams.ScavLanternChance)
            {
                AddItemToScav(scav, AbstractPhysicalObject.AbstractObjectType.Lantern);
            }
            if ((int)UnityEngine.Random.value * 100 < customRegionParams.ScavExplosiveBoomerangChance)
            {
                AddItemToScav(scav, Enums.AbstractObjectType.ExplosiveBoomerang);
            }
            if ((int)UnityEngine.Random.value * 100 < customRegionParams.ScavSingularityBoomerangChance)
            {
                AddItemToScav(scav, Enums.AbstractObjectType.SingularityBoomerang);
            }
            if ((int)UnityEngine.Random.value * 100 < customRegionParams.ScavPoisonDartChance)
            {
                AddItemToScav(scav, Enums.AbstractObjectType.PoisonDart);
            }
        }

        public static void AddItemToScav(AbstractCreature scav, AbstractPhysicalObject.AbstractObjectType itemType)
        {
            int highestUsedSlot = -1;
            foreach (AbstractPhysicalObject.AbstractObjectStick abstractObjectStick in scav.stuckObjects)
            {
                if (abstractObjectStick is AbstractPhysicalObject.CreatureGripStick grip && grip.A == scav)
                {
                    highestUsedSlot = System.Math.Max(highestUsedSlot, grip.grasp);
                }
            }

            int nextSlot = highestUsedSlot + 1;

            if (nextSlot > 3) return;

            AbstractPhysicalObject item = new(scav.world, itemType, null, scav.pos, scav.world.game.GetNewID());
            scav.world.GetAbstractRoom(scav.pos).AddEntity(item);
            new AbstractPhysicalObject.CreatureGripStick(scav, item, nextSlot, carry: true);
        }
    }
}