using Stardust.Slugcats.Scholar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Stardust.CWTs
{
    public static class DialogBoxCWT
    {

        public static readonly ConditionalWeakTable<HUD.DialogBox, DataClass> dialogCWT = new();
        public static bool TryGetData(HUD.DialogBox key, out DataClass data)
        {
            if (key != null)
            {
                data = dialogCWT.GetOrCreateValue(key);
            }
            else data = null;

            return data != null;
        }
        public class DataClass
        {
            public bool isAnchorBox = false;
            public Vector2 positionOffset = new();
        }
    }
}
