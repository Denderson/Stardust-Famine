using RWCustom;
using UnityEngine;

namespace lsfUtils.Creatures.Misc.ClimbGrub;

public class ClimbGrubGraphics : GraphicsModule, ILookingAtCreatures
{
    private const int circle0 = 0;
    private const int circle1 = 1;
    private const int circle2 = 2;
    private const int circle3 = 3;
    private const int circle4 = 4;
    private const int circle5 = 5;
    private const int noseCircle = 6;
    private const int tailMesh = 7;
    private const int spriteCount = 8;
    
    private const int tailTop0 = 0;
    private const int tailTop1 = 2;
    private const int tailTop2 = 4;
    private const int tailTop3 = 6;
    private const int tailEnd = 8;
    
    private const float t0 = 0f;
    private const float t1 = 0.1f;
    private const float t2 = 0.2f;
    private const float t3 = 0.3f;
    private const float t4 = 0.4f;
    private const float t5 = 0.5f;

    public const float padding = 1.1f;
    public const float noseLength = 1.5f;
    public const float noseSize = 0.55f;
    public const float tailLength = 3.4f;
    public const float headSpeed = 0.6f;
    public const float headFollowSpeed = 0.35f;
    public const float noseFollowSpeed = 0.7f;
    
    private const float circleSize = 10f;

    public static Color bodyColor = Color.white;

    private ClimbGrub grub;

    public CreatureLooker looker;

    public ClimbGrubGraphics(ClimbGrub grub) : base(grub, false)
    {
        this.grub = grub;
        looker = new CreatureLooker(this, grub.AI.tracker, grub, 0.6f, 80);
    }

    public override void Update()
    {
        base.Update();
        looker?.Update();
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[spriteCount];

        sLeaser.sprites[circle0] = new FSprite("Circle20", true);
        sLeaser.sprites[circle1] = new FSprite("Circle20", true);
        sLeaser.sprites[circle2] = new FSprite("Circle20", true);
        sLeaser.sprites[circle3] = new FSprite("Circle20", true);
        sLeaser.sprites[circle4] = new FSprite("Circle20", true);
        sLeaser.sprites[circle5] = new FSprite("Circle20", true);
        sLeaser.sprites[noseCircle] = new FSprite("Circle20", true);

        sLeaser.sprites[tailMesh] = new TriangleMesh("Futile_White", BuildTailTriangles(), false, false);

        AddToContainer(sLeaser, rCam, null);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (owner.room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
            return;
        }

        BodyChunk head = owner.bodyChunks[ClimbGrub.headIndex];
        BodyChunk body = owner.bodyChunks[ClimbGrub.bodyIndex];
        BodyChunk tail = owner.bodyChunks[ClimbGrub.tailIndex];

        Vector2 headPos = Vector2.Lerp(head.lastPos, head.pos, timeStacker);
        Vector2 bodyPos = Vector2.Lerp(body.lastPos, body.pos, timeStacker);
        Vector2 tailPos = Vector2.Lerp(tail.lastPos, tail.pos, timeStacker);

        Vector2 look = Vector2.Lerp(grub.lastLookDirection, grub.lookDirection, timeStacker);
        Vector2 lookOffset = look * (head.rad * headSpeed);
        
        Vector2 p0 = GetCurvePoint(headPos, bodyPos, tailPos, t0);
        Vector2 p1 = GetCurvePoint(headPos, bodyPos, tailPos, t1);
        Vector2 p2 = GetCurvePoint(headPos, bodyPos, tailPos, t2);
        Vector2 p3 = GetCurvePoint(headPos, bodyPos, tailPos, t3);
        Vector2 p4 = GetCurvePoint(headPos, bodyPos, tailPos, t4);
        Vector2 p5 = GetCurvePoint(headPos, bodyPos, tailPos, t5);
        
        PlaceCircle(sLeaser.sprites[circle0], p0 + lookOffset * headFollowSpeed, GetBodyRadius(head, body, t0), camPos);
        PlaceCircle(sLeaser.sprites[circle1], p1, GetBodyRadius(head, body, t1), camPos);
        PlaceCircle(sLeaser.sprites[circle2], p2, GetBodyRadius(head, body, t2), camPos);
        PlaceCircle(sLeaser.sprites[circle3], p3, GetBodyRadius(head, body, t3), camPos);
        PlaceCircle(sLeaser.sprites[circle4], p4, GetBodyRadius(head, body, t4), camPos);
        PlaceCircle(sLeaser.sprites[circle5], p5, GetBodyRadius(head, body, t5), camPos);
        
        Vector2 noseDir = Custom.DirVec(p1, p0);
        float noseRadius = head.rad * noseSize * padding;
        Vector2 nosePos = headPos + noseDir * (head.rad * (noseLength - noseSize));
        PlaceCircle(sLeaser.sprites[noseCircle], nosePos + lookOffset * noseFollowSpeed, noseRadius, camPos);

        Vector2 dirBodyTail = Custom.DirVec(tailPos, bodyPos);

        Vector2 tp0 = bodyPos;
        Vector2 tp1 = Vector2.Lerp(bodyPos, tailPos, 0.5f);
        Vector2 tp2 = tailPos;
        Vector2 tp3 = tailPos - dirBodyTail * (tail.rad * 1.6f);
        Vector2 tp4 = tailPos - dirBodyTail * (tail.rad * tailLength);

        Vector2 tperp0 = Custom.PerpendicularVector(Custom.DirVec(tp0, tp1));
        Vector2 tperp1 = Custom.PerpendicularVector(Custom.DirVec(tp0, tp2));
        Vector2 tperp2 = Custom.PerpendicularVector(Custom.DirVec(tp1, tp3));
        Vector2 tperp3 = Custom.PerpendicularVector(Custom.DirVec(tp2, tp4));

        float tw0 = body.rad * 0.6f;
        float tw3 = tail.rad * 0.55f;
        float tw2 = tail.rad * padding;
        float tw1 = (tw0 + tw2) * 0.5f;

        TriangleMesh mesh = sLeaser.sprites[tailMesh] as TriangleMesh;

        PlaceSection(mesh, tailTop0, tp0, tperp0, tw0, camPos);
        PlaceSection(mesh, tailTop1, tp1, tperp1, tw1, camPos);
        PlaceSection(mesh, tailTop2, tp2, tperp2, tw2, camPos);
        PlaceSection(mesh, tailTop3, tp3, tperp3, tw3, camPos);
        MoveVertex(mesh, tailEnd, tp4, camPos);
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        sLeaser.sprites[circle0].color = bodyColor;
        sLeaser.sprites[circle1].color = bodyColor;
        sLeaser.sprites[circle2].color = bodyColor;
        sLeaser.sprites[circle3].color = bodyColor;
        sLeaser.sprites[circle4].color = bodyColor;
        sLeaser.sprites[circle5].color = bodyColor;
        sLeaser.sprites[noseCircle].color = bodyColor;
        sLeaser.sprites[tailMesh].color = bodyColor;
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        newContatiner ??= rCam.ReturnFContainer("Midground");

        AddSpriteToContainer(sLeaser, circle0, newContatiner);
        AddSpriteToContainer(sLeaser, circle1, newContatiner);
        AddSpriteToContainer(sLeaser, circle2, newContatiner);
        AddSpriteToContainer(sLeaser, circle3, newContatiner);
        AddSpriteToContainer(sLeaser, circle4, newContatiner);
        AddSpriteToContainer(sLeaser, circle5, newContatiner);
        AddSpriteToContainer(sLeaser, noseCircle, newContatiner);
        AddSpriteToContainer(sLeaser, tailMesh, newContatiner);
    }

