using RWCustom;
using UnityEngine;

namespace lsfUtils.Creatures.Misc.Porcupine;

public class Porcupine : Creature
{
    public const int headIndex = 0;
    public const int upperBodyIndex = 1;
    public const int lowerBodyIndex = 2;
    public const int tailIndex = 3;

    public const int chunkCount = 4;

    public const float headSize = 6f;
    public const float upperBodySize = 9f;
    public const float lowerBodySize = 7.5f;
    public const float tailSize = 5f;

    public const float headMass = 0.3f;
    public const float bodyMass = 0.6f;
    public const float tailMass = 0.15f;

    public const float headBodyDistance = 15f;
    public const float bodyDistance = 20f;
    public const float bodyTailDistance = 13f;

    public const float lookSpeed = 0.45f;
    
    public const int footingThreshold = 20;
    public const int maxFooting = 35;
    public const int specialMoveTime = 30;
    public const float walkPower = 6.2f;
    public const float walkRecoil = 1f;
    public const float spearReachDistance = 10f;

    public PorcupineAI AI;

    public float runSpeed = 1f;
    public float runCycle;
    public Vector2 travelDir;
    public bool sitting;

    public Vector2 lookDirection;
    public Vector2 lastLookDirection;

    public Color porcupineBodyColor = Color.white;

    public Vector2? spearGrabPos;

    public int footingCounter;
    public int specialMoveCounter;
    public IntVector2 specialMoveDestination;
    public MovementConnection lastFollowedConnection;

    public bool Footing => footingCounter > footingThreshold;

    public Porcupine(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        bodyChunks = new BodyChunk[chunkCount];
        bodyChunks[headIndex] = new BodyChunk(this, headIndex, new Vector2(0f, 0f), headSize, headMass);
        bodyChunks[upperBodyIndex] = new BodyChunk(this, upperBodyIndex, new Vector2(0f, 0f), upperBodySize, bodyMass);
        bodyChunks[lowerBodyIndex] = new BodyChunk(this, lowerBodyIndex, new Vector2(0f, 0f), lowerBodySize, bodyMass);
        bodyChunks[tailIndex] = new BodyChunk(this, tailIndex, new Vector2(0f, 0f), tailSize, tailMass);

        bodyChunkConnections =
        [
            new BodyChunkConnection(bodyChunks[headIndex], bodyChunks[upperBodyIndex], headBodyDistance, BodyChunkConnection.Type.Normal, 1f, 0.5f),
            new BodyChunkConnection(bodyChunks[upperBodyIndex], bodyChunks[lowerBodyIndex], bodyDistance, BodyChunkConnection.Type.Normal, 1f, 0.5f),
            new BodyChunkConnection(bodyChunks[lowerBodyIndex], bodyChunks[tailIndex], bodyTailDistance, BodyChunkConnection.Type.Normal, 1f, 0.5f)
        ];
        
        airFriction = 0.999f;
        gravity = 0.9f;
        bounce = 0f;
        surfaceFriction = 0.4f;
        collisionLayer = 1;
        waterFriction = 0.96f;
        buoyancy = 0.95f;
    }

    public override void InitiateGraphicsModule()
    {
        graphicsModule ??= new PorcupineGraphics(this);
        graphicsModule.Reset();
    }

    public override Color ShortCutColor()
    {
        return Color.Lerp(Color.white, porcupineBodyColor, 0.35f);
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        if (room == null) return;
        if (AI == null)
        {
            Plugin.Log.LogMessage("No AI!");
            return;
        }

        PullBodyTowardsLegs();

        sitting = false;
        if (Consious)
        {
            footingCounter = Mathf.Min(footingCounter + 1, maxFooting);
            AI.Update();
            Act();
        }
        else
        {
            footingCounter = 0;
        }
        if (Footing)
        {
            for (int i = 0; i < bodyChunks.Length; i++)
            {
                bodyChunks[i].vel *= 0.8f;
                bodyChunks[i].vel.y += gravity;
            }
        }

        travelDir *= sitting ? 0.5f : 0.995f;

        if (Consious && !Custom.DistLess(mainBodyChunk.pos, mainBodyChunk.lastPos, 5f))
        {
            runCycle += runSpeed / 10f;
        }

        CheckSpearReach(AI.spearToGrab);
        UpdateLook();
    }

