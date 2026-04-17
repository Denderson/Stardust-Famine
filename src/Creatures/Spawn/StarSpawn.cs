using RWCustom;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static PlateTree;

namespace lsfUtils.Creatures.Spawn;

public class StarSpawn : Creature, IPlayerEdible
{

    enum SpawnType
    {
        Base,
        Jelly,
        Noodle,
        Amoeba,
        Elder
    }

    public StarSpawnAI AI = null!;

    public BodyChunk[] mainBody;

    public float swimCycle;

    public float sizeFac;

    public float runSpeed;

    private float mainChunkFactor = 0.2f;

    public bool canBeDestroyed;

    public bool culled;

    public bool lastCulled;

    public float fade;

    public float lastFade;

    public bool dayLightMode;

    public int timeUntilFadeout;

    public bool startFadeOut;

    public int proximityTimer;

    public int attackTimer;

    public Color effectColor;

    public List<PhysicalObject> trackedFood = new();

    public Vector2? foodPos;

    public PhysicalObject foodToGrab;

    public bool isAnythingGrabbed = false;

    MovementConnection? lastFollowedConnection;

    Vector2 travelDir;

    SpawnType spawnType;

    public StarSpawn(AbstractCreature acrit, World world) : base(acrit, acrit.world)
    {
        var state = Random.state;
        Random.InitState(acrit.ID.RandomSeed);

        int chunkAmount;
        float size = Random.value;
        CreatureTemplate.Type type = abstractCreature.creatureTemplate.type;
        if (type == Enums.CreatureTemplateType.StarNoodles)
        {
            spawnType = SpawnType.Noodle;
            chunkAmount = (int)Mathf.Lerp(9, 16, size);
        }
        else if (type == Enums.CreatureTemplateType.StarJelly)
        {
            spawnType = SpawnType.Jelly;
            chunkAmount = (int)Mathf.Lerp(3, 4, size);
        }
        else if (type == Enums.CreatureTemplateType.StarElder)
        {
            spawnType = SpawnType.Elder;
            chunkAmount = (int)Mathf.Lerp(30, 32, size);
        }
        else
        {
            spawnType = SpawnType.Base;
            chunkAmount = (int)Mathf.Lerp(6, 16, size);
        }
        sizeFac = Mathf.Lerp(0.5f, 2.5f, size);
        if (IsNoodle() || IsJelly())
        {
            sizeFac /= 2f;
        }
        int num2 = 0;

        bodyChunks = new BodyChunk[1];
        bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 8f * (chunkAmount * 0.15f), .15f);

        bodyChunkConnections = new BodyChunkConnection[0];

        airFriction = 1f;
        gravity = 0f;
        bounce = 0.3f;
        surfaceFriction = 0.5f;
        collisionLayer = 1;
        waterFriction = 1f;
        buoyancy = 0f;
        windAffectiveness = 0f;
        currentAffectiveness = 0f;

        swimCycle = Random.value;
        CollideWithTerrain = true;
        CollideWithObjects = true;
        bodyChunks[0].collideWithObjects = false;
        canBeHitByWeapons = true;

        effectColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(244f / 360f, 15 / 360f, .5f), 0.66f, Custom.ClampedRandomVariation(60f / 360f, 15f / 360f, .5f));

        List<BodyChunk> list = new();
        List<BodyChunkConnection> list2 = new();
        float num4 = Mathf.Lerp(3f, 8f, Random.value);
        float num5 = Mathf.Lerp(Mathf.Lerp(0.5f, 4f, Random.value), num4 / 2f, Random.value);

        runSpeed = Mathf.Lerp(0.7f, 1.2f, Random.value);
        if (IsJelly())
        {
            runSpeed /= 2.5f;
        }

        abstractCreature.personality.dominance *= Mathf.Lerp(0f, 1f, size);

        for (int chunkIndex = 0; chunkIndex < chunkAmount; chunkIndex++)
        {
            float num6 = chunkIndex / (float)(chunkAmount - 1);
            float num7 = Mathf.Lerp(Mathf.Lerp(num4, num5, num6), Mathf.Lerp(num5, num4, Mathf.Sin(Mathf.Pow(num6, 0.5f) * (float)System.Math.PI)), 0.5f);
            list.Add(new BodyChunk(this, num2, default, num7, sizeFac / chunkAmount));
            if (chunkIndex > 0)
            {
                list2.Add(new BodyChunkConnection(list[chunkIndex - 1], list[chunkIndex], Mathf.Lerp((list[chunkIndex - 1].rad + list[chunkIndex].rad) * 1.25f, Mathf.Max(list[chunkIndex - 1].rad, list[chunkIndex].rad), 0.5f), BodyChunkConnection.Type.Normal, 1.2f, -1f));
            }
            num2++;
        }
        mainBody = list.ToArray();
        bodyChunks = list.ToArray();
        bodyChunkConnections = list2.ToArray();

        Stun(80);

        Random.state = state;
    }

    public bool IsNoodle()
    {
        return spawnType == SpawnType.Noodle;
    }

    public bool IsJelly()
    {
        return spawnType == SpawnType.Jelly;
    }

    public override Color ShortCutColor()
    {
        return effectColor;
    }

    public override void InitiateGraphicsModule()
    {
        graphicsModule ??= new StarSpawnGraphics(this);
        graphicsModule.Reset();
    }

    public bool BiggerThan(float targetMass)
    {
        return targetMass * 1.5 < TotalMass && !IsJelly();
    }

    public bool MuchBiggerThan(float targetMass)
    {
        return targetMass * 10 < TotalMass && !IsJelly();
    }

    public bool SmallerThan(float targetMass)
    {
        return targetMass > TotalMass * 1.5;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        if (room == null)
        {
            return;
        }
        lastFade = fade;
        if (startFadeOut)
        {
            fade = Mathf.Max(0f, fade - 1f / 120f);
            if (fade == 0f)
            {
                Destroy();
            }
        }
        else
        {
            fade = Mathf.Min(1f, fade + 0.025f);
        }
        if (timeUntilFadeout > 0)
        {
            timeUntilFadeout--;
            if (timeUntilFadeout == 0)
            {
                startFadeOut = true;
            }
        }
        for (int i = 0; i < bodyChunks.Length; i++)
        {
            mainBody[i].vel *= Custom.LerpMap(mainBody[i].vel.magnitude, 0.2f * sizeFac, 6f * sizeFac, 1f, 0.7f);
        }
        if (Consious)
        {
            float num2 = runSpeed;
            float num3 = -1f;
            swimCycle += 0.2f * num2;
            Vector2 p = mainBody[mainBody.Length - 1].pos + Custom.DirVec(mainBody[0].pos, mainBody[mainBody.Length - 1].pos) * 100f;
            Vector2 vector = Custom.DirVec(mainBody[0].pos, p);
            if (travelDir != null)
            {
                vector = travelDir;
            }
            for (int j = 0; j < mainBody.Length; j++)
            {
                float num4 = j / (mainBody.Length - 1f);
                mainBody[j].vel += vector * (Mathf.Lerp(1f, -1f, Mathf.InverseLerp(0f, 0.5f, num4)) * Mathf.InverseLerp(0.5f, 0f, num4) * 0.06f * sizeFac * num2);
                if (j < mainBody.Length - 1)
                {
                    Vector2 vector2 = Custom.DirVec(mainBody[j + 1].pos, mainBody[j].pos);
                    mainBody[j].vel += (vector2 + Custom.PerpendicularVector(vector2) * (Mathf.Sin(swimCycle - j * 1.2f) * 0.8f * Mathf.Pow(num4, 0.3f))).normalized * (0.2f * sizeFac * num2);
                }
                if (num3 >= 0f && mainBody[j].vel.x > num3)
                {
                    mainBody[j].vel.x *= 0.8f;
                }
                if (num3 >= 0f && mainBody[j].vel.y > num3)
                {
                    mainBody[j].vel.y *= 0.8f;
                }
            }
            mainBody[0].vel += vector * (mainChunkFactor * sizeFac * Mathf.Pow(num2, 2f));
        }
        for (int k = 2; k < mainBody.Length; k++)
        {
            WeightedPush(k - 2, k, Custom.DirVec(mainBody[k].pos, mainBody[k - 2].pos), 0.1f * sizeFac);
        }
        if (!room.RoomRect.Vector2Inside(mainBody[0].pos) && !room.ViewedByAnyCamera(mainBody[0].pos, 200f))
        {
            if (canBeDestroyed)
            {
                Destroy();
            }
        }
        else
        {
            canBeDestroyed = true;
        }
        if (graphicsModule is StarSpawnGraphics)
        {
            culled = !room.ViewedByAnyCamera(mainBody[0].pos, 300f);
            if (!culled && lastCulled)
            {
                (graphicsModule as StarSpawnGraphics).Reset();
            }
            lastCulled = culled;
        }
        if (Consious)
        {
            Act();
        }
        if (grasps[0] != null)
        {
            if (Consious)
            {
                CarryObject(eu);
            }
            else
            {
                LoseAllGrasps();
            }

        }
    }

    void Act()
    {
        AI.Update();

        Vector2 followingPos = bodyChunks[0].pos;
        if (room.GetWorldCoordinate(followingPos) == AI.pathFinder.GetDestination && AI.threatTracker.Utility() < 0.5f)
        {
            GoThroughFloors = false;
            return;
        }

        var pather = AI.pathFinder as StandardPather;
        var movementConnection = pather!.FollowPath(room.GetWorldCoordinate(followingPos), true);
        if (movementConnection == default)
        {
            movementConnection = pather.FollowPath(room.GetWorldCoordinate(followingPos), true);
        }
        if (movementConnection != default)
        {
            Run(movementConnection);
        }
        else
        {
            if (lastFollowedConnection != null)
            {
                MoveTowards(room.MiddleOfTile(lastFollowedConnection.Value.DestTile));
            }
        }
    }

    public override void LoseAllGrasps()
    {
        base.LoseAllGrasps();
        isAnythingGrabbed = false;
    }
    void MoveTowards(Vector2 moveTo)
    {
        Vector2 dir = Custom.DirVec(firstChunk.pos, moveTo);
        travelDir = dir;
    }

    void Run(MovementConnection followingConnection)
    {
        if (followingConnection.type is MovementConnection.MovementType.ShortCut or MovementConnection.MovementType.NPCTransportation)
        {
            enteringShortCut = new IntVector2?(followingConnection.StartTile);
            if (followingConnection.type == MovementConnection.MovementType.NPCTransportation)
            {
                NPCTransportationDestination = followingConnection.destinationCoord;
            }
        }
        else
        {
            MoveTowards(room.MiddleOfTile(followingConnection.DestTile));
        }
        lastFollowedConnection = followingConnection;
    }

    public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
    {
        base.Collide(otherObject, myChunk, otherChunk);
        if (!Consious) return;
        if (otherObject is Creature c && AI?.tracker != null)
        {
            AI.tracker.SeeCreature(c.abstractCreature);
            if (myChunk == 0 && grasps[0] == null && AI.DynamicRelationship(c.abstractCreature).type == CreatureTemplate.Relationship.Type.Eats && BiggerThan(c.TotalMass))
            {
                TryToGrabPrey(otherObject);
            }
        }
    }

    public bool TryToGrabPrey(PhysicalObject prey)
    {
        BodyChunk bodyChunk = null;
        float a = float.MaxValue;
        for (int i = 0; i < prey.bodyChunks.Length; i++)
        {
            if (Custom.DistLess(mainBodyChunk.pos, prey.bodyChunks[i].pos, Mathf.Max(a, prey.bodyChunks[i].rad + mainBodyChunk.rad + 3f)))
            {
                a = Vector2.Distance(mainBodyChunk.pos, prey.bodyChunks[i].pos);
                bodyChunk = prey.bodyChunks[i];
            }
        }
        if (bodyChunk == null)
        {
            return false;
        }
        for (int j = 0; j < 2; j++)
        {
            bodyChunks[j].vel *= 0.75f;
            bodyChunks[j].vel += Custom.DegToVec(Random.value * 360f) * 6f;
        }
        return Grab(prey, 0, bodyChunk.index, Grasp.Shareability.CanNotShare, abstractCreature.personality.dominance, overrideEquallyDominant: true, BiggerThan(prey.TotalMass));
    }


    private void CarryObject(bool eu)
    {
        if (grasps[0] == null)
        {
            isAnythingGrabbed = false;
            return;
        }
        isAnythingGrabbed = true;
        if (Random.value < 0.05f && grasps[0].grabbed is Creature && AI.DynamicRelationship((grasps[0].grabbed as Creature).abstractCreature).type != CreatureTemplate.Relationship.Type.Eats)
        {
            LoseAllGrasps();
            return;
        }
        if (Stunned)
        {
            LoseAllGrasps();
            return;
        }
        if (grasps[0].grabbed is not Creature)
        {
            if (AI.behavior == StarSpawnAI.Behavior.EscapeRain || AI.behavior == StarSpawnAI.Behavior.Injured)
            {
                LoseAllGrasps();
                return;
            }
            if (grasps[0].grabbed is Weapon)
            {
                (grasps[0].grabbed as Weapon).setRotation = Custom.PerpendicularVector(mainBodyChunk.pos, grasps[0].grabbed.firstChunk.pos);
            }
        }
        PhysicalObject grabbed = grasps[0].grabbed;
        float num = mainBodyChunk.rad + grasps[0].grabbed.bodyChunks[grasps[0].chunkGrabbed].rad;
        Vector2 vector = -Custom.DirVec(mainBodyChunk.pos, grabbed.bodyChunks[grasps[0].chunkGrabbed].pos) * (num - Vector2.Distance(mainBodyChunk.pos, grabbed.bodyChunks[grasps[0].chunkGrabbed].pos));
        float num2 = grabbed.bodyChunks[grasps[0].chunkGrabbed].mass / (mainBodyChunk.mass + grabbed.bodyChunks[grasps[0].chunkGrabbed].mass);
        num2 *= 0.2f; //* (1f - AI.stuckTracker.Utility());
        mainBodyChunk.pos += vector * num2;
        mainBodyChunk.vel += vector * num2;
        grabbed.bodyChunks[grasps[0].chunkGrabbed].pos -= vector * (1f - num2);
        grabbed.bodyChunks[grasps[0].chunkGrabbed].vel -= vector * (1f - num2);
        Vector2 vector2 = mainBodyChunk.pos + Custom.DirVec(bodyChunks[1].pos, mainBodyChunk.pos) * num;
        Vector2 vector3 = grabbed.bodyChunks[grasps[0].chunkGrabbed].vel - mainBodyChunk.vel;
        grabbed.bodyChunks[grasps[0].chunkGrabbed].vel = mainBodyChunk.vel;
        if (!enteringShortCut.HasValue && (vector3.magnitude * grabbed.bodyChunks[grasps[0].chunkGrabbed].mass > 30f || !Custom.DistLess(vector2, grabbed.bodyChunks[grasps[0].chunkGrabbed].pos, 70f + grabbed.bodyChunks[grasps[0].chunkGrabbed].rad)))
        {
            LoseAllGrasps();
        }
        else
        {
            grabbed.bodyChunks[grasps[0].chunkGrabbed].MoveFromOutsideMyUpdate(eu, vector2);
        }
        if (grasps[0] != null)
        {
            for (int i = 0; i < 2; i++)
            {
                grasps[0].grabbed.PushOutOf(bodyChunks[i].pos, bodyChunks[i].rad, grasps[0].chunkGrabbed);
            }
            if (grasps[0].grabbed is Player player)
            {
                player.warpFatigueEffect += 0.003f;
                player.warpFatigueEffect = Mathf.Clamp(player.warpFatigueEffect, 0f, 1f);
            }
        }
    }

    public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos hitAppendage, DamageType type, float damage, float stunBonus)
    {
        stunBonus /= 2;
        base.Violence(source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
        if (source?.owner is Weapon && directionAndMomentum.HasValue)
        {
            hitChunk.vel = source.vel * source.mass / hitChunk.mass;
        }
    }

    public override float EffectiveRoomGravity => 0f;

    int bites = 2;

    public int BitesLeft => bites;
    public int FoodPoints => 0;
    bool IPlayerEdible.Edible => dead;
    bool IPlayerEdible.AutomaticPickUp => false;

    void IPlayerEdible.ThrowByPlayer() { }

    void IPlayerEdible.BitByPlayer(Grasp grasp, bool eu)
    {
        bites--;

        firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);

        if (bites == 0 && grasp.grabber is Player p)
        {
            p.ObjectEaten(this);
            grasp.Release();
            Destroy();
        }
    }
}