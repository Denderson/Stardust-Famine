using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public AnchorID anchorID = AnchorID.Default;

        public string songName;

        public List<AbstractRoom> presenceRooms;

        public static bool SpawnAnchor(AnchorID anchorID) // feel free to add more parameters, just make sure you can grab them from a story session (ask if unsure)
        {
            if (ModManager.Expedition && Custom.rainWorld.ExpeditionMode && Custom.rainWorld.progression.currentSaveState.cycleNumber == 0)
            {
                return false;
            }
            if (Custom.rainWorld.safariMode)
            {
                return false;
            }
            // add custom conditions to spawn Anchors here (get which anchor this is for from the anchorID parameter)
            return true;
        }

        public AnchorWorldPresence(World world, AnchorID anchorID)
        {
            this.anchorID = anchorID;
            this.world = world;
            string text;
            switch (anchorID)
            {
                case AnchorID.Deeperspace:
                    {
                        text = "PLACEHOLDER";
                        this.songName = "PLACEHOLDER";
                        this.presenceRooms = [];
                        break;
                    }
                case AnchorID.Ripplespace:
                    {
                        text = "PLACEHOLDER";
                        this.songName = "PLACEHOLDER";
                        this.presenceRooms = [];
                        break;
                    }
                case AnchorID.Carnalplane:
                    {
                        text = "PLACEHOLDER";
                        this.songName = "PLACEHOLDER";
                        this.presenceRooms = [];
                        break;
                    }
                case AnchorID.Karmaspace:
                    {
                        text = "PLACEHOLDER";
                        this.songName = "PLACEHOLDER";
                        this.presenceRooms = [];
                        break;
                    }
                case AnchorID.Mindspace:
                    {
                        text = "PLACEHOLDER";
                        this.songName = "PLACEHOLDER";
                        this.presenceRooms = [];
                        break;
                    }
                case AnchorID.Weaverspace:
                    {
                        text = "PLACEHOLDER";
                        this.songName = "PLACEHOLDER";
                        this.presenceRooms = [];
                        break;
                    }
                default:
                    {
                        text = "PLACEHOLDER";
                        this.songName = "PLACEHOLDER";
                        this.presenceRooms = [];
                        break;
                    }
            }
            if (text == null || text.ToLowerInvariant() == "placeholder")
            {
                Log.LogMessage("No room to assign anchor to!!! Or placeholder.");
            }    
            this.anchorRoom = world.GetAbstractRoom(text);
            if (this.anchorRoom == null)
            {
                Log.LogMessage("ANCHOR ROOM NOT FOUND: " + text);
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
            if (presenceRooms != null && presenceRooms.Count > 0 && presenceRooms.Contains(room))
            {
                // placeholder, will add reading it from room effects or txt files later on
                return 0.5f;
            }
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
}
