using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Stardust.Enums;
using static Stardust.Plugin;
using static Stardust.SaveFile.SaveFileMain;

namespace Stardust.SaveFile
{
    public static class SaveFileAnchors
    {
        public static void SetAnchorMeeting(this DeathPersistentSaveData data, AnchorID anchorType)
        {
            string anchorTypeString = anchorType.ToString()?.ToLowerInvariant();
            string anchorData = data.GetString(anchors)?.ToLowerInvariant();

            if (anchorData != null && anchorData.Length > 0)
            {
                Log.LogMessage("Adding anchor meeting!");
                anchorData = anchorData + "+" + anchorTypeString;
            }
            else
            {
                Log.LogMessage("Adding first anchor meeting!");
                anchorData = anchorTypeString;
            }
            data.SetString(anchors, anchorData);
        }

        public static bool GetAnchorMeeting(this DeathPersistentSaveData data, AnchorID anchorType)
        {
            string anchorTypeString = anchorType.ToString()?.ToLowerInvariant();
            string anchorData = data.GetString(anchors)?.ToLowerInvariant();

            if (anchorData == null || anchorData.Length <= 0)
            {
                Log.LogMessage("No anchor data!");
                return false;
            }

            if (anchorData.Contains(anchorTypeString))
            {
                Log.LogMessage("Met this anchor before: " + anchorTypeString);
                return true;
            }
            Log.LogMessage("Didnt meet this anchor before: " + anchorTypeString);
            return false;
        }
    }
}
