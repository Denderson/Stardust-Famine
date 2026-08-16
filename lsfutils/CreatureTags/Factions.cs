using lsfUtils.CWTs;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static lsfUtils.Plugin;

namespace lsfUtils.CreatureTags
{
    public static class Factions
    {
        public static List<FactionData> factions = [];
        public static void LoadAndRegisterAllFactions()
        {
            foreach (ModManager.Mod mod in ModManager.ActiveMods)
            {
                string path = Path.Combine(mod.path, "factions.txt");
                if (!File.Exists(path)) continue;

                string[] lines = File.ReadAllLines(path);
                ParseFactions(lines);
                Log.LogMessage($"[Factions] Loaded factions from mod '{mod.id}'.");
            }

            string legacyPath = AssetManager.ResolveFilePath("factions.txt");
            if (!string.IsNullOrEmpty(legacyPath) && File.Exists(legacyPath))
            {
                string[] lines = File.ReadAllLines(legacyPath);
                ParseFactions(lines);
                Log.LogMessage($"[Factions] Loaded factions from legacy path {legacyPath}.");
            }

            Log.LogMessage($"[Factions] Total factions registered: {factions.Count}.");
        }

        public static void SetupFaction(this AbstractCreature abstractCreature, string faction)
        {
            if (abstractCreature == null) return;
            if (!AbstractCreatureCWT.TryGetData(abstractCreature, out var data))
            {
                Log.LogMessage("Couldn't get AbstractCreatureCWT!");
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
                Log.LogMessage("Couldn't get creature for faction!");
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
                Log.LogMessage("Couldn't get creature for faction!");
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
        public static CreatureTemplate.Relationship GetRelationship(string fromFaction, string toFaction)
        {
            fromFaction = fromFaction?.ToLowerInvariant() ?? "default";
            toFaction = toFaction?.ToLowerInvariant() ?? "default";

            if (fromFaction == toFaction)
                return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Pack, 1f);

            FactionData from = GetFactionData(fromFaction);
            if (from != null)
            {
                if (from.relationships != null && from.relationships.TryGetValue(toFaction, out var rel))
                    return rel;
                return from.defaultRelationship;
            }

            return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Pack, 1f);
        }

        public static CreatureTemplate.Relationship GetRelationship(AbstractCreature from, AbstractCreature to)
            => GetRelationship(from.GetFaction(), to.GetFaction());

        public static CreatureTemplate.Relationship? GetPlayerRelationship(string factionName, float like)
        {
            FactionData data = GetFactionData(factionName);
            if (data == null) return null;

            if (data.highRepRelationship.HasValue && like >= data.highRepThreshold)
                return data.highRepRelationship.Value;
            if (data.lowRepRelationship.HasValue && like <= data.lowRepThreshold)
                return data.lowRepRelationship.Value;

            return null;
        }

        public static CreatureCommunities.CommunityID GetCommunityID(string factionName)
        {
            factionName = factionName?.ToLowerInvariant() ?? "default";
            var id = new CreatureCommunities.CommunityID(factionName);
            return id.Index <= 0 ? null : id;
        }

        public static float LikeOfPlayer(RainWorldGame game, string factionName, int playerNumber)
        {
            var id = GetCommunityID(factionName);
            if (id == null || game?.session?.creatureCommunities == null) return 0f;
            int region = game.world?.RegionNumber ?? -1;
            return game.session.creatureCommunities.LikeOfPlayer(id, region, playerNumber);
        }

        public static void InfluenceLike(RainWorldGame game, string factionName, float influence,
            float regionBleed, float communityBleed, int playerNumber)
        {
            var id = GetCommunityID(factionName);
            if (id == null || game?.session?.creatureCommunities == null) return;
            int region = game.world?.RegionNumber ?? -1;
            game.session.creatureCommunities.InfluenceLikeOfPlayer(id, region, playerNumber,
                influence, regionBleed, communityBleed);
        }

        public static void SetLike(RainWorldGame game, string factionName, float value, int playerNumber)
        {
            var id = GetCommunityID(factionName);
            if (id == null || game?.session?.creatureCommunities == null) return;
            int region = game.world?.RegionNumber ?? -1;
            game.session.creatureCommunities.SetLikeOfPlayer(id, region, playerNumber, value);
        }

