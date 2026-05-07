using DevInterface;
using Fisobs.Creatures;
using Fisobs.Properties;
using Fisobs.Sandbox;
using RWCustom;
using System.Collections.Generic;
using UnityEngine;
using static PathCost.Legality;
using CreatureType = CreatureTemplate.Type;

namespace lsfUtils.Creatures.Spawn;

sealed class StarJellyCritob : Critob
{

    public StarJellyCritob() : base(Enums.CreatureTemplateType.StarJelly)
    {
        LoadedPerformanceCost = 20f;
        SandboxPerformanceCost = new(linear: 0.6f, exponential: 0.1f);
        ShelterDanger = ShelterDanger.Safe;
        CreatureName = "StarJelly";

        RegisterUnlock(killScore: KillScore.Configurable(2), Enums.SandboxUnlockID.StarSpawn, parent: MultiplayerUnlocks.SandboxUnlockID.Slugcat, data: 0);
    }

    public override CreatureTemplate CreateTemplate()
    {

        CreatureTemplate t = new CreatureFormula(this)
        {

            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Ignores, 0f),
            HasAI = true,
            InstantDeathDamage = 3f,
            Pathing = PreBakedPathing.Ancestral(CreatureType.BigNeedleWorm),
            TileResistances = new()
            {
                Air = new(1, Allowed),
                Corridor = new(1, Allowed),
            },
            ConnectionResistances = new()
            {
                Standard = new PathCost(1f, Allowed),
                OpenDiagonal = new PathCost(1f, Allowed),
                ReachOverGap = new PathCost(1f, Allowed),
                ReachUp = new PathCost(1f, Allowed),
                ReachDown = new PathCost(1f, Allowed),
                SemiDiagonalReach = new PathCost(1f, Allowed),
                DropToFloor = new PathCost(1f, Allowed),
                DropToWater = new PathCost(1f, Allowed),
                DropToClimb = new PathCost(1f, Allowed),
                ShortCut = new PathCost(5f, Allowed),
                NPCTransportation = new PathCost(5f, Allowed),
                OffScreenMovement = new PathCost(1.5f, Allowed),
                BetweenRooms = new PathCost(2f, Allowed),
                Slope = new PathCost(1.5f, Allowed),
                CeilingSlope = new PathCost(1.5f, Allowed)
            },
            DamageResistances = new()
            {
                Base = 1f
            },
            StunResistances = new()
            {
                Base = 1f,
            }
        }.IntoTemplate();
        t.lungCapacity = float.MaxValue;
        t.scaryness = 1f;
        t.smallCreature = false;
        t.socialMemory = true;
        t.wormGrassImmune = true;
        t.communityID = Enums.CreatureCommunityID.StarSpawn;
        t.communityInfluence = 1f;
        t.shortcutColor = RainWorld.RippleColor;
        t.shortcutSegments = 3;
        t.offScreenSpeed = 0.1f;
        t.abstractedLaziness = 200;
        t.roamBetweenRoomsChance = 0.07f;
        t.bodySize = 1f;
        t.stowFoodInDen = true;
        t.shortcutSegments = 2;
        t.grasps = 1;
        t.visualRadius = 1200f;
        t.movementBasedVision = 0.2f;
        t.communityInfluence = 0.5f;
        t.waterRelationship = CreatureTemplate.WaterRelationship.Amphibious;
        t.waterPathingResistance = 2f;
        t.canFly = true;
        t.meatPoints = 1;
        t.dangerousToPlayer = 0.5f;
        t.baseDamageResistance = 2.5f;
        t.baseStunResistance = 2f;
        t.ghostSedationImmune = true;

