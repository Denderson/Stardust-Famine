using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;
using Watcher;

namespace lsfUtils.Creatures.Spawn;

public class StarSpawnGraphics : ComplexGraphicsModule
{
    public class Antenna : GraphicsSubModule
    {
        public Vector2[,] segments;

        public float conRad;

        public float thickness;

        public float rigid;

        public float forceDirection;

        public float ang;

        public int rigidSegments;

        public StarSpawnGraphics spawnGraphics => owner as StarSpawnGraphics;

        public virtual Vector2 ResetPos => spawnGraphics.spawn.mainBody[0].pos;

        public virtual Vector2 ResetDir => Custom.RNV();

        public Antenna(ComplexGraphicsModule owner, int firstSprite, int segs, float conRad, float thickness, float ang, float rigid, int rigidSegments, float forceDirection)
            : base(owner, firstSprite)
        {
            this.conRad = conRad;
            this.thickness = thickness;
            this.rigid = rigid;
            this.rigidSegments = rigidSegments;
            this.forceDirection = forceDirection;
            this.ang = ang;
            totalSprites = 1;
            segments = new Vector2[segs, 3];
        }

        public override void Update()
        {
            base.Update();
            for (int i = 0; i < segments.GetLength(0); i++)
            {
                segments[i, 1] = segments[i, 0];
                segments[i, 0] += segments[i, 2];
                segments[i, 2] *= Custom.LerpMap(segments[i, 2].magnitude, 0.2f * spawnGraphics.spawn.sizeFac, 6f * spawnGraphics.spawn.sizeFac, 1f, 0.7f);
            }
            for (int j = 1; j < segments.GetLength(0); j++)
            {
                Vector2 vector = Custom.DirVec(segments[j, 0], segments[j - 1, 0]);
                float num = Vector2.Distance(segments[j, 0], segments[j - 1, 0]);
                Vector2 vector2 = vector * ((conRad - num) * 0.5f);
                segments[j, 0] -= vector2;
                segments[j, 2] -= vector2;
                segments[j - 1, 0] += vector2;
                segments[j - 1, 2] += vector2;
            }
            for (int k = 2; k < segments.GetLength(0); k++)
            {
                Vector2 vector3 = Custom.DirVec(segments[k, 0], segments[k - 2, 0]);
                segments[k, 2] -= vector3 * rigid;
                segments[k - 2, 2] += vector3 * rigid;
            }
        }

        public override void Reset()
        {
            base.Reset();
            Vector2 resetPos = ResetPos;
            Vector2 resetDir = ResetDir;
            for (int i = 0; i < segments.GetLength(0); i++)
            {
                segments[i, 0] = resetPos + resetDir * conRad;
                segments[i, 1] = segments[i, 0];
                segments[i, 2] *= 0f;
            }
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            base.InitiateSprites(sLeaser, rCam);
            sLeaser.sprites[firstSprite] = TriangleMesh.MakeLongMesh(segments.GetLength(0), pointyTip: false, customColor: true);
            sLeaser.sprites[firstSprite].shader = spawnGraphics.BodyShader;
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            Vector2 vector = Vector2.Lerp(segments[0, 1], segments[0, 0], timeStacker);
            vector += Custom.DirVec(Vector2.Lerp(segments[1, 1], segments[1, 0], timeStacker), vector) * (conRad * 0.3f);
            float num = 1f;
            for (int i = 0; i < segments.GetLength(0); i++)
            {
                float f = i / (float)(segments.GetLength(0) - 1);
                Vector2 vector2 = Vector2.Lerp(segments[i, 1], segments[i, 0], timeStacker);
                Vector2 normalized = (vector2 - vector).normalized;
                Vector2 vector3 = Custom.PerpendicularVector(normalized);
                float num2 = Vector2.Distance(vector2, vector) / 5f;
                float num3 = Mathf.Lerp(thickness, 0.5f, Mathf.Pow(f, 0.2f));
                Vector2 vector4 = vector3 * ((num + num3) * 0.5f);
                (sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4, vector - vector4 + normalized * num2 - camPos);
                (sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4 + 1, vector + vector4 + normalized * num2 - camPos);
                (sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 - vector3 * num3 - normalized * num2 - camPos);
                (sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + vector3 * num3 - normalized * num2 - camPos);
                vector = vector2;
                num = num3;
            }
            for (int j = 0; j < (sLeaser.sprites[firstSprite] as TriangleMesh).verticeColors.Length; j++)
            {
                (sLeaser.sprites[firstSprite] as TriangleMesh).verticeColors[j] = new Color(spawnGraphics.meshColor.r, spawnGraphics.meshColor.g, spawnGraphics.meshColor.b);
            }
            sLeaser.sprites[firstSprite].shader = spawnGraphics.BodyShader;
        }
    }