        public static void ApplyReputationLeans(CreatureCommunities communities)
        {
            for (int i = 0; i < factions.Count; i++)
            {
                FactionData data = factions[i];
                if (!data.reputationLean.HasValue) continue;

                var id = GetCommunityID(data.name);
                if (id == null || id.Index - 1 >= communities.playerOpinions.GetLength(0)) continue;

                int comm = id.Index - 1;
                float target = data.reputationLean.Value;
                float strength = data.reputationLeanStrength;

                for (int reg = 0; reg < communities.playerOpinions.GetLength(1); reg++)
                {
                    for (int plr = 0; plr < communities.playerOpinions.GetLength(2); plr++)
                    {
                        float leaned = Mathf.Lerp(communities.playerOpinions[comm, reg, plr], target, strength);
                        communities.playerOpinions[comm, reg, plr] = leaned;
                        communities.loadedPlayerOpinions[comm, reg, plr] = leaned;
                    }
                }
            }
        }

        public static void ApplyCycleTickLeans(CreatureCommunities communities)
        {
            for (int i = 0; i < factions.Count; i++)
            {
                FactionData data = factions[i];
                if (!data.reputationLean.HasValue) continue;

                var id = GetCommunityID(data.name);
                if (id == null || id.Index - 1 >= communities.playerOpinions.GetLength(0)) continue;

                int comm = id.Index - 1;
                float target = data.reputationLean.Value;

                for (int reg = 0; reg < communities.playerOpinions.GetLength(1); reg++)
                {
                    for (int plr = 0; plr < communities.playerOpinions.GetLength(2); plr++)
                    {
                        float current = communities.playerOpinions[comm, reg, plr];
                        if (current == target) continue;

                        float nudge = Custom.LerpMap(Mathf.Abs(current - target), 0f, 1f, 0.01f, 0.1f);
                        communities.playerOpinions[comm, reg, plr] = Mathf.MoveTowards(current, target, nudge);
                    }
                }
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
                    if (current != null) FinaliseFaction(current);

                    string name = line.Substring(1, line.Length - 2).Trim().ToLowerInvariant();
                    if (string.IsNullOrEmpty(name))
                    {
                        Log.LogWarning($"[Factions] Empty faction name at line {lineNum + 1}.");
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
                    Log.LogWarning($"[Factions] Line {lineNum + 1} is outside a faction block, skipping.");
                    continue;
                }

                int colonIdx = line.IndexOf(':');
                if (colonIdx < 0) continue;

                string key = line.Substring(0, colonIdx).Trim().ToLowerInvariant();
                string value = line.Substring(colonIdx + 1).Trim();

                if (key == "reputationlean")
                {
                    if (!TryParseReputationLean(value, out float target, out float strength, out string leanErr))
                    {
                        Log.LogWarning($"[Factions] Line {lineNum + 1}: could not parse reputationlean '{value}': {leanErr}");
                        continue;
                    }
                    current.reputationLean = target;
                    current.reputationLeanStrength = strength;
                    continue;
                }

                if (key == "lowrep" || key == "highrep")
                {
                    if (!TryParseThresholdRelationship(value, out float threshold, out var threshRel, out string threshErr))
                    {
                        Log.LogWarning($"[Factions] Line {lineNum + 1}: could not parse '{key}' '{value}': {threshErr}");
                        continue;
                    }
                    if (key == "lowrep") { current.lowRepThreshold = threshold; current.lowRepRelationship = threshRel; }
                    else { current.highRepThreshold = threshold; current.highRepRelationship = threshRel; }
                    continue;
                }

                if (!TryParseRelationship(value, out var rel, out string err))
                {
                    Log.LogWarning($"[Factions] Line {lineNum + 1}: could not parse '{value}': {err}");
                    continue;
                }

                if (key == "default") current.defaultRelationship = rel;
                else current.relationships[key] = rel;
            }

            if (current != null) FinaliseFaction(current);
        }

        public static void FinaliseFaction(FactionData data)
        {
            if (GetFactionData(data.name) != null)
            {
                Log.LogWarning($"Faction '{data.name}' already registered, skipping.");
                return;
            }

            factions.Add(data);

            _ = new CreatureCommunities.CommunityID(data.name, register: true);

            Log.LogMessage($"Registered faction '{data.name}' as CommunityID.");
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

            if (parts.Length > 2)
                Log.LogWarning($"[Factions] Extra tokens after intensity in '{s}' — they will be ignored.");

            if (!TryParseRelType(parts[0], out var type))
            {
                error = $"Unknown relationship type '{parts[0]}'.";
                return false;
            }

            if (!float.TryParse(parts[1],
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out float intensity))
            {
                error = $"Could not parse intensity '{parts[1]}' as float.";
                return false;
            }

            rel = new CreatureTemplate.Relationship(type, Mathf.Clamp01(intensity));
            return true;
        }

