using RWCustom;
using UnityEngine;
using Watcher;
using static lsfUtils.Plugin;

namespace lsfUtils.Creatures.Misc.Porcupine;

public class PorcupineAI : ArtificialIntelligence, IUseARelationshipTracker
{
    public class Behavior : ExtEnum<Behavior>
    {
        public static readonly Behavior Idle = new("Idle", register: true);

        public static readonly Behavior Flee = new("Flee", register: true);

        public static readonly Behavior EscapeRain = new("EscapeRain", register: true);

        public Behavior(string value, bool register = false) : base(value, register)
        {
        }
    }

    public const int maxCreatures = 3;
    public const int spearResetTime = 800;
    public const int spearRetryTime = 200;

    public Porcupine porcupine;
    public Porcupine buddyPorcupine;

    public float currentUtility;
    public Behavior behavior;
    public int idlePosCounter;
    public WorldCoordinate tempIdlePos;

    public Spear spearToGrab;
    public int spearCounter;
    public float fear;

    public PorcupineAI(AbstractCreature creature, World world) : base(creature, world)
    {
        spearToGrab = null;
        spearCounter = spearRetryTime;
        fear = 0.5f;
        porcupine = creature.realizedCreature as Porcupine;
        buddyPorcupine = null;

        if (porcupine != null) porcupine.AI = this;
        else Log.LogMessage("No AI!");

        AddModule(new StandardPather(this, world, creature));
        pathFinder.stepsPerFrame = 10;
        AddModule(new Tracker(this, 10, maxCreatures, 450, 0.5f, 5, 5, 10));
        AddModule(new ThreatTracker(this, maxCreatures));
        AddModule(new RainTracker(this));
        AddModule(new DenFinder(this, creature));
        AddModule(new NoiseTracker(this, tracker));
        AddModule(new UtilityComparer(this));
        AddModule(new RelationshipTracker(this, tracker));
        FloatTweener.FloatTween smoother = new FloatTweener.FloatTweenUpAndDown(new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Lerp, 0.5f), new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Tick, 0.005f));
        utilityComparer.AddComparedModule(threatTracker, smoother, 1f, 1.1f);
        utilityComparer.AddComparedModule(rainTracker, null, 1f, 1.1f);
        behavior = Behavior.Idle;
    }

    AIModule IUseARelationshipTracker.ModuleToTrackRelationship(CreatureTemplate.Relationship relationship)
    {
        return threatTracker;
    }

    RelationshipTracker.TrackedCreatureState IUseARelationshipTracker.CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel)
    {
        return new RelationshipTracker.TrackedCreatureState();
    }

    CreatureTemplate.Relationship IUseARelationshipTracker.UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation)
    {
        if (dRelation.trackerRep.VisualContact)
        {
            dRelation.state.alive = dRelation.trackerRep.representedCreature.state.alive;
        }
        CreatureTemplate.Relationship result = StaticRelationship(dRelation.trackerRep.representedCreature);
        if (result.type == CreatureTemplate.Relationship.Type.Afraid && !dRelation.state.alive)
        {
            result.intensity = 0f;
        }
        return result;
    }

    public override void NewRoom(Room room)
    {
        base.NewRoom(room);
        Log.LogMessage("Changed room!");
        spearCounter = spearRetryTime;
        spearToGrab = null;
    }

    public bool IsSpearGrabbable(Spear spear)
    {
        if (porcupine == null) return false;
        if (porcupine.room == null) return false;
        if (spear == null) return false;
        if (spear.room == null) return false;
        if (porcupine.room != spear.room) return false;
        if (porcupine.abstractCreature == null) return false;
        if (porcupine.abstractPhysicalObject == null) return false;
        if (spear.abstractPhysicalObject == null) return false;
        if (porcupine.abstractCreature.rippleLayer != spear.abstractPhysicalObject.rippleLayer && !porcupine.abstractPhysicalObject.rippleBothSides && !spear.abstractPhysicalObject.rippleBothSides) return false;
        if (spear.slatedForDeletetion) return false;
        if (spear.mode != Weapon.Mode.Free) return false;
        if (spear.grabbedBy != null && spear.grabbedBy.Count > 0) return false;
        if (spear.firstChunk == null) return false;
        return true;
    }

    public override void Update()
    {
        base.Update();

        if (porcupine == null)
        {
            porcupine = creature?.realizedCreature as Porcupine;
            if (porcupine == null) return;
            porcupine.AI = this;
        }
        if (porcupine?.room == null) return;

        pathFinder.walkPastPointOfNoReturn = stranded || !denFinder.GetDenPosition().HasValue || !pathFinder.CoordinatePossibleToGetBackFrom(denFinder.GetDenPosition().Value) || threatTracker.Utility() > 0.95f;
        noiseTracker.hearingSkill = 2f;
        AIModule aIModule = utilityComparer.HighestUtilityModule();
        currentUtility = utilityComparer.HighestUtility();
        if (aIModule != null)
        {
            behavior = Behavior.Idle;
            /*if (aIModule is ThreatTracker)
            {
                behavior = Behavior.Flee;
            }
            else if (aIModule is RainTracker)
            {
                behavior = Behavior.EscapeRain;
            }*/
        }
        if (currentUtility < 0.2f)
        {
            behavior = Behavior.Idle;
        }
        if (behavior == Behavior.Idle)
        {
            if (spearToGrab != null && !IsSpearGrabbable(spearToGrab))
            {
                spearToGrab = null;
                spearCounter = spearRetryTime;
            }

            spearCounter--;
            if (spearCounter <= 0) GetNewSpear();

            if (spearToGrab == null)
            {
                porcupine.runSpeed = Custom.LerpAndTick(porcupine.runSpeed, 0.5f + 0.5f * Mathf.Max(threatTracker.Utility(), fear), 0.01f, 1f / 60f);
                bool flag = pathFinder.GetDestination.room != porcupine.room.abstractRoom.index;
                if (!flag && idlePosCounter <= 0)
                {
                    int abstractNode = porcupine.room.abstractRoom.RandomNodeInRoom().abstractNode;
                    if (porcupine.room.abstractRoom.nodes[abstractNode].type == AbstractRoomNode.Type.Exit)
                    {
                        int num2 = porcupine.room.abstractRoom.CommonToCreatureSpecificNodeIndex(abstractNode, porcupine.Template);
                        if (num2 > -1)
                        {
                            int num3 = porcupine.room.aimap.ExitDistanceForCreatureAndCheckNeighbours(porcupine.abstractCreature.pos.Tile, num2, porcupine.Template);
                            if (num3 > -1 && num3 < 400)
                            {
                                AbstractRoom abstractRoom = null;
                                if (porcupine.room.game != null && porcupine.room.game.world != null)
                                {
                                    abstractRoom = porcupine.room.game.world.GetAbstractRoom(porcupine.room.abstractRoom.connections[abstractNode]);
                                }
                                if (abstractRoom != null)
                                {
                                    WorldCoordinate worldCoordinate = abstractRoom.RandomNodeInRoom();
                                    if (pathFinder.CoordinateReachableAndGetbackable(worldCoordinate))
                                    {
                                        Log.LogMessage("Porcupine leaving room!");
                                        creature.abstractAI.SetDestination(worldCoordinate);
                                        idlePosCounter = Random.Range(200, 500);
                                        flag = true;
                                    }
                                }
                            }
                        }
                    }
                }
                if (!flag)
                {
                    WorldCoordinate coord = new WorldCoordinate(porcupine.room.abstractRoom.index, Random.Range(0, porcupine.room.TileWidth), Random.Range(0, porcupine.room.TileHeight), -1);
                    if (IdleScore(coord) < IdleScore(tempIdlePos))
                    {
                        tempIdlePos = coord;
                    }
                    if (IdleScore(tempIdlePos) < IdleScore(pathFinder.GetDestination) + Custom.LerpMap(idlePosCounter, 0f, 300f, 100f, -300f))
                    {
                        creature.abstractAI.SetDestination(tempIdlePos);
                        idlePosCounter = Random.Range(200, 800);
                        tempIdlePos = new WorldCoordinate(porcupine.room.abstractRoom.index, Random.Range(0, porcupine.room.TileWidth), Random.Range(0, porcupine.room.TileHeight), -1);
                    }
                }
                idlePosCounter--;
            }
            else if (spearToGrab?.firstChunk != null)
            {
                WorldCoordinate? spearCoord = porcupine?.room?.GetWorldCoordinate(spearToGrab.firstChunk.pos);
                if (pathFinder.GetDestination != spearCoord && spearCoord != null)
                {
                    creature.abstractAI.SetDestination(spearCoord.Value);
                }
            }
        }
        else if (behavior == Behavior.Flee)
        {
            porcupine.runSpeed = Custom.LerpAndTick(porcupine.runSpeed, 1f, 0.01f, 0.1f);
            creature.abstractAI.SetDestination(threatTracker.FleeTo(creature.pos, 10, 30, considerLeavingRoom: true));
        }
        else if (behavior == Behavior.EscapeRain)
        {
            porcupine.runSpeed = Custom.LerpAndTick(porcupine.runSpeed, 1f, 0.01f, 0.1f);
            if (denFinder.GetDenPosition().HasValue)
            {
                creature.abstractAI.SetDestination(denFinder.GetDenPosition().Value);
            }
        }
    }

    public void GetNewSpear()
    {
        spearToGrab = null;
        spearCounter = spearRetryTime;

        if (porcupine?.room == null) return;
        if (porcupine.firstChunk?.pos == null) return;
        if (porcupine.room.physicalObjects == null || porcupine.room.physicalObjects.Length == 0) return;

        Spear closestSpear = null;
        float lowestDist = float.MaxValue;
        for (int i = 0; i < porcupine.room.physicalObjects.Length; i++)
        {
            for (int j = 0; j < porcupine.room.physicalObjects[i].Count; j++)
            {
                if (porcupine.room.physicalObjects[i][j] is not Spear spear) continue;
                if (IsSpearGrabbable(spear) && pathFinder.CoordinateReachableAndGetbackable(porcupine.room.GetWorldCoordinate(spear.firstChunk.pos)))
                {
                    float currentDist = Vector2.Distance(spear.firstChunk.pos, porcupine.firstChunk.pos);
                    if (currentDist < lowestDist)
                    {
                        lowestDist = currentDist;
                        closestSpear = spear;
                    }
                }
            }
        }

        if (closestSpear != null)
        {
            spearToGrab = closestSpear;
            spearCounter = spearResetTime;
        }
        else
        {
            spearCounter = spearRetryTime;
        }
    }

    public float IdleScore(WorldCoordinate coord)
    {
        if (porcupine == null || porcupine.room == null || porcupine.room.aimap == null)
        {
            return float.MaxValue;
        }
        if (coord.room != creature.pos.room || !pathFinder.CoordinateReachableAndGetbackable(coord) || porcupine.room.aimap.getAItile(coord).acc >= AItile.Accessibility.Wall)
        {
            return float.MaxValue;
        }
        float num = 1f;
        if (pathFinder.CoordinateReachableAndGetbackable(coord + new IntVector2(0, -1)))
        {
            num += 10f;
        }
        if (porcupine.room.aimap.getAItile(coord).narrowSpace)
        {
            num += 50f;
        }
        num += threatTracker.ThreatOfTile(coord, accountThreatCreatureAccessibility: true) * 1000f;
        num += threatTracker.ThreatOfTile(porcupine.room.GetWorldCoordinate((porcupine.room.MiddleOfTile(coord) + porcupine.room.MiddleOfTile(creature.pos)) / 2f), accountThreatCreatureAccessibility: true) * 1000f;
        for (int i = 0; i < noiseTracker.sources.Count; i++)
        {
            num += Custom.LerpMap(Vector2.Distance(porcupine.room.MiddleOfTile(coord), noiseTracker.sources[i].pos), 40f, 400f, 100f, 0f);
        }
        return num;
    }

    public override bool WantToStayInDenUntilEndOfCycle()
    {
        return rainTracker.Utility() > 0.01f;
    }

    public override Tracker.CreatureRepresentation CreateTrackerRepresentationForCreature(AbstractCreature otherCreature)
    {
        if (otherCreature.creatureTemplate.smallCreature)
        {
            return new Tracker.SimpleCreatureRepresentation(tracker, otherCreature, 0f, forgetWhenNotVisible: false);
        }
        return new Tracker.ElaborateCreatureRepresentation(tracker, otherCreature, 1f, 3);
    }

    public override PathCost TravelPreference(MovementConnection coord, PathCost cost)
    {
        float val = Mathf.Max(0f, threatTracker.ThreatOfTile(coord.destinationCoord, accountThreatCreatureAccessibility: false) - threatTracker.ThreatOfTile(creature.pos, accountThreatCreatureAccessibility: false));
        return new PathCost(cost.resistance + Custom.LerpMap(val, 0f, 1.5f, 0f, 10000f, 5f), cost.legality);
    }
}