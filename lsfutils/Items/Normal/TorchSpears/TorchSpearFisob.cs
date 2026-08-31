using Fisobs.Core;
using Fisobs.Items;
using Fisobs.Properties;
using Fisobs.Sandbox;
using UnityEngine;

namespace lsfUtils.Items.Normal.TorchSpears
{
    public class TorchSpearFisob : Fisob
    {
        public TorchSpearFisob() : base(Enums.AbstractObjectType.TorchSpear)
        {
            Icon = new SimpleIcon("Symbol_Boomerang", new(0.5f, 0.5f, 0.5f));
            SandboxPerformanceCost = new SandboxPerformanceCost(0.5f, 0.2f);
            RegisterUnlock(Enums.SandboxUnlockID.TorchSpear, MultiplayerUnlocks.SandboxUnlockID.Slugcat, 20);
        }

        public override AbstractPhysicalObject Parse(World world, EntitySaveData saveData, SandboxUnlock unlock)
        {
            return new TorchSpearAbstract(world, saveData.Pos, saveData.ID);
        }

        public override ItemProperties Properties(PhysicalObject forObject)
        {
            if (forObject is TorchSpear spear) return new TorchSpearProperties(spear);
            return null;
        }
    }

    public class TorchSpearProperties : ItemProperties
    {
        private readonly TorchSpear spear;

        public TorchSpearProperties(TorchSpear spear)
        {
            this.spear = spear;
        }
        public override void ScavCollectScore(Scavenger scav, ref int score) => score = spear.isLit ? 4 : 3;
        public override void ScavWeaponPickupScore(Scavenger scav, ref int score) => score = spear.isLit ? 4 : 3;
    }
}