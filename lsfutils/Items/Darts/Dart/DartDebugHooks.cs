using System;
using lsfUtils.Items.Darts.Dart;
using RWCustom;
using UnityEngine;
using static lsfUtils.Enums;
using static lsfUtils.Plugin;

public static class DartDebugHooks
{
    public static void ApplyHooks()
    {
        On.Player.Update += Player_Update;
    }

    private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);

        if (self.room == null || self.room.game == null || !self.room.game.devToolsActive) return;
        if (self.playerState.playerNumber != 0) return;
        if (!Input.GetKeyDown(KeyCode.D)) return;

        SpawnDartInPlayer(self, Input.GetKey(KeyCode.LeftShift));
    }

    public static void SpawnDartInPlayer(Player player, bool poison)
    {
        Room room = player.room;
        if (room == null) return;

        var abstractDart = new AbstractDart(room.world, null, player.abstractCreature.pos, room.game.GetNewID());
        if (poison)
        {
            abstractDart.dartType = DartType.Poison;
            abstractDart.poison = 1f;
        }

        room.abstractRoom.AddEntity(abstractDart);
        abstractDart.RealizeInRoom();

        Dart dart = abstractDart.realisedDart;
        if (dart == null)
        {
            Log.LogError("Debug dart failed to realize (realisedDart is null)");
            return;
        }

        dart.throwDir = new IntVector2(player.flipDirection != 0 ? player.flipDirection : 1, 0);
        dart.firstChunk.pos = player.mainBodyChunk.pos;
        dart.firstChunk.vel = Vector2.zero;

        var result = new SharedPhysics.CollisionResult(player, player.mainBodyChunk, null, true, player.mainBodyChunk.pos);
        dart.LodgeInCreature(result, false);

        Log.LogMessage($"Debug dart lodged in player. mode={dart.mode}, sticks={player.abstractCreature.stuckObjects.Count}");
    }
}