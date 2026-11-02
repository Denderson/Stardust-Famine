using System.Collections.Generic;
using static lsfUtils.Plugin;

namespace lsfUtils.RegionParams;

public static class ShelterParams
{
    public static readonly ParamTable<List<string>> Table = new();

    public static readonly string ConfigPath = "lsf/shelterLinks.txt";

    public static void ApplyHooks()
    {
        On.SaveState.BringUpToDate += SaveState_BringUpToDate;
    }

    public static void LoadParams()
    {
        Table.Clear();
        foreach ((ParamKey key, string value) in ParamFile.Read(ConfigPath))
        {
            List<string> candidates = ParseCandidates(value);
            if (candidates.Count > 0) Table.Add(key, candidates);
        }
    }

    public static List<string> ParseCandidates(string value)
    {
        List<string> candidates = [];
        foreach (string raw in value.Split(','))
        {
            string candidate = raw.Trim().ToLowerInvariant();
            if (candidate.Length > 0) candidates.Add(candidate);
        }
        return candidates;
    }

    public static List<string> GetCandidates(string shelter, RainWorldGame game)
    {
        List<string> candidates = [];
        foreach (List<string> next in Table.Get(shelter, game))
        {
            foreach (string candidate in next) candidates.Remove(candidate);
            candidates.InsertRange(0, next);
        }
        return candidates;
    }

    public static void SaveState_BringUpToDate(On.SaveState.orig_BringUpToDate orig, SaveState self, RainWorldGame game)
    {
        orig(self, game);

        string fromShelter = self.denPosition;
        if (fromShelter == null) return;

        List<string> candidates = GetCandidates(fromShelter, game);
        if (candidates.Count == 0) return;

        string toShelter = SelectTarget(candidates, self, game);
        if (toShelter == null || !ShouldRedirect(self, game, fromShelter, toShelter)) return;

        MoveDen(self, game, fromShelter, toShelter);
    }

    public static string SelectTarget(List<string> candidates, SaveState save, RainWorldGame game)
    {
        return candidates.Count > 0 ? candidates[0] : null;
    }

    public static bool ShouldRedirect(SaveState save, RainWorldGame game, string fromShelter, string toShelter)
    {
        // TODO
        if (toShelter == fromShelter) return false;
        return true;
    }

    public static void MoveDen(SaveState save, RainWorldGame game, string fromShelter, string toShelter)
    {
        save.denPosition = toShelter;
        save.TrySetVanillaDen(toShelter);

        AbstractRoom fromRoom = game?.world?.GetAbstractRoom(fromShelter);
        if (fromRoom == null) return;

        List<AbstractPhysicalObject> objects = GetObjects(fromRoom, game);

        foreach (AbstractPhysicalObject obj in objects)
        {
            fromRoom.RemoveEntity(obj);
            save.pendingObjects.Add(obj is AbstractCreature critter ? SaveState.AbstractCreatureToStringStoryWorld(critter) : obj.ToString());
        }
    }

    public static List<AbstractPhysicalObject> GetObjects(AbstractRoom fromRoom, RainWorldGame game)
    {
        List<AbstractPhysicalObject> result = [];
        CollectFromList(fromRoom.entities, game, result);
        CollectFromList(fromRoom.entitiesInDens, game, result);
        return result;
    }

    public static void CollectFromList(List<AbstractWorldEntity> source, RainWorldGame game, List<AbstractPhysicalObject> result)
    {
        for (int i = 0; i < source.Count; i++)
        {
            if (source[i] is not AbstractPhysicalObject obj) continue;
            if (obj is AbstractCreature crit && crit.creatureTemplate.type == CreatureTemplate.Type.Slugcat) continue;
            if (HeldByPlayer(obj, game)) continue;
            result.Add(obj);
        }
    }

    public static bool HeldByPlayer(AbstractPhysicalObject obj, RainWorldGame game)
    {
        for (int p = 0; p < game.session.Players.Count; p++)
        {
            if (game.session.Players[p].realizedCreature is not Player player) continue;

            for (int g = 0; g < player.grasps.Length; g++)
            {
                if (player.grasps[g]?.grabbed?.abstractPhysicalObject == obj)
                {
                    return true;
                }
            }
        }
        return false;
    }
}