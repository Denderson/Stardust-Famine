using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stardust.Anchors
{
    public class Anchor : UpdatableAndDeletable //, IDrawable
    {
        private AnchorData data;

        public Anchor(PlacedObject placedObject, Room room)
        {
            AnchorData maybedata = placedObject.data as AnchorData;
            if (maybedata == null)
            {
                throw new ArgumentException($"{nameof(PlacedObject)} was null or didn't contain a {nameof(AnchorData)} instance");
            }
            data = maybedata;
            this.room = room;

            // here should be the main part of Anchor code, including its graphics and stuff
        }

        public void AnchorMeetingFinished()
        {
            if (this?.room?.game?.GetStorySession?.saveState?.deathPersistentSaveData != null)
            {
                this.room.game.GetStorySession.saveState.deathPersistentSaveData.SetAnchorMeeting(this.data.type);
            }
            // do the dissapearing code here
        }
    }
}
