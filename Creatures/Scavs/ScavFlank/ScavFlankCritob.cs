using DevInterface;
using Fisobs.Core;
using Fisobs.Creatures;
using Fisobs.Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace lsfUtils.Creatures.Scavs.ScavFlank
{
    public class ScavFlankCritob : Critob
    {

        public ScavFlankCritob() : base(Enums.CreatureTemplateType.ScavFlank)
        {
            LoadedPerformanceCost = 300f;
            SandboxPerformanceCost = new(0.5f, 0.925f);
            ShelterDanger = 0;
            Icon = new SimpleIcon("Kill_Scavenger", RainWorld.SaturatedGold);
            RegisterUnlock(KillScore.Configurable(1), Enums.SandboxUnlockID.ScavFlank, MultiplayerUnlocks.SandboxUnlockID.Slugcat);
        }

        public override int ExpeditionScore() => 10;

        public override Color DevtoolsMapColor(AbstractCreature acrit) => RainWorld.SaturatedGold;

        public override string DevtoolsMapName(AbstractCreature acrit) => "Sfla";

        public override IEnumerable<string> WorldFileAliases() => ["scavflank", "scavengerflank"];
        public override CreatureTemplate CreateTemplate()
        {
            CreatureTemplate creatureTemplate = new CreatureFormula(CreatureTemplate.Type.Scavenger, Type, "ScavFlank")
            {
                DefaultRelationship = new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0f),
                HasAI = true,
                Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.Scavenger),
                DamageResistances = new()
                {
                    Base = 3f
                },
                StunResistances = new()
                {
                    Base = 1.5f
                }
            }.IntoTemplate();
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
            relationships.IsInPack(CreatureTemplate.Type.Scavenger, 0.7f);
            relationships.IsInPack(Type, 0.5f);
        }
        public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new ScavengerAI(acrit, acrit.world);
        public override AbstractCreatureAI CreateAbstractAI(AbstractCreature absCt)
        {
            return new ScavengerAbstractAI(absCt.world, absCt);
        }
        public override Creature CreateRealizedCreature(AbstractCreature acrit) => new ScavFlank(acrit, acrit.world);
        public override CreatureTemplate.Type ArenaFallback() => CreatureTemplate.Type.Scavenger;
    }
}
