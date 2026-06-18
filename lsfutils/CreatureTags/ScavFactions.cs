using lsfUtils.CWTs;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static lsfUtils.Plugin;

namespace lsfUtils.CreatureTags
{
    public static class ScavFactions
    {
        public static List<FactionData> factions = [];

        public static void SetupFaction(this AbstractCreature abstractCreature, string faction)
        {
            if (abstractCreature == null) return;
            if (!AbstractCreatureCWT.TryGetData(abstractCreature, out var data))
            {
                Log.LogMessage("Couldnt get AbstractCreatureCWT!");
                return;
            }
            data.faction = faction;
        }

        public static bool IsCorrectFaction(this AbstractCreature abstractCreature, string faction)
        {
            if (abstractCreature == null) return false;
            if (!AbstractCreatureCWT.TryGetData(abstractCreature, out var data)) return false;
            return data.faction == faction;
        }

        public static bool IsCorrectFaction(this Creature creature, string faction)
        {
            if (creature?.abstractCreature == null) return false;
            return IsCorrectFaction(creature.abstractCreature, faction);
        }

        public static string GetFaction(this AbstractCreature creature)
        {
            if (creature == null)
            {
                Log.LogMessage("Couldnt get creature for faction!");
                return "default";
            }
            if (!AbstractCreatureCWT.TryGetData(creature, out var data)) return "default";
            if (data.faction == null) return "default";
            return data.faction.ToLowerInvariant();
        }

        public static string GetFaction(this Creature creature)
        {
            if (creature?.abstractCreature == null)
            {
                Log.LogMessage("Couldnt get creature for faction!");
                return "default";
            }
            return GetFaction(creature.abstractCreature);
        }
        public static FactionData GetFactionData(string name)
        {
            name = name?.ToLowerInvariant() ?? "default";
            for (int i = 0; i < factions.Count; i++) if (factions[i].name == name) return factions[i];
            return null;
        }

        public static CreatureTemplate.Relationship GetFactionRelationship(string fromFaction, string toFaction)
        {
            fromFaction = fromFaction?.ToLowerInvariant() ?? "default";
            toFaction = toFaction?.ToLowerInvariant() ?? "default";

            if (fromFaction == toFaction) return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Pack, 1f);

            FactionData from = GetFactionData(fromFaction);
            if (from != null)
            {
                if (from.relationships != null && from.relationships.TryGetValue(toFaction, out var explicit_rel)) return explicit_rel;
                return from.defaultRelationship;
            }

            FactionData to = GetFactionData(toFaction);
            if (to != null)
            {
                if (to.relationships != null && to.relationships.TryGetValue(fromFaction, out var mirror_rel)) return mirror_rel;
                return to.defaultRelationship;
            }

