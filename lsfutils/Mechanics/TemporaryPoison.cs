using lsfUtils.CWTs;
using UnityEngine;

namespace lsfUtils.Mechanics;

public static class TemporaryPoison
{
    public static float recoverySpeed = 1f / 400f;
    public static void ApplyHooks()
    {
        On.Creature.Update += Creature_Update;
    }
    public static void Creature_Update(On.Creature.orig_Update orig, Creature self, bool eu)
    {
        if (!CreatureCWT.TryGetData(self, out var data))
        {
            orig(self, eu);
            return;
        }
        float oldPoison = self.injectedPoison;
        self.injectedPoison = Mathf.Min(1f, self.injectedPoison + data.temporaryPoison);
        try
        {
            orig(self, eu);
        }
        finally
        {
            self.injectedPoison = oldPoison;
        }
        if (data.cannotRecoverPoison > 0) data.cannotRecoverPoison--;
        else if (data.temporaryPoison > 0) data.temporaryPoison = Mathf.Max(data.temporaryPoison - recoverySpeed, 0);
    }
}