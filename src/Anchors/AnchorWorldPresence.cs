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
using static Stardust.Plugin;
using static Stardust.Enums;

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
                Log.LogMessage("No room to assign anchor to!!! Or placeholder.");
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
                Log.LogMessage($"ANCHOR ROOM NOT FOUND: {spotRoom}");
            }
        }

        public bool CreaturesSleepInRoom(AbstractRoom room)
        {
            if (room == null || anchorRoom == null)
                return false;
            if (room.index == anchorRoom.index)
                return true;
            return AnchorMode(room) > 0.05f;
        }

        public float AnchorMode(AbstractRoom room)
        {
            if (anchorRoom == room)
                return 1f;
            if (presenceRooms != null && presenceRooms.Count > 0)
            {
                if (presenceRooms.TryGetValue(room.name, out int value))
                    return value * 0.01f;
                Log.LogMessage("Couldnt find this room in presence rooms!");
                return 0f;
            }
            Log.LogMessage("Presence rooms is empty!");
            return 0f;
        }

        public float AttractionValueForCreature(AbstractRoom room, CreatureTemplate.Type tp, float defValue)
        {
            if (room.index == anchorRoom.index)
                return 0f;
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
}
