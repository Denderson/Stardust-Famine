using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace lsfUtils.DevtoolsObjects.FloatMud
{
    public class FloatMudBubble : CosmeticSprite
    {
        private float radius;
        private float floatHeight;
        private int age;
        private int popTime;
        private int riseTime;
        private int dieTime;
        private Color color;

        public FloatMudBubble(Vector2 pos, float radius, float floatHeight, int lifetime, Color color)
        {
            base.pos = pos;
            this.radius = radius;
            this.floatHeight = floatHeight;
            this.color = color;
            lastPos = pos;
            dieTime = lifetime;
            popTime = Mathf.Max(dieTime - 10, (dieTime + 1) / 2);
            riseTime = UnityEngine.Random.Range(lifetime / 4, lifetime / 2);
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            age++;
            if (age == popTime) Pop();
            if (age == dieTime) Destroy();
        }

        private void Pop()
        {
            int num = UnityEngine.Random.Range(0, 4);
            for (int i = 0; i < num; i++)
            {
                float y = Mathf.Lerp(5f, 12f, Mathf.Pow(UnityEngine.Random.value, 2f));
                Vector2 vector = Custom.RotateAroundOrigo(new Vector2(0f, y), UnityEngine.Random.Range(-45f, 45f));
                room.AddObject(new Spark(pos, vector, color, null, 80, 90));
            }
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite("Circle20")
            {
                color = color
            };
            AddToContainer(sLeaser, rCam, null);
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) { }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            float riseFrac = Mathf.InverseLerp(0f, riseTime, (float)age + timeStacker);
            float floatFrac = Mathf.InverseLerp(riseTime, popTime, (float)age + timeStacker);
            Vector2 ps = Vector2.Lerp(lastPos, pos, timeStacker) - new Vector2(0f, radius * (1f - riseFrac)) + new Vector2(0f, floatHeight * floatFrac);
            ps = rCam.ApplyDepth(ps, 5f);
            sLeaser.sprites[0].SetPosition(ps - camPos);
            sLeaser.sprites[0].scale = radius * riseFrac / 5.5f;
            sLeaser.sprites[0].scaleX *= 1f - Mathf.InverseLerp(popTime, dieTime, (float)age + timeStacker);
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }
    }
}
