using System;
using UnityEngine;
using Watcher;

namespace lsfUtils.Items.KnotSpawnV2;
public class KnotSpawnV2Graphics : ComplexGraphicsModule
{
    private float radius = 20f;

    private float thickness = 2f;
    
    private static readonly Color goldColor = new Color(0.95f, 0.75f, 0.2f);

    private static readonly string[] karmaSpriteNames = ["ripple1.0", "ripple1.5", "ripple2.0", "ripple2.5", "ripple3.0", "ripple3.5", "ripple4.0", "ripple4.5", "ripple5.0", "karma9-9"];

    private readonly int karmaSpriteID;

    private KnotSpawnV2 critter => base.owner as KnotSpawnV2;

    public KnotSpawnV2Graphics(KnotSpawnV2 owner) : base(owner, internalContainers: false)
    {
        karmaSpriteID = UnityEngine.Random.Range(0, karmaSpriteNames.Length);
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);
        sLeaser.sprites = new FSprite[3];
        TriangleMesh triangleMesh = TriangleMesh.MakeLongMeshAtlased(24, pointyTip: false, customColor: false);
        triangleMesh.color = goldColor;
        sLeaser.sprites[0] = triangleMesh;
        TriangleMesh triangleMesh2 = TriangleMesh.MakeLongMeshAtlased(24, pointyTip: false, customColor: false);
        for (int i = 0; i < triangleMesh2.vertices.Length; i += 2)
        {
            float f = (float)i / ((float)triangleMesh2.vertices.Length - 2f) * (float)Math.PI * 2f;
            triangleMesh2.vertices[i] = new Vector2(Mathf.Cos(f), Mathf.Sin(f)) * (radius - thickness);
            triangleMesh2.vertices[i + 1] = new Vector2(Mathf.Cos(f), Mathf.Sin(f)) * (radius + thickness);
        }
        triangleMesh2.color = goldColor;
        sLeaser.sprites[1] = triangleMesh2;
        FSprite fSprite = new FSprite(karmaSpriteNames[karmaSpriteID]);
        fSprite.color = goldColor;
        sLeaser.sprites[2] = fSprite;
        AddToContainer(sLeaser, rCam, null);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        float num = ((float)rCam.game.clock + timeStacker) * 0.5f;
        Vector2 vector = Vector2.Lerp(critter.bodyChunks[0].lastPos, critter.bodyChunks[0].pos, timeStacker) - camPos;
        Quaternion quaternion = Quaternion.AngleAxis(num + 90f, Vector3.up);
        TriangleMesh triangleMesh = sLeaser.sprites[0] as TriangleMesh;
        for (int i = 0; i < triangleMesh.vertices.Length; i += 2)
        {
            float f = (float)i / ((float)triangleMesh.vertices.Length - 2f) * (float)Math.PI * 2f;
            Vector3 vector2 = new Vector3(Mathf.Cos(f) * radius, Mathf.Sin(f) * radius, 0f - thickness);
            vector2 = quaternion * vector2;
            Vector3 vector3 = new Vector3(Mathf.Cos(f) * radius, Mathf.Sin(f) * radius, thickness);
            vector3 = quaternion * vector3;
            triangleMesh.MoveVertice(i, new Vector2(vector2.x, vector2.y));
            triangleMesh.MoveVertice(i + 1, new Vector2(vector3.x, vector3.y));
        }
        triangleMesh.SetPosition(vector);
        sLeaser.sprites[1].SetPosition(vector);
        sLeaser.sprites[1].scaleX = Mathf.Sin(num * ((float)Math.PI / 180f));
        sLeaser.sprites[2].SetPosition(vector);
        sLeaser.sprites[2].scaleX = Mathf.Sin(num * ((float)Math.PI / 180f));
    }
}