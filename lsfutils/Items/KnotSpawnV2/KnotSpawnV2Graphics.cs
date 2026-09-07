using System;
using RWCustom;
using UnityEngine;
using Watcher;

namespace lsfUtils.Items.KnotSpawnV2;

public class KnotSpawnV2Graphics : ComplexGraphicsModule
{
    private const float BaseRadius = 25f;
    private const float SizeScale = 0.8f;

    private float radius = BaseRadius;
    private float thickness = 2f;

    private static readonly Color goldColor = Enums.Colors.KnotSpawnColor;
    private static readonly string[] karmaSpriteNames = ["ripple1.0", "ripple1.5", "ripple2.0", "ripple2.5", "ripple3.0", "ripple3.5", "ripple4.0", "ripple4.5", "ripple5.0", "karma9-9"];
    private readonly int karmaSpriteID;

    private const int TailSprite = 3;
    private const int TailSegmentCount = 12;
    private const float TailSegmentLength = 12f;
    private const float TailThickness = 5f;
    private const float TailRigidity = 0.1f;
    private const float TailDamping = 0.8f;

    private Vector2[,] tailSegments;

    private KnotSpawnV2 Critter => base.owner as KnotSpawnV2;

    public Vector2 Direction()
    {
        if (Critter?.firstChunk == null) return direction;
        if (Critter.firstChunk.vel != Vector2.zero) return Critter.firstChunk.vel.normalized;
        if (Critter.firstChunk != null)
        {
            Vector2 delta = Critter.firstChunk.lastPos - Critter.firstChunk.pos;
            if (delta != Vector2.zero) return delta.normalized;
        }
        return direction;
    }

    public Vector2 direction = Vector2.one;

    public Vector2 TailPos
    {
        get
        {
            return Critter.bodyChunks[0].pos + (direction.normalized * radius);
        }
    }

    public KnotSpawnV2Graphics(KnotSpawnV2 owner) : base(owner, internalContainers: false)
    {
        karmaSpriteID = UnityEngine.Random.Range(0, karmaSpriteNames.Length);
        Reset();
    }

    public override void Reset()
    {
        base.Reset();
        tailSegments = new Vector2[TailSegmentCount, 3];
        Vector2 resetPos = Critter.bodyChunks[0].pos;
        Vector2 resetDir = Custom.RNV();
        for (int i = 0; i < tailSegments.GetLength(0); i++)
        {
            tailSegments[i, 0] = resetPos + resetDir * TailSegmentLength;
            tailSegments[i, 1] = tailSegments[i, 0];
            tailSegments[i, 2] *= 0f;
        }
    }

    public override void Update()
    {
        base.Update();

        for (int i = 0; i < tailSegments.GetLength(0); i++)
        {
            tailSegments[i, 1] = tailSegments[i, 0];
            tailSegments[i, 0] += tailSegments[i, 2];
            tailSegments[i, 2] *= TailDamping;
        }

        tailSegments[0, 0] = TailPos;
        tailSegments[0, 2] *= 0f;

        for (int j = 1; j < tailSegments.GetLength(0); j++)
        {
            Vector2 dirVec = Custom.DirVec(tailSegments[j, 0], tailSegments[j - 1, 0]);
            float dist = Vector2.Distance(tailSegments[j, 0], tailSegments[j - 1, 0]);
            Vector2 correction = dirVec * ((TailSegmentLength - dist) * 0.5f);
            tailSegments[j, 0] -= correction;
            tailSegments[j, 2] -= correction;
            tailSegments[j - 1, 0] += correction;
            tailSegments[j - 1, 2] += correction;
        }

        for (int k = 2; k < tailSegments.GetLength(0); k++)
        {
            Vector2 straighten = Custom.DirVec(tailSegments[k, 0], tailSegments[k - 2, 0]);
            tailSegments[k, 2] -= straighten * TailRigidity;
            tailSegments[k - 2, 2] += straighten * TailRigidity;
        }

        tailSegments[0, 0] = TailPos;
        direction = Direction();
    }

