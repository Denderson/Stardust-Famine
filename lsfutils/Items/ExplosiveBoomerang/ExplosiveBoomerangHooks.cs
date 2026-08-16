using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lsfUtils.Items.ExplosiveBoomerang
{
    public static class ExplosiveBoomerangHooks
    {
        public static void ApplyHooks()
        {
            On.Weapon.HitAnotherThrownWeapon += ExplosiveBoomerangHooks.Weapon_HitAnotherThrownWeapon;
        }
        public static void Weapon_HitAnotherThrownWeapon(On.Weapon.orig_HitAnotherThrownWeapon orig, Weapon self, Weapon obj)
        {
            orig(self, obj);
            if (self is ExplosiveBoomerang boom)
            {
                boom.Explode(obj.firstChunk);
            }
        }
    }
}
