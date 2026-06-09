using Stardust.RippleLayers;
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
    public static class RoomCameraCWT
    {

        public static readonly ConditionalWeakTable<RoomCamera, DataClass> roomCameraCWT = new();
        public static bool TryGetData(RoomCamera key, out DataClass data)
        {
            if (key != null)
            {
                data = roomCameraCWT.GetOrCreateValue(key);
            }
            else data = null;

            return data != null;
        }
        public class DataClass
        {
            public DeeperspaceData deeperspaceData;
        }
    }
}
