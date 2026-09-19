using lsfUtils.CWTs;

namespace lsfUtils.ScreenVariants.Deeperspace;

public static class DeeperspaceHooks
{
    public static void ApplyHooks()
    {
        On.RoomCamera.DrawUpdate += RoomCamera_DrawUpdate;
    }
    public static void RoomCamera_DrawUpdate(On.RoomCamera.orig_DrawUpdate orig, RoomCamera self, float timeStacker, float timeSpeed)
    {
        orig(self, timeStacker, timeSpeed);

        if (!RoomCameraCWT.TryGetData(self, out var data)) return;
        if (data.screenVariantController == null) return;

        if (self.room != null && self.followAbstractCreature != null && self.followAbstractCreature.rippleLayer == 2)
        {
            if (data.screenVariantController.current is not DeeperspaceBackground)
            {
                data.screenVariantController.SwitchTo<DeeperspaceBackground>();
            }
        }
        else if (data.screenVariantController.current is DeeperspaceBackground)
        {
            data.screenVariantController.Clear();
        }
        data.screenVariantController.DrawUpdate(timeStacker);
    }
}