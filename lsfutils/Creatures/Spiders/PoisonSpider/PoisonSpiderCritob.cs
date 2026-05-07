using Fisobs.Core;
using Fisobs.Creatures;
using Fisobs.Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace lsfUtils.Creatures.Spiders.PoisonSpider
{
    internal class PoisonSpiderCritob : Critob
    {
        internal PoisonSpiderCritob() : base(Enums.CreatureTemplateType.PoisonSpider)
        {
            Icon = new SimpleIcon("Kill_BigSpider", Enums.Colors.PoisonColor);
            LoadedPerformanceCost = 50f;
            SandboxPerformanceCost = new(.25f, .25f);
            RegisterUnlock(KillScore.Configurable(1), Enums.SandboxUnlockID.PoisonSpider, MultiplayerUnlocks.SandboxUnlockID.Slugcat);
        }

        public override void GraspParalyzesPlayer(Creature.Grasp grasp, ref bool paralyzing)
        {
            paralyzing = true;
        }

        public override int ExpeditionScore() => 8;
        public override Color DevtoolsMapColor(AbstractCreature acrit) => Enums.Colors.PoisonColor;
        public override string DevtoolsMapName(AbstractCreature acrit) => "PS";
        public override IEnumerable<string> WorldFileAliases() => new string[] { "poisonspider", "venomspider" };
        public override CreatureTemplate CreateTemplate()
        {
            CreatureTemplate creatureTemplate = new CreatureFormula(CreatureTemplate.Type.SpitterSpider, Type, "PoisonSpider")
            {
                TileResistances = new TileResist
                {
                    OffScreen = new PathCost(4f, PathCost.Legality.Allowed),
                    Floor = new PathCost(4f, PathCost.Legality.Allowed),
                    Corridor = new PathCost(4f, PathCost.Legality.Allowed),
                    Climb = new PathCost(6f, PathCost.Legality.Allowed),
                    Wall = new PathCost(12f, PathCost.Legality.Allowed),
                    Ceiling = new PathCost(12f, PathCost.Legality.Allowed)
                },
                ConnectionResistances = new ConnectionResist
                {
                    Standard = new PathCost(1f, PathCost.Legality.Allowed),
                    OpenDiagonal = new PathCost(4f, PathCost.Legality.Allowed),
                    ReachOverGap = new PathCost(4f, PathCost.Legality.Allowed),
                    ReachUp = new PathCost(3f, PathCost.Legality.Allowed),
                    ReachDown = new PathCost(3f, PathCost.Legality.Allowed),
                    SemiDiagonalReach = new PathCost(2f, PathCost.Legality.Allowed),
                    DropToFloor = new PathCost(10f, PathCost.Legality.Allowed),
                    DropToWater = new PathCost(10f, PathCost.Legality.Allowed),
                    DropToClimb = new PathCost(10f, PathCost.Legality.Allowed),
                    ShortCut = new PathCost(1.5f, PathCost.Legality.Allowed),
                    NPCTransportation = new PathCost(3f, PathCost.Legality.Allowed),
                    OffScreenMovement = new PathCost(1f, PathCost.Legality.Allowed),
                    BetweenRooms = new PathCost(5f, PathCost.Legality.Allowed),
                    Slope = new PathCost(1.5f, PathCost.Legality.Allowed),
                    CeilingSlope = new PathCost(1.5f, PathCost.Legality.Allowed)
                },
                DefaultRelationship = new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0f),
                HasAI = true,
                Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.SpitterSpider),
                DamageResistances = new AttackResist
                {
                    Base = 2.2f
                },
                StunResistances = new AttackResist
                {
                    Base = 1.2f
                }
            }.IntoTemplate();
            creatureTemplate.jumpAction = "ATTACK";
            return creatureTemplate;
        }
        public override void EstablishRelationships()
        {
            Relationships relationships = new Relationships(Type);
            List<string> entries = ExtEnum<CreatureTemplate.Type>.values.entries;
            for (int i = 0; i < entries.Count; i++)
            {
                relationships.Ignores(new CreatureTemplate.Type(entries[i], false));
            }
            relationships.Attacks(CreatureTemplate.Type.LizardTemplate, 0.5f);
            relationships.Fears(CreatureTemplate.Type.DaddyLongLegs, 0.5f);
            relationships.Fears(CreatureTemplate.Type.RedCentipede, 0.5f);
            relationships.Fears(CreatureTemplate.Type.RedLizard, 0.3f);
            relationships.Fears(CreatureTemplate.Type.Centiwing, 0.2f);
            relationships.Fears(CreatureTemplate.Type.BrotherLongLegs, 0.3f);
            relationships.Fears(CreatureTemplate.Type.BigEel, 0.6f);
            relationships.Eats(CreatureTemplate.Type.Centipede, 0.2f);
            relationships.Eats(CreatureTemplate.Type.LanternMouse, 0.1f);
            relationships.Eats(CreatureTemplate.Type.Slugcat, 0.2f);
            relationships.Eats(CreatureTemplate.Type.Scavenger, 0.6f);
            relationships.AttackedBy(CreatureTemplate.Type.Scavenger, 0.7f);
            relationships.Ignores(Type);
        }
        public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit)
        {
            return new BigSpiderAI(acrit, acrit.world);
        }
        public override Creature CreateRealizedCreature(AbstractCreature acrit)
        {
            return new PoisonSpider(acrit, acrit.world);
        }
        public override void LoadResources(RainWorld rainWorld)
        {
        }
        public override CreatureTemplate.Type ArenaFallback()
        {
            return CreatureTemplate.Type.SpitterSpider;
        }
    }
}