            return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Pack, 1f);
        }
        public static void LoadAllFactions()
        {
            string path = AssetManager.ResolveFilePath("factions.txt");
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                Log.LogWarning("Couldnt find factions.txt!");
                return;
            }
            string[] lines = File.ReadAllLines(path);
            ParseFactions(lines);
            Log.LogMessage($"Loaded {factions.Count} faction(s) from {path}.");
        }
        public static void LoadAllFactionsFromAllMods()
        {
            foreach (ModManager.Mod mod in ModManager.ActiveMods)
            {
                string path = Path.Combine(mod.path, "factions.txt");
                if (!File.Exists(path)) continue;

                string[] lines = File.ReadAllLines(path);
                ParseFactions(lines);
                Log.LogMessage($"[ScavFactions] Loaded factions from mod '{mod.id}'.");
            }
        }

        public static void ParseFactions(string[] lines)
        {
            FactionData current = null;

            for (int lineNum = 0; lineNum < lines.Length; lineNum++)
            {
                string line = lines[lineNum].Trim();

                if (string.IsNullOrEmpty(line) || line.StartsWith("/") || line.StartsWith("#")) continue;

                if (line.StartsWith("(") && line.EndsWith(")"))
                {
                    if (current != null) factions.Add(current);

                    string name = line.Substring(1, line.Length - 2).Trim().ToLowerInvariant();
                    if (string.IsNullOrEmpty(name))
                    {
                        Log.LogWarning($"Empty faction name at line {lineNum + 1}.");
                        current = null;
                        continue;
                    }

                    current = new FactionData
                    {
                        name = name,
                        relationships = [],
                        defaultRelationship = new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Pack, 1f)
                    };
                    continue;
                }

                if (current == null)
                {
                    Log.LogWarning($"Line {lineNum + 1} is outside a faction block, skipping.");
                    continue;
                }

                int colonIdx = line.IndexOf(':');
                if (colonIdx < 0) continue;

                string key = line.Substring(0, colonIdx).Trim().ToLowerInvariant();
                string value = line.Substring(colonIdx + 1).Trim();

                if (!TryParseRelationship(value, out CreatureTemplate.Relationship rel, out string err))
                {
                    Log.LogWarning($"Line {lineNum + 1}: could not parse '{value}': {err}");
                    continue;
                }

                if (key == "default") current.defaultRelationship = rel;
                else current.relationships[key] = rel;
            }

            if (current != null) factions.Add(current);
        }

        public static bool TryParseRelationship(string s, out CreatureTemplate.Relationship rel, out string error)
        {
            rel = default;
            error = null;

            string[] parts = s.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
            {
                error = "Expected 'type intensity'";
                return false;
            }

            if (!TryParseRelType(parts[0], out CreatureTemplate.Relationship.Type type))
            {
                error = $"Unknown relationship type '{parts[0]}'.";
                return false;
            }

            if (!float.TryParse(parts[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float intensity))
            {
                error = $"Could not parse intensity '{parts[1]}' as float.";
                return false;
            }

            rel = new CreatureTemplate.Relationship(type, Mathf.Clamp01(intensity));
            return true;
        }

        public static bool TryParseRelType(string s, out CreatureTemplate.Relationship.Type type)
        {
            switch (s.ToLowerInvariant())
            {
                case "pack": type = CreatureTemplate.Relationship.Type.Pack; return true;
                case "attacks": type = CreatureTemplate.Relationship.Type.Attacks; return true;
                case "afraid": type = CreatureTemplate.Relationship.Type.Afraid; return true;
                case "ignores": type = CreatureTemplate.Relationship.Type.Ignores; return true;
                case "uncomfortable": type = CreatureTemplate.Relationship.Type.Uncomfortable; return true;
                case "socialdependent": type = CreatureTemplate.Relationship.Type.SocialDependent; return true;
                case "eats": type = CreatureTemplate.Relationship.Type.Eats; return true;
                case "stayoutofway": type = CreatureTemplate.Relationship.Type.StayOutOfWay; return true;
                default: type = default; return false;
            }
        }
    }

    public class FactionData
    {
        public string name;
        public Dictionary<string, CreatureTemplate.Relationship> relationships;
        public CreatureTemplate.Relationship defaultRelationship;
    }

    public static class ScavFactionHooks
    {

        public static void IL_UpdateDynamicRelationship(ILContext il)
        {
            var c = new ILCursor(il);

            int retCount = 0;
            while (c.TryGotoNext(MoveType.Before, i => i.MatchRet()))
            {
                retCount++;
                if (retCount > 2) break;

                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldarg_1);
                c.EmitDelegate<Func<CreatureTemplate.Relationship, ScavengerAI, RelationshipTracker.DynamicRelationship, CreatureTemplate.Relationship>>(ApplyFactionRelationship);

                c.Index++;
            }
        }

        public static CreatureTemplate.Relationship ApplyFactionRelationship(CreatureTemplate.Relationship vanillaResult, ScavengerAI self, RelationshipTracker.DynamicRelationship dRelation)
        {
            AbstractCreature selfAbstract = self.creature;
            AbstractCreature otherAbstract = dRelation.trackerRep.representedCreature;

            string selfFaction = selfAbstract.GetFaction();
            string otherFaction = otherAbstract.GetFaction();

            if (selfFaction == "default" || otherFaction == "default") return vanillaResult;

            return ScavFactions.GetFactionRelationship(selfFaction, otherFaction);
        }
        public static bool Squad_DoesScavengerWantToBeInSquad(On.ScavengerAbstractAI.ScavengerSquad.orig_DoesScavengerWantToBeInSquad orig, ScavengerAbstractAI.ScavengerSquad self, ScavengerAbstractAI testScav)
        {
            if (!orig(self, testScav)) return false;

            string leaderFaction = self.leader.GetFaction();
            string recruitFaction = testScav.parent.GetFaction();

            CreatureTemplate.Relationship rel = ScavFactions.GetFactionRelationship(leaderFaction, recruitFaction);

            return rel.type == CreatureTemplate.Relationship.Type.Pack;
        }

        public static void Squad_AddMember(On.ScavengerAbstractAI.ScavengerSquad.orig_AddMember orig, ScavengerAbstractAI.ScavengerSquad self, AbstractCreature newMember)
        {
            if (self.leader == newMember)
            {
                orig(self, newMember);
                return;
            }

            string leaderFaction = self.leader.GetFaction();
            string memberFaction = newMember.GetFaction();

            CreatureTemplate.Relationship rel = ScavFactions.GetFactionRelationship(leaderFaction, memberFaction);

            if (rel.type != CreatureTemplate.Relationship.Type.Pack) return;

            orig(self, newMember);
        }
    }
}