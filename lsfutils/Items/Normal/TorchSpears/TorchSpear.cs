using BepInEx;
using RWCustom;
using System;
using UnityEngine;

namespace lsfUtils.Items.Normal.TorchSpears
{
    public class TorchSpear : Spear
    {
        public int clothSegments = 6;
        public Vector2[] clothPos;
        public Vector2[] lastClothPos;
        public Vector2[] clothVel;

        public int clothSpriteIndex = -1;
        public int knotSpriteIndex = -1;

        public LightSource lightSource;
        public float[,] flicker;
        public TorchFlameParticle flamePool;

        public bool isLit = true;
        public float emberGlow = 1f;
        public float extinguishFadeSpeed = 0.002f;

        public TorchSpear(AbstractSpear abstractObject, World world) : base(abstractObject, world)
        {
            clothPos = new Vector2[clothSegments];
            lastClothPos = new Vector2[clothSegments];
            clothVel = new Vector2[clothSegments];

            if (abstractObject is TorchSpearAbstract absTorch)
            {
                isLit = absTorch.isLit;
            }

            for (int i = 0; i < clothSegments; i++)
            {
                clothPos[i] = firstChunk.pos;
                lastClothPos[i] = firstChunk.pos;
            }

            flicker = new float[2, 3];
            for (int i = 0; i < flicker.GetLength(0); i++)
            {
                flicker[i, 0] = 1f;
                flicker[i, 1] = 1f;
                flicker[i, 2] = 1f;
            }
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            base.InitiateSprites(sLeaser, rCam);

            int baseIndex = sLeaser.sprites.Length;
            Array.Resize(ref sLeaser.sprites, baseIndex + 2);

            clothSpriteIndex = baseIndex;
            knotSpriteIndex = baseIndex + 1;

            TriangleMesh.Triangle[] clothTris = new TriangleMesh.Triangle[(clothSegments - 1) * 2];
            for (int i = 0; i < clothSegments - 1; i++)
            {
                int idx = i * 2;
                clothTris[i * 2] = new TriangleMesh.Triangle(idx, idx + 1, idx + 2);
                clothTris[i * 2 + 1] = new TriangleMesh.Triangle(idx + 1, idx + 2, idx + 3);
            }

            TriangleMesh clothMesh = new TriangleMesh("Futile_White", clothTris, true, true);

            for (int i = 0; i < clothSegments; i++)
            {
                float normalizedLength = (float)i / (clothSegments - 1);
                clothMesh.UVvertices[i * 2] = new Vector2(0f, normalizedLength);
                clothMesh.UVvertices[i * 2 + 1] = new Vector2(1f, normalizedLength);
            }

            clothMesh.shader = Custom.rainWorld.Shaders["ClothBurnShader"];

            sLeaser.sprites[clothSpriteIndex] = clothMesh;

            TriangleMesh.Triangle[] knotTris = new TriangleMesh.Triangle[]
            {
                new TriangleMesh.Triangle(0, 1, 2),
                new TriangleMesh.Triangle(1, 3, 2),
                new TriangleMesh.Triangle(2, 3, 4),
                new TriangleMesh.Triangle(3, 5, 4)
            };

            TriangleMesh knotMesh = new TriangleMesh("Futile_White", knotTris, true, true);

            knotMesh.UVvertices[0] = new Vector2(0f, 1f);
            knotMesh.UVvertices[1] = new Vector2(1f, 1f);
            knotMesh.UVvertices[2] = new Vector2(0f, 0.5f);
            knotMesh.UVvertices[3] = new Vector2(1f, 0.5f);
            knotMesh.UVvertices[4] = new Vector2(0f, 0f);
            knotMesh.UVvertices[5] = new Vector2(1f, 0f);

            knotMesh.shader = Custom.rainWorld.Shaders["ScorchBurnShader"];

            sLeaser.sprites[knotSpriteIndex] = knotMesh;

            AddToContainer(sLeaser, rCam, null);
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            for (int i = 0; i < clothSegments; i++) lastClothPos[i] = clothPos[i];

            if (isLit && firstChunk.submersion > 0.1f)
            {
                Extinguish(true);
            }

            Vector2 attachPos = firstChunk.pos + rotation * 10f;
            clothPos[0] = attachPos;
            clothVel[0] = Vector2.zero;

            float gravity = 0.7f;
            float friction = 0.85f;
            float segmentLength = 5f;

            for (int i = 1; i < clothSegments; i++)
            {
                float tailFactor = (float)i / clothSegments;
                float wave = Mathf.Sin(Time.time * 4f - i * 0.5f);
                float draft = Mathf.Cos(Time.time * 2.5f + i * 0.3f);
                Vector2 wind = new Vector2(wave * 0.4f + draft * 0.15f, wave * 0.1f) * tailFactor;

                clothVel[i].y -= gravity;
                clothVel[i] += wind;
                clothVel[i] *= friction;
                clothPos[i] += clothVel[i];
            }

            for (int iter = 0; iter < 4; iter++)
            {
                for (int i = 1; i < clothSegments; i++)
                {
                    Vector2 diff = clothPos[i] - clothPos[i - 1];
                    float dist = diff.magnitude;

                    if (dist > segmentLength)
                    {
                        Vector2 dir = diff / dist;
                        Vector2 correction = dir * (dist - segmentLength);
                        clothPos[i] -= correction;
                        clothVel[i] -= correction * 0.3f;
                    }
                }
            }

            for (int i = 0; i < flicker.GetLength(0); i++)
            {
                flicker[i, 1] = flicker[i, 0];
                flicker[i, 0] += Mathf.Pow(UnityEngine.Random.value, 3f) * 0.1f * (UnityEngine.Random.value < 0.5f ? -1f : 1f);
                flicker[i, 0] = Custom.LerpAndTick(flicker[i, 0], flicker[i, 2], 0.05f, 0.033333335f);

                if (UnityEngine.Random.value < 0.2f)
                {
                    flicker[i, 2] = 1f + Mathf.Pow(UnityEngine.Random.value, 3f) * 0.2f * (UnityEngine.Random.value < 0.5f ? -1f : 1f);
                }
                flicker[i, 2] = Mathf.Lerp(flicker[i, 2], 1f, 0.01f);
            }

            Vector2 spearAxis = rotation.normalized;
            Vector2 knotPos = firstChunk.pos + spearAxis * 15f;

            if (room != null)
            {
                if (lightSource != null && (lightSource.slatedForDeletetion || lightSource.room != room))
                {
                    lightSource = null;
                }

                if (lightSource == null && (isLit || emberGlow > 0f))
                {
                    lightSource = new LightSource(knotPos, false, new Color(1f, 0.4f, 0.1f), this);
                    lightSource.affectedByPaletteDarkness = 0.5f;
                    room.AddObject(lightSource);
                }
                else if (lightSource != null)
                {
                    lightSource.setPos = knotPos;

                    if (isLit)
                    {
                        lightSource.setRad = 220f * flicker[0, 0];
                        lightSource.color = new Color(1f, 0.4f, 0.1f);
                        lightSource.setAlpha = 1f;
                    }
                    else
                    {
                        emberGlow = Mathf.Max(0f, emberGlow - extinguishFadeSpeed);
                        lightSource.setRad = 40f * emberGlow * flicker[0, 0];
                        lightSource.color = new Color(1f, 0.15f, 0f);
                        lightSource.setAlpha = emberGlow;
                        if (emberGlow <= 0f)
                        {
                            lightSource.Destroy();
                            lightSource = null;
                        }
                    }
                }

                Vector2 fireSpawnPos = knotPos + spearAxis * 2f;
                if (isLit && UnityEngine.Random.value < 0.75f)
                {
                    if (flamePool == null || flamePool.slatedForDeletetion || flamePool.room != room)
                    {
                        flamePool = new TorchFlameParticle();
                        room.AddObject(flamePool);
                    }
                    flamePool.Emit(fireSpawnPos, firstChunk.vel);
                }
            }
        }