        return t;
    }

    public override void EstablishRelationships()
    {
        Relationships self = new(Enums.CreatureTemplateType.StarJelly);

        foreach (var template in StaticWorld.creatureTemplates)
        {
            if (template.quantified)
            {
                self.Ignores(template.type);
                self.IgnoredBy(template.type);
            }
        }

        self.Ignores(Enums.CreatureTemplateType.StarSpawn);

        self.Eats(CreatureType.Slugcat, 1f);
        self.Eats(CreatureType.Scavenger, 0.6f);
        self.Eats(CreatureType.LizardTemplate, 0.3f);
        self.Eats(CreatureType.CicadaA, 0.4f);

        self.Intimidates(CreatureType.LizardTemplate, 0.35f);
        self.Intimidates(CreatureType.CicadaA, 0.3f);

        self.AttackedBy(CreatureType.Slugcat, 0.2f);
        self.AttackedBy(CreatureType.Scavenger, 0.2f);

        self.EatenBy(CreatureType.BigSpider, 0.35f);

        self.Fears(CreatureType.Spider, 0.2f);
        self.Fears(CreatureType.BigSpider, 0.2f);
        self.Fears(CreatureType.SpitterSpider, 0.6f);
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit)
    {
        return new StarSpawnAI(acrit, (StarSpawn)acrit.realizedCreature);
    }

    public override Creature CreateRealizedCreature(AbstractCreature acrit)
    {
        return new StarJelly(acrit, acrit.world);
    }

    //public override void ConnectionIsAllowed(AImap map, MovementConnection connection, ref bool? allowed)
    //{
    // DLLs don't travel through shortcuts that start and end in the same room—they only travel through room exits.
    // To emulate this behavior, use something like:

    //ShortcutData.Type n = ShortcutData.Type.Normal;
    //if (connection.type == MovementConnection.MovementType.ShortCut) {
    //    allowed &=
    //        connection.startCoord.TileDefined && map.room.shortcutData(connection.StartTile).shortCutType == n ||
    //        connection.destinationCoord.TileDefined && map.room.shortcutData(connection.DestTile).shortCutType == n
    //        ;
    //} else if (connection.type == MovementConnection.MovementType.BigCreatureShortCutSqueeze) {
    //    allowed &=
    //        map.room.GetTile(connection.startCoord).Terrain == Room.Tile.TerrainType.ShortcutEntrance && map.room.shortcutData(connection.StartTile).shortCutType == n ||
    //        map.room.GetTile(connection.destinationCoord).Terrain == Room.Tile.TerrainType.ShortcutEntrance && map.room.shortcutData(connection.DestTile).shortCutType == n
    //        ;
    //}
    //}

    //public override void TileIsAllowed(AImap map, IntVector2 tilePos, ref bool? allowed)
    //{
    // Large creatures like vultures, miros birds, and DLLs need 2 tiles of free space to move around in. Leviathans need 4! None of them can fit in one-tile tunnels.
    // To emulate this behavior, use something like:

    //allowed &= map.IsFreeSpace(tilePos, tilesOfFreeSpace: 2);

    // DLLs can fit into shortcuts despite being fat.
    // To emulate this behavior, use something like:

    //allowed |= map.room.GetTile(tilePos).Terrain == Room.Tile.TerrainType.ShortcutEntrance;
    //}

    public override IEnumerable<string> WorldFileAliases()
    {
        yield return "StarJelly";
        yield return "SJelly";
    }

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction()
    {
        yield return RoomAttractivenessPanel.Category.Flying;
        yield return RoomAttractivenessPanel.Category.LikesOutside;
    }

    public override string DevtoolsMapName(AbstractCreature acrit)
    {
        return "sn";
    }

    public override Color DevtoolsMapColor(AbstractCreature acrit)
    {
        return RainWorld.RippleColor;
    }

    public override ItemProperties Properties(Creature crit)
    {
        // If you don't need the `forObject` parameter, store one ItemProperties instance as a static object and return that.
        // The CentiShields example demonstrates this.
        if (crit is StarSpawn spawn)
        {
            return new StarSpawnProperties(spawn);
        }

        return null;
    }
}