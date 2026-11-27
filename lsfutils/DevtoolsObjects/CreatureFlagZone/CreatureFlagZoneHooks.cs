using BepInEx;
using MoreSlugcats;
using lsfUtils.CWTs;
using lsfUtils.DevtoolsObjects.ConditionalFilter;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Watcher;
using HUD;
using lsfUtils.Items.Spears.TorchSpears;

namespace lsfUtils.DevtoolsObjects.CreatureFlagZone;

public static class CreatureFlagZoneHooks
{
    public static void ApplyHooks()
    {
        On.Creature.PlaceInRoom += Creature_PlaceInRoom;
    }

    public static void Creature_PlaceInRoom(On.Creature.orig_PlaceInRoom orig, Creature self, Room placeRoom)
    {
        orig(self, placeRoom);
        if (self?.abstractCreature == null) return;

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

            if (Custom.DistLess(self.abstractCreature.pos.Vec2(), list[i].owner.pos, list[i].radius.magnitude) && self.abstractCreature.spawnDen == new WorldCoordinate(-1, -1, -1, -1) && self is not Player)
            {
                string flags = list[i].creatureFlags.Trim();
                string data = self.abstractCreature.spawnData;

                if (!string.IsNullOrEmpty(data) && data.Contains('{') && data.Contains('}'))
                {
                    int openIndex = data.IndexOf('{');
                    int bracketIndex = data.IndexOf('}');

                    string inner = data.Substring(openIndex + 1, bracketIndex - openIndex - 1);
                    
                    string insert = string.IsNullOrWhiteSpace(inner) ? flags : flags + ",";
                    data = data.Insert(openIndex + 1, insert);
                }
                else
                {
                    data += "{";
                    for (int j = 0; j < list.Count; j++)
                    {
                        data += list[j].creatureFlags;
                        if (j != list.Count - 1) data += ",";
                    }
                    data += "}";
                }

                self.abstractCreature.spawnData = data;
                self.abstractCreature.setCustomFlags();
                break;
            }
        }
    }
}
