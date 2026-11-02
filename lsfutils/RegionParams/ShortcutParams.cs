using lsfUtils.CWTs;
using System.Collections.Generic;
using static lsfUtils.Mechanics.ConditionalLogic;
using static lsfUtils.Plugin;

namespace lsfUtils.RegionParams;

public static class ShortcutParams
{
    public struct ShortcutLink(string room, int node)
    {
        public string room = room;
        public int node = node;
    }

    public static ParamTable<ShortcutLink> Table = new();

    public static string ConfigPath = "lsf/shortcutLinks.txt";

    public static List<ShortcutLink> overrides = [];
    public static List<AbstractRoom> redirected = [];

    public static void ApplyHooks()
    {
        On.Room.Update += Room_Update;
    }

    public static void LoadParams()
    {
        Table.Clear();

        foreach ((ParamKey key, string value) in ParamFile.Read(ConfigPath))
        {
            foreach (string raw in value.Split(','))
            {
                if (TryParseLink(raw, out ShortcutLink link)) Table.Add(key, link);
                else Log.LogWarning($"Parsing shortcut connection failed! {raw}");
            }
        }
    }

    public static bool TryParseLink(string raw, out ShortcutLink link)
    {
        link = default;

        string[] parts = raw.Split(':');
        if (parts.Length < 2 || parts.Length > 3) return false;

        string room = parts[0].Trim().ToUpperInvariant();
        if (room.Length == 0) return false;

        if (!int.TryParse(parts[1], out int node) || node < 0) return false;

        link = new ShortcutLink(room, node);
        return true;
    }

    public static void Room_Update(On.Room.orig_Update orig, Room self)
    {
        orig(self);

        RainWorldGame game = self?.game;
        if (game == null) return;

        if (redirected.Count > 0 && !AnyPlayerInPipe(game)) RestoreConnections();

        if (game.IsStorySession && game.AllPlayersRealized && self.shortCutsReady)
        {
            bool anyPlayerEnteredPipe = false;
            foreach (AbstractCreature player in game.Players)
            {
                if (player?.realizedCreature is not Player realisedPlayer || realisedPlayer.enteringShortCut == null) continue;
                if (realisedPlayer.room != self) continue;
                anyPlayerEnteredPipe = true;
                break;
            }

            if (!anyPlayerEnteredPipe) return;

            AbstractRoom abstractRoom = self.abstractRoom;

            if (redirected.Contains(abstractRoom)) return;

            for (int i = 0; i < abstractRoom.connections.Length; i++)
            {
                overrides.Clear();
                foreach (ShortcutLink link in Table.Get(abstractRoom.name + ":" + i, game))
                {
                    overrides.Add(link);
                }
                if (overrides.Count == 0) continue;

                ShortcutLink activeOverride = overrides[UnityEngine.Random.Range(0, overrides.Count)];
                Redirect(abstractRoom, i, activeOverride);
            }
        }
    }

    public static bool AnyPlayerInPipe(RainWorldGame game)
    {
        if (game?.shortcuts == null) return false;
        foreach (AbstractCreature player in game.Players)
        {
            if (player?.realizedCreature is Player realisedPlayer && realisedPlayer.enteringShortCut != null) return true;
        }

        for (int i = 0; i < game.shortcuts.transportVessels.Count; i++)
        {
            if (game.shortcuts.transportVessels[i].creature is Player) return true;
        }
        return false;
    }

    public static void Redirect(AbstractRoom from, int fromNode, ShortcutLink link)
    {
        AbstractRoom dest = from.world.GetAbstractRoom(link.room);
        if (dest == null)
        {
            Log.LogWarning($"Unknown destination room! {link.room} (from {from.name}:{fromNode})");
            return;
        }

        if (link.node >= dest.connections.Length)
        {
            Log.LogWarning($"Invalid destination node {link.node} for room {link.room}!");
            return;
        }

        if (!SaveConnections(from) || !SaveConnections(dest)) return;

        Log.LogMessage($"Redirect: {from.name}:{fromNode} -> {link.room}:{link.node}");

        from.connections[fromNode] = dest.index;
        dest.connections[link.node] = from.index;
    }

    public static bool SaveConnections(AbstractRoom room)
    {
        if (!AbstractRoomCWT.TryGetData(room, out var data)) return false;

        data.originalConnections ??= (int[])room.connections.Clone();
        if (!redirected.Contains(room)) redirected.Add(room);
        return true;
    }

    public static void RestoreConnections()
    {
        for (int i = 0; i < redirected.Count; i++)
        {
            AbstractRoom room = redirected[i];
            if (!AbstractRoomCWT.TryGetData(room, out var data) || data.originalConnections == null) continue;

            for (int node = 0; node < data.originalConnections.Length; node++)
            {
                room.connections[node] = data.originalConnections[node];
            }
        }
        redirected.Clear();
    }
}