using lsfUtils.CWTs;
using lsfUtils.Items.Darts.Dart;
using MonoMod.Cil;
using RWCustom;
using System;
using System.Drawing;
using Unity.Mathematics;
using UnityEngine;
using static lsfUtils.Enums;
using static lsfUtils.Plugin;


public static class DartHooks
{
    public static void ApplyHooks()
    {
        On.Player.GrabUpdate += Player_GrabUpdate;
        On.Player.Grabability += Player_Grabability;
        //IL.Room.Loaded += Room_Loaded;
        On.Creature.Update += Creature_Update;
        On.PlayerGraphics.Update += PlayerGraphics_Update;
    }

    private static Player.ObjectGrabability Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
    {
        var value = orig(self, obj);
        if (PlayerCWT.TryGetData(self, out var data) && data.pullingOutThisDart != null)
        {
            return Player.ObjectGrabability.CantGrab;
        }
        return value;
    }

    public const int pullOutDuration = 80;
    public const float pullOutChance = 0.25f;
    public const float pullOutChanceIncrease = 0.20f;
    public const int pullOutFailStun = 40;
    public const int pullOutSuccessStun = 20;
    public const float pullReach = 60f;

    //Makes it so Darts can replace Spears when a Room is being Loaded
    /*public static void Room_Loaded(ILContext il)
    {
        try
        {
            ILCursor main = new ILCursor(il);
            main.GotoNext(
                MoveType.After,
                i => i.MatchCall<SlugcatStats>(nameof(SlugcatStats.SpearSpawnModifier)),
                i => i.MatchBgeUn(out _)
            );
            ILCursor dartCursor = new ILCursor(main);
            dartCursor.GotoNext(
                MoveType.After,
                i => i.MatchStfld<AbstractSpear>(nameof(AbstractSpear.electric))
            );
            dartCursor.MoveAfterLabels();
            ILLabel dartCodeLoc = dartCursor.MarkLabel();
            dartCursor.EmitDelegate(MakeDart);
            main.Emit(Mono.Cecil.Cil.OpCodes.Ldarg_0);
            main.EmitDelegate(WillDartSpawn);
            main.Emit(Mono.Cecil.Cil.OpCodes.Brtrue, dartCodeLoc);
            Log.LogMessage(il);
        }
        catch (Exception e)
        {
            Log.LogMessage(e);
        }
    }*/

    public static bool WillDartSpawn(Room self)
    {
        int x = (int)(UnityEngine.Random.value * 100);
        if (!RegionCWT.TryGetCustomRegionParams(self?.world?.region, out var customRegionParams)) return false;
        return x < customRegionParams.DartFrequency;
    }

    public static void MakeDart(AbstractPhysicalObject obj, Room self, IntVector2 intVector, EntityID ID)
    {
        if (obj is AbstractSpear) return;
        obj = new AbstractDart(self.world, null, new WorldCoordinate(self.abstractRoom.index, intVector.x, intVector.y, -1), ID);
        if (UnityEngine.Random.value < 0.1f)
        {
            (obj as AbstractDart).dartType = DartType.Poison;
        }
    }


    public static void Creature_Update(On.Creature.orig_Update orig, Creature self, bool eu)
    {
        orig(self, eu);
        if (self?.abstractCreature?.stuckObjects == null || self.abstractCreature.stuckObjects.Count == 0) return;
    }

    public static void Player_GrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
    {
        if (self != null && self.Consious && self.input != null && self.input.Length > 0 && !self.input[0].jmp && self.input[0].pckp)
        {
            HandleDartPull(self);
        }
        else
        {
            ResetSelfPull(self);
        }
        orig(self, eu);
    }

    public static void HandleDartPull(Player player)
    {
        if (!FindDartToPull(player, out Dart dart)) return;

        int hand = player.FreeHand();
        if (hand < 0)
        {
            ResetSelfPull(player);
            return;
        }

        Creature from = dart.stuckInObject as Creature;

        player.slowMovementStun = math.max(player.slowMovementStun, 40);
        player.eyesClosedTime = math.max(player.eyesClosedTime, 40);
        if (player.graphicsModule is PlayerGraphics graphics)
        {
            graphics.hands[hand].reachingForObject = true;
            graphics.hands[hand].absoluteHuntPos = dart.firstChunk.pos;
            graphics.LookAtObject(dart);
        }

        dart.pullOutTimer++;
        int shake = Mathf.RoundToInt(Mathf.Lerp(2f, 15f, (float)dart.pullOutTimer / pullOutDuration));
        dart.vibrate = Mathf.Max(dart.vibrate, shake);

        if (dart.pullOutTimer >= pullOutDuration)
        {
            AttemptPullOut(player, dart, hand, from);
        }
    }

