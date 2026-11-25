using lsfUtils.Items.Darts.Dart;
using RWCustom;
using UnityEngine;
using static lsfUtils.Plugin;

namespace lsfUtils.Creatures.Misc.Porcupine;

public class PorcupineGraphics : GraphicsModule, ILookingAtCreatures
{
    public class Spike
    {
        public bool onBody;
        public int bodyChunkIndex;
        public float offsetX;
        public float offsetY;
        public Vector2 pos;
        public float angle; 
        public float length;
        public float baseCurve;
        public float temCurve;
        public float lastTempCurve;
    }

    public const int legPairCount = 8;
    public const int legCount = legPairCount * 2;
    public const float legLength = 22f;
    public const float legUpperLength = legLength / 3f;
    public const float legLowerLength = legLength - legUpperLength;
    public const float legReachMultiplier = 1.5f;

    public const int firstLegSprite = Porcupine.chunkCount;
    public const int eyeSprite = Porcupine.chunkCount + legCount * 2;
    public const int spikeCount = 12;
    public const int firstSpikeSprite = eyeSprite + 1;
    public const int spriteCount = firstSpikeSprite + spikeCount;

    public const float chunkSize = 16f;
    public const float eyeSizeX = 0.15f;
    public const float eyeSizeY = 0.18f;
    public const float eyeLookOffset = 3f;

    public const float spikeMinLength = 40f;
    public const float spikeMaxLength = 60f;
    public const float spikeMinAngle = 0f;
    public const float spikeMaxAngle = 40f;
    public const float spikeMinCurve = 0.1f;
    public const float spikeMaxCurve = 0.3f;
    public const float spikeRange = 0.6f;
    public const float spikeMinOffset = 0.1f;
    public const float spikeMaxOffset = 0.7f;
    public const float spikeMinDist = 4f;
    public const float flipTreshold = 0.3f;
    public const float spikeCurvingFactor = 0.03f;
    public const float spikeMaxTempCurve = 0.15f;
    public const float speedLimitForAttach = 4f;

    public static readonly float[] pairAngle = [40f, 55f, 70f, 85f, 100f, 115f, 130f, 145f];

    public Porcupine porcupine;
    public CreatureLooker looker;
    public Limb[] legs;
    public int legsDangleCounter;

    public Spike[] spikes = new Spike[spikeCount];

    public float flip = 1f;
    public float legDir = 1f;

    public PorcupineGraphics(PhysicalObject owner) : base(owner, internalContainers: false)
    {
        porcupine = owner as Porcupine;
        looker = new CreatureLooker(this, porcupine.AI.tracker, porcupine, 0.5f, 120);

        legs = new Limb[legCount];
        bodyParts = new BodyPart[legCount];
        for (int i = 0; i < legCount; i++)
        {
            legs[i] = new Limb(this, porcupine.bodyChunks[LegChunkIndex(i)], i, 0.1f, 0.7f, 0.99f, 12f, 0.8f);
            bodyParts[i] = legs[i];
        }

        Random.State state = Random.state;
        Random.InitState(porcupine.abstractCreature.ID.RandomSeed);

        for (int i = 0; i < spikeCount; i++)
        {
            float maxDist = -1f;

            for (int attempt = 0; attempt < 20; attempt++)
            {
                int chunkIndex = Random.value < 0.5f ? Porcupine.upperBodyIndex : Porcupine.lowerBodyIndex;
                float radius = porcupine.bodyChunks[chunkIndex].rad;

                Spike spike = new()
                {
                    onBody = true,
                    bodyChunkIndex = chunkIndex,
                    offsetX = GetRandomNumber(-spikeRange, spikeRange) * radius,
                    offsetY = GetRandomNumber(spikeMinOffset, spikeMaxOffset) * radius,
                    angle = GetRandomNumber(spikeMinAngle, spikeMaxAngle),
                    length = GetRandomNumber(spikeMinLength, spikeMaxLength),
                    baseCurve = GetRandomNumber(spikeMinCurve, spikeMaxCurve)
                };

                float horizontalPos = 0f;
                if (chunkIndex == Porcupine.upperBodyIndex) horizontalPos = Vector2.Distance(porcupine.bodyChunks[Porcupine.upperBodyIndex].pos, porcupine.bodyChunks[Porcupine.lowerBodyIndex].pos);

                spike.pos = new Vector2(horizontalPos + spike.offsetX, spike.offsetY);

                float dist = float.MaxValue;
                for (int j = 0; j < i; j++)
                {
                    dist = Mathf.Min(dist, Vector2.Distance(spike.pos, spikes[j].pos));
                }
                
                if (dist > maxDist)
                {
                    spikes[i] = spike;
                    maxDist = dist;
                }

                if (maxDist >= spikeMinDist) break;
            }
        }

        Random.state = state;
    }

