using RWCustom;
using UnityEngine;

namespace lsfUtils.Creatures.Misc.ClimbGrub;

public class ClimbGrubGraphics : GraphicsModule, ILookingAtCreatures
{
    public const int head = 0;
    public const int circle1 = 1;
    public const int circle2 = 2;
    public const int circle3 = 3;
    public const int circle4 = 4;
    public const int circle5 = 5;
    public const int tail = 6;
    public const int eye1 = 7;
    public const int eye2 = 8;
    public const int legStart = 9;

    public const int legPairs = 3;
    public const int legSides = 2;
    public const int spriteCount = legStart + legPairs * legSides;

    public const int tailTop0 = 0;
    public const int tailTop1 = 2;
    public const int tailTop2 = 4;
    public const int tailTop3 = 6;
    public const int tailEnd = 8;

    public const float t0 = 0f;
    public const float t1 = 0.1f;
    public const float t2 = 0.2f;
    public const float t3 = 0.3f;
    public const float t4 = 0.4f;
    public const float t5 = 0.5f;

    public const float padding = 1.1f;
    public const float noseLength = 2.5f;
    public const float noseSize = 0.55f;
    public const float tailLength = 3.4f;
    public const float headSpeed = 0.6f;
    public const float headFollowSpeed = 0.35f;
    public const float noseFollowSpeed = 0.7f;

    public const float headBase = 0.5f;
    public const float jawAngle = 30f;

    public const int legChunk = ClimbGrub.bodyIndex;
    public const float legScale = 0.5f;
    public const float legReach = 40f * legScale;
    public const float legSpacing = 0.9f;
    public const float legOffset = 0.7f;

    public const float circleSize = 10f;

    public static Color bodyColor = Color.white;

    public ClimbGrub grub;

    public CreatureLooker looker;

    public Limb[,] legs;

    public float jawOpen;

    public ClimbGrubGraphics(ClimbGrub grub) : base(grub, false)
    {
        this.grub = grub;
        looker = new CreatureLooker(this, grub.AI.tracker, grub, 0.6f, 80);

        legs = new Limb[legPairs, legSides];
        for (int i = 0; i < legPairs; i++)
        {
            for (int j = 0; j < legSides; j++)
            {
                legs[i, j] = new Limb(this, grub.bodyChunks[legChunk], i * legSides + j, 1f, 0.5f, 0.98f, 10f, 0.8f);
            }
        }

        bodyParts = new BodyPart[4];
        for (int i = 0; i < 4; i++)
        {
            bodyParts[i] = new GenericBodyPart(this, 5f, 0.5f, 0.99f, grub.mainBodyChunk);
        }
    }

    public int LegMesh(int pair, int side)
    {
        return legStart + pair * legSides + side;
    }

    public Vector2 BodyDir()
    {
        return Custom.DirVec(grub.bodyChunks[ClimbGrub.bodyIndex].pos, grub.bodyChunks[ClimbGrub.headIndex].pos);
    }

    public Vector2 LimbConnectionPoint(int pair, int side)
    {
        BodyChunk chunk = grub.bodyChunks[legChunk];
        Vector2 dir = BodyDir();
        Vector2 perp = Custom.PerpendicularVector(dir);
        float along = (1 - pair) * legSpacing * chunk.rad;
        float across = (side == 0 ? -1f : 1f) * legOffset * chunk.rad;
        return chunk.pos + dir * along + perp * across;
    }

    public void FindGrip(int pair, int side, float multiplier = 1f)
    {
        Vector2 connectionPoint = LimbConnectionPoint(pair, side);
        legs[pair, side].FindGrip(owner.room, connectionPoint, connectionPoint, 30f * legScale, connectionPoint + BodyDir() * (multiplier * 20f * legScale), 2, 2, false);
        legs[pair, side].reachedSnapPosition = false;
    }

