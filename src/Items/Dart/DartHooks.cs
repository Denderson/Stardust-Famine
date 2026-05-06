using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
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
            if (self != null && self.Consious && !self.input[0].pckp && self.FreeHand() > -1 && self.input[0].y == 0)
            {
                return;
            }
            HandleSelfDartPull(self);
        }

        public static void HandleSelfDartPull(Player player)
        {
            if (!HasEmbeddedDart(player)) return;


            int required = 40;

            player.slowMovementStun = math.max(player.slowMovementStun, 40);

            if (selfPullTimers[player] >= required)
            {
                PullOutDartFromSelf(player);
                selfPullTimers[player] = 0;
            }
        }

        public static bool HasEmbeddedDart(Player player, out Dart dart)
        {
            dart = null;
            if (player?.abstractPhysicalObject?.stuckObjects == null || player.abstractPhysicalObject.stuckObjects.Count < 1) return false;

            foreach (AbstractPhysicalObject.AbstractSpearStick stick in player.abstractCreature.stuckObjects)
            {
                if (stick?.Spear is AbstractDart)
                {
                    dart = ((stick.Spear as AbstractDart).realizedObject as ;
                }
            }

            bool result = player.abstractCreature.stuckObjects.Exists(obj => obj is AbstractPhysicalObject.AbstractSpearStick stick && stick.Spear is AbstractDart abstractDart);
            Log.LogMessage("Result: " +  result);
            return result;
        }

        public static void PullOutDartFromSelf(Player player)
        {
            var stuckList = player.abstractCreature.stuckObjects;

            for (int i = 0; i < stuckList.Count; i++)
            {
                var stick = stuckList[i] as AbstractPhysicalObject.AbstractObjectStick;
                if (stick == null) continue;

                if (stick.A.type == AbstractPhysicalObject.AbstractObjectType.Dart)
                {
                    AbstractPhysicalObject dartAbstract = stick.A;

                    // remove from body
                    stuckList.RemoveAt(i);

                    // realize it
                    dartAbstract.RealizeInRoom();
                    PhysicalObject dart = dartAbstract.realizedObject;

                    // spawn at player
                    dart.firstChunk.pos = player.mainBodyChunk.pos;

                    // try to put in hand
                    if (player.grasps[0] == null)
                        player.SlugcatGrab(dart, 0);
                    else if (player.grasps[1] == null)
                        player.SlugcatGrab(dart, 1);
                    else
                        dart.firstChunk.vel = Custom.RNV() * 5f; // drop if hands full

                    // feedback
                    player.room.PlaySound(SoundID.Spear_Pull_Out_Of_Creature, player.mainBodyChunk);

                    // small stun (feels more real)
                    player.Stun(10);

                    break;
                }
            }
        }

        public static void ResetPull(Player player)
        {
            if (selfPullTimers.ContainsKey(player))
            {
                selfPullTimers[player] = 0;
            }
        }
    }
}
