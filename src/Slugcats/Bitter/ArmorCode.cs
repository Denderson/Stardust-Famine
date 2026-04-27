using Stardust.CWTs;
using Stardust.SaveFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using static Stardust.SaveFile.SaveFileBitter;
using static Stardust.SaveFile.SaveFileMain;
using static Stardust.Plugin;
using UnityEngine;

namespace Stardust.Slugcats.Bitter
{
    public class ArmorCode
    {
        public static void SaveArmorOnHibernation(On.ShelterDoor.orig_Close orig, ShelterDoor self)
        {
            if (self?.room == null)
            {
                Log.LogMessage("Room is null in SaveArmorOnHibernation!");
                return;
            }
            if (self.room.game?.StoryCharacter != Enums.SlugcatStatsName.bitter)
            {
                return;
            }
            if (self.room.PlayersInRoom == null || self.room.PlayersInRoom.Count <= 0)
            {
                Log.LogMessage("No players in SaveArmorOnHibernation!");
                return;
            }
            float armor = 0f;
            bool anyoneHasArmor = false;
            foreach (Player player in self.room.PlayersInRoom)
            {
                if (player != null && player.SlugCatClass == Enums.SlugcatStatsName.bitter && PlayerCWT.TryGetData(player, out var data))
                {
                    anyoneHasArmor = true;
                    armor = math.max(armor, data.armorHealth);
                }
            }
            if (anyoneHasArmor)
            {
                self.room.game.GetStorySession.saveState.SetInt(bitterArmorRemaining, ArmorFloatToInt(armor));
            }
            orig(self);
        }

        public static void SetArmorOnPlayerCreation(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            if (self?.SlugCatClass != Enums.SlugcatStatsName.bitter)
            {
                return;
            }
            if (!PlayerCWT.TryGetData(self, out var data))
            {
                return;
            }
            if (world?.game?.StoryCharacter != Enums.SlugcatStatsName.bitter)
            {
                data.armorHealth = maxArmor;
            }
            else
            {
                data.armorHealth = ArmorFromSave(world.game.GetStorySession?.saveState);
            }
        }

        public static bool HitArmor(Player player, Vector2 direction)
        {
            // TODO, do NOT use velocity to avoid issues when standing still
            return true;
        }

        public static bool Player_SpearStick(On.Player.orig_SpearStick orig, Player self, Weapon source, float dmg, BodyChunk chunk, PhysicalObject.Appendage.Pos appPos, Vector2 direction)
        {
            bool value = orig(self, source, dmg, chunk, appPos, direction);
            if (!value)
            {
                return false;
            }
            if (self?.SlugCatClass == Enums.SlugcatStatsName.bitter && PlayerCWT.TryGetData(self, out var data) && data.armorHealth > 0f)
            {
                if (HitArmor(self, direction))
                {
                    return false;
                }
            }
            return value;
        }

        public static bool GetArmorValues(Creature.DamageType type, out float armorEfficency)
        {
            armorEfficency = 0f;

            if (type == Creature.DamageType.Blunt)
            {
                armorEfficency = 1f;
            }
            if (type == Creature.DamageType.Bite)
            {
                armorEfficency = 0.85f;
            }
            if (type == Creature.DamageType.Stab)
            {
                armorEfficency = 0.7f;
            }
            if (type == Creature.DamageType.Explosion)
            {
                armorEfficency = 0.35f;
            }

            return armorEfficency > 0f;
        }

        public static void Creature_Violence(On.Creature.orig_Violence orig, Creature self, BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppendage, Creature.DamageType type, float damage, float stunBonus)
        {
            if (!self.RippleViolenceCheck(source))
            {
                return;
            }
            if (directionAndMomentum.HasValue && hitChunk?.owner != null && hitChunk.owner is Player player && player.SlugCatClass == Enums.SlugcatStatsName.bitter && PlayerCWT.TryGetData(player, out var data))
            {
                if (HitArmor(player, directionAndMomentum.Value))
                {
                    if (GetArmorValues(type, out float armorEfficency)) // only block if armor works against that damage type
                    {
                        float blockedDamage = armorEfficency * damage;
                        data.armorHealth -= blockedDamage; // reduce armor health
                        damage -= blockedDamage; // reduce damage (survives if less than 1f)
                        stunBonus /= 2; // halve stun bonus
                    }
                }
            }
            orig(self, source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
        }
    }
}
