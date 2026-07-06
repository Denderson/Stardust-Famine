using MoreSlugcats;
using Noise;
using RWCustom;
using Smoke;
using UnityEngine;
using Watcher;

namespace lsfUtils.Items.ExplosiveBoomerang
{
    public class ExplosiveBoomerang : Boomerang
    {
        public Color explodeColor = new(1f, 0.55f, 0.1f);

        public bool ignited;
        public BombSmoke smoke;
        public float burn;

        public int explodeCooldown;

        private const int ExplodeCooldownDuration = 20;
        private const int SpriteGlow = 3;
        private const int TotalSprites = 4;

        public ExplosiveBoomerang(AbstractPhysicalObject abstractPhysicalObject, World world) : base(abstractPhysicalObject, world)
        {
            firstChunk.mass = 0.15f;
            ignited = false;
            smoke = null;
            explodeCooldown = ExplodeCooldownDuration;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            if (explodeCooldown > 0) explodeCooldown--;

            if (ignited || burn > 0f)
            {
                if (Submersion == 1f && room.waterObject != null && !room.waterObject.WaterIsLethal)
                {
                    ignited = false;
                    burn = 0f;
                }

                if (ignited && burn == 0f && mode != Mode.Thrown) burn = 0.5f + Random.value * 0.5f;

                for (int i = 0; i < 2; i++)
                {
                    room.AddObject(new Spark(Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, Random.value), firstChunk.vel * 0.1f + Custom.RNV() * 3f * Random.value, explodeColor, null, 7, 25));
                }

                if (smoke == null)
                {
                    smoke = new BombSmoke(room, firstChunk.pos, firstChunk, explodeColor);
                    room.AddObject(smoke);
                }
            }
            else
            {
                smoke?.Destroy();
                smoke = null;
            }

            if (burn > 0f)
            {
                burn -= 1f / 30f;
                if (burn <= 0f)
                {
                    ignited = false;
                    Explode(null);
                }
            }
        }

        public override void Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
        {
            base.Thrown(thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);
            ignited = true;
        }

        public override void PickedUp(Creature upPicker)
        {
            room.PlaySound(SoundID.Slugcat_Pick_Up_Bomb, firstChunk);
        }

        public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
        {
            if (result.obj == null) return false;

            if (result.obj.abstractPhysicalObject.rippleLayer != abstractPhysicalObject.rippleLayer && !result.obj.abstractPhysicalObject.rippleBothSides && !abstractPhysicalObject.rippleBothSides) return false;

            if (thrownBy is Scavenger && thrownBy == result.obj)
            {
                SetValuesBack();
                return false;
            }

            vibrate = 20;

            if (result.obj is Creature creature)
            {
                creature.Violence(firstChunk, firstChunk.vel * firstChunk.mass, result.chunk, result.onAppendagePos, Creature.DamageType.Explosion, 0.6f, 60f);

                if (explodeCooldown == 0) Explode(result.chunk);
            }
            else if (result.chunk != null)
            {
                result.chunk.vel += firstChunk.vel * firstChunk.mass / result.chunk.mass;
            }
            else if (result.onAppendagePos != null)
            {
                (result.obj as IHaveAppendages).ApplyForceOnAppendage(result.onAppendagePos, firstChunk.vel * firstChunk.mass);
            }

            return true;
        }

        public override void HitWall()
        {
            bool wasThrown = mode == Mode.Thrown;
            base.HitWall();

            if (wasThrown && mode == Mode.Free && explodeCooldown == 0) Explode(null);
        }

        public void Explode(BodyChunk hitChunk)
        {
            if (slatedForDeletetion) return;
            if (explodeCooldown > 0) return;

            Vector2 pos = Vector2.Lerp(firstChunk.pos, firstChunk.lastPos, 0.35f);

            room.AddObject(new SootMark(room, pos, 70f, true));
            room.AddObject(new Explosion(room, null, pos, 7, 200f, 5f, 1.5f, 220f, 0.25f, thrownBy, 0.7f, 130f, 1f));
            room.AddObject(new Explosion.ExplosionLight(pos, 240f, 1f, 7, explodeColor));
            room.AddObject(new Explosion.ExplosionLight(pos, 180f, 1f, 3, Color.white));
            room.AddObject(new ExplosionSpikes(room, pos, 10, 25f, 7f, 6f, 130f, explodeColor));
            room.AddObject(new ShockWave(pos, 260f, 0.04f, 5));

            for (int i = 0; i < 18; i++)
            {
                Vector2 dir = Custom.RNV();
                if (room.GetTile(pos + dir * 20f).Solid && !room.GetTile(pos - dir * 20f).Solid) dir *= -1f;

                for (int j = 0; j < 2; j++)
                {
                    room.AddObject(new Spark(pos + dir * Mathf.Lerp(20f, 50f, Random.value), dir * Mathf.Lerp(6f, 30f, Random.value) + Custom.RNV() * 15f * Random.value, Color.Lerp(explodeColor, Color.white, Random.value), null, 10, 25));
                }

                room.AddObject(new Explosion.FlashingSmoke(pos + dir * 35f * Random.value, dir * Mathf.Lerp(3f, 16f, Mathf.Pow(Random.value, 2f)), 1f + 0.05f * Random.value, Color.white, explodeColor, Random.Range(3, 10)));
            }

            if (smoke != null)
            {
                for (int k = 0; k < 6; k++)
                {
                    smoke.EmitWithMyLifeTime(pos + Custom.RNV(), Custom.RNV() * Random.value * 14f);
                }
            }

            room.ScreenMovement(pos, default, 1.1f);

            bool hitNearWall = hitChunk != null;
            for (int n = 0; n < 5; n++)
            {
                if (room.GetTile(pos + Custom.fourDirectionsAndZero[n].ToVector2() * 20f).Solid)
                {
                    hitNearWall = true;
                    break;
                }
            }

            if (hitNearWall)
            {
                if (smoke == null)
                {
                    smoke = new BombSmoke(room, pos, null, explodeColor);
                    room.AddObject(smoke);
                }
                smoke.chunk = hitChunk;
                smoke.pos = pos;
                smoke.stationary = true;
                if (hitChunk == null) smoke.fadeIn = 1f;
                smoke.DisconnectSmoke();
                smoke = null;
            }

            room.PlaySound(SoundID.Bomb_Explode, pos, abstractPhysicalObject);
            room.InGameNoise(new InGameNoise(pos, 7000f, this, 1f));

            for (int m = 0; m < abstractPhysicalObject.stuckObjects.Count; m++) abstractPhysicalObject.stuckObjects[m].Deactivate();

            explodeCooldown = ExplodeCooldownDuration;
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[TotalSprites];
            base.InitiateSprites(sLeaser, rCam);

            sLeaser.sprites[SpriteGlow] = new FSprite("Futile_White")
            {
                shader = rCam.game.rainWorld.Shaders["JaggedCircle"],
                scale = (firstChunk.rad + 1f) / 10f,
                alpha = 0f,
            };

            AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            if (slatedForDeletetion || room != rCam.room) return;

            Vector2 pos = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);

            sLeaser.sprites[SpriteGlow].x = pos.x - camPos.x;
            sLeaser.sprites[SpriteGlow].y = pos.y - camPos.y;
            sLeaser.sprites[SpriteGlow].color = explodeColor;
            sLeaser.sprites[SpriteGlow].alpha = ignited ? Mathf.Lerp(0.15f, 0.45f, Mathf.Pow(Random.value, 2f)) : 0f;
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            base.ApplyPalette(sLeaser, rCam, palette);
            sLeaser.sprites[SpriteGlow].color = explodeColor;
        }
    }
}