    public static bool HasStuckDart(Player player, Creature creature, out Dart result)
    {
        result = null;
        var sticks = creature?.abstractCreature?.stuckObjects;
        if (sticks == null || sticks.Count == 0) return false;

        foreach (var stick in sticks)
        {
            if (stick is AbstractPhysicalObject.AbstractSpearStick spearStick && spearStick.Spear is AbstractDart abstractDart && abstractDart.realisedDart is Dart dart && (player == creature || Custom.DistLess(player.firstChunk.pos, dart.firstChunk.pos, pullReach)))
            {
                result = dart;
                return true;
            }
        }
        return false;
    }
    
    public static bool FindDartToPull(Player player, out Dart dart)
    {
        dart = null;
        if (!PlayerCWT.TryGetData(player, out var data)) return false;

        if (data.pullingOutThisDart != null)
        {
            dart = data.pullingOutThisDart;
            return true;
        }

        if (HasStuckDart(player, player, out dart)) return true;

        if (dart == null)
        {
            foreach (var grasp in player.grasps)
            {
                if (grasp?.grabbed is Creature held && held != player)
                {
                    if (HasStuckDart(player, held, out dart)) break;
                }
            }
        }
        
        if (dart == null && player.room?.abstractRoom?.creatures != null)
        {
            foreach (var abstractCreature in player.room.abstractRoom.creatures)
            {
                if (abstractCreature?.realizedCreature == null || abstractCreature.realizedCreature == player) continue;
                if (HasStuckDart(player, abstractCreature.realizedCreature, out dart)) break;
            }
        }

        if (dart == null) return false;
        data.pullingOutThisDart = dart;
        return true;
    }

    public static void AttemptPullOut(Player player, Dart dart, int hand, Creature creature)
    {
        float chance = pullOutChance + (dart.pullOutAttempts * pullOutChanceIncrease);

        if (UnityEngine.Random.value < chance) PullOutDart(player, dart, hand, creature);
        else FailPullOut(player, dart, creature);
    }

    public static void FailPullOut(Player player, Dart dart, Creature creature)
    {
        int attempts = dart.pullOutAttempts + 1;

        ResetSelfPull(player);
        dart.pullOutAttempts = attempts;
        dart.pullOutTimer = 0;
        dart.vibrate = 20;

        player.Stun(pullOutFailStun);
    }

    public static void PullOutDart(Player player, Dart dart, int hand, Creature creature)
    {
        if (player == null || dart == null) return;

        ResetSelfPull(player);

        dart.ChangeMode(Weapon.Mode.Free);
        dart.pullOutTimer = 0;
        dart.pullOutAttempts = 0;

        if (hand < 0) hand = player.FreeHand();
        if (hand > -1) player.SlugcatGrab(dart, hand);

        if (creature == player) player.Stun(20);
    }

    private static void PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
    {
        orig(self);

        Player player = self.player;
        if (!PlayerCWT.TryGetData(player, out var data)) return;

        Dart dart = data.pullingOutThisDart;
        if (dart == null || dart.pullOutTimer <= 0) return;

        int handIndex = player.FreeHand();
        if (handIndex < 0) return;

        float progress = Mathf.Clamp01((float)dart.pullOutTimer / pullOutDuration);
        Vector2 dartPos = dart.firstChunk.pos;
        Vector2 anchor = dart.stuckInObject != null ? dart.stuckInObject.firstChunk.pos : player.firstChunk.pos;

        Vector2 dist = Custom.DirVec(anchor, dartPos);
        if (dist.sqrMagnitude < 0.01f) dist = Vector2.up;

        float strain = Mathf.Pow(Mathf.Sin(dart.pullOutTimer * 0.3f) * 0.5f + 0.5f, 2f);
        Vector2 target = dartPos + dist * Mathf.Lerp(2f, 12f, progress) * strain + Custom.RNV() * Mathf.Lerp(0f, 1.5f, progress);

        SlugcatHand hand = self.hands[handIndex];
        hand.mode = Limb.Mode.HuntAbsolutePosition;
        hand.absoluteHuntPos = target;
        hand.huntSpeed = 50f;
        hand.quickness = 1f;
        hand.reachingForObject = true;
    }

    public static void ResetSelfPull(Player player)
    {
        if (!PlayerCWT.TryGetData(player, out var data)) return;

        if (data?.pullingOutThisDart == null) return;
        data.pullingOutThisDart.pullOutTimer = 0;
        data.pullingOutThisDart = null;
    }
}