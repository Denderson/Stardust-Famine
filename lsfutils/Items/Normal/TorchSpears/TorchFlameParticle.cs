using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace lsfUtils.Items.Normal.TorchSpears
{
    public class TorchFlameParticle : CosmeticSprite // added object pooling for optimizing, though tbh i have no clue if theres any reason for adding this lmao, just wasted 2 hours
    {
        private class FlameData
        {
            public bool active = false;
            public Vector2 pos, lastPos, vel;
            public float life, maxLife, size;
        }

        private FlameData[] flames;
        private int maxFlames = 30;
        private int idleTimer = 0;

        public TorchFlameParticle()
        {
            flames = new FlameData[maxFlames];
            for (int i = 0; i < maxFlames; i++)
            {
                flames[i] = new FlameData();
            }
        }

        public void Emit(Vector2 startPos, Vector2 startVel, float sizeMult = 1f)
        {
            for (int i = 0; i < maxFlames; i++)
            {
                if (!flames[i].active)
                {
                    flames[i].pos = startPos + Custom.RNV() * UnityEngine.Random.value * 1.2f;
                    flames[i].lastPos = flames[i].pos;
                    flames[i].vel = startVel * 0.15f + Custom.RNV() * UnityEngine.Random.value * 1.5f + new Vector2(0f, 1.2f);
                    flames[i].maxLife = Mathf.Lerp(14f, 26f, UnityEngine.Random.value);
                    flames[i].life = 1f;
                    flames[i].size = Mathf.Lerp(14f, 24f, UnityEngine.Random.value) * sizeMult;
                    flames[i].active = true;
                    return;
                }
            }
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            int activeCount = 0;

            for (int i = 0; i < maxFlames; i++)
            {
                if (!flames[i].active) continue;

                activeCount++;

                flames[i].lastPos = flames[i].pos;
                flames[i].pos += flames[i].vel;

                flames[i].vel.y += Mathf.Lerp(0.15f, 0.45f, 1f - flames[i].life);
                flames[i].vel.x += (UnityEngine.Random.value - 0.5f) * 1.2f;
                flames[i].vel.x *= 0.75f;
                flames[i].vel.y *= 0.90f;

                flames[i].life -= 1f / flames[i].maxLife;
                if (flames[i].life <= 0f)
                {
                    flames[i].active = false;
                }
            }

            if (activeCount == 0)
            {
                idleTimer++;
                if (idleTimer > 80)
                {
                    Destroy();
                }
            }
            else
            {
                idleTimer = 0;
            }
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[maxFlames];

            for (int i = 0; i < maxFlames; i++)
            {
                sLeaser.sprites[i] = new FSprite("Futile_White");
                sLeaser.sprites[i].isVisible = false;

                sLeaser.sprites[i].shader = Custom.rainWorld.Shaders["ProceduralFireShader"];
            }

            AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            for (int i = 0; i < maxFlames; i++)
            {
                if (!flames[i].active)
                {
                    sLeaser.sprites[i].isVisible = false;
                    continue;
                }

                sLeaser.sprites[i].isVisible = true;

                Vector2 smoothPos = Vector2.Lerp(flames[i].lastPos, flames[i].pos, timeStacker);
                sLeaser.sprites[i].SetPosition(smoothPos - camPos);

                float scaleCurve = Mathf.Pow(flames[i].life, 0.5f);
                sLeaser.sprites[i].scale = flames[i].size * scaleCurve / 12f;

                Color pColor;
                if (flames[i].life > 0.65f)
                    pColor = Color.Lerp(new Color(1f, 0.65f, 0.15f), new Color(1f, 1f, 0.9f), (flames[i].life - 0.65f) / 0.35f);
                else if (flames[i].life > 0.3f)
                    pColor = Color.Lerp(new Color(0.9f, 0.15f, 0.0f), new Color(1f, 0.65f, 0.15f), (flames[i].life - 0.3f) / 0.4f);
                else
                    pColor = Color.Lerp(new Color(0.3f, 0.0f, 0.0f), new Color(0.9f, 0.15f, 0.0f), flames[i].life / 0.3f);

                pColor.a = flames[i].life;
                sLeaser.sprites[i].color = pColor;
            }

            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
        {
            base.AddToContainer(sLeaser, rCam, newContainer ?? rCam.ReturnFContainer("Foreground"));
        }
    }
}