    public static void AddSpriteToContainer(RoomCamera.SpriteLeaser sLeaser, int index, FContainer container)
    {
        sLeaser.sprites[index].RemoveFromContainer();
        container.AddChild(sLeaser.sprites[index]);
    }

    public static TriangleMesh.Triangle[] BuildTailTriangles()
    {
        return
        [
            new TriangleMesh.Triangle(0, 1, 2),
            new TriangleMesh.Triangle(1, 2, 3),

            new TriangleMesh.Triangle(2, 3, 4),
            new TriangleMesh.Triangle(3, 4, 5),

            new TriangleMesh.Triangle(4, 5, 6),
            new TriangleMesh.Triangle(5, 6, 7),

            new TriangleMesh.Triangle(6, 7, tailEnd)
        ];
    }

    public static Vector2 GetCurvePoint(Vector2 headPos, Vector2 bodyPos, Vector2 tailPos, float t)
    {
        Vector2 control = bodyPos * 2f - (headPos + tailPos) * 0.5f;
        float inv = 1f - t;
        return headPos * (inv * inv) + control * (2f * inv * t) + tailPos * (t * t);
    }

    public static float GetBodyRadius(BodyChunk head, BodyChunk body, float t)
    {
        return Mathf.Lerp(head.rad, body.rad, t * 2f) * padding;
    }

    public static void PlaceCircle(FSprite sprite, Vector2 worldPos, float radius, Vector2 camPos)
    {
        sprite.x = worldPos.x - camPos.x;
        sprite.y = worldPos.y - camPos.y;
        sprite.scale = radius / circleSize;
    }

    public static void PlaceSection(TriangleMesh mesh, int topIndex, Vector2 center, Vector2 perp, float halfWidth, Vector2 camPos)
    {
        MoveVertex(mesh, topIndex, center + perp * halfWidth, camPos);
        MoveVertex(mesh, topIndex + 1, center - perp * halfWidth, camPos);
    }

    public static void MoveVertex(TriangleMesh mesh, int index, Vector2 worldPos, Vector2 camPos)
    {
        mesh.MoveVertice(index, worldPos - camPos);
    }

    public Tracker.CreatureRepresentation ForcedLookCreature()
    {
        return null;
    }
    public void LookAtNothing()
    {

    }

    public float CreatureInterestBonus(Tracker.CreatureRepresentation crit, float score)
    {
        return 0.5f;
    }
}