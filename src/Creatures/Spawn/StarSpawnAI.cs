using RWCustom;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static CreatureTemplate.Relationship.Type;

namespace lsfUtils.Creatures.Spawn;

public class StarSpawnAI : ArtificialIntelligence, IUseARelationshipTracker
{
    public enum Behavior
    {
        Idle,
        Swarm,
        Flee,
        EscapeRain,
        Hunt,
        Follow,
        Injured
    }

    public class StardustSpawnTrackedState : RelationshipTracker.TrackedCreatureState
    {
        bool holdingFlower;
    }

    public StarSpawn spawn;
    public int tiredOfHuntingCounter;
    public AbstractCreature tiredOfHuntingCreature;
    public Behavior behavior;
    private int behaviorCounter;
    private WorldCoordinate tempIdlePos;

    public WorldCoordinate foodPos;
    public bool hasFoodPos;
    public int foodCounter = 1000;
    public List<PhysicalObject> foodInRoom = new();

    public StarSpawnAI(AbstractCreature acrit, StarSpawn spawn) : base(acrit, acrit.world)
    {
        this.spawn = spawn;
        spawn.AI = this;
        AddModule(new StandardPather(this, acrit.world, acrit));
        pathFinder.stepsPerFrame = 20;
        AddModule(new Tracker(this, 10, 10, 600, 0.5f, 5, 5, 10));
        AddModule(new ThreatTracker(this, 3));
        AddModule(new RainTracker(this));
        AddModule(new DenFinder(this, acrit));
        AddModule(new NoiseTracker(this, tracker));
        AddModule(new PreyTracker(this, 5, 1f, 5f, 150f, 0.05f));
        AddModule(new UtilityComparer(this));
        AddModule(new RelationshipTracker(this, tracker));
        AddModule(new FriendTracker(this));
        friendTracker.followClosestFriend = true;
        var smoother = new FloatTweener.FloatTweenUpAndDown(new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Lerp, 0.5f), new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Tick, 0.005f));

        utilityComparer.AddComparedModule(threatTracker, smoother, 1f, 1.1f);
        utilityComparer.AddComparedModule(rainTracker, null, 1f, 1.1f);
        utilityComparer.AddComparedModule(preyTracker, null, 0.4f, 1.1f);
        noiseTracker.hearingSkill = 0.5f;
        behavior = Behavior.Idle;
        Debug.Log("Star spawn AI start");
    }

    AIModule IUseARelationshipTracker.ModuleToTrackRelationship(CreatureTemplate.Relationship relationship)
    {
        if (relationship.type == Eats) return preyTracker;
        if (relationship.type == Afraid) return threatTracker;
        return null;
    }

    RelationshipTracker.TrackedCreatureState IUseARelationshipTracker.CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel)
    {
        return new StardustSpawnTrackedState();
    }
    CreatureTemplate.Relationship IUseARelationshipTracker.UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation)
    {
        if (dRelation.state is not StardustSpawnTrackedState) return default;

        CreatureTemplate.Relationship result = StaticRelationship(dRelation.trackerRep.representedCreature).Duplicate();
        bool biggerThan = dRelation.trackerRep?.representedCreature?.realizedCreature != null && spawn?.TotalMass != null && spawn.BiggerThan(dRelation.trackerRep.representedCreature.realizedCreature.TotalMass);
        bool smallerThan = dRelation.trackerRep?.representedCreature?.realizedCreature != null && spawn?.TotalMass != null && spawn.SmallerThan(dRelation.trackerRep.representedCreature.realizedCreature.TotalMass);
        if (result.type == Ignores)
        {
            return result;
        }
        if (dRelation.trackerRep.representedCreature.realizedCreature != null)
        {
            if (!spawn.isAnythingGrabbed)
            {
                return new CreatureTemplate.Relationship(Afraid, Mathf.Clamp(Mathf.Abs(spawn.TotalMass - dRelation.trackerRep.representedCreature.realizedCreature.TotalMass) / spawn.TotalMass, 0f, 1f));
            }
            if (dRelation.trackerRep.representedCreature.realizedCreature != null && dRelation.trackerRep.representedCreature.realizedCreature is Player)
            {
                if (friendTracker.giftOfferedToMe != null && friendTracker.giftOfferedToMe.owner == dRelation.trackerRep.representedCreature.realizedCreature)
                {
                    return new CreatureTemplate.Relationship(Ignores, 0f);
                }
                
                float likeOfPlayer = creature.world.game.session.creatureCommunities.LikeOfPlayer(Enums.CreatureCommunityID.StarSpawn, creature.world.RegionNumber, (dRelation.trackerRep.representedCreature.state as PlayerState).playerNumber);
                if (creature?.state?.socialMemory != null)
                {
                    float tempLike = creature.state.socialMemory.GetTempLike(dRelation.trackerRep.representedCreature.ID);
                    likeOfPlayer = Mathf.Lerp(likeOfPlayer, tempLike, Mathf.Abs(tempLike));
                }
                if (likeOfPlayer > 0.5f)
                {
                    return new CreatureTemplate.Relationship(PlaysWith, 1f); // TODO friend behaviour
                }
                else if (likeOfPlayer < -0.5f)
                {
                    return new CreatureTemplate.Relationship(Afraid, 1f);
                }
            }
            if (result.type == Eats)
            {
                if (biggerThan)
                {
                    if (friendTracker.giftOfferedToMe != null && friendTracker.giftOfferedToMe.active && friendTracker.giftOfferedToMe.item == dRelation.trackerRep.representedCreature.realizedCreature)
                    {
                        return new CreatureTemplate.Relationship(Eats, dRelation.trackerRep.representedCreature.state.dead ? 1f : 0.65f);
                    }
                    return new CreatureTemplate.Relationship(Eats, Mathf.Clamp(Mathf.Abs(spawn.TotalMass - dRelation.trackerRep.representedCreature.realizedCreature.TotalMass) / spawn.TotalMass, 0f, 1f));
                }
                if (dRelation.trackerRep.representedCreature.realizedCreature.dead || !smallerThan)
                {
                    return new CreatureTemplate.Relationship(Ignores, 0f);
                }
                return new CreatureTemplate.Relationship(Afraid, Mathf.Clamp(Mathf.Abs(spawn.TotalMass - dRelation.trackerRep.representedCreature.realizedCreature.TotalMass) / spawn.TotalMass, 0f, 1f));
            }
        }
        return StaticRelationship(dRelation.trackerRep.representedCreature);
    }

    public override void Update()
    {
        base.Update();

        if (spawn.room == null)
        {
            return;
        }
        pathFinder.walkPastPointOfNoReturn = stranded
            || denFinder.GetDenPosition() is not WorldCoordinate denPos
            || !pathFinder.CoordinatePossibleToGetBackFrom(denPos)
            || threatTracker.Utility() > 0.95f;

        utilityComparer.GetUtilityTracker(threatTracker).weight = Custom.LerpMap(threatTracker.ThreatOfTile(creature.pos, true), 0.1f, 2f, 0.1f, 1f, 0.5f);

        if (utilityComparer.HighestUtility() < 0.02f && (behavior != Behavior.Hunt || preyTracker.MostAttractivePrey == null))
        {
            if (behavior is not Behavior.Idle or Behavior.Swarm)
            {
                behaviorCounter = 0;
                behavior = Random.value < 0.1f ? Behavior.Idle : Behavior.Swarm;
            }
        }
        else
        {
            behavior = utilityComparer.HighestUtilityModule() switch
            {
                ThreatTracker => Behavior.Flee,
                RainTracker => Behavior.EscapeRain,
                PreyTracker => Behavior.Hunt,
                InjuryTracker => Behavior.Flee,
                FriendTracker => Behavior.Follow,
                _ => behavior
            };
        }

        switch (behavior)
        {
            case Behavior.Idle:
                spawn.runSpeed = Custom.LerpAndTick(spawn.runSpeed, 0.6f + 0.4f * threatTracker.Utility(), 0.01f, 0.016666668f);

                if (!hasFoodPos)
                {
                    if (foodCounter == 0)
                    {
                        foodCounter = 1000;
                        foodInRoom = new();
                        for (int i = 0; i < creature.Room.realizedRoom.physicalObjects.Length; i++)
                        {
                            foreach (PhysicalObject item in creature.Room.realizedRoom.physicalObjects[i])
                            {
                                if (item is KarmaFlower)
                                {
                                    foodInRoom.Add(item);
                                }
                            }
                        }
                        if (foodInRoom.Count > 0)
                        {
                            hasFoodPos = true;
                            foodCounter = 5000;
                            foodPos = spawn.room.GetWorldCoordinate(foodInRoom[Random.Range(0, foodInRoom.Count - 1)].firstChunk.pos);
                        }
                    }
                }
                else
                {
                   foodCounter--;
                    if (foodCounter == 0)
                    {
                        hasFoodPos = false;
                    }
                    pathFinder.SetDestination(foodPos);
                    break;
                }
                WorldCoordinate coord = new(spawn.room.abstractRoom.index, Random.Range(0, spawn.room.TileWidth), Random.Range(0, spawn.room.TileHeight), -1);
                if (IdleScore(tempIdlePos) > IdleScore(coord))
                {
                    tempIdlePos = coord;
                }

                if (IdleScore(tempIdlePos) < IdleScore(pathFinder.GetDestination) + Custom.LerpMap(behaviorCounter, 0f, 300f, 100f, -300f))
                {
                    SetDestination(tempIdlePos);
                    behaviorCounter = Random.Range(100, 400);
                    tempIdlePos = new WorldCoordinate(spawn.room.abstractRoom.index, Random.Range(0, spawn.room.TileWidth), Random.Range(0, spawn.room.TileHeight), -1);
                }

                behaviorCounter--;
                break;

            case Behavior.Flee:
                spawn.runSpeed = Custom.LerpAndTick(spawn.runSpeed, 0.8f, 0.01f, 0.01f);
                creature.abstractAI.SetDestination(threatTracker.FleeTo(creature.pos, 20, 20, true));
                break;

            case Behavior.Hunt:
                spawn.runSpeed = Custom.LerpAndTick(spawn.runSpeed, 0.8f, 0.01f, 0.01f);

                if (preyTracker.MostAttractivePrey != null)
                    creature.abstractAI.SetDestination(preyTracker.MostAttractivePrey.BestGuessForPosition());

                tiredOfHuntingCounter++;
                if (tiredOfHuntingCounter > 280)
                {
                    tiredOfHuntingCreature = preyTracker.MostAttractivePrey?.representedCreature;
                    tiredOfHuntingCounter = 0;
                    preyTracker.ForgetPrey(tiredOfHuntingCreature);
                    tracker.ForgetCreature(tiredOfHuntingCreature);
                }
                break;

            case Behavior.EscapeRain:
                spawn.runSpeed = Custom.LerpAndTick(spawn.runSpeed, 1f, 0.01f, 0.1f);
                if (denFinder.GetDenPosition() is WorldCoordinate den)
                {
                    creature.abstractAI.SetDestination(den);
                }
                break;
        }
    }

    private float IdleScore(WorldCoordinate coord)
    {
        if (coord.NodeDefined || coord.room != creature.pos.room || !pathFinder.CoordinateReachableAndGetbackable(coord) || spawn.room.aimap.getAItile(coord).acc == AItile.Accessibility.Solid)
        {
            return float.MaxValue;
        }
        float result = 1f;
        if (spawn.room.aimap.getAItile(coord).narrowSpace)
        {
            result += 100f;
        }
        result += threatTracker.ThreatOfTile(coord, true) * 1000f;
        result += threatTracker.ThreatOfTile(spawn.room.GetWorldCoordinate((spawn.room.MiddleOfTile(coord) + spawn.room.MiddleOfTile(creature.pos)) / 2f), true) * 1000f;
        for (int i = 0; i < noiseTracker.sources.Count; i++)
        {
            result += Custom.LerpMap(Vector2.Distance(spawn.room.MiddleOfTile(coord), noiseTracker.sources[i].pos), 40f, 400f, 100f, 0f);
        }
        return result;
    }

    public override bool WantToStayInDenUntilEndOfCycle()
    {
        return rainTracker.Utility() > 0.01f;
    }

    public override Tracker.CreatureRepresentation CreateTrackerRepresentationForCreature(AbstractCreature otherCreature)
    {
        return otherCreature.creatureTemplate.smallCreature
            ? new Tracker.SimpleCreatureRepresentation(tracker, otherCreature, 0f, false)
            : new Tracker.ElaborateCreatureRepresentation(tracker, otherCreature, 1f, 3);
    }

    public override PathCost TravelPreference(MovementConnection coord, PathCost cost)
    {
        float val = Mathf.Max(0f, threatTracker.ThreatOfTile(coord.destinationCoord, false) - threatTracker.ThreatOfTile(creature.pos, false));
        return new PathCost(cost.resistance + Custom.LerpMap(val, 0f, 1.5f, 0f, 10000f, 5f), cost.legality);
    }
}