using System;
using lsfUtils.CWTs;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace lsfUtils.DevtoolsObjects.BackgroundMud
{
    public static class BackgroundMudHooks
    {
        public static void ApplyHooks()
        {
            On.MudPit.SpawnBubbles += BackgroundMudHooks.MudPit_SpawnBubbles;
            On.MudPit.MudBubble.InitiateSprites += BackgroundMudHooks.MudBubble_InitiateSprites;
        }

        public static bool spawnBackgroundBubble = false;
        public static void MudPit_ApplyPalette(On.MudPit.orig_ApplyPalette orig, MudPit self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            if (self is BackgroundMud)
            {
                sLeaser.sprites[1].color = self.color;
                TriangleMesh triangleMesh = sLeaser.sprites[0] as TriangleMesh;
                for (int i = 0; i < triangleMesh.verticeColors.Length; i++)
                {
                    triangleMesh.verticeColors[i] = self.color;
                }
            }
            else
            {
                orig(self, sLeaser, rCam, palette);
            }
        }

        public static void MudPit_SpawnBubbles(On.MudPit.orig_SpawnBubbles orig, MudPit self)
        {
            spawnBackgroundBubble = self is BackgroundMud;
            orig(self);
        }

        public static void MudBubble_InitiateSprites(On.MudPit.MudBubble.orig_InitiateSprites orig, MudPit.MudBubble self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            orig(self, sLeaser, rCam);
            if (spawnBackgroundBubble)
            {
                spawnBackgroundBubble = false;

            }
        }
    }
}