using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using lsfUtils.Items.Dart;
using MoreSlugcats;
using Noise;
using RWCustom;
using Smoke;
using UnityEngine;
using static lsfUtils.Plugin;
using static lsfUtils.Items.Dart.Dart;

namespace lsfUtils.Items.PoisonDart
{
    public class PoisonDart : Dart.Dart
    {
        public static readonly float poisonPrecentagePerTick = 0.005f;
        public static float Rand => Random.value;

        public PoisonDart(AbstractDart abstr) : base(abstr)
        {
            Log.LogMessage("Remaining poison from ctor: " + poison);
        }

        public override void Update(bool eu)
        {
            Update(eu);
            if (poison > 0f)
            {
                if (mode == Mode.StuckInCreature && stuckInObject != null)
                {
                    if (stuckInObject is Creature creature)
                    {
                        creature.InjectPoison(poisonPrecentagePerTick, Enums.Colors.PoisonColor);
                        poison = Mathf.Max(poison - poisonPrecentagePerTick, 0f);
                    }
                }
            }
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            DrawSprites(sLeaser, rCam, timeStacker, camPos);
            sLeaser.sprites[0].color = Color.red;
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            Color waterShineColor = palette.waterShineColor;
            Color blackColor = palette.blackColor;
            color = Color.Lerp(waterShineColor, blackColor, 0.6f);
            sLeaser.sprites[0].color = Color.green;
        }
    }
}
