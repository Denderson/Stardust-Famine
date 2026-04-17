using ScavengerCosmetic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static lsfUtils.Plugin;

namespace lsfUtils.Creatures.Scavs.ScavSeer
{
    public class SeerHalo : Template
    {
        public int rows;

        public int lines;

        public float haloBaseAlpha;

        public float haloFluxAlpha;

        private const int divs = 64;

        public int attachedChunkIndex;

        public BodyChunk headChunk;

        public int animCounter;

        public SeerHalo(ScavengerGraphics owner, int firstSprite) : base(owner, firstSprite)
        {
            this.firstSprite = firstSprite;
            this.owner = owner;
            totalSprites = 2;
            headChunk = scavGrphs.scavenger.mainBodyChunk;
            animCounter = 0;
        }

        public override void Update()
        {
            base.Update();
            if (scavGrphs.scavenger.Consious && (scavGrphs.scavenger as ScavSeer).haloActivationTime > 0)
            {
                haloBaseAlpha = Mathf.Lerp(haloBaseAlpha, 1f, Mathf.Lerp(0.01f, 0.1f, haloBaseAlpha));
            }
            else
            {
                haloBaseAlpha = Mathf.Lerp(haloBaseAlpha, 0f, 0.1f);
            }
            if (UnityEngine.Random.value < 0.1f)
            {
                haloFluxAlpha = Mathf.Lerp(0.6f, 1f, UnityEngine.Random.value);
            }
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {

            TriangleMesh.Triangle[] array = new TriangleMesh.Triangle[256];
            for (int i = 0; i < 64; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    int num = i * 4 + j * 2;
                    int num2 = i * 3 + j;
                    int num3 = (i + 1) % 64 * 3 + j;
                    array[num] = new TriangleMesh.Triangle(num2, num2 + 1, num3);
                    array[num + 1] = new TriangleMesh.Triangle(num3, num2 + 1, num3 + 1);
                }
            }
            sLeaser.sprites[firstSprite] = new TriangleMesh("Futile_White", array, customColor: false)
            {
                shader = rCam.game.rainWorld.Shaders["WeaverGlow"]
            };
            sLeaser.sprites[firstSprite + 1] = new FSprite("Futile_White")
            {
                shader = rCam.game.rainWorld.Shaders["FlatLightNoisy"]
            };
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            TriangleMesh triangleMesh = sLeaser.sprites[firstSprite] as TriangleMesh;
            Vector2 vector = Vector2.Lerp(headChunk.lastPos, headChunk.pos, timeStacker);
            float num2 = 20f;
            float num3 = 50f;
            float num4 = 100f + (400f - 150f * UnityEngine.Random.value * Mathf.Sqrt(0)) * Mathf.SmoothStep(0f, 1f, 0);
            float num5 = (Mathf.PerlinNoise((animCounter + timeStacker) / 40f * 0.2f, 20f) - 0.46f) * 2f * 0.5f;
            float f = (Mathf.PerlinNoise((animCounter + timeStacker) / 40f * 0.1f + 0.5f, 40f) - 0.46f) * 2f;
            f = Mathf.Pow(Mathf.Abs(f), 0.75f) * Mathf.Sign(f);
            f = Mathf.Lerp(f, 0f, Mathf.Sqrt(0));
            for (int i = 0; i < 64; i++)
            {
                float num7 = i / 64f;
                Vector2 vector2 = new Vector2(Mathf.Cos((0f - num7) * (float)Math.PI * 2f), Mathf.Sin((0f - num7) * (float)Math.PI * 2f));
                for (int j = 0; j < 3; j++)
                {
                    float y = j / 2f;
                    triangleMesh.MoveVertice(i * 3 + j, vector + vector2 * j switch
                    {
                        1 => num3,
                        0 => num2,
                        _ => num4,
                    } - camPos);
                    triangleMesh.UVvertices[i * 3 + j] = new Vector2(Mathf.Lerp(-0.1f, -0.05f, f) + Mathf.Pow(Mathf.PingPong(num7 * 8f, 1f), 2f + f) * (0.25f + num5 * 0.1f) + Mathf.Pow(Mathf.PingPong(num7 * 8f + 1f, 1f), 2f + f) * (0.25f - num5 * 0.1f), y);
                }
            }
            triangleMesh.alpha = Mathf.Lerp(0.25f, 1f, Mathf.Pow(0, 2f));
            sLeaser.sprites[firstSprite + 1].SetPosition(vector - camPos);
            sLeaser.sprites[firstSprite + 1].scale = 50f;
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            sLeaser.sprites[firstSprite].color = RainWorld.SaturatedGold;
            sLeaser.sprites[firstSprite + 1].color = RainWorld.GoldRGB;
            sLeaser.sprites[firstSprite + 1].alpha = 0.1f;
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            if (sLeaser == null)
            {
                Log.LogMessage("sleaser is null!");
            }
            if (sLeaser.sprites == null)
            {
                Log.LogMessage("sprites is null!");
            }
            if (sLeaser.sprites.Length < firstSprite)
            {
                Log.LogMessage("sprites is short!!");
            }
            rCam.ReturnFContainer("Water").AddChild(sLeaser.sprites[firstSprite]);
            rCam.ReturnFContainer("ForegroundLights").AddChild(sLeaser.sprites[firstSprite + 1]);
        }
    }
}
