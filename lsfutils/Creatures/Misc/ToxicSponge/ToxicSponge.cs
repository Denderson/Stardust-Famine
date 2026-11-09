using RWCustom;
using UnityEngine;
using Watcher;

namespace lsfUtils.Creatures.Misc.ToxicSponge;

public class ToxicSponge : Barnacle
{
    public const int poisonMaxTimer = 120;
    public const int poisonCooldownTime = 120;
    public const int poisonCloudDuration = 120;
    public const int spawnCooldown = 60;
    public const float cloudRange = 90f;
    public const float dangerRange = cloudRange * 1.5f;
    public const float cloudMultiplierOnCollision = 0.7f;
    public const float cloudMultiplierOnDamage = 1f;
    public const float cloudMultiplierOnDeath = 0.4f;
    public const float maxCloudSize = 3f;

    public int poisonBuildup;
    public int poisonCooldown;

    public ToxicSponge(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        poisonBuildup = 0;
        poisonCooldown = spawnCooldown;
    }

    public override void InitiateGraphicsModule()
    {
        graphicsModule ??= new ToxicSpongeGraphics(this);
        graphicsModule.Reset();
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        if (room == null) return;
        if (poisonCooldown > 0) poisonCooldown--;

        if (Consious && poisonCooldown <= 0 && IsDangerNearby()) TickBuildup();
        else if (poisonBuildup > 0) poisonBuildup = Mathf.Max(0, poisonBuildup - 2);
    }

    public bool IsDangerNearby()
    {
        if (room?.abstractRoom == null) return false;
        foreach (AbstractCreature other in room.abstractRoom.creatures)
        {
            Creature creature = other.realizedCreature;
            if (creature == null || creature is ToxicSponge) continue;
            if (creature.dead || creature.room != room) continue;
            if (Custom.DistLess(mainBodyChunk.pos, creature.mainBodyChunk.pos, dangerRange)) return true;
        }
        return false;
    }

    public bool CanTriggerPoisonCloud()
    {
        if (room == null || dead) return false;
        if (poisonCooldown > 0) return false;
        if (poisonBuildup > poisonMaxTimer / 2 && poisonBuildup < poisonMaxTimer) return false;
        return true;
    }

    public void TickBuildup()
    {
        if (poisonCooldown > 0) return;
        poisonBuildup++;
        if (poisonBuildup >= poisonMaxTimer)
        {
            poisonCooldown = poisonCooldownTime;
            poisonBuildup = 0;
            TriggerPoisonCloud(poisonCloudDuration, 1.5f);
        }
    }

    public void TriggerPoisonCloud(int duration, float size)
    {
        if (room == null) return;
        size = Mathf.Clamp(size, 0.5f, maxCloudSize);
        PoisonCloud cloud = new(firstChunk.pos, cloudRange * size, duration);
        room.AddObject(cloud);
    }

    public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppendage, DamageType type, float damage, float stunBonus)
    {
        base.Violence(source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
        if (!CanTriggerPoisonCloud()) return;

        float violentTriggerFactor = 8f * (damage + stunBonus);
        TriggerPoisonCloud((int)(poisonCloudDuration * cloudMultiplierOnDamage), violentTriggerFactor * cloudMultiplierOnDamage / 25f);
        poisonBuildup = 0;
        poisonCooldown = (int)violentTriggerFactor;
        Stun(Mathf.Clamp((int)violentTriggerFactor, 40, 120));
        if (directionAndMomentum != null && hitChunk != null) hitChunk.vel += directionAndMomentum.Value * violentTriggerFactor / 40f;
    }

    public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
    {
        base.TerrainImpact(chunk, direction, speed, firstContact);
        if (!firstContact || speed <= 12f || !CanTriggerPoisonCloud()) return;

        float violentTriggerFactor = 8f * speed;
        TriggerPoisonCloud((int)(poisonCloudDuration * cloudMultiplierOnCollision), violentTriggerFactor * cloudMultiplierOnCollision / 25f);
        poisonBuildup = 0;
        poisonCooldown += (int)violentTriggerFactor;
        Stun(Mathf.Clamp((int)violentTriggerFactor, 40, 120));
        bodyChunks[chunk].vel -= new Vector2(direction.x, direction.y) * violentTriggerFactor / 40f;
    }

    public override void Die()
    {
        if (!dead && CanTriggerPoisonCloud())
        {
            TriggerPoisonCloud((int)(poisonCloudDuration * cloudMultiplierOnDeath), cloudMultiplierOnDeath);
        }
        base.Die();
    }

    public override void Destroy()
    {
        base.Destroy();
    }
}