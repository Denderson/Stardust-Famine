using Stardust.CWTs;
using Stardust.RippleLayers;
using UnityEngine;

namespace Stardust.RippleLayers
{
    public static class DeeperspaceHooks
    {
        public static void RoomCamera_ctor(On.RoomCamera.orig_ctor orig, RoomCamera self, RainWorldGame game, int cameraNumber)
        {
            orig(self, game, cameraNumber);

            if (!RoomCameraCWT.TryGetData(self, out var cwt)) return;

            cwt.deeperspaceData = new DeeperspaceData(self);
        }

        public static void RoomCamera_DrawUpdate(On.RoomCamera.orig_DrawUpdate orig, RoomCamera self, float timeStacker, float timeSpeed)
        {
            orig(self, timeStacker, timeSpeed);

            if (!RoomCameraCWT.TryGetData(self, out var cwt)) return;
            if (cwt.deeperspaceData == null) return;

            bool inDeeperspace = self.room != null
                && self.followAbstractCreature != null
                && self.followAbstractCreature.rippleLayer == 2;

            if (inDeeperspace) cwt.deeperspaceData.Activate();
            else cwt.deeperspaceData.Deactivate();

            cwt.deeperspaceData.DrawUpdate(timeStacker);
        }

        public static void RoomCamera_ClearAllSprites(On.RoomCamera.orig_ClearAllSprites orig, RoomCamera self)
        {
            if (RoomCameraCWT.TryGetData(self, out var cwt)) cwt.deeperspaceData?.Dispose();

            orig(self);
        }
    }
}