    private static void SetRingVertices(TriangleMesh mesh, float radius, float thickness)
    {
        for (int i = 0; i < mesh.vertices.Length; i += 2)
        {
            float f = (float)i / ((float)mesh.vertices.Length - 2f) * (float)Math.PI * 2f;
            mesh.vertices[i] = new Vector2(Mathf.Cos(f), Mathf.Sin(f)) * (radius - thickness);
            mesh.vertices[i + 1] = new Vector2(Mathf.Cos(f), Mathf.Sin(f)) * (radius + thickness);
        }
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);
        sLeaser.sprites = new FSprite[4];

        TriangleMesh triangleMesh = TriangleMesh.MakeLongMeshAtlased(24, pointyTip: false, customColor: false);
        SetRingVertices(triangleMesh, radius, thickness);
        triangleMesh.color = goldColor;
        sLeaser.sprites[0] = triangleMesh;

        TriangleMesh triangleMesh2 = TriangleMesh.MakeLongMeshAtlased(24, pointyTip: false, customColor: false);
        SetRingVertices(triangleMesh2, radius, thickness);
        triangleMesh2.color = goldColor;
        sLeaser.sprites[1] = triangleMesh2;

        FSprite fSprite = new(karmaSpriteNames[karmaSpriteID])
        {
            color = goldColor
        };
        sLeaser.sprites[2] = fSprite;

        TriangleMesh tailMesh = TriangleMesh.MakeLongMesh(tailSegments.GetLength(0), pointyTip: false, customColor: false);
        tailMesh.color = goldColor;
        sLeaser.sprites[TailSprite] = tailMesh;

        AddToContainer(sLeaser, rCam, null);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        float num = ((float)rCam.game.clock + timeStacker) * 0.5f;
        Vector2 vector = Vector2.Lerp(Critter.bodyChunks[0].lastPos, Critter.bodyChunks[0].pos, timeStacker) - camPos;
        float signFlip = Mathf.Sin(num * ((float)Math.PI / 180f));

        sLeaser.sprites[0].SetPosition(vector);

        sLeaser.sprites[1].SetPosition(vector);
        sLeaser.sprites[1].scaleY = SizeScale;
        sLeaser.sprites[1].scaleX = signFlip * SizeScale;
        sLeaser.sprites[1].rotation = direction.GetAngle();

        sLeaser.sprites[2].SetPosition(vector);
        sLeaser.sprites[2].scaleY = SizeScale;
        sLeaser.sprites[2].scaleX = signFlip * SizeScale;
        sLeaser.sprites[2].rotation = direction.GetAngle();

        Vector2 tailAnchor = Vector2.Lerp(tailSegments[0, 1], tailSegments[0, 0], timeStacker);
        tailAnchor += Custom.DirVec(Vector2.Lerp(tailSegments[1, 1], tailSegments[1, 0], timeStacker), tailAnchor) * (TailSegmentLength * 0.3f);
        float prevThickness = 1f;
        TriangleMesh tailMesh = sLeaser.sprites[TailSprite] as TriangleMesh;
        for (int i = 0; i < tailSegments.GetLength(0); i++)
        {
            float f = i / (float)(tailSegments.GetLength(0) - 1);
            Vector2 segPos = Vector2.Lerp(tailSegments[i, 1], tailSegments[i, 0], timeStacker);
            Vector2 normalized = (segPos - tailAnchor).normalized;
            Vector2 perp = Custom.PerpendicularVector(normalized);
            float gapFill = Vector2.Distance(segPos, tailAnchor) / 5f;
            float taperedThickness = Mathf.Lerp(TailThickness, 0.5f, Mathf.Pow(f, 0.2f));
            Vector2 sideOffset = perp * ((prevThickness + taperedThickness) * 0.5f);
            tailMesh.MoveVertice(i * 4, tailAnchor - sideOffset + normalized * gapFill - camPos);
            tailMesh.MoveVertice(i * 4 + 1, tailAnchor + sideOffset + normalized * gapFill - camPos);
            tailMesh.MoveVertice(i * 4 + 2, segPos - perp * taperedThickness - normalized * gapFill - camPos);
            tailMesh.MoveVertice(i * 4 + 3, segPos + perp * taperedThickness - normalized * gapFill - camPos);
            tailAnchor = segPos;
            prevThickness = taperedThickness;
        }
    }
}