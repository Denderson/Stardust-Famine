using lsfUtils.CWTs;
using MonoMod.RuntimeDetour;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace lsfUtils.Creatures.Worm
{
    public static class ClimbGrubHooks
    {
        public static Color yellowTintColor = new(0.75f, 0.95f, 0.25f);
        public static void Apply()
        {
            new Hook(typeof(TubeWorm.Tongue).GetMethod("Shoot", BindingFlags.Public | BindingFlags.Instance), (Tongue_Shoot_Orig orig, TubeWorm.Tongue self, Vector2 dir) => Tongue_Shoot(orig, self, dir));

            On.Player.Update += Player_Update;
            On.PlayerGraphics.ApplyPalette += PlayerGraphics_ApplyPalette;
        }

        private delegate void Tongue_Shoot_Orig(TubeWorm.Tongue self, Vector2 dir);

        private static void Tongue_Shoot(Tongue_Shoot_Orig orig, TubeWorm.Tongue self, Vector2 dir)
        {
            if (self.worm is ClimbGrub)
            {
                return;
            }
            orig(self, dir);
        }

        private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);
            if (!PlayerCWT.TryGetData(self, out var data)) return;
            if (data.freeClimbTimer > 0)
            {
                data.freeClimbTimer--;
            }
        }

        private static void PlayerGraphics_ApplyPalette(On.PlayerGraphics.orig_ApplyPalette orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig(self, sLeaser, rCam, palette);

            if (!PlayerCWT.TryGetData(self.player, out var data)) return;
            if (data.freeClimbTimer <= 0) return;

            float fadeOut = Mathf.InverseLerp(0f, 120f, data.freeClimbTimer);
            float strength = 0.5f * fadeOut;

            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                sLeaser.sprites[i].color = Color.Lerp(sLeaser.sprites[i].color, yellowTintColor, strength);
            }
        }
    }
}