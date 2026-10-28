using UnityEngine;
using Watcher;

namespace lsfUtils.Creatures.Misc.ToxicSponge;

public static class ToxicSpongeHooks
{
    public static void ApplyHooks()
    {
        On.Watcher.Barnacle.Collide += Barnacle_Collide;
        On.Watcher.Barnacle.Die += Barnacle_Die;
        On.Watcher.Barnacle.TerrainImpact += Barnacle_TerrainImpact;
        On.Watcher.Barnacle.TryJump += Barnacle_TryJump;
        On.Watcher.Barnacle.Violence += Barnacle_Violence;

        On.Watcher.BarnacleAI.CanBeALeader += BarnacleAI_CanBeALeader;
        On.Watcher.BarnacleAI.FollowLeader += BarnacleAI_FollowLeader;
        On.Watcher.BarnacleAI.LeadingUpdate += BarnacleAI_LeadingUpdate;

    }
    private static void BarnacleAI_LeadingUpdate(On.Watcher.BarnacleAI.orig_LeadingUpdate orig, BarnacleAI self)
    {
        orig(self);
        if (self?.realizedCreature is ToxicSponge)
        {
            self.AbandonLeader();
            return;
        }
        if (self == null) return;
        for (int i = self.leading.Count - 1; i >= 0; i--)
        {
            if (self.leading[i] is ToxicSponge s) s.AI.AbandonLeader();
        }
    }

    private static bool BarnacleAI_CanBeALeader(On.Watcher.BarnacleAI.orig_CanBeALeader orig, BarnacleAI self, Barnacle b)
    {
        if (self?.realizedCreature is ToxicSponge || b is ToxicSponge) return false;
        return orig(self, b);
    }

    private static void BarnacleAI_FollowLeader(On.Watcher.BarnacleAI.orig_FollowLeader orig, BarnacleAI self)
    {
        if (self?.creature?.realizedCreature is ToxicSponge) return;
        orig(self);
    }

    private static void Barnacle_Violence(On.Watcher.Barnacle.orig_Violence orig, Barnacle self, BodyChunk source, UnityEngine.Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos onAppendagePos, Creature.DamageType type, float damage, float stunBonus)
    {
        orig(self, source, directionAndMomentum, hitChunk, onAppendagePos, type, damage, stunBonus);
        if (self is ToxicSponge sponge && sponge.CanTriggerPoisonCloud())
        {
            float violentTriggerFactor = Mathf.Min(40 * damage, 200f);
            sponge.TriggerPoisonCloud(40, damage);
            sponge.poisonCooldown = (int)violentTriggerFactor;
            if (violentTriggerFactor > 10f)
            {
                sponge.Stun((int)violentTriggerFactor);
                if (directionAndMomentum != null && hitChunk != null)
                {
                    hitChunk.vel += new UnityEngine.Vector2(directionAndMomentum.Value.x * violentTriggerFactor / 10, directionAndMomentum.Value.y * violentTriggerFactor / 10);
                }
            }
        }
    }

    private static void Barnacle_TryJump(On.Watcher.Barnacle.orig_TryJump orig, Barnacle self, UnityEngine.Vector2 towardsPoint)
    {
        if (self is ToxicSponge)
        {
            self.delayBeforeJumping = 0;
            return;
        }
        orig(self, towardsPoint);
    }

    private static void Barnacle_TerrainImpact(On.Watcher.Barnacle.orig_TerrainImpact orig, Barnacle self, int chunk, RWCustom.IntVector2 direction, float speed, bool firstContact)
    {
        orig(self, chunk, direction, speed, firstContact);
        if (firstContact && self is ToxicSponge sponge && sponge.CanTriggerPoisonCloud() && speed > 2f)
        {
            float violentTriggerFactor = Mathf.Min(40 * speed, 200f);
            sponge.TriggerPoisonCloud(40, speed);
            sponge.poisonCooldown = (int)violentTriggerFactor;
            if (violentTriggerFactor > 10f)
            {
                sponge.Stun((int)violentTriggerFactor);
                self.bodyChunks[chunk].vel -= new Vector2(direction.x * violentTriggerFactor / 10, direction.y * violentTriggerFactor / 10);
            }
        }
    }

    private static void Barnacle_Die(On.Watcher.Barnacle.orig_Die orig, Barnacle self)
    {
        if (self is ToxicSponge sponge && sponge.CanTriggerPoisonCloud())
        {
            sponge.TriggerPoisonCloud(40, 0.7f);
            sponge.poisonCooldown = 80;
        }
        orig(self);
    }

    private static void Barnacle_Collide(On.Watcher.Barnacle.orig_Collide orig, Barnacle self, PhysicalObject otherObject, int myChunk, int otherChunk)
    {
        orig(self, otherObject, myChunk, otherChunk);
        if (self is ToxicSponge sponge && sponge.CanTriggerPoisonCloud() && otherObject is Creature && otherObject is not Barnacle)
        {
            sponge.TriggerPoisonCloud(40, 0.5f);
            sponge.poisonCooldown = 40;
        }
    }
}
