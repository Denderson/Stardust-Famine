using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreSlugcats;
using Noise;
using RWCustom;
using Smoke;
using UnityEngine;
using static lsfUtils.Plugin;

namespace lsfUtils.Items.PoisonDart
{
    public class PoisonDart : Spear
    {
        public static readonly float poisonPrecentagePerTick = 0.005f;

        public float remainingPoison;
        public int pullOutCounter;
        public int pullOutAttempts;

        public AbstractCreature shotFrom;
        private static float Rand => Random.value;

        public PoisonDartAbstract abstractDart { get; }

        public PoisonDart(PoisonDartAbstract abstr, float remainingPoison) : base(abstr, abstr.world)
        {
            abstractDart = abstr;
            bodyChunks = new BodyChunk[1];
            bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 2f, 0.02f);
            bodyChunkConnections = new BodyChunkConnection[0];
            airFriction = 0.999f;
            gravity = 0.9f;
            bounce = 0.6f;
            surfaceFriction = 0.4f;
            collisionLayer = 2;
            waterFriction = 0.98f;
            buoyancy = 0.4f;
            pivotAtTip = false;
            lastPivotAtTip = false;
            stuckBodyPart = -1;
            firstChunk.loudness = 5f;
            tailPos = firstChunk.pos;
            soundLoop = new ChunkDynamicSoundLoop(firstChunk);
            wasHorizontalBeam = new bool[3];
            jollyCustomColor = null;

            this.remainingPoison = remainingPoison;
            Log.LogMessage("Remaining poison from ctor: " +  this.remainingPoison);
            pullOutAttempts = 0;
            pullOutCounter = 0;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            if (abstractDart.remainingPoison > 0f)
            {
                if (mode == Mode.StuckInCreature && stuckInObject != null)
                {
                    if (stuckInObject is Creature creature)
                    {
                        creature.InjectPoison(poisonPrecentagePerTick, Enums.Colors.PoisonColor);
                        abstractDart.remainingPoison = Mathf.Max(abstractDart.remainingPoison - poisonPrecentagePerTick, 0f);
                    }
                }
            }
        }

        public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
        {
            if (result.obj?.abstractPhysicalObject == null || result.obj?.abstractPhysicalObject == shotFrom)
            {
                return false;
            }
            if (result.obj.abstractPhysicalObject.rippleLayer != abstractPhysicalObject.rippleLayer && !result.obj.abstractPhysicalObject.rippleBothSides && !abstractPhysicalObject.rippleBothSides)
            {
                return false;
            }
            if (result.obj is Creature)
            {
                if ((result.obj as Creature).SpearStick(this, Mathf.Lerp(0.55f, 0.62f, Random.value), result.chunk, result.onAppendagePos, firstChunk.vel))
                {
                    (result.obj as Creature).Violence(firstChunk, firstChunk.vel * (firstChunk.mass * 2f), result.chunk, result.onAppendagePos, Creature.DamageType.Stab, 0.1f, 20f);
                }
            }
            else if (result.chunk != null)
            {
                result.chunk.vel += firstChunk.vel * firstChunk.mass / result.chunk.mass;
            }
            else if (result.onAppendagePos != null)
            {
                (result.obj as IHaveAppendages).ApplyForceOnAppendage(result.onAppendagePos, firstChunk.vel * firstChunk.mass);
            }
            if (result.obj is Creature && (result.obj as Creature).SpearStick(this, Mathf.Lerp(0.55f, 0.62f, Random.value), result.chunk, result.onAppendagePos, firstChunk.vel))
            {
                Creature creature = result.obj as Creature;
                room.PlaySound(SoundID.Spear_Stick_In_Creature, firstChunk.pos, 0.5f, 0.5f);
                LodgeInCreature(result, eu);
                return true;
            }
            room.PlaySound(SoundID.Spear_Bounce_Off_Creauture_Shell, firstChunk);
            vibrate = 20;
            ChangeMode(Mode.Free);
            firstChunk.vel = firstChunk.vel * -0.5f + Custom.DegToVec(Random.value * 360f) * (Mathf.Lerp(0.1f, 0.4f, Random.value) * firstChunk.vel.magnitude);
            SetRandomSpin();
            return false;
        }

        public override void Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
        {
            base.Thrown(thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            sLeaser.sprites[0].color = Color.red;
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            Color waterShineColor = palette.waterShineColor;
            Color blackColor = palette.blackColor;
            color = Color.Lerp(waterShineColor, blackColor, 0.6f);
            sLeaser.sprites[0].color = Color.green;
        }

        public override void ChangeMode(Mode newMode)
        {
            if (newMode == Mode.StuckInWall)
            {
                newMode = Mode.Free;
            }
            if (newMode == Mode.StuckInCreature)
            {
                pullOutCounter = 120;
                pullOutAttempts = 0;
            }
            base.ChangeMode(newMode);
        }
    }
}
