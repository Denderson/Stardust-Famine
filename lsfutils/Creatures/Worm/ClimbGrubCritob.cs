using Fisobs.Core;
using Fisobs.Creatures;
using Fisobs.Sandbox;
using lsfUtils.Creatures.Worm;
using System.Collections.Generic;
using UnityEngine;

namespace lsfUtils.Creatures.Worm
{
    public class ClimbGrubCritob : Critob
    {
        public ClimbGrubCritob() : base(Enums.CreatureTemplateType.ClimbGrub)
        {
            Icon = new SimpleIcon("Kill_Tubeworm", new Color(1f, 0.6f, 0.8f));
            LoadedPerformanceCost = 20f;
            SandboxPerformanceCost = new(.15f, .15f);
            RegisterUnlock(KillScore.Configurable(1), Enums.SandboxUnlockID.ClimbGrub, MultiplayerUnlocks.SandboxUnlockID.Slugcat);
        }

        public override int ExpeditionScore() => 3;
        public override Color DevtoolsMapColor(AbstractCreature acrit) => new(1f, 0.6f, 0.8f);
        public override string DevtoolsMapName(AbstractCreature acrit) => "CG";
        public override IEnumerable<string> WorldFileAliases() => ["climbgrub"];

        public override CreatureTemplate CreateTemplate()
        {
            CreatureTemplate creatureTemplate = new CreatureFormula(CreatureTemplate.Type.TubeWorm, Type, "ClimbGrub")
            {
                TileResistances = new TileResist
                {
                    OffScreen = new PathCost(4f, PathCost.Legality.Unallowed),
                    Floor = new PathCost(4f, PathCost.Legality.Allowed),
                    Corridor = new PathCost(4f, PathCost.Legality.Allowed),
                    Climb = new PathCost(6f, PathCost.Legality.Unallowed),
                    Wall = new PathCost(12f, PathCost.Legality.Unallowed),
                    Ceiling = new PathCost(12f, PathCost.Legality.Unallowed)
                },
                ConnectionResistances = new ConnectionResist
                {
                    Standard = new PathCost(1f, PathCost.Legality.Allowed),
                    OpenDiagonal = new PathCost(4f, PathCost.Legality.Allowed),
                    ReachOverGap = new PathCost(4f, PathCost.Legality.Unallowed),
                    ReachUp = new PathCost(3f, PathCost.Legality.Allowed),
                    ReachDown = new PathCost(3f, PathCost.Legality.Allowed),
                    SemiDiagonalReach = new PathCost(2f, PathCost.Legality.Allowed),
                    DropToFloor = new PathCost(10f, PathCost.Legality.Unallowed),
                    DropToWater = new PathCost(10f, PathCost.Legality.Unallowed),
                    DropToClimb = new PathCost(10f, PathCost.Legality.Unallowed),
                    ShortCut = new PathCost(1.5f, PathCost.Legality.Unallowed),
                    NPCTransportation = new PathCost(3f, PathCost.Legality.Unallowed),
                    OffScreenMovement = new PathCost(1f, PathCost.Legality.Unallowed),
                    BetweenRooms = new PathCost(5f, PathCost.Legality.Unallowed),
                    Slope = new PathCost(1.5f, PathCost.Legality.Unallowed),
                    CeilingSlope = new PathCost(1.5f, PathCost.Legality.Unallowed)
                },
                DefaultRelationship = new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f),
                HasAI = true,
                Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.TubeWorm),
                DamageResistances = new AttackResist { Base = 1f },
                StunResistances = new AttackResist { Base = 1f }
            }.IntoTemplate();
            return creatureTemplate;
        }

        public override void EstablishRelationships()
        {
            Relationships relationships = new(Type);
            List<string> entries = ExtEnum<CreatureTemplate.Type>.values.entries;
            for (int i = 0; i < entries.Count; i++)
            {
                relationships.Ignores(new CreatureTemplate.Type(entries[i], false));
            }
            relationships.Ignores(Type);
        }

        public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit)
        {
            return new TubeWormAI(acrit, acrit.world);
        }

        public override Creature CreateRealizedCreature(AbstractCreature acrit)
        {
            return new ClimbGrub(acrit, acrit.world);
        }

        public override CreatureTemplate.Type ArenaFallback()
        {
            return CreatureTemplate.Type.TubeWorm;
        }
    }
}