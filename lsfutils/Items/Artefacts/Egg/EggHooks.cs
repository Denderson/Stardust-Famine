namespace lsfUtils.Items.Artefacts.Egg;

public static class EggHooks
{
    public static void ApplyHooks()
    {
        On.SaveState.SessionEnded += SaveState_SessionEnded;
    }

    private static void SaveState_SessionEnded(On.SaveState.orig_SessionEnded orig, SaveState self, RainWorldGame game, bool survived, bool newMalnourished)
    {
        if (!survived || game?.world == null || game.Players == null || game.Players.Count > 0)
        {
            orig(self, game, survived, newMalnourished);
            return;
        }

        foreach (var player in game.Players)
        {
            if (player?.Room == null) continue;
            foreach (var entity in player.Room.entities)
            {
                if (entity is AbstractPhysicalObject egg && egg.type == Enums.AbstractObjectType.Egg)
                {
                    egg.realizedObject?.Destroy();
                    egg.Destroy();
                }
            }
        }
        orig(self, game, survived, newMalnourished);
    }
}