    public void GetSpikePos(Spike spike, float timeStacker, out Vector2 spikePos, out Vector2 direction, out Vector2 curlDirection)
    {
        BodyChunk headChunk = porcupine.mainBodyChunk;
        BodyChunk lowerChunk = porcupine.bodyChunks[Porcupine.lowerBodyIndex];
        BodyChunk spikeChunk = porcupine.bodyChunks[spike.bodyChunkIndex];
        
        Vector2 bodyAxis = (Vector2.Lerp(headChunk.lastPos, headChunk.pos, timeStacker) - Vector2.Lerp(lowerChunk.lastPos, lowerChunk.pos, timeStacker)).normalized;
        Vector2 backAxis = Custom.PerpendicularVector(bodyAxis) * flip;

        spikePos = Vector2.Lerp(spikeChunk.lastPos, spikeChunk.pos, timeStacker) + bodyAxis * spike.offsetX + backAxis * spike.offsetY;

        float angle = spike.angle * Mathf.Deg2Rad;
        direction = -bodyAxis * Mathf.Cos(angle) + backAxis * Mathf.Sin(angle);

        Vector2 perpendicular = Custom.PerpendicularVector(direction);
        curlDirection = Vector2.Dot(perpendicular, backAxis) <= 0f ? perpendicular : -perpendicular;
    }

    public override void Update()
    {
        base.Update();
        if (porcupine?.room == null || culled) return;

        looker?.Update();
        DangleUpdate();
        LegUpdate();
        SpikeUpdate();
    }

    
    public void SpikeUpdate()
    {
        Vector2 axis = (porcupine.bodyChunks[Porcupine.upperBodyIndex].pos - porcupine.bodyChunks[Porcupine.lowerBodyIndex].pos).normalized;
        float perpendicularY = Custom.PerpendicularVector(axis).y;
        if (perpendicularY > flipTreshold) flip = 1f;
        else if (perpendicularY < -flipTreshold) flip = -1f;

        foreach (Spike spike in spikes)
        {
            spike.lastTempCurve = spike.temCurve;
            if (!spike.onBody) continue;

            GetSpikePos(spike, 1f, out Vector2 _, out Vector2 _, out Vector2 curlDirection); // additional out params are used for attach code in main class
            BodyChunk posChunk = porcupine.bodyChunks[spike.bodyChunkIndex];
            
            float tempCurve = Mathf.Clamp(-Vector2.Dot(posChunk.vel, curlDirection) * spikeCurvingFactor, -spikeMaxTempCurve, spikeMaxTempCurve);
            spike.temCurve = Mathf.Lerp(spike.temCurve, tempCurve, 0.3f);
        }
    }

    public static int LegChunkIndex(int leg)
    {
        if (leg > legCount / 2) return Porcupine.upperBodyIndex;
        return Porcupine.lowerBodyIndex;
    }
    
    public void DangleUpdate()
    {
        if (!porcupine.Consious || !porcupine.Footing)
        {
            legsDangleCounter = 20;
            return;
        }

        if (legsDangleCounter <= 0) return;
        legsDangleCounter--;

        if (porcupine?.room?.aimap == null) return;

        bool upperAccessible = porcupine.room.aimap.TileAccessibleToCreature(porcupine.bodyChunks[Porcupine.upperBodyIndex].pos, porcupine.Template);
        bool lowerAccessible = porcupine.room.aimap.TileAccessibleToCreature(porcupine.bodyChunks[Porcupine.lowerBodyIndex].pos, porcupine.Template);
        if (upperAccessible || lowerAccessible) legsDangleCounter = 0;
    }

