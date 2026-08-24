using lsfUtils.CWTs;
using lsfUtils.Items.Darts.Dart;
using MonoMod.Cil;
using RWCustom;
using Unity.Mathematics;
using UnityEngine;
using static lsfUtils.Enums;

public static class DartHooks
{
    public static void ApplyHooks()
    {
        On.Player.GrabUpdate += DartHooks.Player_GrabUpdate;
        //IL.Room.Loaded += DartHooks.Room_Loaded;
        On.Creature.Update += DartHooks.Creature_Update;
    }

    public const int pullOutDuration = 80;
    public const float pullOutChance = 0.01f;

    public static void Player_GrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
    {
        orig(self, eu);
        if (self == null || !self.Consious)
        {
            ResetSelfPull(self);
            return;
        }
        if (self.input[0].y != 0 || !self.input[0].pckp)
        {
            ResetSelfPull(self);
            return;
        }
        HandleSelfDartPull(self);
    }

    //Makes it so Darts can replace Spears when a Room is being Loaded
    /*public static void Room_Loaded(ILContext il)
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

        HandleCreatureDartPullout(self);
    }
    public static void HandleCreatureDartPullout(Creature creature)
    {
        for (int i = creature.abstractCreature.stuckObjects.Count - 1; i >= 0; i--)
        {
            if (creature.abstractCreature.stuckObjects[i] is not AbstractPhysicalObject.AbstractSpearStick stick) continue;

            if (stick.Spear.realizedObject is not Dart dart || dart.mode != Weapon.Mode.StuckInCreature) continue;

            dart.pullOutTimer++;

            int shake = Mathf.RoundToInt(Mathf.Lerp(1f, 8f, (float)dart.pullOutTimer / pullOutDuration));
            dart.vibrate = Mathf.Max(dart.vibrate, shake);

            if (UnityEngine.Random.value < pullOutChance && dart.pullOutTimer >= 40)
            {
                PullOutDartFromCreature(creature, dart, stick);
                break;
            }

            if (dart.pullOutTimer >= pullOutDuration * 2)
            {
                PullOutDartFromCreature(creature, dart, stick);
                break;
            }
        }
    }

    public static void HandleSelfDartPull(Player player)
    {
        if (!HasEmbeddedDart(player, out Dart dart)) return;

        int hand = player.FreeHand();
        if (hand < 0)
        {
            ResetSelfPull(player);
            return;
        }

        if (player.graphicsModule is PlayerGraphics graphics)
        {
            graphics.hands[hand].reachingForObject = true;
            graphics.hands[hand].absoluteHuntPos = dart.firstChunk.pos;
        }

        player.slowMovementStun = math.max(player.slowMovementStun, 40);
        player.eyesClosedTime = math.max(player.eyesClosedTime, 40);

        dart.pullOutTimer++;
        int shake = Mathf.RoundToInt(Mathf.Lerp(2f, 15f, (float)dart.pullOutTimer / pullOutDuration));
        dart.vibrate = Mathf.Max(dart.vibrate, shake);

        if (dart.pullOutTimer >= pullOutDuration)
        {
            PullOutDartFromSelf(player, dart, hand);
        }
    }

    public static bool HasEmbeddedDart(Player player, out Dart dart)
    {
        dart = null;
        if (player?.abstractCreature?.stuckObjects == null || player.abstractCreature.stuckObjects.Count < 1) return false;

        if (!PlayerCWT.TryGetData(player, out var data)) return false;

        if (data.pullingOutThisDart != null)
        {
            dart = data.pullingOutThisDart;
            return true;
        }

        foreach (AbstractPhysicalObject.AbstractObjectStick stick in player.abstractCreature.stuckObjects)
        {
            if (stick is AbstractPhysicalObject.AbstractSpearStick spearStick && spearStick.Spear is AbstractDart abstractDart && abstractDart?.realisedDart?.mode == Weapon.Mode.StuckInCreature)
            {
                data.pullingOutThisDart = abstractDart.realisedDart;
                dart = data.pullingOutThisDart;
                return true;
            }
        }

        return false;
    }

    public static void PullOutDartFromSelf(Player player, Dart dart, int hand)
    {
        if (player == null || dart == null) return;

        dart.ChangeMode(Weapon.Mode.Free);
        dart.pullOutTimer = 0;

        if (hand < 0) hand = player.FreeHand();
        if (hand > -1) player.SlugcatGrab(dart, hand);

        player.Stun(10);
        ResetSelfPull(player);
    }

    public static void PullOutDartFromCreature(Creature creature, Dart dart, AbstractPhysicalObject.AbstractSpearStick stick)
    {
        if (creature == null || dart == null) return;

        dart.ChangeMode(Weapon.Mode.Free);
        dart.pullOutTimer = 0;
        dart.pullOutAttempts++;

        dart.firstChunk.vel = Custom.DegToVec(UnityEngine.Random.value * 360f) * Mathf.Lerp(3f, 8f, UnityEngine.Random.value);

        creature.Violence(dart.firstChunk, -dart.firstChunk.vel, null, null, Creature.DamageType.Stab, 0.05f, 5f);

        if (creature.room?.BeingViewed == true)
        {
            creature.room.PlaySound(SoundID.Spear_Dislodged_From_Creature, dart.firstChunk);
            for (int i = 0; i < 4; i++)
            {
                creature.room.AddObject(new WaterDrip(dart.firstChunk.pos, Custom.RNV() * Mathf.Lerp(2f, 5f, UnityEngine.Random.value), waterColor: false));
            }
        }
    }

    public static void ResetSelfPull(Player player)
    {
        if (player == null) return;

        if (!PlayerCWT.TryGetData(player, out var data)) return;

        if (data?.pullingOutThisDart == null) return;

        data.pullingOutThisDart.pullOutTimer = 0;
        data.pullingOutThisDart = null;
    }
}