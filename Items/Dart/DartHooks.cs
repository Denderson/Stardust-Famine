using JetBrains.Annotations;
using lsfUtils.CWTs;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using static lsfUtils.Plugin;

namespace lsfUtils.Items.Dart
{
    public static class DartHooks
    {
        public static void Player_GrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
        {
            orig(self, eu);
            if (self == null || !self.Consious || self.FreeHand() < 0)
            {
                return;
            }
            if (self.input[0].y != 0 || self.input[0].pckp)
            {
                ResetPull(self);
                return;
            }
            HandleSelfDartPull(self);
        }

        public static void HandleSelfDartPull(Player player)
        {
            if (!HasEmbeddedDart(player, out Dart dart)) return;

            player.slowMovementStun = math.max(player.slowMovementStun, 40);
            player.eyesClosedTime = math.max(player.eyesClosedTime, 40);

            dart.pullOutTimer++;
            // do animation part here

            if (dart.PullOutLuckCheck())
            {
                PullOutDartFromSelf(player, dart);
            }
            else
            {
                ResetPull(player);
                player.Stun(40);
                player.lungsExhausted = true;
            }
        }

        public static bool HasEmbeddedDart(Player player, out Dart dart)
        {
            dart = null;
            if (player?.abstractPhysicalObject?.stuckObjects == null || player.abstractPhysicalObject.stuckObjects.Count < 1) return false;
            if (!PlayerCWT.TryGetData(player, out var data)) return false;
            if (data.pullingOutThisDart != null)
            {
                dart = data.pullingOutThisDart;
                return true;
            }

            foreach (AbstractPhysicalObject.AbstractObjectStick stick in player.abstractCreature.stuckObjects)
            {
                if (stick is AbstractPhysicalObject.AbstractSpearStick && (stick as AbstractPhysicalObject.AbstractSpearStick).Spear is AbstractDart abstractDart)
                {
                    data.pullingOutThisDart = abstractDart.realisedDart;
                    dart = data.pullingOutThisDart;
                    Log.LogMessage("Found a dart in self!");
                    return true;
                }
            }
            return false;
        }

        public static void PullOutDartFromSelf(Player player, Dart dart)
        {
            if (player == null || dart == null) return;

            dart.ChangeMode(Weapon.Mode.Free);

            int hand = player.FreeHand();

            if (hand > -1)
            {
                player.SlugcatGrab(dart, hand);
            }

            player.room.PlaySound(SoundID.Spear_Dislodged_From_Creature, player.mainBodyChunk);
            player.Stun(10);
        }

        public static void ResetPull(Player player)
        {
            if (player == null) return;
            if (!PlayerCWT.TryGetData(player, out var data)) return;
            data.pullingOutThisDart.pullOutTimer = 0;
            data.pullingOutThisDart.pullOutAttempts = 0;
            data.pullingOutThisDart.pullOutChance = 1;
            data.pullingOutThisDart = null;
        }

        public static bool PullOutLuckCheck(this Dart dart)
        {
            if (dart == null) return false;

            dart.pullOutAttempts++;
            float randomValue = UnityEngine.Random.value;
            float numberToBeat = math.pow(dart.pullOutChance, dart.pullOutAttempts);
            return (randomValue > numberToBeat);
        }
    }
}
