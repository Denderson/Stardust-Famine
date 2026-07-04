using ScavengerCosmetic;
using UnityEngine;
using static lsfUtils.Plugin;

namespace lsfUtils.Creatures.Scavs.ScavSeer
{
    public class SeerHalo : Template
    {
        public readonly SeerHaloUniforms uniforms = new();

        public BodyChunk headChunk;

        public bool activated;

        public SeerHalo(ScavengerGraphics owner, int firstSprite) : base(owner, firstSprite)
        {
            totalSprites = 1;
            headChunk = scavGrphs.scavenger.mainBodyChunk;

            uniforms.SetActiveSlotCount(3);
            uniforms.SetSlot(0, 0, 1, false);
            uniforms.SetSlot(1, 1, 2, true);
            uniforms.SetSlot(2, 2, 0, false);
        }

        public override void Update()
        {
            base.Update();

            bool shouldBeActive = scavGrphs.scavenger.Consious && (scavGrphs.scavenger as ScavSeer).haloActivationTime > 0;

            if (shouldBeActive && !activated)
            {
                uniforms.Activate();
                activated = true;
            }
            else if (!shouldBeActive)
            {
                activated = false;
            }
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites[firstSprite] = new FSprite("Futile_White", true)
            {
                shader = rCam.game.rainWorld.Shaders["SeerHalo"],
                scale = 35f
            };
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 pos = Vector2.Lerp(headChunk.lastPos, headChunk.pos, timeStacker);
            sLeaser.sprites[firstSprite].SetPosition(pos - camPos);

            uniforms.PushGlobals();
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            sLeaser.sprites[firstSprite].color = RainWorld.SaturatedGold;
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            rCam.ReturnFContainer("ForegroundLights").AddChild(sLeaser.sprites[firstSprite]);
        }
    }
}