    public void PullBodyTowardsLegs()
    {
        if (graphicsModule is not PorcupineGraphics graphics) return;
        if (!Footing) return;
        if (room.aimap.TileAccessibleToCreature(bodyChunks[upperBodyIndex].pos, Template) || room.aimap.TileAccessibleToCreature(bodyChunks[lowerBodyIndex].pos, Template)) return;

        for (int i = 0; i < graphics.legs.Length; i++)
        {
            Limb leg = graphics.legs[i];
            if (!leg.reachedSnapPosition) continue;
            if (Custom.DistLess(mainBodyChunk.pos, leg.absoluteHuntPos, PorcupineGraphics.legLength)) continue;
            if (!Custom.DistLess(mainBodyChunk.pos, leg.absoluteHuntPos, PorcupineGraphics.legLength + 15f)) continue;

            if (Random.value >= 0.3f) continue;

            Vector2 pull = Custom.DirVec(mainBodyChunk.pos, leg.absoluteHuntPos) * (Vector2.Distance(mainBodyChunk.pos, leg.absoluteHuntPos) - PorcupineGraphics.legLength);
            mainBodyChunk.pos += pull;
            mainBodyChunk.vel += pull;
        }
    }

    public void Act()
    {
        if (AI.pathFinder is not StandardPather pather)
        {
            Plugin.Log.LogMessage("Pathfinder isnt StandardPather!");
            return;
        }

        if (Submersion > 0.3f) return;

        if (specialMoveCounter > 0)
        {
            specialMoveCounter--;
            Vector2 target = room.MiddleOfTile(specialMoveDestination);
            MoveTowards(target);
            travelDir = Vector2.Lerp(travelDir, Custom.DirVec(mainBodyChunk.pos, target), 0.4f);
            if (Custom.DistLess(mainBodyChunk.pos, target, 5f))
            {
                specialMoveCounter = 0;
            }
            return;
        }
        
        if (!room.aimap.TileAccessibleToCreature(mainBodyChunk.pos, Template) && !room.aimap.TileAccessibleToCreature(bodyChunks[upperBodyIndex].pos, Template))
        {
            footingCounter = Custom.IntClamp(footingCounter - 3, 0, maxFooting);
        }

        WorldCoordinate headCoord = room.GetWorldCoordinate(mainBodyChunk.pos);
        WorldCoordinate upperCoord = room.GetWorldCoordinate(bodyChunks[upperBodyIndex].pos);
        WorldCoordinate destination = AI.pathFinder.GetDestination;
        if (headCoord == destination || upperCoord == destination)
        {
            sitting = true;
            GoThroughFloors = false;
            return;
        }

        MovementConnection connection = pather.FollowPath(headCoord, true);
        if (connection == default)
        {
            connection = pather.FollowPath(upperCoord, true);
        }

        if (connection == default)
        {
            GoThroughFloors = false;
            return;
        }

        Run(connection);
        travelDir = Vector2.Lerp(travelDir, Custom.DirVec(mainBodyChunk.pos, room.MiddleOfTile(connection.destinationCoord)), 0.4f);
    }

