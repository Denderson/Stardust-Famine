using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace lsfUtils.Creatures.Scavs.ScavSeer
{
    public class ScavSeerGraphics : ScavengerGraphics
    {
        public SeerHalo halo;
        public ScavSeerGraphics(ScavSeer Seer) : base(Seer) 
        {
            halo = new SeerHalo(this, this.TotalSprites);
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
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

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            base.InitiateSprites(sLeaser, rCam);
            halo.InitiateSprites(sLeaser, rCam);
        }

        public override void Update()
        {
            base.Update();
            halo.Update();
        }
    }
}
