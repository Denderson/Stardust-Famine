using RWCustom;
using Stardust.CWTs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Stardust.Anchors.AnchorEnums;
using static Stardust.Plugin;

namespace Stardust.Anchors
{
    public class AnchorWorldPresence : World.IMigrationInfluence
    {
        public World world;

        public AbstractRoom anchorRoom;

        public AnchorID anchorID = AnchorID.None;

        public string songName;

        public Dictionary<string, int> presenceRooms;

        public AnchorWorldPresence(World world, AnchorID anchorID, string spotRoom, Dictionary<string, int> presenceRooms)
        {
            this.anchorID = anchorID;
            this.world = world;
            if (spotRoom == null || spotRoom.Trim().ToLowerInvariant() == "placeholder")
            {
                Log.LogMessage("No room to assign anchor to!!! Or placeholder.");
            }
            this.anchorRoom = world.GetAbstractRoom(spotRoom);
            this.presenceRooms = [];
            this.presenceRooms = presenceRooms;
            switch (anchorID)
            {
                case AnchorID.Deeperspace:
                    {
                        this.songName = null;
                        break;
                    }
                case AnchorID.Ripplespace:
                    {
                        this.songName = null;
                        break;
                    }
                case AnchorID.Carnalplane:
                    {
                        this.songName = null;
                        break;
                    }
                case AnchorID.Karmaspace:
                    {
                        this.songName = null;
                        break;
                    }
                case AnchorID.Mindspace:
                    {
                        this.songName = null;
                        break;
                    }
                case AnchorID.Weaverspace:
                    {
                        this.songName = null;
                        break;
                    }
                default:
                    {
                        Log.LogMessage("No AnchorID!");
                        this.songName = null;
                        break;
                    }
            }
            
            if (this.anchorRoom == null)
            {
                Log.LogMessage("ANCHOR ROOM NOT FOUND: " + spotRoom);
            }
        }

        public bool CreaturesSleepInRoom(AbstractRoom room)
        {
            if (room == null || anchorRoom == null)
            {
                return false;
            }
            if (room.index == anchorRoom.index)
            {
                return true;
            }
            return AnchorMode(room) > 0.05f;
        }

        public float AnchorMode(AbstractRoom room)
        {
            if (anchorRoom == room)
            {
                return 1f;
            }
            string roomName = room.name;

            if (presenceRooms != null && presenceRooms.Count > 0)
            {
                if (presenceRooms.TryGetValue(roomName, out int value))
                {
                    return value * 0.01f;
                }
                Log.LogMessage("Couldnt find this room in presence rooms!");
                return 0f;
            }
            Log.LogMessage("Presence rooms is empty!");
            return 0f;
        }

        public float AttractionValueForCreature(AbstractRoom room, CreatureTemplate.Type tp, float defValue)
        {
            if (room.index == anchorRoom.index)
            {
                return 0f;
            }
            float num = CreaturesAllowedInThisRoom(room);
            return Mathf.Lerp(Mathf.Min(defValue, num), defValue * num, 0.5f);
        }

        public float AttractionValueForCreature(AbstractRoom room, string namedAttr, float defValue)
        {
            return AttractionValueForCreature(room, (CreatureTemplate.Type)null, defValue);
        }

        private float CreaturesAllowedInThisRoom(AbstractRoom room)
        {
            return Mathf.Pow(1f - AnchorMode(room), 7f);
        }
    }

    public static class AnchorWorldPresenceSetup
    {
        public static void SpawnAnchor(this World world)
        {
            if (ModManager.Expedition && Custom.rainWorld.ExpeditionMode)
            {
                return;
            }
            if (Custom.rainWorld.safariMode)
            {
                return;
            }
            if (!WorldCWT.TryGetData(world, out var data)) 
            {
                Log.LogMessage("Couldnt get WorldCWT value!");
                return;
            }

            string filePath = "world/" + world.region.name + "/anchor.txt";
            if (!File.Exists(AssetManager.ResolveFilePath(filePath)))
            {
                Log.LogMessage("Anchor file doesnt exist!");
                return;
            }

            string[] array = File.ReadAllLines(AssetManager.ResolveFilePath(filePath));
            if (array == null || array.Length < 2)
            {
                Log.LogMessage("Anchor file is empty!");
                return;
            }

            AnchorID anchorID = AnchorIDFromString(array[0]);
            if (!AnchorData.Active(world.game, anchorID))
            {
                Log.LogMessage("Anchor isnt active!");
                return;
            }

            string anchorSpotRoom = null;
            Dictionary<string, int> anchorPresenceRooms = [];

            for (int i = 1; i < array.Length; i++)
            {
                if (!array[i].StartsWith("//") && array[i].Length > 0)
                {
                    Log.LogMessage("Reading line: " + i);
                    if (array[i].Contains(':'))
                    {
                        string[] splitLine = array[i].Split(':');
                        if (splitLine.Length != 2)
                        {
                            Log.LogMessage("Incorrect formatting in Anchor file, line " + i);
                            continue;
                        }
                        if (splitLine[1].ToLowerInvariant().StartsWith("spot"))
                        {
                            anchorSpotRoom = splitLine[0];
                        }
                        else if (int.TryParse(splitLine[1], out int value))
                        {
                            anchorPresenceRooms.Add(splitLine[0], value);
                        }
                    }
                }
            }
            if (anchorSpotRoom != null && anchorSpotRoom.Length > 0)
            {
                Log.LogMessage("Spawning Anchor!");
                data.hasAnchorWorldPresence = true;
                data.anchorWorldPresence = new AnchorWorldPresence(world, anchorID, anchorSpotRoom, anchorPresenceRooms);
                return;
            }
            Log.LogMessage("No anchor to load!");
            data.hasAnchorWorldPresence = false;
        }

        public static bool AnchorPresenceInRoom(this AbstractRoom abstractRoom, out float intensity, out AnchorWorldPresence presence)
        {
            intensity = 0f;
            presence = null;
            if (abstractRoom?.world == null)
            {
                return false;
            }
            if (!CWTs.WorldCWT.TryGetData(abstractRoom.world, out var data))
            {
                Log.LogMessage("Couldnt get WorldCWT!");
                return false;
            }
            if (data?.anchorWorldPresence == null)
            {
                return false;
            }
            presence = data.anchorWorldPresence;
            intensity = data.anchorWorldPresence.AnchorMode(abstractRoom);
            return intensity > 0f;
        }

        public static AnchorID AnchorIDFromString(string id)
        {
            if (id == null)
            {
                Log.LogMessage("AnchorID is null!");
                return AnchorID.None;
            }
            id = id.Trim().ToLowerInvariant();
            switch (id) 
            {
                case "deeperspace": return AnchorID.Deeperspace;
                case "ripplespace": return AnchorID.Ripplespace;
                case "carnalplane": return AnchorID.Carnalplane;
                case "karmaspace": return AnchorID.Karmaspace;
                case "mindspace": return AnchorID.Karmaspace;
                case "weaverspace": return AnchorID.Weaverspace;
                default:
                    {
                        Log.LogMessage("Couldnt parse the AnchorID from string!");
                        return AnchorID.None;
                    }
            }
        }
    }
}
