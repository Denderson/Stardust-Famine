using RWCustom;
using UnityEngine;

namespace lsfUtils.Creatures.Misc.Porcupine;

public class PorcupineGraphics : GraphicsModule, ILookingAtCreatures
{

    public const int legPairs = 8;
    public const int legCount = legPairs * 2;
    public const float legLength = 22f;
    public const float legThickness = 1f;
    public const float legReach = 1.5f;

    public const int legSpriteStart = Porcupine.chunkCount;
    public const int eyeSprite = Porcupine.chunkCount + legCount * 2;
    public const int spriteCount = eyeSprite + 1;

    public static readonly float[] pairAngle = { 40f, 55f, 70f, 85f, 100f, 115f, 130f, 145f };

    public Porcupine porcupine;
    public CreatureLooker looker;
    public Limb[] legs;
    public int legsDangleCounter;

    public PorcupineGraphics(PhysicalObject ow) : base(ow, internalContainers: false)
    {
        porcupine = ow as Porcupine;
        looker = new CreatureLooker(this, porcupine.AI.tracker, porcupine, 0.6f, 80);

        legs = new Limb[legCount];
        bodyParts = new BodyPart[legCount];
        for (int i = 0; i < legCount; i++)
        {
            legs[i] = new Limb(this, porcupine.bodyChunks[LegChunkIndex(i)], i, 0.1f, 0.7f, 0.99f, 12f, 0.8f);
            bodyParts[i] = legs[i];
        }
    }

    public static int LegChunkIndex(int leg)
    {
        if (leg > legCount / 2) return Porcupine.upperBodyIndex;
        return Porcupine.lowerBodyIndex;
    }
    public static float LegSide(int leg) => leg % 2 == 0 ? 1f : -1f;

    public static int LegSegmentSprite(int leg, int part) => legSpriteStart + leg * 2 + part;

    public override void Update()
    {
        base.Update();
        if (culled) return;
        if (porcupine?.room == null) return;

        looker?.Update();
        UpdateDangle();
        UpdateLegs();
    }
    
    public void UpdateDangle()
    {
        if (!porcupine.Consious || !porcupine.Footing)
        {
            legsDangleCounter = 30;
        }
        else if (legsDangleCounter > 0)
        {
            legsDangleCounter--;

            if (porcupine?.bodyChunks == null || porcupine.bodyChunks.Length <= 0) return;
            if (porcupine?.room?.aimap == null) return;

            if (porcupine.room.aimap.TileAccessibleToCreature(porcupine.bodyChunks[Porcupine.upperBodyIndex].pos, porcupine.Template))
            {
                legsDangleCounter = 0;
            }
            if (porcupine.room.aimap.TileAccessibleToCreature(porcupine.bodyChunks[Porcupine.lowerBodyIndex].pos, porcupine.Template))
            {
                legsDangleCounter = 0;
            }
        }
    }

