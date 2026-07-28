using Fisobs.Core;
using Fisobs.Items;
using Fisobs.Properties;
using Fisobs.Sandbox;
using UnityEngine;

namespace lsfUtils.Items.ExplosiveBoomerang
{
    public class SingularityBoomerangFisob : Fisob
    {
        public SingularityBoomerangFisob() : base(Enums.AbstractObjectType.SingularityBoomerang)
        {
            Icon = new SimpleIcon("Symbol_Boomerang", new(0.2f, 0.2f, 1f));
            SandboxPerformanceCost = new SandboxPerformanceCost(0.5f, 0.2f);
            RegisterUnlock(Enums.SandboxUnlockID.SingularityBoomerang, MultiplayerUnlocks.SandboxUnlockID.Slugcat, 20);
        }

        public override AbstractPhysicalObject Parse(World world, EntitySaveData saveData, SandboxUnlock unlock)
        {
            return new AbstractExplosiveBoomerang(world, saveData.Pos, saveData.ID, true);
        }

        public override ItemProperties Properties(PhysicalObject forObject)
        {
            if (forObject is ExplosiveBoomerang boom) return new SingularityBoomerangProperties(boom);
            return null;
        }
    }

    public class SingularityBoomerangProperties : ItemProperties
    {
        private readonly ExplosiveBoomerang boom;

        public SingularityBoomerangProperties(ExplosiveBoomerang boom)
        {
            this.boom = boom;
        }

        public override void ScavCollectScore(Scavenger scav, ref int score) => score = 10;
        public override void ScavWeaponPickupScore(Scavenger scav, ref int score) => score = 10;

        public override void Grabability(Player player, ref Player.ObjectGrabability grabability)
        {
            if (boom.mode == Weapon.Mode.Thrown) grabability = Player.ObjectGrabability.CantGrab;
            else grabability = Player.ObjectGrabability.OneHand;
        }
    }
}