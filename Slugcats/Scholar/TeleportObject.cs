using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Noise;
using RWCustom;
using UnityEngine;
using Watcher;

namespace Stardust.Slugcats.Scholar
{

    public class TeleportPoint : UpdatableAndDeletable, IDrawable
    {

        public PhysicalObject sourceObject;

        public bool fading;

        private readonly float shakeAmount = 0;

        private Vector2 pos;

        public float rad;

        private float lastShakeAmount;

        private Vector2 lastPos;

        private float alpha;

        private float lastAlpha;

        public float fader;

        public TeleportPoint(PhysicalObject sourceObject)
        {
            this.sourceObject = sourceObject;
            rad = 15f;
            pos = sourceObject.firstChunk.pos;
            lastPos = pos;
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite(HUD.KarmaMeter.KarmaSymbolSprite(small: false, new IntVector2(9, 9)));
            AddToContainer(sLeaser, rCam, null);
            AddToContainer(sLeaser, rCam, null);
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer container)
        {
            if (container == null)
            {
                container = rCam.ReturnFContainer("GrabShaders");
            }
            FSprite[] sprites = sLeaser.sprites;
            foreach (FSprite node in sprites)
            {
                container.AddChild(node);
            }
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            float g = Mathf.Lerp(lastShakeAmount, shakeAmount, timeStacker);
            sLeaser.sprites[0].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
            sLeaser.sprites[0].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
            sLeaser.sprites[0].scale = rad / 8f * 1.2f;
            //sLeaser.sprites[0].color = new Color((float)1f / 10f, g, 0f, Mathf.Lerp(lastAlpha, alpha, timeStacker));
            if (!sLeaser.deleteMeNextFrame && (base.slatedForDeletetion || room != rCam.room))
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            Color a = Color.Lerp(RainWorld.GoldRGB, Color.yellow, 0.25f + Mathf.Sin(fader) / 5f);
            a = Color.Lerp(a, palette.blackColor, 0.6f - Mathf.Sin(fader) / 5f);
            sLeaser.sprites[0].color = a;
        }

        public override void Update(bool eu)
        {
            lastPos = pos;
            lastShakeAmount = shakeAmount;
            lastAlpha = alpha;
            if (true)
            {
                alpha = 1f;
            }
            /*else
            {
                alpha = Custom.LerpAndTick(alpha, 0.5f, 0.05f, 0.02f);
            }
            if (shakeAmount > 0f )
            {
                shakeAmount = Mathf.Max(shakeAmount - 0.01f, 0f);
            }
            else if (false)
            {
                shakeAmount = Mathf.Min(shakeAmount + 0.015f, 1f);
                if (shakeAmount >= 1f)
                {
                    //Explode();
                }
            }
            if (false)
            {
                shakeAmount = Mathf.Clamp01(shakeAmount + 0.5f);
                room.AddObject(new TemplarCircle(sourceObject, pos, rad, 7f, 0f, 25, followSource: true)
                {
                    radDamping = 0.1f
                });
            }
            */
            if (sourceObject.slatedForDeletetion || sourceObject.room != room)
            {
                Destroy();
            }
            //pos = sourceObject.firstChunk.pos;
            fader += 0.02f;
            base.Update(eu);
        }

        private Spark SpawnSpark(Vector2 pos, Vector2 vel)
        {
            if (room.GetTile(pos).Solid)
            {
                return null;
            }
            Spark spark = new Spark(pos, vel, Color.Lerp(RainWorld.SaturatedGold, Color.white, UnityEngine.Random.value), null, 20, 30);
            room.AddObject(spark);
            return spark;
        }
    }
}