    public void LegUpdate()
    {
        BodyChunk headChunk = porcupine.bodyChunks[Porcupine.headIndex];
        BodyChunk lowerChunk = porcupine.bodyChunks[Porcupine.lowerBodyIndex];
        float bodyDir = Custom.AimFromOneVectorToAnother(lowerChunk.pos, headChunk.pos);
        Vector2 backDir = Custom.PerpendicularVector((headChunk.pos - lowerChunk.pos).normalized) * flip;
        legDir = Vector2.Dot(Custom.DegToVec(bodyDir + 90f), backDir) > 0f ? -1f : 1f;

        float moving = Mathf.InverseLerp(0f, 0.1f, porcupine.travelDir.magnitude);
        if (legsDangleCounter > 0) moving = 1f;

        for (int i = 0; i < legCount; i++)
        {
            Limb leg = legs[i];
            BodyChunk legChunk = porcupine.bodyChunks[LegChunkIndex(i)];

            leg.Update();

            float cyclePhase = 0.5f + 0.5f * Mathf.Sin((porcupine.runCycle + i * 0.25f) * Mathf.PI);

            if (leg.mode == Limb.Mode.HuntRelativePosition || legsDangleCounter > 0)
            {
                leg.mode = Limb.Mode.Dangle;
            }
            
            float pairAngleOffset = pairAngle[i / 2] + (i % 2) * 8f;
            Vector2 restDir = Custom.DegToVec(bodyDir + pairAngleOffset * legDir);
            
            Vector2 dir = restDir;
            if (porcupine.Consious)
            {
                float movePush = Custom.LerpMap(moving, 0f, 0.6f, 3f, 0f);
                dir += porcupine.travelDir * (movePush * Mathf.Pow(cyclePhase, 0.5f));
                dir.Normalize();
            }
            
            Vector2 pos = legChunk.pos + (dir * legLength * 0.2f) + (restDir * legLength * 0.1f);
            leg.ConnectToPoint(pos, legLength, false, 0f, legChunk.vel, 0.1f, 0f);
            leg.ConnectToPoint(legChunk.pos, legLength, false, 0f, legChunk.vel, 0.1f, 0f);

            if (Custom.DistLess(leg.pos, legChunk.pos, 6f))
            {
                leg.pos = legChunk.pos + Custom.DirVec(legChunk.pos, leg.pos) * 6f;
            }

            if (legsDangleCounter > 0)
            {
                Vector2 dangleGoal = pos + dir * legLength * 0.7f;
                leg.vel = Vector2.Lerp(leg.vel, dangleGoal - leg.pos, 0.3f);
                leg.vel.y -= 0.5f;
                continue;
            }

            Vector2 legGoal = pos + dir * legLength;
            for (int otherLeg = 0; otherLeg < legCount; otherLeg++)
            {
                if (otherLeg != i && Custom.DistLess(legGoal, legs[otherLeg].absoluteHuntPos, legLength * 0.3f))
                {
                    legGoal = legs[otherLeg].absoluteHuntPos + Custom.DirVec(legs[otherLeg].absoluteHuntPos, legGoal) * (legLength * 0.3f);
                }
            }

            if (!leg.reachedSnapPosition)
            {
                leg.FindGrip(porcupine.room, pos, pos, legLength * legReachMultiplier, legGoal, -2, -2, true);
                if (leg.mode != Limb.Mode.HuntAbsolutePosition)
                {
                    leg.FindGrip(porcupine.room, pos, pos + (dir * legLength * 0.5f), legLength * legReachMultiplier, legGoal, -2, -2, true);
                }
            }

            float maxDist = legLength * legReachMultiplier * Mathf.Pow(1f - cyclePhase, 0.2f);
            if (!Custom.DistLess(leg.pos, leg.absoluteHuntPos, maxDist))
            {
                leg.mode = Limb.Mode.Dangle;
                leg.vel += dir * 7f;
                leg.vel = Vector2.Lerp(leg.vel, legGoal - leg.pos, 0.5f);
            }
            else
            {
                leg.vel += dir * 2f;
            }
        }
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser spriteLeaser, RoomCamera roomCamera)
    {
        spriteLeaser.sprites = new FSprite[spriteCount];

        for (int i = 0; i < Porcupine.chunkCount; i++)
        {
            spriteLeaser.sprites[i] = new FSprite("Circle20", true)
            {
                scale = porcupine.bodyChunks[i].rad * 2f / chunkSize
            };
        }
        
        for (int i = 0; i < legCount; i++)
        {
            for (int part = 0; part < 2; part++)
            {
                spriteLeaser.sprites[firstLegSprite + i * 2 + part] = new FSprite("pixel", true) 
                { 
                    anchorY = 0f
                };
            }
        }

        spriteLeaser.sprites[eyeSprite] = new FSprite("Circle20", true) 
        { 
            scaleX = eyeSizeX, 
            scaleY = eyeSizeY 
        };

        for (int s = 0; s < spikeCount; s++)
        {
            spriteLeaser.sprites[firstSpikeSprite + s] = SpikeShape.MakeMesh();
        }

        AddToContainer(spriteLeaser, roomCamera, null);
        base.InitiateSprites(spriteLeaser, roomCamera);
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser spriteLeaser, RoomCamera roomCamera, FContainer newContainer)
    {
        base.AddToContainer(spriteLeaser, roomCamera, newContainer);
        for (int s = 0; s < spikeCount; s++)
        {
            spriteLeaser.sprites[firstSpikeSprite + s].MoveInFrontOfOtherNode(spriteLeaser.sprites[Porcupine.chunkCount - 1]);
        }
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser spriteLeaser, RoomCamera roomCamera, float timeStacker, Vector2 cameraPosition)
    {
        base.DrawSprites(spriteLeaser, roomCamera, timeStacker, cameraPosition);
        if (culled || porcupine == null) return;

        BodyChunk headChunk = porcupine.bodyChunks[Porcupine.headIndex];

        for (int i = 0; i < Porcupine.chunkCount; i++)
        {
            BodyChunk chunk = porcupine.bodyChunks[i];
            Vector2 pos = Vector2.Lerp(chunk.lastPos, chunk.pos, timeStacker) - cameraPosition;
            spriteLeaser.sprites[i].x = pos.x;
            spriteLeaser.sprites[i].y = pos.y;
        }

        for (int i = 0; i < legCount; i++)
        {
            BodyChunk legChunk = porcupine.bodyChunks[LegChunkIndex(i)];
            Vector2 hip = Vector2.Lerp(legChunk.lastPos, legChunk.pos, timeStacker);
            Vector2 foot = Vector2.Lerp(legs[i].lastPos, legs[i].pos, timeStacker);
            Vector2 knee = Custom.InverseKinematic(hip, foot, legUpperLength, legLowerLength, legDir);

            for (int part = 0; part < 2; part++)
            {
                Vector2 from = part == 0 ? hip : knee;
                Vector2 to = part == 0 ? knee : foot;
                FSprite segment = spriteLeaser.sprites[firstLegSprite + i * 2 + part];

                segment.x = from.x - cameraPosition.x;
                segment.y = from.y - cameraPosition.y;
                segment.rotation = Custom.AimFromOneVectorToAnother(from, to);
                segment.scaleY = Vector2.Distance(from, to);
            }
        }

        Vector2 headPos = Vector2.Lerp(headChunk.lastPos, headChunk.pos, timeStacker) - cameraPosition;
        Vector2 lookDir = Vector2.Lerp(porcupine.lastLookDirection, porcupine.lookDirection, timeStacker);
        spriteLeaser.sprites[eyeSprite].x = headPos.x + lookDir.x * eyeLookOffset;
        spriteLeaser.sprites[eyeSprite].y = headPos.y + lookDir.y * eyeLookOffset;

        for (int s = 0; s < spikeCount; s++)
        {
            Spike spike = spikes[s];
            FSprite sprite = spriteLeaser.sprites[firstSpikeSprite + s];

            sprite.isVisible = spike.onBody;
            if (!spike.onBody) continue;

            GetSpikePos(spike, timeStacker, out Vector2 pos, out Vector2 direction, out Vector2 curlDirection);
            float curveAt = spike.baseCurve + Mathf.Lerp(spike.lastTempCurve, spike.temCurve, timeStacker);
            SpikeShape.Draw(sprite as TriangleMesh, pos, direction, curlDirection, spike.length, curveAt, cameraPosition);
        }
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser spriteLeaser, RoomCamera roomCamera, RoomPalette palette)
    {
        for (int i = 0; i < eyeSprite; i++)
        {
            spriteLeaser.sprites[i].color = Color.white;
        }
        spriteLeaser.sprites[eyeSprite].color = Color.black;

        for (int s = 0; s < spikeCount; s++)
        {
            SpikeShape.ApplyPalette(spriteLeaser.sprites[firstSpikeSprite + s] as TriangleMesh, palette);
        }
    }

    public Tracker.CreatureRepresentation ForcedLookCreature()
    {
        return null;
    }

    public void LookAtNothing() 
    {

    }

    public float CreatureInterestBonus(Tracker.CreatureRepresentation creature, float score)
    {
        return 0.5f;
        // TODO
    }
}