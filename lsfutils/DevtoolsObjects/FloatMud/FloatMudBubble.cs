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
        public Vector2 basePos;

        public FloatMudBubble(Vector2 pos, float radius, float floatHeight, int lifetime, Color color)
        {
            base.pos = pos;
            basePos = pos;
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
            lastPos = pos;
            base.Update(eu);
            age++;

            float riseFrac = Mathf.InverseLerp(0f, riseTime, age);
            float floatFrac = Mathf.InverseLerp(riseTime, popTime, age);
            pos = basePos - new Vector2(0f, radius * (1f - riseFrac)) + new Vector2(0f, floatHeight * floatFrac);

            if (age == popTime) Pop();
        }

        public void Pop()
        {
            int num = UnityEngine.Random.Range(3, 7);
            for (int i = 0; i < num; i++)
            {
                room.AddObject(new WaterDrip(pos, Custom.RNV() * Random.value * 14f, waterColor: false));
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
            Vector2 ps = Vector2.Lerp(lastPos, pos, timeStacker);
            ps = rCam.ApplyDepth(ps, 5f);
            sLeaser.sprites[0].SetPosition(ps - camPos);
            sLeaser.sprites[0].scale = radius * riseFrac / 5.5f;
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }
    }
}