    public class FrontAntenna : Antenna
    {
        public float startLength;

        public override Vector2 ResetPos => spawnGraphics.spawn.mainBody[0].pos;

        public override Vector2 ResetDir => Custom.DegToVec(Custom.AimFromOneVectorToAnother(spawnGraphics.spawn.mainBody[1].pos, spawnGraphics.spawn.mainBody[0].pos) + ang);

        public FrontAntenna(ComplexGraphicsModule owner, int firstSprite, int segs, float conRad, float thickness, float ang, float rigid, int rigidSegments, float forceDirection)
            : base(owner, firstSprite, segs, conRad, thickness, ang, rigid, rigidSegments, forceDirection)
        {
        }

        public override void Update()
        {
            base.Update();
            segments[0, 2] *= 0f;
            Vector2 vector = Custom.DegToVec(Custom.AimFromOneVectorToAnother(spawnGraphics.spawn.mainBody[1].pos, spawnGraphics.spawn.mainBody[0].pos) + ang);
            segments[0, 0] = spawnGraphics.spawn.mainBody[0].pos + vector * startLength;
            for (int i = 1; i < segments.GetLength(0) && i < rigidSegments; i++)
            {
                segments[i, 2] += vector * (forceDirection * Mathf.InverseLerp(rigidSegments, 1f, i));
            }
        }
    }

    public class TailAntenna : Antenna
    {
        public override Vector2 ResetPos => spawnGraphics.spawn.mainBody[spawnGraphics.spawn.mainBody.Length - 1].pos;

        public override Vector2 ResetDir => Custom.DegToVec(Custom.AimFromOneVectorToAnother(spawnGraphics.spawn.mainBody[spawnGraphics.spawn.mainBody.Length - 2].pos, spawnGraphics.spawn.mainBody[spawnGraphics.spawn.mainBody.Length - 1].pos) + ang);

        public TailAntenna(ComplexGraphicsModule owner, int firstSprite, int segs, float conRad, float thickness, float ang, float rigid, int rigidSegments, float forceDirection)
            : base(owner, firstSprite, segs, conRad, thickness, ang, rigid, rigidSegments, forceDirection)
        {
        }

        public override void Update()
        {
            base.Update();
            segments[0, 0] = spawnGraphics.spawn.mainBody[spawnGraphics.spawn.mainBody.Length - 1].pos;
            segments[0, 2] *= 0f;
            Vector2 vector = Custom.DegToVec(Custom.AimFromOneVectorToAnother(spawnGraphics.spawn.mainBody[spawnGraphics.spawn.mainBody.Length - 2].pos, spawnGraphics.spawn.mainBody[spawnGraphics.spawn.mainBody.Length - 1].pos) + ang);
            for (int i = 1; i < segments.GetLength(0) && i < rigidSegments; i++)
            {
                segments[i, 2] += vector * (forceDirection * Mathf.InverseLerp(rigidSegments, 1f, i));
            }
        }
    }

    public Vector2 glowPos;

    public float darkness;

    public List<Antenna> antennae;

    private int chunkResolution = 1;

    public Color meshColor;

    public StarSpawn spawn => owner as StarSpawn;

    public bool dayLightMode => spawn.dayLightMode;

    public int BodyMeshSprite => 0;

    public int GlowSprite => 1;

    public int EffectSprite => 2;

    private FShader BodyShader => Custom.rainWorld.Shaders["RippleSpawnBody"];

    private FShader EffectShader => Custom.rainWorld.Shaders["RippleGlow"];

    private FShader GlowShader => Custom.rainWorld.Shaders["FlatWaterLightRippleSpawn"];

