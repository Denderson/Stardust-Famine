using System;
using System.Collections.Generic;

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
}