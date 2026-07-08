using UnityEngine;

namespace lsfUtils.Creatures.Worm
{
    internal class ClimbGrubGraphics : TubeWormGraphics
    {
        public ClimbGrubGraphics(TubeWorm worm) : base(worm)
        {
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            base.ApplyPalette(sLeaser, rCam, palette);
            sLeaser.sprites[0].color = Color.white;
            sLeaser.sprites[1].color = new Color(1f, 0.45f, 0.7f); // pink
        }
    }
}