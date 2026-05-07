using Fisobs.Properties;
using System;

namespace lsfUtils.Items.PoisonDart
{
    public class PoisonDartProperties : ItemProperties
    {
        private readonly PoisonDart PoisonDart;

        public PoisonDartProperties(PoisonDart poisonDart)
        {
            PoisonDart = poisonDart;
        }

        public override void ScavCollectScore(Scavenger scavenger, ref int score)
        {
            score = 1 + (int)Math.Round(PoisonDart.poison) * 2;
        }
        public override void ScavWeaponPickupScore(Scavenger scav, ref int score)
        {
            score = 1 + (int)Math.Round(PoisonDart.poison) * 2;
        }

        public override void Grabability(Player player, ref Player.ObjectGrabability grabability)
        {
            if (PoisonDart.mode == Weapon.Mode.StuckInCreature)
            {
                if (PoisonDart.pullOutTimer > 0)
                {
                    grabability = Player.ObjectGrabability.Drag;
                    return;
                }
            }
            grabability = Player.ObjectGrabability.OneHand;
        }
    }

}
