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
        public Color explodeColor = new(1f, 0.4f, 0.3f);
        public Color singularityColor = new(0.2f, 0.2f, 1f);

        public bool ignited;
        public BombSmoke smoke;
        public float burn;

        public bool isSingularity;

        private const int SpriteGlow = 3;

        public Color ExplosionColor => isSingularity ? singularityColor : explodeColor;

        public ExplosiveBoomerang(AbstractPhysicalObject abstractPhysicalObject, World world, bool isSingularity) : base(abstractPhysicalObject, world)
        {
            this.isSingularity = isSingularity;
            firstChunk.mass = 0.15f;
            ignited = false;
            smoke = null;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            if (ignited || burn > 0f)
            {
                if (!isSingularity && Submersion == 1f && room.waterObject != null)
                {
                    if (!room.waterObject.WaterIsLethal)
                    {
                        ignited = false;
                        burn = 0f;
                    }
                    else
                    {
                        ignited = true;
                    }
                }

                if (ignited && burn == 0f && mode != Mode.Thrown) burn = 0.5f + Random.value * 0.5f;

                for (int i = 0; i < 2; i++)
                {
                    room.AddObject(new Spark(Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, Random.value), firstChunk.vel * 0.1f + Custom.RNV() * 3f * Random.value, ExplosionColor, null, 7, 25));
                }

                if (smoke == null)
                {
                    smoke = new BombSmoke(room, firstChunk.pos, firstChunk, ExplosionColor);
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
                    Explode(null);
                    ignited = false;
                    burn = 0f;
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
            ignited = false;
            burn = 0f;
            room.PlaySound(SoundID.Slugcat_Pick_Up_Bomb, firstChunk);
        }

        public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
        {
            bool hit = base.HitSomething(result, eu);
            if (hit)
            {
                Explode(result.chunk);
            }
            return hit;
        }

        public override void HitWall()
        {
            Explode(null);
            base.HitWall();
        }

        public void Explode(BodyChunk hitChunk)
        {
            if (slatedForDeletetion) return;

            Vector2 pos = Vector2.Lerp(firstChunk.pos, firstChunk.lastPos, 0.35f);

            if (isSingularity)
                ExplodeSingularity(pos);
            else
                ExplodeNormal(pos, hitChunk);

            room.PlaySound(SoundID.Bomb_Explode, pos, abstractPhysicalObject);
            room.InGameNoise(new InGameNoise(pos, 7000f, this, 1f));

            for (int m = 0; m < abstractPhysicalObject.stuckObjects.Count; m++)
                abstractPhysicalObject.stuckObjects[m].Deactivate();
        }

        private void ExplodeNormal(Vector2 pos, BodyChunk hitChunk)
        {
            room.AddObject(new SootMark(room, pos, 80f, true));
            room.AddObject(new Explosion(room, this, pos, 7, 250f, 6.2f, 2f, 280f, 0.25f, thrownBy, 0.7f, 160f, 1f));
            room.AddObject(new Explosion.ExplosionLight(pos, 280f, 1f, 7, explodeColor));
            room.AddObject(new Explosion.ExplosionLight(pos, 230f, 1f, 3, Color.white));
            room.AddObject(new ExplosionSpikes(room, pos, 14, 30f, 9f, 7f, 170f, explodeColor));
            room.AddObject(new ShockWave(pos, 330f, 0.045f, 5));

            for (int i = 0; i < 25; i++)
            {
                Vector2 dir = Custom.RNV();
                if (room.GetTile(pos + dir * 20f).Solid && !room.GetTile(pos - dir * 20f).Solid) dir *= -1f;

                for (int j = 0; j < 3; j++)
                {
                    room.AddObject(new Spark(pos + dir * Mathf.Lerp(30f, 60f, Random.value), dir * Mathf.Lerp(7f, 38f, Random.value) + Custom.RNV() * 20f * Random.value, Color.Lerp(explodeColor, Color.white, Random.value), null, 11, 28));
                }
                room.AddObject(new Explosion.FlashingSmoke(pos + dir * 40f * Random.value, dir * Mathf.Lerp(4f, 20f, Mathf.Pow(Random.value, 2f)), 1f + 0.05f * Random.value, Color.white, explodeColor, Random.Range(3, 11)));
            }

            for (int k = 0; k < 6; k++)
            {
                room.AddObject(new SingularityBomb.BombFragment(pos, Custom.DegToVec(((float)k + Random.value) / 6f * 360f) * Mathf.Lerp(18f, 38f, Random.value)));
            }

            if (smoke != null)
            {
                for (int k = 0; k < 8; k++)
                {
                    smoke.EmitWithMyLifeTime(pos + Custom.RNV(), Custom.RNV() * Random.value * 17f);
                }
            }

            room.ScreenMovement(pos, default, 1.3f);

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
            else
            {
                smoke?.Destroy();
                smoke = null;
            }
        }

        private void ExplodeSingularity(Vector2 pos)
        {
            room.AddObject(new SingularityBomb.SparkFlash(firstChunk.pos, 300f, new Color(0f, 0f, 1f)));
            room.AddObject(new Explosion(room, this, pos, 7, 450f, 6.2f, 10f, 280f, 0.25f, thrownBy, 0.3f, 160f, 1f));
            room.AddObject(new Explosion(room, this, pos, 7, 2000f, 4f, 0f, 400f, 0.25f, thrownBy, 0.3f, 200f, 1f));
            room.AddObject(new Explosion.ExplosionLight(pos, 280f, 1f, 7, singularityColor));
            room.AddObject(new Explosion.ExplosionLight(pos, 230f, 1f, 3, Color.white));
            room.AddObject(new Explosion.ExplosionLight(pos, 2000f, 2f, 60, singularityColor));
            room.AddObject(new ShockWave(pos, 350f, 0.485f, 300, highLayer: true));
            room.AddObject(new ShockWave(pos, 2000f, 0.185f, 180));

            for (int i = 0; i < 25; i++)
            {
                Vector2 dir = Custom.RNV();
                if (room.GetTile(pos + dir * 20f).Solid)
                {
                    if (!room.GetTile(pos - dir * 20f).Solid) dir *= -1f;
                    else dir = Custom.RNV();
                }
                for (int j = 0; j < 3; j++)
                {
                    room.AddObject(new Spark(pos + dir * Mathf.Lerp(30f, 60f, Random.value), dir * Mathf.Lerp(7f, 38f, Random.value) + Custom.RNV() * 20f * Random.value, Color.Lerp(singularityColor, Color.white, Random.value), null, 11, 28));
                }
                room.AddObject(new Explosion.FlashingSmoke(pos + dir * 40f * Random.value, dir * Mathf.Lerp(4f, 20f, Mathf.Pow(Random.value, 2f)), 1f + 0.05f * Random.value, Color.white, singularityColor, Random.Range(3, 11)));
            }

            for (int k = 0; k < 6; k++)
            {
                room.AddObject(new SingularityBomb.BombFragment(pos, Custom.DegToVec(((float)k + Random.value) / 6f * 360f) * Mathf.Lerp(18f, 38f, Random.value)));
            }

            for (int m = 0; m < room.physicalObjects.Length; m++)
            {
                for (int n = 0; n < room.physicalObjects[m].Count; n++)
                {
                    if (room.physicalObjects[m][n].abstractPhysicalObject.rippleLayer != abstractPhysicalObject.rippleLayer
                        && !room.physicalObjects[m][n].abstractPhysicalObject.rippleBothSides
                        && !abstractPhysicalObject.rippleBothSides) continue;

                    if (room.physicalObjects[m][n] is Creature c && Custom.Dist(c.firstChunk.pos, firstChunk.pos) < 350f)
                    {
                        if (thrownBy != null) c.killTag = thrownBy.abstractCreature;
                        c.Die();
                    }
                }
            }

            room.ScreenMovement(pos, default, 0.9f);

            smoke?.Destroy();
            smoke = null;
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            base.InitiateSprites(sLeaser, rCam);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            if (slatedForDeletetion || room != rCam.room || sLeaser?.sprites == null || sLeaser.sprites.Length <= SpriteGlow) return;

            sLeaser.sprites[0].color = ExplosionColor;
            sLeaser.sprites[2].color = Color.Lerp(ExplosionColor, Color.black, 0.4f);

            Vector2 pos = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
            sLeaser.sprites[SpriteGlow].x = pos.x - camPos.x;
            sLeaser.sprites[SpriteGlow].y = pos.y - camPos.y;
            sLeaser.sprites[SpriteGlow].color = ExplosionColor;
            sLeaser.sprites[SpriteGlow].alpha = ignited ? Mathf.Lerp(0.15f, 0.45f, Mathf.Pow(Random.value, 2f)) : 0f;
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            base.ApplyPalette(sLeaser, rCam, palette);
            if (slatedForDeletetion || room != rCam.room || sLeaser?.sprites == null || sLeaser.sprites.Length <= SpriteGlow) return;
            sLeaser.sprites[0].color = ExplosionColor;
            sLeaser.sprites[2].color = Color.Lerp(ExplosionColor, Color.black, 0.4f);
            sLeaser.sprites[SpriteGlow].color = ExplosionColor;
        }
    }
}