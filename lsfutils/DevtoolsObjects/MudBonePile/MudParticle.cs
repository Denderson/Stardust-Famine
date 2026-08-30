using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace lsfUtils.DevtoolsObjects.MudBonePile
{
    public class MudParticle : CosmeticSprite // copy pasted from my other particle, just renamed
    {
        private float life;
        private float lifeTime;
        private float scale;
        private float rotation;
        private float rotSpeed;
        private Color color;

        public MudParticle(Vector2 p, Vector2 v, Color c)
        {
            pos = p;
            lastPos = p;
            vel = v;
            color = c;
            lifeTime = UnityEngine.Random.Range(15f, 35f);
            life = 1f;
            scale = UnityEngine.Random.Range(0.6f, 1.4f);
            rotation = UnityEngine.Random.Range(0f, 360f);
            rotSpeed = UnityEngine.Random.Range(-5f, 5f);
        }

        public override void Update(bool eu)
        {
            lastPos = pos;
            life -= 1f / lifeTime;
            rotation += rotSpeed;

            vel.y -= 0.6f;
            vel.x *= 0.95f; //adj

            pos += vel;

            if (room.GetTile(pos).Solid)
            {
                vel.y *= -0.1f;
                vel.x *= 0.3f;
                pos.y = room.MiddleOfTile(pos).y + 10f;
            }

            if (life <= 0f)
            {
                Destroy();
            }

            base.Update(eu);
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite("pixel", true);
            AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 drawPos = Vector2.Lerp(lastPos, pos, timeStacker) - camPos;
            float currentAlpha = Mathf.Pow(Mathf.Max(0f, life), 0.5f);

            sLeaser.sprites[0].x = drawPos.x;
            sLeaser.sprites[0].y = drawPos.y;
            sLeaser.sprites[0].rotation = rotation;

            sLeaser.sprites[0].scaleX = scale * 2.5f;
            sLeaser.sprites[0].scaleY = scale * 2.5f + vel.magnitude * 0.4f;
            sLeaser.sprites[0].rotation = Custom.VecToDeg(vel);

            sLeaser.sprites[0].color = color;
            sLeaser.sprites[0].alpha = currentAlpha;

            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) { }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
        {
            newContainer = newContainer ?? rCam.ReturnFContainer("Midground");
            newContainer.AddChild(sLeaser.sprites[0]);
        }
    }
}
