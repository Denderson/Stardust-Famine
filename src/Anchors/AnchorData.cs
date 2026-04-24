using System;
using UnityEngine;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Pom.Pom;
using static Stardust.Plugin;
using static Stardust.Enums;

namespace Stardust.Anchors
{
    public class AnchorData : ManagedData
    {
        public AnchorData(PlacedObject po) : base(po, new ManagedField[] { })
        {

        }
        [StringField("Name", "Anchor Name", "Name: ")]
        public string name;

        [EnumField<AnchorID>("Type", AnchorID.None)]
        public AnchorID type;

        public bool Active(RainWorldGame game)
        {
            // more convenient to access Active
            return Active(game, type);
        }

        public static bool Active(RainWorldGame game, AnchorID type)
        {
            // add custom conditions to spawn Anchors here (get which anchor this is for from the anchorID parameter)

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