    public StarSpawnGraphics(PhysicalObject owner) : base(owner, internalContainers: false)
    {
        totalSprites = 3;
        antennae = new List<Antenna>();
        float num = Mathf.Lerp(spawn.sizeFac, 0.5f + 0.5f * UnityEngine.Random.value, UnityEngine.Random.value);
        int num2;
        int segs;
        float num7;
        float num3;
        float forceDirection;
        float num5;
        if (spawn is StarJelly)
        {
            num2 = UnityEngine.Random.Range(8, 10);
            num3 = Mathf.Lerp(Mathf.Lerp(8f, 24f, UnityEngine.Random.value) * num2, Mathf.Lerp(16f, 45f, UnityEngine.Random.value), UnityEngine.Random.value);
            segs = UnityEngine.Random.Range(4, 14);
            float a = Mathf.Lerp(0.2f, 1f, UnityEngine.Random.value);
            for (int i = 0; i < num2; i++)
            {
                float num4 = i / (float)(num2 - 1);
                antennae.Add(new TailAntenna(this, totalSprites, segs, 12f * num * Mathf.Lerp(a, 1f, Mathf.Sin(num4 * (float)Math.PI)), spawn.mainBody[spawn.mainBody.Length - 1].rad, Mathf.Lerp(0f - num3, num3, num4), 0.1f * num, 2, 2.2f));
                AddSubModule(antennae[antennae.Count - 1]);
            }
        }
        else
        {
            switch (UnityEngine.Random.Range(0, 3))
            {
                case 0:
                    antennae.Add(new TailAntenna(this, totalSprites, UnityEngine.Random.Range(3, 18), 12f * num, spawn.mainBody[spawn.mainBody.Length - 1].rad, 0f, 0.1f * num, 2, 2.2f));
                    AddSubModule(antennae[antennae.Count - 1]);
                    break;
                case 1:
                    {
                        num2 = UnityEngine.Random.Range(2, UnityEngine.Random.Range(2, 5));
                        num3 = Mathf.Lerp(Mathf.Lerp(2f, 15f, UnityEngine.Random.value) * num2, Mathf.Lerp(8f, 70f, UnityEngine.Random.value), UnityEngine.Random.value);
                        segs = UnityEngine.Random.Range(3, 18);
                        float a = Mathf.Lerp(0.2f, 1f, UnityEngine.Random.value);
                        for (int k = 0; k < num2; k++)
                        {
                            float num9 = k / (float)(num2 - 1);
                            antennae.Add(new TailAntenna(this, totalSprites, segs, 12f * num * Mathf.Lerp(a, 1f, Mathf.Sin(num9 * (float)Math.PI)), spawn.mainBody[spawn.mainBody.Length - 1].rad, Mathf.Lerp(0f - num3, num3, num9), 0.1f * num, 2, 2.2f));
                            AddSubModule(antennae[antennae.Count - 1]);
                        }
                        break;
                    }
                case 2:
                    {
                        num2 = UnityEngine.Random.Range(2, 6);
                        num5 = Mathf.Lerp(0.1f, 1.8f, UnityEngine.Random.value);
                        num3 = Mathf.Lerp(Mathf.Lerp(2f, 15f, UnityEngine.Random.value) * num2, Mathf.Lerp(8f, 70f, UnityEngine.Random.value), UnityEngine.Random.value);
                        segs = UnityEngine.Random.Range(3, UnityEngine.Random.Range(5, 8));
                        int num6 = UnityEngine.Random.Range(1, segs + 1);
                        forceDirection = Mathf.Lerp(1.5f, 7f, UnityEngine.Random.value) * num5 / Mathf.Lerp(1f, num6, 0.5f);
                        num7 = Mathf.Lerp(4f, 12f, UnityEngine.Random.value) * num;
                        float a = Mathf.Lerp(0.2f, 1f, UnityEngine.Random.value);
                        for (int j = 0; j < num2; j++)
                        {
                            float num8 = j / (float)(num2 - 1);
                            antennae.Add(new TailAntenna(this, totalSprites, segs, num7 * Mathf.Lerp(a, 1f, Mathf.Sin(num8 * (float)Math.PI)), 2f, Mathf.Lerp(0f - num3, num3, num8), num5, num6, forceDirection));
                            AddSubModule(antennae[antennae.Count - 1]);
                        }
                        break;
                    }
            }
        }
            
        num2 = UnityEngine.Random.Range(2, UnityEngine.Random.Range(2, 7));
        segs = UnityEngine.Random.Range(2, UnityEngine.Random.Range(4, (int)Custom.LerpMap(spawn.mainBody.Length, 3f, 16f, 6f, 12f, 0.5f)));
        int num10 = segs;
        if (UnityEngine.Random.value < 0.5f)
        {
            num10 = UnityEngine.Random.Range(1, segs + 1);
        }
        num7 = Mathf.Lerp(3f, 8f, UnityEngine.Random.value);
        num3 = Mathf.Lerp(12f, 50f, Mathf.Pow(UnityEngine.Random.value, 1.5f));
        forceDirection = Mathf.Lerp(2f, 7f, UnityEngine.Random.value) / Mathf.Lerp(1f, num10, 0.5f);
        num5 = Mathf.Lerp(0.4f, 2.2f, UnityEngine.Random.value);
        for (int l = 0; l < num2; l++)
        {
            float t = l / (float)(num2 - 1);
            antennae.Add(new FrontAntenna(this, totalSprites, segs, num7, 2f * num, Mathf.Lerp(0f - num3, num3, t), num5, num10, forceDirection));
            AddSubModule(antennae[antennae.Count - 1]);
        }
        Reset();
    }

