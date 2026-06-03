using lsfUtils.CWTs;
using MoreSlugcats;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static lsfUtils.Plugin;

namespace lsfUtils.Items.KarmaMask
{
    public static class KarmaMaskHooks
    {
        public static bool Meet_Requirement(Func<RegionGate, bool> orig, RegionGate self)
        {
            bool value = orig(self);

            if (self.room.PlayersInRoom.Count <= 0) return value;

            AbstractCreature firstAlivePlayer = self.room.game.FirstAlivePlayer;
            if (self.room.game.Players.Count == 0 || firstAlivePlayer == null || firstAlivePlayer.realizedCreature == null && ModManager.CoopAvailable)
            {
                return value;
            }
            foreach (Player player in self.room.PlayersInRoom)
            {
                if (player?.grasps != null && player.grasps.Length != 0)
                {
                    foreach (Creature.Grasp t in player.grasps)
                    {
                        if (t != null && t.grabbed != null && t.grabbed is KarmaMask)
                        {
                            return true;
                        }
                    }
                }
            }
            return value;
        }

        public static void KarmaMeter_Update(On.HUD.KarmaMeter.orig_Update orig, HUD.KarmaMeter self)
        {
            orig(self);
            if (self.hud?.owner is Player player && CWTs.PlayerCWT.TryGetData(player, out var data))
            {

                if (!data.karmaMode && data.previousKarmaMode)
                {
                    self.karmaSprite.element = Futile.atlasManager.GetElementWithName(HUD.KarmaMeter.RippleSymbolSprite(small: true, 5));
                    self.forceVisibleCounter = Math.Max(self.forceVisibleCounter, 120);
                }
                if (data.karmaMode && !data.previousKarmaMode)
                {
                    self.displayKarma.x = 9;
                    self.displayKarma.y = 9;
                    self.karmaSprite.element = Futile.atlasManager.GetElementWithName(HUD.KarmaMeter.KarmaSymbolSprite(small: true, self.displayKarma));
                    self.forceVisibleCounter = Math.Max(self.forceVisibleCounter, 120);
                }
            }
        }

        public static void VultureMaskGraphics_ctor_PhysicalObject_MaskType_int_string(On.MoreSlugcats.VultureMaskGraphics.orig_ctor_PhysicalObject_MaskType_int_string orig, VultureMaskGraphics self, PhysicalObject attached, VultureMask.MaskType type, int firstSprite, string overrideSprite)
        {
            orig(self, attached, type, firstSprite, overrideSprite);
            if (self.attachedTo is KarmaMask)
            {
                self.maskType = VultureMask.MaskType.SCAVTEMPLAR;
                self.glimmer = true;
                self.ignoreDarkness = true;
            }
        }

        public static void VultureMaskGraphics_ctor(On.MoreSlugcats.VultureMaskGraphics.orig_ctor_PhysicalObject_AbstractVultureMask_int orig, VultureMaskGraphics self, PhysicalObject attached, VultureMask.AbstractVultureMask abstractMask, int firstSprite)
        {
            orig(self, attached, abstractMask, firstSprite);
            if (self.attachedTo is KarmaMask)
            {
                self.maskType = VultureMask.MaskType.SCAVTEMPLAR;
                self.glimmer = true;
                self.ignoreDarkness = true;
            }
        }

        public static void VultureMaskGraphics_DrawSprites(On.MoreSlugcats.VultureMaskGraphics.orig_DrawSprites orig, VultureMaskGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if (self.attachedTo is KarmaMask)
            {
                sLeaser.sprites[self.firstSprite].color = RainWorld.GoldRGB;
                sLeaser.sprites[self.firstSprite].shader = Custom.rainWorld.Shaders["RippleBasicBothSides"];
            }
        }

        public static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);
            if (self?.grasps == null)
            {
                return;
            }
            if (!PlayerCWT.TryGetData(self, out var data))
            {
                return;
            }
            data.previousKarmaMode = data.karmaMode;
            if (self.grasps.Length != 0)
            {
                bool flag = false;
                for (int i = 0; i < self.grasps.Length; i++)
                {
                    if (self.grasps[i]?.grabbed is KarmaMask)
                    {
                        data.karmaMode = true;
                        flag = true;
                    }
                    if (!flag) data.karmaMode = false;
                }
            }
        }
    }
}
