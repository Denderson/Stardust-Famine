using lsfUtils.CWTs;
using lsfUtils.ScreenVariants.Deeperspace;
using UnityEngine;

namespace lsfUtils.ScreenVariants;

public static class ScreenVariantHooks
{
    public static void ApplyHooks()
    {
        On.RoomCamera.ctor += RoomCamera_ctor;
        On.RoomCamera.ClearAllSprites += RoomCamera_ClearAllSprites;
        On.RoomCamera.ApplyEffectColorsToAllPaletteTextures += RoomCamera_ApplyEffectColorsToAllPaletteTextures;
        On.RoomCamera.ApplyFade += RoomCamera_ApplyFade;
        On.RoomCamera.ApplyFadeTexToPalleteTexture += RoomCamera_ApplyFadeTexToPalleteTexture;
        On.RoomCamera.ChangeRoom += RoomCamera_ChangeRoom;
    }

    private static void RoomCamera_ChangeRoom(On.RoomCamera.orig_ChangeRoom orig, RoomCamera self, Room newRoom, int cameraPosition)
    {
        orig(self, newRoom, cameraPosition);

    }

    private static void RoomCamera_ApplyFadeTexToPalleteTexture(On.RoomCamera.orig_ApplyFadeTexToPalleteTexture orig, RoomCamera self, Texture2D fadeTexture, float baseFade, float texFade, bool skipEffect, bool skipRipple)
    {
        orig(self, fadeTexture, baseFade, texFade, skipEffect, skipRipple);
        if (!RoomCameraCWT.TryGetData(self, out var data)) return;
        if (skipRipple) return;
        ScreenVariant screenVariant = data.screenVariantController?.current;
        if (screenVariant == null) return;

        for (int i = 0; i < 32; i++)
        {
            for (int j = 8; j < 16; j++)
            {
                int num = j - 8;
                if (!skipEffect || i < 30 || ((num < 2 || num > 5) && (num < 10 || num > 13)))
                {
                    Color pixel = screenVariant.paletteTexture.GetPixel(i, num);
                    pixel = Color.Lerp(pixel, new Color((pixel.r + pixel.g + pixel.b) / 3f, (pixel.r + pixel.g + pixel.b) / 3f, (pixel.r + pixel.g + pixel.b) / 3f), baseFade);
                    screenVariant.paletteTexture.SetPixel(i, num, ((Object)(object)fadeTexture == (Object)null) ? pixel : Color.Lerp(pixel, fadeTexture.GetPixel(i, j), texFade));
                    pixel = screenVariant.gameplayPaletteTexture.GetPixel(i, num);
                    pixel = Color.Lerp(pixel, new Color((pixel.r + pixel.g + pixel.b) / 3f, (pixel.r + pixel.g + pixel.b) / 3f, (pixel.r + pixel.g + pixel.b) / 3f), baseFade);
                    screenVariant.gameplayPaletteTexture.SetPixel(i, num, ((Object)(object)fadeTexture == (Object)null) ? pixel : Color.Lerp(pixel, fadeTexture.GetPixel(i, j), texFade));
                }
            }
        }
    }

    private static void RoomCamera_ApplyFade(On.RoomCamera.orig_ApplyFade orig, RoomCamera self)
    {
        orig(self);
        if (!RoomCameraCWT.TryGetData(self, out var data)) return;
        data.screenVariantController?.ApplyFade(self, self.fadeCoord);
    }

    private static void RoomCamera_ApplyEffectColorsToAllPaletteTextures(On.RoomCamera.orig_ApplyEffectColorsToAllPaletteTextures orig, RoomCamera self, int color1, int color2)
    {
        orig(self, color1, color2);
        if (!RoomCameraCWT.TryGetData(self, out var data)) return;
        data.screenVariantController?.ApplyEffectColors(self, color1, color2);
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