        public static bool TryParseThresholdRelationship(string s, out float threshold, out CreatureTemplate.Relationship rel, out string error)
        {
            threshold = 0f;
            rel = default;
            error = null;

            string[] parts = s.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 3)
            {
                error = "Expected 'threshold type intensity'";
                return false;
            }

            if (!float.TryParse(parts[0],
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out threshold))
            {
                error = $"Could not parse threshold '{parts[0]}' as float.";
                return false;
            }

            if (!TryParseRelType(parts[1], out var type))
            {
                error = $"Unknown relationship type '{parts[1]}'.";
                return false;
            }

            if (!float.TryParse(parts[2],
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out float intensity))
            {
                error = $"Could not parse intensity '{parts[2]}' as float.";
                return false;
            }

            rel = new CreatureTemplate.Relationship(type, Mathf.Clamp01(intensity));
            return true;
        }

        public static bool TryParseReputationLean(string s, out float target, out float strength, out string error)
        {
            target = 0f;
            strength = 0f;
            error = null;

            string[] parts = s.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
            {
                error = "Expected 'target strength'";
                return false;
            }

            if (!float.TryParse(parts[0],
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out target))
            {
                error = $"Could not parse lean target '{parts[0]}' as float.";
                return false;
            }

            if (!float.TryParse(parts[1],
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out strength))
            {
                error = $"Could not parse lean strength '{parts[1]}' as float.";
                return false;
            }

            target = Mathf.Clamp(target, -1f, 1f);
            strength = Mathf.Clamp01(strength);
            return true;
        }

        public static bool TryParseRelType(string s,
            out CreatureTemplate.Relationship.Type type)
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

        public float? reputationLean = null;
        public float reputationLeanStrength = 0f;

        public float lowRepThreshold = 0f;
        public CreatureTemplate.Relationship? lowRepRelationship = null;

        public float highRepThreshold = 1f;
        public CreatureTemplate.Relationship? highRepRelationship = null;
    }

    public static class FactionHooks
    {
        public static void ApplyHooks()
        {
            IL.ScavengerAI.IUseARelationshipTracker_UpdateDynamicRelationship += FactionHooks.IL_UpdateDynamicRelationship;
            On.ScavengerAbstractAI.ScavengerSquad.DoesScavengerWantToBeInSquad += FactionHooks.Squad_DoesScavengerWantToBeInSquad;
            On.ScavengerAbstractAI.ScavengerSquad.AddMember += FactionHooks.Squad_AddMember;
            On.CreatureCommunities.LoadDefaultCommunityAlignments += FactionHooks.LoadDefaultCommunityAlignments_Post;
            On.CreatureCommunities.CycleTick += FactionHooks.CycleTick_Post;
        }

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

            return Factions.GetRelationship(selfFaction, otherFaction);
        }

        public static bool Squad_DoesScavengerWantToBeInSquad(On.ScavengerAbstractAI.ScavengerSquad.orig_DoesScavengerWantToBeInSquad orig, ScavengerAbstractAI.ScavengerSquad self, ScavengerAbstractAI testScav)
        {
            if (!orig(self, testScav)) return false;

            string leaderFaction = self.leader.GetFaction();
            string recruitFaction = testScav.parent.GetFaction();

            CreatureTemplate.Relationship rel = Factions.GetRelationship(leaderFaction, recruitFaction);

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

            CreatureTemplate.Relationship rel = Factions.GetRelationship(leaderFaction, memberFaction);

            if (rel.type != CreatureTemplate.Relationship.Type.Pack) return;

            orig(self, newMember);
        }

        public static void LoadDefaultCommunityAlignments_Post(On.CreatureCommunities.orig_LoadDefaultCommunityAlignments orig, CreatureCommunities self, SlugcatStats.Name saveStateNumber)
        {
            orig(self, saveStateNumber);
            Factions.ApplyReputationLeans(self);
        }

        public static void CycleTick_Post(On.CreatureCommunities.orig_CycleTick orig, CreatureCommunities self, int cycle, SlugcatStats.Name saveStateNumber)
        {
            orig(self, cycle, saveStateNumber);
            Factions.ApplyCycleTickLeans(self);
        }
    }
}