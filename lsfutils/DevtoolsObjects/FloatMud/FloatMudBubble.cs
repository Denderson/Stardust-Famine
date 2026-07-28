using RWCustom;
using UnityEngine;

namespace lsfUtils.DevtoolsObjects.FloatMud
{
    public class FloatMudBubble : CosmeticSprite
    {
        public float radius;
        public float floatHeight;
        public int age;
        public int popTime;
        public int riseTime;
        public int dieTime;
        public Color color;

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
        }

        public void Pop()
        {
            int num = UnityEngine.Random.Range(3, 7);
            for (int i = 0; i < num; i++)
            {
                float speed = Mathf.Lerp(4f, 9f, UnityEngine.Random.value);
                Vector2 vel = Custom.RotateAroundOrigo(new Vector2(0f, speed), UnityEngine.Random.Range(0f, 360f));
                room.AddObject(new WaterDrip(pos, vel, false));
            }
            Destroy();
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
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }
    }
}