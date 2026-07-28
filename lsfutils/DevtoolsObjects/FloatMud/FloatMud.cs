using lsfUtils.CWTs;
using RWCustom;
using Unity.Mathematics;
using UnityEngine;

namespace lsfUtils.DevtoolsObjects.FloatMud
{
    public class FloatMud : MudPit
    {
        private readonly PlacedObject myPObj;
        private Rect MyRect => new(myPObj.pos, Data.handlePos);

        public static Color defaultFloatMudColor = new(0.31f, 0.19f, 0.13f);

        public FloatMud(PlacedObject pObj) : base(pObj)
        {
            myPObj = pObj;
            color = GetMudColor();
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            Rect rect = MyRect;
            for (int i = 0; i < room.physicalObjects[1].Count; i++)
            {
                if (room.physicalObjects[1][i] is Creature crit && crit.muddy > 0)
                {
                    Rect other = new Rect(crit.firstChunk.pos, Vector2.zero).CloneWithExpansion(crit.collisionRange);
                    if (!rect.Overlaps(other)) continue;

                    if (!CreatureCWT.TryGetData(crit, out var data)) continue;
                    data.floatingMudTimer = math.clamp(data.floatingMudTimer + 16, 0, FloatMudHooks.maxFloatingMudTimer);
                }
            }

            if (UnityEngine.Random.value < rect.width / 4000f)
            {
                Vector2 pos = new(UnityEngine.Random.Range(rect.xMin, rect.xMax), rect.yMax);
                if (!room.GetTile(pos).Solid)
                {
                    float radius = UnityEngine.Random.Range(2.5f, 5.5f);
                    int lifetime = UnityEngine.Random.Range(60, 180);
                    room.AddObject(new FloatMudBubble(pos, radius, 60f, lifetime, Color.Lerp(GetMudColor(), Color.black, UnityEngine.Random.Range(0.3f, 0.6f))));
                }
            }
        }
        public Color GetMudColor()
        {
            if (RegionCWT.TryGetCustomRegionParams(this.room?.world?.region, out var customRegionParams) && customRegionParams.FloatMudColor != null) return customRegionParams.FloatMudColor.Value;
            return defaultFloatMudColor;
        }
    }
}