    public void Run(MovementConnection connection)
    {
        if (connection.type == MovementConnection.MovementType.ReachUp)
        {
            (AI.pathFinder as StandardPather)?.pastConnections.Clear();
        }

        if (connection.type == MovementConnection.MovementType.ShortCut)
        {
            enteringShortCut = connection.StartTile;
        }
        else if (connection.type == MovementConnection.MovementType.OpenDiagonal
            || connection.type == MovementConnection.MovementType.ReachOverGap
            || connection.type == MovementConnection.MovementType.ReachUp
            || connection.type == MovementConnection.MovementType.ReachDown
            || connection.type == MovementConnection.MovementType.SemiDiagonalReach)
        {
            specialMoveCounter = specialMoveTime;
            specialMoveDestination = connection.DestTile;
        }
        else
        {
            Vector2 target = room.MiddleOfTile(connection.DestTile);

            if (lastFollowedConnection.type == MovementConnection.MovementType.ReachUp)
            {
                mainBodyChunk.vel += Custom.DirVec(mainBodyChunk.pos, target) * 4f;
            }
            
            if (Footing)
            {
                for (int i = 0; i < bodyChunks.Length; i++)
                {
                    if (connection.startCoord.x == connection.destinationCoord.x)
                    {
                        bodyChunks[i].vel.x += Mathf.Min((target.x - bodyChunks[i].pos.x) / 8f, 1.2f);
                    }
                    else if (connection.startCoord.y == connection.destinationCoord.y)
                    {
                        bodyChunks[i].vel.y += Mathf.Min((target.y - bodyChunks[i].pos.y) / 8f, 1.2f);
                    }
                }
            }
            
            bool turnedHorizontal = connection.startCoord.x != connection.destinationCoord.x && lastFollowedConnection.startCoord.x == lastFollowedConnection.destinationCoord.x;
            bool turnedVertical = connection.startCoord.y != connection.destinationCoord.y && lastFollowedConnection.startCoord.y == lastFollowedConnection.destinationCoord.y;
            if ((Footing || room.aimap.TileAccessibleToCreature(mainBodyChunk.pos, Template)) && (turnedHorizontal || turnedVertical))
            {
                mainBodyChunk.vel *= 0.7f;
                bodyChunks[upperBodyIndex].vel *= 0.5f;
            }

            if (connection.type == MovementConnection.MovementType.DropToFloor)
            {
                footingCounter = 0;
            }

            MoveTowards(target);
        }

        lastFollowedConnection = connection;
    }

    public void MoveTowards(Vector2 moveTo)
    {
        Vector2 dir = Custom.DirVec(mainBodyChunk.pos, moveTo);
        if (!Footing)
        {
            dir *= 0.3f;
        }

        BodyChunk head = mainBodyChunk;
        BodyChunk upper = bodyChunks[upperBodyIndex];
        
        if (IsTileSolid(upperBodyIndex, 0, -1) && ((dir.x < -0.5f && head.pos.x > upper.pos.x + 5f) || (dir.x > 0.5f && head.pos.x < upper.pos.x - 5f)))
        {
            float sign = dir.x < 0f ? -1f : 1f;
            head.vel.x -= sign * 1.3f;
            upper.vel.x += sign * 0.5f;
            if (!IsTileSolid(headIndex, 0, 1))
            {
                head.vel.y += 3.2f;
            }
        }
        
        float power = 0.85f;
        if (graphicsModule is PorcupineGraphics graphics)
        {
            int overlapping = 0;
            for (int i = 0; i < graphics.legs.Length; i++)
            {
                if (graphics.legs[i].OverLappingHuntPos) overlapping++;
            }
            power = 0.6f + 0.4f * overlapping / graphics.legs.Length;
        }
        power = Mathf.Pow(power, 0.6f);

        head.vel += dir * (walkPower * runSpeed * power);
        upper.vel -= dir * (walkRecoil * runSpeed * power);

        GoThroughFloors = moveTo.y < head.pos.y - 5f;
    }

    public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
    {
        base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
        Vector2 holeDir = Custom.IntVector2ToVector2(newRoom.ShorcutEntranceHoleDirection(pos));
        for (int i = 0; i < bodyChunks.Length; i++)
        {
            bodyChunks[i].pos = newRoom.MiddleOfTile(pos) - holeDir * ((-1.5f + i) * 15f);
            bodyChunks[i].lastPos = newRoom.MiddleOfTile(pos);
            bodyChunks[i].vel = holeDir * 2f;
        }
        graphicsModule?.Reset();
    }

    public void UpdateLook()
    {
        lastLookDirection = lookDirection;
        Vector2 targetDirection = Vector2.zero;
        if (graphicsModule is PorcupineGraphics graphics && graphics.looker?.lookCreature?.representedCreature?.realizedCreature != null)
        {
            targetDirection = Custom.DirVec(bodyChunks[headIndex].pos, graphics.looker.lookCreature.representedCreature.realizedCreature.mainBodyChunk.pos);
        }
        lookDirection = Vector2.Lerp(lookDirection, targetDirection, lookSpeed);
    }

    public bool CheckSpearReach(Spear spear)
    {
        if (!AI.IsSpearGrabbable(spear)) return false;
        if (!Custom.DistLess(spear.firstChunk.pos, mainBodyChunk.pos, spearReachDistance)) return false;

        return true;
    }
}