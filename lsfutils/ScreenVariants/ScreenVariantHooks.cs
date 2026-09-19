using lsfUtils.CWTs;

namespace lsfUtils.ScreenVariants;

public static class ScreenVariantHooks
{
    public static void ApplyHooks()
    {
        On.RoomCamera.ctor += RoomCamera_ctor;
        On.RoomCamera.ClearAllSprites += RoomCamera_ClearAllSprites;
    }

    public static void RoomCamera_ctor(On.RoomCamera.orig_ctor orig, RoomCamera self, RainWorldGame game, int cameraNumber)
    {
        orig(self, game, cameraNumber);

        if (!RoomCameraCWT.TryGetData(self, out var data)) return;
        data.screenVariantController = new ScreenVariantController(self);
    }

    public static void RoomCamera_ClearAllSprites(On.RoomCamera.orig_ClearAllSprites orig, RoomCamera self)
    {
        orig(self);
        if (!RoomCameraCWT.TryGetData(self, out var data)) return;
        data.screenVariantController?.Dispose();
    }
}