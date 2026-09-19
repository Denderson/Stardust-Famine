using System;
using System.Collections.Generic;
using UnityEngine;

namespace lsfUtils.ScreenVariants;

public class ScreenVariantController
{
    public RoomCamera camera;
    public Dictionary<Type, ScreenVariant> variants = [];

    public ScreenVariant current;
    public ScreenVariant previous;

    public ScreenVariantController(RoomCamera camera)
    {
        this.camera = camera;
    }

    public T GetVariant<T>() where T : ScreenVariant
    {
        if (!variants.TryGetValue(typeof(T), out var variant))
        {
            variant = (ScreenVariant)Activator.CreateInstance(typeof(T), camera);
            variants[typeof(T)] = variant;
        }
        return (T)variant;
    }

    public void SwitchTo<T>() where T : ScreenVariant
    {
        var target = GetVariant<T>();

        if (target == current) return;

        previous?.Dispose();
        previous = current;
        current = target;
        previous?.Deactivate();
        current.Activate();
    }

    public void Clear()
    {
        if (current == null) return;

        previous?.Dispose();
        previous = current;
        current = null;
        previous?.Deactivate();
    }

    public void DrawUpdate(float timeStacker)
    {
        current?.DrawUpdate(timeStacker);

        if (previous == null) return;

        previous.DrawUpdate(timeStacker);
        if (!previous.IsVisible)
        {
            previous.Dispose();
            previous = null;
        }
    }

    public void Dispose()
    {
        foreach (var variant in variants.Values)
        {
            variant.Dispose();
        }
        variants.Clear();
        current = null;
        previous = null;
    }

    public void ApplyEffectColors(RoomCamera camera, int color1, int color2)
    {
        if (current?.paletteTexture == null) return;
        camera.ApplyEffectColorsToPaletteTexture(ref current.paletteTexture, color1, color2);
    }

    public void ApplyFade(RoomCamera camera, Vector2 fadeCoord)
    {
        if (current == null) return;

        if (current.fadeTexture == null)
        {
            current.LoadFadeTex();
        }
        if (current.fadeTexture != null)
        {
            current.EnsureOverrideTexture();
            for (int i = 0; i < 32; i++)
            {
                for (int j = 8; j < 16; j++)
                {
                    current.paletteTexture.SetPixel(i, j - 8, Color.Lerp(
                        current.fadeTexture.GetPixel(i, j),
                        current.fadeTexture.GetPixel(i, j - 8),
                        fadeCoord.y));
                }
            }
            current.paletteTexture.Apply(false);
        }

        if (current.gameplayFadeTexture == null)
        {
            current.LoadGameplayFadeTex();
        }
        if (current.gameplayFadeTexture != null)
        {
            current.EnsureGameplayOverrideTexture();
            for (int i = 0; i < 32; i++)
            {
                for (int j = 8; j < 16; j++)
                {
                    current.gameplayPaletteTexture.SetPixel(i, j - 8, Color.Lerp(
                        current.gameplayFadeTexture.GetPixel(i, j),
                        current.gameplayFadeTexture.GetPixel(i, j - 8),
                        fadeCoord.y * 0.6f));
                }
            }
            current.gameplayPaletteTexture.Apply(false);
        }
    }
}