using BepInEx;
using UnityEngine;
using lsfUtils.DevtoolsObjects.ConditionalFilter;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lsfUtils.DevtoolsObjects.CreatureFlagZone
{
    public static class CreatureFlagZoneHooks
    {
        public static void ApplyHooks()
        {
            On.Creature.PlaceInRoom += Creature_PlaceInRoom;
            On.AbstractCreature.IsVoided += AbstractCreature_IsVoided;
            On.HazerGraphics.ApplyPalette += HazerGraphics_ApplyPalette;
        }

        public static void Creature_PlaceInRoom(On.Creature.orig_PlaceInRoom orig, Creature self, Room placeRoom)
        {
            orig(self, placeRoom);
            List<CreatureFlagZoneData> list = [];
            RoomSettings rSettings = placeRoom.roomSettings;
            foreach (PlacedObject placedObject in rSettings.placedObjects)
            {
                if (placeRoom == null) break;

                if (placedObject.data is CreatureFlagZoneData zone)
                {
                    list.Add(zone);
                }
            }
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].creatureFlags.IsNullOrWhiteSpace()) continue;

                if (Custom.DistLess(self.abstractCreature.pos.Vec2(), list[i].owner.pos, list[i].radius.magnitude) && self is not Player)
                {
                    self.abstractCreature.spawnData = "{" + list[i].creatureFlags + "}";
                    self.abstractCreature.setCustomFlags();
                    break;
                }
            }
        }

        public static bool AbstractCreature_IsVoided(On.AbstractCreature.orig_IsVoided orig, AbstractCreature self)
        {
            if (self.voidCreature && self.creatureTemplate.type == CreatureTemplate.Type.Hazer) return true;
            else return orig(self);
        }

        private static void HazerGraphics_ApplyPalette(On.HazerGraphics.orig_ApplyPalette orig, HazerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig(self, sLeaser, rCam, palette);
            if ((self.owner as Creature).abstractCreature.IsVoided())
            {
                HSLColor saturatedGoldHSL = new HSLColor(0.0944f, 1f, 0.68f);
                HSLColor secondaryColor = new HSLColor(saturatedGoldHSL.hue, 0.7f, 0.38f);
                //please dont kill me for the variable names im just copying the vanilla code
                float num = Custom.SCurve(Mathf.InverseLerp(0.1f, 1.1f, self.camo), 0.8f);
                Color color = Color.Lerp(Custom.HSL2RGB(saturatedGoldHSL.hue, saturatedGoldHSL.saturation * (1f - 0.4f * self.deadColor), saturatedGoldHSL.lightness * (1f - 0.3f * self.deadColor)), palette.fogColor, 0.2f + 0.1f * self.deadColor);
                Color color2 = Color.Lerp(Custom.HSL2RGB(secondaryColor.hue, secondaryColor.saturation * (1f - 0.2f * self.deadColor), secondaryColor.lightness * (1f - 0.5f * self.deadColor)), palette.blackColor, 0.05f + 0.4f * Mathf.Max(self.deadColor * 0.5f, Mathf.Pow(num, 2f)));
                color = Color.Lerp(color, palette.blackColor, 0.2f * self.deadColor);
                color2 = Color.Lerp(color2, self.camoPickupColor, num * 0.5f);
                sLeaser.sprites[self.MeshSprite].color = color;
                sLeaser.sprites[self.BodySprite].color = color;
                Color b = Color.Lerp(color2, palette.blackColor, 0.1f + 0.3f * Mathf.Max(self.deadColor, num));

                for (int i = 0; i < self.tentacles.Count; i++)
                {
                    for (int j = 0; j < (sLeaser.sprites[self.TentacleSprite(i)] as TriangleMesh).verticeColors.Length; j++)
                    {
                        (sLeaser.sprites[self.TentacleSprite(i)] as TriangleMesh).verticeColors[j] = Color.Lerp(color, b, Mathf.InverseLerp(1f, (float)((sLeaser.sprites[self.TentacleSprite(i)] as TriangleMesh).verticeColors.Length - 1), (float)j));
                    }
                }
                sLeaser.sprites[self.ClosedEyeSprite].color = Color.Lerp(Color.Lerp(color, color2, 0.8f), palette.blackColor, 0.3f);
                color = Color.Lerp(Color.Lerp(color, palette.blackColor, 0.7f - 0.5f * self.deadColor), color2, 0.6f + 0.4f * num);
                for (int k = 0; k < self.scales.Length; k++)
                {
                    sLeaser.sprites[self.ScaleSprite(k)].color = color;
                }
            }

        }

    }
}