    public override void Update()
    {
        if (!spawn.culled)
        {
            base.Update();
        }
        if (owner.room == null)
        {
            return;
        }
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[totalSprites];
        base.InitiateSprites(sLeaser, rCam);
        sLeaser.sprites[BodyMeshSprite] = TriangleMesh.MakeLongMesh(spawn.mainBody.Length * chunkResolution, pointyTip: false, customColor: true);
        sLeaser.sprites[BodyMeshSprite].shader = BodyShader;
        sLeaser.sprites[GlowSprite] = new FSprite("Futile_White");
        sLeaser.sprites[GlowSprite].shader = GlowShader;
        sLeaser.sprites[EffectSprite] = new FSprite("Futile_White");
        sLeaser.sprites[EffectSprite].shader = EffectShader;
        AddToContainer(sLeaser, rCam, null);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        Vector2 vector = Vector2.Lerp(spawn.mainBody[0].lastPos, spawn.mainBody[0].pos, timeStacker);
        if (!spawn.culled)
        {
            sLeaser.sprites[BodyMeshSprite].shader = BodyShader;
            sLeaser.sprites[EffectSprite].shader = EffectShader;

            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                sLeaser.sprites[i].isVisible = true;
            }
            Vector2 a = vector;
            if (dayLightMode)
            {
                meshColor = new Color(0.5f, 0.5f, 0f);
            }
            else
            {
                meshColor = new Color(0.7f + 0.3f * Mathf.InverseLerp(0.1f, 0.9f, darkness), Mathf.Lerp(Mathf.Lerp(Custom.LerpMap(darkness, 0.1f, 0.9f, 0.1f, 0.075f, 0.5f), 0.24f, 0f), 0.8f, 0f), 0f);
            }
            UpdateGlowSpriteColor(sLeaser);
            sLeaser.sprites[GlowSprite].x = a.x - camPos.x;
            sLeaser.sprites[GlowSprite].y = a.y - camPos.y;
            sLeaser.sprites[GlowSprite].scale = 1f * spawn.TotalMass * Mathf.Lerp(0.6f, 1f, darkness);
            sLeaser.sprites[GlowSprite].shader = GlowShader;
            sLeaser.sprites[EffectSprite].x = a.x - camPos.x;
            sLeaser.sprites[EffectSprite].y = a.y - camPos.y;
            sLeaser.sprites[EffectSprite].scale = 6f * spawn.TotalMass;
            vector += Custom.DirVec(Vector2.Lerp(spawn.mainBody[1].lastPos, spawn.mainBody[1].pos, timeStacker), vector) * spawn.mainBody[0].rad;
            float num = spawn.mainBody[0].rad / 2f;
            if (chunkResolution == 1)
            {
                for (int j = 0; j < spawn.mainBody.Length; j++)
                {
                    Vector2 vector2 = Vector2.Lerp(spawn.mainBody[j].lastPos, spawn.mainBody[j].pos, timeStacker);
                    Vector2 normalized = (vector2 - vector).normalized;
                    Vector2 vector3 = Custom.PerpendicularVector(normalized);
                    float num2 = Vector2.Distance(vector2, vector) / 5f;
                    float rad = spawn.mainBody[j].rad;
                    Vector2 vector4 = vector3 * ((num + rad) * 0.5f);
                    (sLeaser.sprites[BodyMeshSprite] as TriangleMesh).MoveVertice(j * 4, vector - vector4 + normalized * num2 - camPos);
                    (sLeaser.sprites[BodyMeshSprite] as TriangleMesh).MoveVertice(j * 4 + 1, vector + vector4 + normalized * num2 - camPos);
                    (sLeaser.sprites[BodyMeshSprite] as TriangleMesh).MoveVertice(j * 4 + 2, vector2 - vector3 * rad - normalized * num2 - camPos);
                    (sLeaser.sprites[BodyMeshSprite] as TriangleMesh).MoveVertice(j * 4 + 3, vector2 + vector3 * rad - normalized * num2 - camPos);
                    vector = vector2;
                    num = rad;
                }
            }
            else
            {
                Vector2[,] array = new Vector2[spawn.mainBody.Length + 1, 2];
                Vector2[,] array2 = new Vector2[spawn.mainBody.Length + 1, 2];
                Vector2 b = Vector2.zero;
                Vector2 b2 = Vector2.zero;
                for (int k = 0; k < spawn.mainBody.Length; k++)
                {
                    Vector2 vector5 = Vector2.Lerp(spawn.mainBody[k].lastPos, spawn.mainBody[k].pos, timeStacker);
                    Vector2 normalized2 = (vector5 - vector).normalized;
                    Vector2 vector6 = Custom.PerpendicularVector(normalized2);
                    float num3 = Vector2.Distance(vector5, vector) / 5f;
                    float rad2 = spawn.mainBody[k].rad;
                    if (k == 0)
                    {
                        Vector2 vector7 = vector6 * ((num + rad2) * 0.5f);
                        array[k, 0] = vector - vector7 + normalized2 * num3 - camPos;
                        array[k, 1] = vector + vector7 + normalized2 * num3 - camPos;
                    }
                    array[k + 1, 0] = vector5 - vector6 * rad2 - normalized2 * num3 - camPos;
                    array[k + 1, 1] = vector5 + vector6 * rad2 - normalized2 * num3 - camPos;
                    Vector2 normalized3 = (array[k + 1, 0] - array[k, 0]).normalized;
                    Vector2 normalized4 = (array[k + 1, 1] - array[k, 1]).normalized;
                    float num4 = 2f;
                    array2[k, 0] = Vector2.Lerp(normalized3, b, 0.5f) * (num3 * num4);
                    array2[k, 1] = Vector2.Lerp(normalized4, b2, 0.5f) * (num3 * num4);
                    if (k == spawn.mainBody.Length - 1)
                    {
                        array2[k + 1, 0] = Vector2.Lerp(normalized3, b, 0.5f);
                        array2[k + 1, 1] = Vector2.Lerp(normalized4, b2, 0.5f);
                    }
                    vector = vector5;
                    num = rad2;
                    b = normalized3;
                    b2 = normalized4;
                }
                for (int l = 0; l < spawn.mainBody.Length; l++)
                {
                    BezierCurve bezierCurve = new BezierCurve(array[l, 0], array[l, 0] + array2[l, 0], array[l + 1, 0], array[l + 1, 0] - array2[l + 1, 0]);
                    BezierCurve bezierCurve2 = new BezierCurve(array[l, 1], array[l, 1] + array2[l, 1], array[l + 1, 1], array[l + 1, 1] - array2[l + 1, 1]);
                    for (int m = 0; m < chunkResolution * 2; m++)
                    {
                        float f = m / (float)(chunkResolution * 2);
                        (sLeaser.sprites[BodyMeshSprite] as TriangleMesh).MoveVertice(l * 4 * chunkResolution + m * 2, bezierCurve.GetPoint(f));
                        (sLeaser.sprites[BodyMeshSprite] as TriangleMesh).MoveVertice(l * 4 * chunkResolution + m * 2 + 1, bezierCurve2.GetPoint(f));
                    }
                }
            }
            for (int n = 0; n < (sLeaser.sprites[BodyMeshSprite] as TriangleMesh).verticeColors.Length; n++)
            {
                (sLeaser.sprites[BodyMeshSprite] as TriangleMesh).verticeColors[n] = new Color(meshColor.r, meshColor.g, meshColor.b);
            }
        }
        else
        {
            for (int num6 = 0; num6 < sLeaser.sprites.Length; num6++)
            {
                sLeaser.sprites[num6].isVisible = false;
            }
        }
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        base.ApplyPalette(sLeaser, rCam, palette);
        darkness = palette.darkness;
        UpdateGlowSpriteColor(sLeaser);
    }

    private void UpdateGlowSpriteColor(RoomCamera.SpriteLeaser sLeaser)
    {
        sLeaser.sprites[GlowSprite].color = spawn.effectColor;
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        if (newContatiner == null)
        {
            newContatiner = rCam.ReturnFContainer("GrabShaders");
        }
        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            sLeaser.sprites[i].RemoveFromContainer();
            if (i == GlowSprite)
            {
                rCam.ReturnFContainer("Water").AddChild(sLeaser.sprites[i]);
            }
            else if (i == EffectSprite)
            {
                rCam.ReturnFContainer("Bloom").AddChild(sLeaser.sprites[i]);
            }
            else
            {
                newContatiner.AddChild(sLeaser.sprites[i]);
            }
        }
    }
}