    public void UpdateLegs()
    {
        BodyChunk head = porcupine.mainBodyChunk;
        BodyChunk lower = porcupine.bodyChunks[Porcupine.lowerBodyIndex];
        float bodyAngle = Custom.AimFromOneVectorToAnother(lower.pos, head.pos);
        
        float travelAlignment = 0f;
        for (int i = 0; i < legCount; i++)
        {
            float dot = Vector2.Dot(Custom.DirVec(head.pos, legs[i].absoluteHuntPos), porcupine.travelDir.normalized);
            travelAlignment += Custom.LerpMap(dot, -0.6f, 0.6f, 0f, 1f / legCount);
        }
        travelAlignment *= Mathf.InverseLerp(0f, 0.1f, porcupine.travelDir.magnitude);
        if (legsDangleCounter > 0) travelAlignment = 1f;

        for (int i = 0; i < legCount; i++)
        {
            Limb leg = legs[i];
            BodyChunk legChunk = porcupine.bodyChunks[LegChunkIndex(i)];
            float side = LegSide(i);

            leg.Update();
            
            float cyclePhase = 0.5f + 0.5f * Mathf.Sin((porcupine.runCycle + i * 0.25f) * Mathf.PI);

            if (leg.mode == Limb.Mode.HuntRelativePosition || legsDangleCounter > 0)
            {
                leg.mode = Limb.Mode.Dangle;
            }
            
            Vector2 baseDir = Custom.DegToVec(bodyAngle + pairAngle[i / 2] * side);
            Vector2 dir = baseDir;
            if (porcupine.Consious)
            {
                dir += porcupine.travelDir * (Custom.LerpMap(travelAlignment, 0f, 0.6f, 3f, 0f) * Mathf.Pow(cyclePhase, 0.5f));
                dir.Normalize();
            }
            
            Vector2 attach = legChunk.pos + dir * (legLength * 0.2f) + baseDir * (legLength * 0.1f);
            leg.ConnectToPoint(attach, legLength, push: false, 0f, legChunk.vel, 0.1f, 0f);
            leg.ConnectToPoint(legChunk.pos, legLength, push: false, 0f, legChunk.vel, 0.1f, 0f);

            if (Custom.DistLess(leg.pos, legChunk.pos, 6f))
            {
                leg.pos = legChunk.pos + Custom.DirVec(legChunk.pos, leg.pos) * 6f;
            }

            if (legsDangleCounter > 0)
            {
                Vector2 dangleGoal = attach + dir * (legLength * 0.7f);
                leg.vel = Vector2.Lerp(leg.vel, dangleGoal - leg.pos, 0.3f);
                leg.vel.y -= 0.4f;
                if (porcupine.Consious)
                {
                    leg.vel += Custom.RNV() * 3f;
                }
                continue;
            }
            
            Vector2 goal = attach + dir * legLength;
            for (int n = 0; n < legCount; n++)
            {
                if (n != i && Custom.DistLess(goal, legs[n].absoluteHuntPos, legLength * 0.3f))
                {
                    goal = legs[n].absoluteHuntPos + Custom.DirVec(legs[n].absoluteHuntPos, goal) * (legLength * 0.3f);
                }
            }

            if (!leg.reachedSnapPosition)
            {
                leg.FindGrip(porcupine.room, attach, attach, legLength * legReach, goal, -2, -2, behindWalls: true);
                if (leg.mode != Limb.Mode.HuntAbsolutePosition)
                {
                    leg.FindGrip(porcupine.room, attach, attach + dir * (legLength * 0.5f), legLength * legReach, goal, -2, -2, behindWalls: true);
                }
            }
            
            if (!Custom.DistLess(leg.pos, leg.absoluteHuntPos, legLength * legReach * Mathf.Pow(1f - cyclePhase, 0.2f)))
            {
                leg.mode = Limb.Mode.Dangle;
                leg.vel += dir * 7f;
                leg.vel = Vector2.Lerp(leg.vel, goal - leg.pos, 0.5f);
            }
            else
            {
                leg.vel += dir * 2f;
            }
        }
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[spriteCount];

        for (int i = 0; i < Porcupine.chunkCount; i++)
        {
            sLeaser.sprites[i] = new FSprite("Circle20", true)
            {
                scale = porcupine.bodyChunks[i].rad * 2f / 16f
            };
        }
        for (int i = 0; i < legCount; i++)
        {
            for (int part = 0; part < 2; part++)
            {
                FSprite segment = new FSprite("pixel", true)
                {
                    anchorY = 0f,
                    scaleX = legThickness
                };
                sLeaser.sprites[LegSegmentSprite(i, part)] = segment;
            }
        }
        sLeaser.sprites[eyeSprite] = new FSprite("Circle20", true) { scaleX = 0.15f, scaleY = 0.18f };

        AddToContainer(sLeaser, rCam, null);
        base.InitiateSprites(sLeaser, rCam);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        if (culled) return;
        if (porcupine == null) return;

        for (int i = 0; i < Porcupine.chunkCount; i++)
        {
            BodyChunk chunk = porcupine.bodyChunks[i];
            Vector2 pos = Vector2.Lerp(chunk.lastPos, chunk.pos, timeStacker) - camPos;
            sLeaser.sprites[i].x = pos.x;
            sLeaser.sprites[i].y = pos.y;
        }
        
        for (int i = 0; i < legCount; i++)
        {
            BodyChunk legChunk = porcupine.bodyChunks[LegChunkIndex(i)];
            Vector2 start = Vector2.Lerp(legChunk.lastPos, legChunk.pos, timeStacker);
            Vector2 foot = Vector2.Lerp(legs[i].lastPos, legs[i].pos, timeStacker);
            Vector2 knee = Custom.InverseKinematic(start, foot, legLength / 3f, legLength * (2f / 3f), LegSide(i));

            FSprite upperSegment = sLeaser.sprites[LegSegmentSprite(i, 0)];
            upperSegment.x = start.x - camPos.x;
            upperSegment.y = start.y - camPos.y;
            upperSegment.rotation = Custom.AimFromOneVectorToAnother(start, knee);
            upperSegment.scaleY = Vector2.Distance(start, knee);

            FSprite lowerSegment = sLeaser.sprites[LegSegmentSprite(i, 1)];
            lowerSegment.x = knee.x - camPos.x;
            lowerSegment.y = knee.y - camPos.y;
            lowerSegment.rotation = Custom.AimFromOneVectorToAnother(knee, foot);
            lowerSegment.scaleY = Vector2.Distance(knee, foot);
        }

        BodyChunk head = porcupine.bodyChunks[Porcupine.headIndex];
        Vector2 headPos = Vector2.Lerp(head.lastPos, head.pos, timeStacker) - camPos;
        Vector2 currentLookDirection = Vector2.Lerp(porcupine.lastLookDirection, porcupine.lookDirection, timeStacker);
        sLeaser.sprites[eyeSprite].x = headPos.x + currentLookDirection.x * 3f;
        sLeaser.sprites[eyeSprite].y = headPos.y + currentLookDirection.y * 3f;
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        for (int i = 0; i < eyeSprite; i++)
        {
            sLeaser.sprites[i].color = Color.white;
        }
        sLeaser.sprites[eyeSprite].color = Color.black;
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