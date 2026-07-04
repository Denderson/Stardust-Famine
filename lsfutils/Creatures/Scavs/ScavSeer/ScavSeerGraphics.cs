using System;
using UnityEngine;

namespace lsfUtils.Creatures.Scavs.ScavSeer
{
    public class ScavSeerGraphics : ScavengerGraphics
    {
        public SeerHalo halo;

        private readonly int haloFirstSprite;
        private bool haloSpritesReady;

        public ScavSeerGraphics(ScavSeer Seer) : base(Seer)
        {
            haloFirstSprite = TotalSprites;
            halo = new SeerHalo(this, haloFirstSprite);
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            if (!haloSpritesReady)
            {
                Array.Resize(ref sLeaser.sprites, haloFirstSprite + halo.totalSprites);
                halo.InitiateSprites(sLeaser, rCam);
                haloSpritesReady = true;
            }
            base.AddToContainer(sLeaser, rCam, newContatiner);
            halo.AddToContainer(sLeaser, rCam, newContatiner);
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            base.ApplyPalette(sLeaser, rCam, palette);
            halo.ApplyPalette(sLeaser, rCam, palette);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPosV2)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPosV2);
            halo.DrawSprites(sLeaser, rCam, timeStacker, camPosV2);
        }

        public override void Update()
        {
            base.Update();
            halo.Update();
        }
    }
}