using lsfUtils.CWTs;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace lsfUtils.Creatures.Worm
{
    public static class ClimbGrubHooks
    {
        public static Color yellowTintColor = new(0.75f, 0.95f, 0.25f);

        public const float wallBounceFactorX = 0.05f;
        public const float wallBounceFactorY = 1.1f;

        public static void Player_WallJump(On.Player.orig_WallJump orig, Player self, int direction)
        {
            if (!PlayerCWT.TryGetData(self, out var data) || data.freeClimbTimer <= 0)
            {
                orig(self, direction);
                return;
            }

            Vector2 velBefore0 = self.bodyChunks[0].vel;
            Vector2 velBefore1 = self.bodyChunks[1].vel;

            orig(self, direction);

            Vector2 delta0 = self.bodyChunks[0].vel - velBefore0;
            Vector2 delta1 = self.bodyChunks[1].vel - velBefore1;

            self.bodyChunks[0].vel.x = velBefore0.x + delta0.x * wallBounceFactorX;
            self.bodyChunks[1].vel.x = velBefore1.x + delta1.x * wallBounceFactorX;

            self.bodyChunks[0].vel.y = velBefore0.y + delta0.y * wallBounceFactorY;
            self.bodyChunks[1].vel.y = velBefore1.y + delta1.y * wallBounceFactorY;

            self.jumpStun = 0;
        }

        public static void Tongue_Shoot(On.TubeWorm.Tongue.orig_Shoot orig, TubeWorm.Tongue self, Vector2 dir)
        {
            if (self?.worm != null && self.worm is ClimbGrub)
            {
                return;
            }
            orig(self, dir);
        }

        public static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);
            if (!PlayerCWT.TryGetData(self, out var data)) return;
            if (data.freeClimbTimer > 0)
            {
                data.freeClimbTimer--;
            }
        }

        public static void PlayerGraphics_ApplyPalette(On.PlayerGraphics.orig_ApplyPalette orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig(self, sLeaser, rCam, palette);

            if (!PlayerCWT.TryGetData(self.player, out var data)) return;
            if (data.freeClimbTimer <= 0) return;

            float fadeOut = Mathf.InverseLerp(0f, 120f, data.freeClimbTimer);

            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                sLeaser.sprites[i].color = Color.Lerp(sLeaser.sprites[i].color, yellowTintColor, fadeOut);
            }
        }
    }
}