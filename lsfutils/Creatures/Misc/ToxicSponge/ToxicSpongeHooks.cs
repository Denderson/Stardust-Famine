using RWCustom;
using UnityEngine;
using Watcher;

namespace lsfUtils.Creatures.Misc.ToxicSponge;

public static class ToxicSpongeHooks
{
    public static void ApplyHooks()
    {
        On.Watcher.Barnacle.TryJump += Barnacle_TryJump;

        On.Watcher.BarnacleAI.CanBeALeader += BarnacleAI_CanBeALeader;
        On.Watcher.BarnacleAI.FollowLeader += BarnacleAI_FollowLeader;
        On.Watcher.BarnacleAI.LeadingUpdate += BarnacleAI_LeadingUpdate;
        On.Watcher.BarnacleAI.DetermineBehavior += BarnacleAI_DetermineBehavior;
        On.Watcher.BarnacleAI.Act += BarnacleAI_Act;
    }

    public static BarnacleAI.Behavior BarnacleAI_DetermineBehavior(On.Watcher.BarnacleAI.orig_DetermineBehavior orig, BarnacleAI self)
    {
        var value = orig(self);
        if (self is not ToxicSpongeAI) return value;
        if (value == BarnacleAI.Behavior.Flee) return BarnacleAI.Behavior.Idle;
        return value;
    }

    public static void BarnacleAI_Act(On.Watcher.BarnacleAI.orig_Act orig, BarnacleAI self)
    {
        orig(self);
        if (self is not ToxicSpongeAI) return;
        if (self.creature?.realizedCreature?.firstChunk == null) return;
        Creature threat = self.threatTracker?.mostThreateningCreature?.representedCreature?.realizedCreature;
        if (threat == null) return;
        if (self.threatTracker.currentThreat > 0.3f && Custom.DistLess(threat.DangerPos, self.creature.realizedCreature.firstChunk.pos, 10f)) self.runSpeed = 0.1f;
    }

    public static void BarnacleAI_LeadingUpdate(On.Watcher.BarnacleAI.orig_LeadingUpdate orig, BarnacleAI self)
    {
        orig(self);
        if (self is ToxicSpongeAI)
        {
            self.AbandonLeader();
            return;
        }
        for (int i = self.leading.Count - 1; i >= 0; i--)
        {
            if (self.leading[i] is ToxicSponge sponge) sponge.AI.AbandonLeader();
        }
    }

    public static bool BarnacleAI_CanBeALeader(On.Watcher.BarnacleAI.orig_CanBeALeader orig, BarnacleAI self, Barnacle b)
    {
        if (self is ToxicSpongeAI || b is ToxicSponge) return false;
        return orig(self, b);
    }

    public static void BarnacleAI_FollowLeader(On.Watcher.BarnacleAI.orig_FollowLeader orig, BarnacleAI self)
    {
        if (self is ToxicSpongeAI) return;
        orig(self);
    }

    public static void Barnacle_TryJump(On.Watcher.Barnacle.orig_TryJump orig, Barnacle self, Vector2 towardsPoint)
    {
        if (self is ToxicSponge)
        {
            self.delayBeforeJumping = 0;
            return;
        }
        orig(self, towardsPoint);
    }
}