using Fisobs.Properties;
using lsfUtils.Items.Darts.PoisonDart;
using System;

public class PoisonDartProperties : ItemProperties
{
    private readonly PoisonDart dart;

    public PoisonDartProperties(PoisonDart poisonDart)
    {
        dart = poisonDart;
    }

    public override void ScavCollectScore(Scavenger scavenger, ref int score)
    {
        score = 1 + (int)Math.Round(dart.poison) * 2;
    }

    public override void ScavWeaponPickupScore(Scavenger scav, ref int score)
    {
        score = 1 + (int)Math.Round(dart.poison) * 2;
    }

    public override void Grabability(Player player, ref Player.ObjectGrabability grabability)
    {
        if (dart.mode == Weapon.Mode.StuckInCreature && dart.pullOutTimer > 0)
        {
            grabability = Player.ObjectGrabability.Drag;
            return;
        }
        grabability = Player.ObjectGrabability.OneHand;
    }
}