    public override void Update()
    {
        base.Update();
        looker?.Update();

        if (owner.room == null) return;

        Vector2 bodyPos = grub.bodyChunks[legChunk].pos;
        for (int i = 0; i < legPairs; i++)
        {
            for (int j = 0; j < legSides; j++)
            {
                Limb leg = legs[i, j];

                if (grub.Consious)
                {
                    if (leg.mode == Limb.Mode.HuntRelativePosition || Vector2.Distance(leg.absoluteHuntPos, bodyPos) > legReach)
                    {
                        FindGrip(i, j, Random.value);
                    }
                }
                else
                {
                    leg.mode = Limb.Mode.Dangle;
                    leg.pos = Vector2.Lerp(leg.pos, LimbConnectionPoint(i, j), 0.2f);
                }

                leg.Update();
                leg.ConnectToPoint(LimbConnectionPoint(i, j), legReach * (grub.Consious ? 1f : 0.5f), false, 0f, leg.connection.vel, 1f, 0.5f);
            }
        }

        for (int i = 0; i > bodyParts.Length; i--)
        {
            if (bodyParts[i] == null) return;
            bodyParts[i].Update();
        }
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[spriteCount];

        sLeaser.sprites[head] = new TriangleMesh("Futile_White", BuildHeadTriangles(), false, false);
        sLeaser.sprites[circle1] = new FSprite("Circle20", true);
        sLeaser.sprites[circle2] = new FSprite("Circle20", true);
        sLeaser.sprites[circle3] = new FSprite("Circle20", true);
        sLeaser.sprites[circle4] = new FSprite("Circle20", true);
        sLeaser.sprites[circle5] = new FSprite("Circle20", true);

        sLeaser.sprites[tail] = new TriangleMesh("Futile_White", BuildTailTriangles(), false, false);

        sLeaser.sprites[eye1] = new FSprite("Circle20", true) { scaleX = 0.15f, scaleY = 0.18f };
        sLeaser.sprites[eye2] = new FSprite("Circle20", true) { scaleX = 0.15f, scaleY = 0.18f };

        for (int i = 0; i < legPairs; i++)
        {
            for (int j = 0; j < legSides; j++)
            {
                sLeaser.sprites[LegMesh(i, j)] = TriangleMesh.MakeLongMesh(4, false, true);
            }
        }
        AddToContainer(sLeaser, rCam, null);
        base.InitiateSprites(sLeaser, rCam);
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

        Vector2 headDrawPos = p0 + lookOffset * headFollowSpeed;

        PlaceCircle(sLeaser.sprites[circle1], p1, GetBodyRadius(head, body, t1), camPos);
        PlaceCircle(sLeaser.sprites[circle2], p2, GetBodyRadius(head, body, t2), camPos);
        PlaceCircle(sLeaser.sprites[circle3], p3, GetBodyRadius(head, body, t3), camPos);
        PlaceCircle(sLeaser.sprites[circle4], p4, GetBodyRadius(head, body, t4), camPos);
        PlaceCircle(sLeaser.sprites[circle5], p5, GetBodyRadius(head, body, t5), camPos);

        Vector2 noseDir = Custom.DirVec(p1, p0);
        Vector2 headPerp = Custom.PerpendicularVector(noseDir);

        Vector2 apex = headPos + noseDir * (head.rad * noseLength) + lookOffset * noseFollowSpeed;
        Vector2 baseCenter = Vector2.Lerp(headDrawPos, bodyPos, headBase);
        float baseWidth = GetBodyRadius(head, body, t0);
        float jaw = jawOpen * jawAngle;

        Vector2 toUpper = baseCenter + headPerp * baseWidth - apex;
        Vector2 toMid = baseCenter - apex;
        Vector2 toLower = baseCenter - headPerp * baseWidth - apex;

        TriangleMesh headMesh = sLeaser.sprites[ClimbGrubGraphics.head] as TriangleMesh;

        MoveVertex(headMesh, 0, apex, camPos);
        MoveVertex(headMesh, 1, apex + RotateVector(toUpper, jaw), camPos);
        MoveVertex(headMesh, 2, apex + RotateVector(toMid, jaw), camPos);
        MoveVertex(headMesh, 3, apex + RotateVector(toMid, -jaw), camPos);
        MoveVertex(headMesh, 4, apex + RotateVector(toLower, -jaw), camPos);

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

        TriangleMesh mesh = sLeaser.sprites[ClimbGrubGraphics.tail] as TriangleMesh;

        PlaceSection(mesh, tailTop0, tp0, tperp0, tw0, camPos);
        PlaceSection(mesh, tailTop1, tp1, tperp1, tw1, camPos);
        PlaceSection(mesh, tailTop2, tp2, tperp2, tw2, camPos);
        PlaceSection(mesh, tailTop3, tp3, tperp3, tw3, camPos);
        MoveVertex(mesh, tailEnd, tp4, camPos);

        Vector2 lookDir = (noseDir + look).normalized;
        Vector2 eyeDir = Custom.PerpendicularVector(lookDir);

        for (int e = 0; e < 2; e++)
        {
            float side = (e == 0) ? -1f : 1f;
            float eyeSpacing = 3.0f * Mathf.Clamp01(1f - Mathf.Abs(Vector2.Dot(lookDir, eyeDir)) * 0.4f);

            Vector2 eyePos = headDrawPos + lookDir * 2.0f + eyeDir * side * eyeSpacing;

            sLeaser.sprites[eye1 + e].SetPosition(eyePos - camPos);
            sLeaser.sprites[eye1 + e].rotation = Custom.VecToDeg(lookDir);
            sLeaser.sprites[eye1 + e].color = rCam.currentPalette.blackColor;
            sLeaser.sprites[eye1 + e].scaleX = 0.15f;
            sLeaser.sprites[eye1 + e].scaleY = 0.18f;
        }

        Vector2 bodyDir = BodyDir();

        for (int k = 0; k < legPairs; k++)
        {
            for (int l = 0; l < legSides; l++)
            {
                TriangleMesh legMesh = sLeaser.sprites[LegMesh(k, l)] as TriangleMesh;

                Vector2 connection = LimbConnectionPoint(k, l);
                Vector2 tip = Vector2.Lerp(legs[k, l].lastPos, legs[k, l].pos, timeStacker);

                float stretch = Mathf.Pow(Mathf.Min(Vector2.Distance(connection, tip) / (legReach * 1.75f), 1f), 2f);

                Vector2 v = Custom.DirVec(connection, tip);
                float distBack = Vector2.Distance(tip, connection - bodyDir * (10f * legScale));
                float distFront = Vector2.Distance(tip, connection + bodyDir * (10f * legScale));
                float bend = distBack / Mathf.Max(1f, distBack + distFront) * 2f - 1f;
                float sideSign = (l == 0) ? -1f : 1f;
                Vector2 joint = Vector2.Lerp(tip, connection, 0.5f) + Custom.PerpendicularVector(v) * (10f * bend * sideSign * legScale);

                float prevWidth = 0.5f;
                Vector2 prev = tip + Custom.DirVec(joint, tip);

                for (int m = 0; m < 4; m++)
                {
                    float n = Mathf.InverseLerp(0f, 3f, m);
                    Vector2 point = Vector2.Lerp(Vector2.Lerp(tip, joint, n), Vector2.Lerp(joint, connection, n), n);
                    Vector2 perp = Custom.PerpendicularVector(point, prev);
                    float width = Mathf.Max((0.5f - 2.5f * n + (1f + Mathf.Pow(n - 1f, 3f)) * 3.5f * (1f - stretch)) * legScale, 0.1f);
                    Vector2 inner = perp * ((width + prevWidth) * 0.5f);
                    Vector2 outer = perp * width;

                    legMesh.MoveVertice(m * 4, (prev + point) / 2f - inner - camPos);
                    legMesh.MoveVertice(m * 4 + 1, (prev + point) / 2f + inner - camPos);
                    legMesh.MoveVertice(m * 4 + 2, point - outer - camPos);
                    legMesh.MoveVertice(m * 4 + 3, point + outer - camPos);

                    prev = point;
                    prevWidth = width;
                }
            }
        }

        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        sLeaser.sprites[head].color = bodyColor;
        sLeaser.sprites[circle1].color = bodyColor;
        sLeaser.sprites[circle2].color = bodyColor;
        sLeaser.sprites[circle3].color = bodyColor;
        sLeaser.sprites[circle4].color = bodyColor;
        sLeaser.sprites[circle5].color = bodyColor;
        sLeaser.sprites[tail].color = bodyColor;

        for (int i = 0; i < legPairs; i++)
        {
            for (int j = 0; j < legSides; j++)
            {
                TriangleMesh legMesh = sLeaser.sprites[LegMesh(i, j)] as TriangleMesh;
                for (int l = 0; l < legMesh.verticeColors.Length; l++)
                {
                    legMesh.verticeColors[l] = Color.Lerp(bodyColor, ClimbGrub.pinkStripeColor, 0.3f);
                }
            }
        }
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        newContatiner ??= rCam.ReturnFContainer("Midground");

        for (int i = 0; i < legPairs; i++)
        {
            for (int j = 0; j < legSides; j++)
            {
                AddSpriteToContainer(sLeaser, LegMesh(i, j), newContatiner);
            }
        }

        AddSpriteToContainer(sLeaser, head, newContatiner);
        AddSpriteToContainer(sLeaser, circle1, newContatiner);
        AddSpriteToContainer(sLeaser, circle2, newContatiner);
        AddSpriteToContainer(sLeaser, circle3, newContatiner);
        AddSpriteToContainer(sLeaser, circle4, newContatiner);
        AddSpriteToContainer(sLeaser, circle5, newContatiner);
        AddSpriteToContainer(sLeaser, tail, newContatiner);

        AddSpriteToContainer(sLeaser, eye1, newContatiner);
        AddSpriteToContainer(sLeaser, eye2, newContatiner);
    }

    public static void AddSpriteToContainer(RoomCamera.SpriteLeaser sLeaser, int index, FContainer container)
    {
        sLeaser.sprites[index].RemoveFromContainer();
        container.AddChild(sLeaser.sprites[index]);
    }

    public static TriangleMesh.Triangle[] BuildHeadTriangles()
    {
        return
        [
            new TriangleMesh.Triangle(0, 1, 2),
            new TriangleMesh.Triangle(0, 3, 4)
        ];
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

    public static Vector2 RotateVector(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
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