        public override void Destroy()
        {
            base.Destroy();

            if (lightSource != null)
            {
                lightSource.Destroy();
                lightSource = null;
            }
        }

        public override void NewRoom(Room newRoom)
        {
            base.NewRoom(newRoom);

            if (lightSource != null)
            {
                lightSource = null;
            }

            Vector2 attachPos = firstChunk.pos + rotation.normalized * 10f; // fix cloth flinging when entering a new room
            for (int i = 0; i < clothSegments; i++)
            {
                clothPos[i] = attachPos;
                lastClothPos[i] = attachPos;
                clothVel[i] = Vector2.zero;
            }
        }

        public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
        {
            bool hit = base.HitSomething(result, eu);

            if (isLit && result.obj != null && result.obj is Creature)
            {
                Extinguish(false);
            }

            return hit;
        }

        public void Extinguish(bool byWater)
        {
            if (!isLit) return;

            extinguishFadeSpeed = byWater ? 0.02f : 0.002f;
            isLit = false;
            emberGlow = 1f;

            if (abstractPhysicalObject is TorchSpearAbstract absTorch)
            {
                absTorch.isLit = false;
            }

            if (room != null)
            {
                room.PlaySound(SoundID.Fire_Spear_Ignite, firstChunk.pos);

                for (int i = 0; i < 8; i++)
                {
                    Vector2 sparkVel = Custom.RNV() * UnityEngine.Random.value * 6f;
                    room.AddObject(new Spark(firstChunk.pos, sparkVel, new Color(1f, 0.4f, 0.1f), null, 15, 30));
                }

                Smoke.BombSmoke smoke = new Smoke.BombSmoke(room, firstChunk.pos, null, new Color(0.8f, 0.8f, 0.8f)); // le smoke that doesnt work half the time, fuck you
                room.AddObject(smoke);
                smoke.chunk = null;
                smoke.fadeIn = 1f;
                smoke.pos = firstChunk.pos;
                smoke.minParticleDistance = 0f;

                for (int k = 0; k < 30; k++)
                {
                    Vector2 spawnPos = firstChunk.pos + Custom.RNV() * UnityEngine.Random.Range(1f, 5f);
                    Vector2 spawnVel = Custom.RNV() * (UnityEngine.Random.value * 2f) + new Vector2(0f, 3f);

                    smoke.EmitWithMyLifeTime(spawnPos, spawnVel);
                }

                smoke.stationary = true;
                smoke.DisconnectSmoke();
            }
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

            if (clothSpriteIndex == -1 || sLeaser.sprites.Length <= knotSpriteIndex) return;

            Vector2 centerPos = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
            Vector2 currentRot = Vector3.Slerp(lastRotation, rotation, timeStacker);

            Vector2 spearAxis = currentRot.normalized;
            Vector2 attachPos = centerPos + spearAxis * 15f;
            Vector2 perpendicular = Custom.PerpendicularVector(spearAxis);

            float activeGlow = isLit ? 1f : emberGlow;
            Color scorchColor = new Color(0.1f, 0.1f, 0.1f, activeGlow);
            Color endColor = isLit ? new Color(0.76f, 0.70f, 0.60f, activeGlow) : new Color(0.15f, 0.15f, 0.15f, activeGlow);
            float currentKnotDepth = 5f;

            if (sLeaser.sprites[clothSpriteIndex] is TriangleMesh tailMesh)
            {
                float thickness = 2.5f;
                tailMesh.customColor = true;

                for (int i = 0; i < clothSegments; i++) // works
                {
                    Vector2 smoothPos = i == 0 ? attachPos : Vector2.Lerp(lastClothPos[i], clothPos[i], timeStacker);
                    Vector2 prevPos = i == 0 ? attachPos : Vector2.Lerp(lastClothPos[i - 1], clothPos[i - 1], timeStacker);
                    Vector2 nextPos = i == clothSegments - 1 ? smoothPos : Vector2.Lerp(lastClothPos[i + 1], clothPos[i + 1], timeStacker);

                    Vector2 dir = i == 0 ? (nextPos - smoothPos).normalized
                                : i == clothSegments - 1 ? (smoothPos - prevPos).normalized
                                : (nextPos - prevPos).normalized;

                    Vector2 clothPerp = Custom.PerpendicularVector(dir);
                    float currentThickness = i == clothSegments - 1 ? 0.5f : thickness;

                    tailMesh.MoveVertice(i * 2, smoothPos - clothPerp * currentThickness - camPos);
                    tailMesh.MoveVertice(i * 2 + 1, smoothPos + clothPerp * currentThickness - camPos);

                    Color currentColor = Color.Lerp(scorchColor, endColor, (float)i / (clothSegments - 1));
                    if (tailMesh.verticeColors != null)
                    {
                        tailMesh.verticeColors[i * 2] = currentColor;
                        tailMesh.verticeColors[i * 2 + 1] = currentColor;
                    }
                }
            }

            if (sLeaser.sprites[knotSpriteIndex] is TriangleMesh knotMesh)
            {
                float currentKnotWidth = 3f;

                Vector2 topCenter = attachPos + spearAxis * currentKnotDepth;
                Vector2 middleCenter = attachPos;
                Vector2 bottomCenter = attachPos - spearAxis * currentKnotDepth;

                knotMesh.MoveVertice(0, topCenter - perpendicular * currentKnotWidth - camPos);
                knotMesh.MoveVertice(1, topCenter + perpendicular * currentKnotWidth - camPos);
                knotMesh.MoveVertice(2, middleCenter - perpendicular * (currentKnotWidth * 1.3f) - camPos);
                knotMesh.MoveVertice(3, middleCenter + perpendicular * (currentKnotWidth * 1.3f) - camPos);
                knotMesh.MoveVertice(4, bottomCenter - perpendicular * currentKnotWidth - camPos);
                knotMesh.MoveVertice(5, bottomCenter + perpendicular * currentKnotWidth - camPos);

                knotMesh.color = scorchColor;
            }
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
        {
            base.AddToContainer(sLeaser, rCam, newContainer);

            if (clothSpriteIndex != -1 && sLeaser.sprites.Length > knotSpriteIndex)
            {
                FContainer container = newContainer ?? rCam.ReturnFContainer("Midground");
                FSprite tailMesh = sLeaser.sprites[clothSpriteIndex];
                FSprite wrapKnot = sLeaser.sprites[knotSpriteIndex];

                if (tailMesh != null)
                {
                    container.AddChild(tailMesh);
                    tailMesh.MoveBehindOtherNode(sLeaser.sprites[0]);
                }
                if (wrapKnot != null)
                {
                    container.AddChild(wrapKnot);
                    wrapKnot.MoveInFrontOfOtherNode(sLeaser.sprites[0]);
                }
            }
        }
    }
}