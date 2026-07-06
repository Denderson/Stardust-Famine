using Fisobs.Core;
using Fisobs.Items;
using Fisobs.Properties;
using Fisobs.Sandbox;
using UnityEngine;

namespace lsfUtils.Items.ExplosiveBoomerang
{
    public class ExplosiveBoomerangFisob : Fisob
    {
        public ExplosiveBoomerangFisob() : base(Enums.AbstractPhysicalObjectType.ExplosiveBoomerang)
        {
            Icon = new SimpleIcon("Symbol_Boomerang", new(1f, 0.4f, 0.3f));
            SandboxPerformanceCost = new SandboxPerformanceCost(0.5f, 0.2f);
            RegisterUnlock(Enums.SandboxUnlockID.ExplosiveBoomerang, MultiplayerUnlocks.SandboxUnlockID.Slugcat, 20);
        }

        public override AbstractPhysicalObject Parse(World world, EntitySaveData saveData, SandboxUnlock unlock)
        {
            return new AbstractExplosiveBoomerang(world, saveData.Pos, saveData.ID);
        }

        public override ItemProperties Properties(PhysicalObject forObject)
        {
            if (forObject is ExplosiveBoomerang boom) return new ExplosiveBoomerangProperties(boom);
            return null;
        }
    }

    public class ExplosiveBoomerangProperties : ItemProperties
    {
        private readonly ExplosiveBoomerang boom;

        public ExplosiveBoomerangProperties(ExplosiveBoomerang boom)
        {
            this.boom = boom;
        }
        public override void ScavCollectScore(Scavenger scav, ref int score) => score = 6;
        public override void ScavWeaponPickupScore(Scavenger scav, ref int score) => score = 6;
        public override void Grabability(Player player, ref Player.ObjectGrabability grabability) => grabability = Player.ObjectGrabability.OneHand;
    }
}