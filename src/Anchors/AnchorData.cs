using System;
using UnityEngine;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Pom.Pom;
using static Stardust.Anchors.AnchorEnums;
using static Stardust.Plugin;

namespace Stardust.Anchors
{
    public class AnchorData : ManagedData
    {
        public AnchorData(PlacedObject po) : base(po, new ManagedField[] { })
        {

        }
        [StringField("Name", "Anchor Name", "Name: ")]
        public string name;

        [EnumField<AnchorID>("Type", AnchorID.Default)]
        public AnchorID type;

        public bool Active(ref RainWorldGame game)
        {
            // more convenient to access Active
            return Active(ref game, type);
        }

        public static bool Active(ref RainWorldGame game, AnchorID type)
        {
            // here should be code for when to trigger it, aka checking for save data stuff
            if (game != null && game.IsStorySession && game.GetStorySession.saveState?.deathPersistentSaveData != null && game.GetStorySession.saveState.deathPersistentSaveData.GetAnchorMeeting(type))
            {
                Log.LogMessage("Anchor is active!");
                return true;
            }
            Log.LogMessage("Anchor isnt active!");
            return false;
        }
    }
}