using UnityEngine;
using Watcher;

namespace lsfUtils.Creatures.Misc.ToxicSponge;

public class ToxicSponge : Barnacle
{
    public const int poisonMaxTimer = 120;
    public const float baseCloudRadius = 60f;

    public int poisonTimer;
    public int poisonCooldown;
    public PoisonCloud cloud;

    public ToxicSponge(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        poisonTimer = 0;
        poisonCooldown = 0;
    }

    public override void InitiateGraphicsModule()
    {
        graphicsModule ??= new BarnacleGraphics(this);
        graphicsModule.Reset();
    }

    public override void LoseAllGrasps()
    {
        ReleaseGrasp(0);
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (poisonCooldown > 0) poisonCooldown--;
    }

    public bool CanTriggerPoisonCloud()
    {
        if (room == null || dead) return false;
        if (poisonCooldown > 0) return false;
        if (poisonTimer > poisonMaxTimer / 2 && poisonTimer < poisonMaxTimer) return false;
        return true;
    }

    public void TriggerPoisonCloud(int duration, float size)
    {
        if (room == null) return;
        cloud?.Destroy();
        cloud = new PoisonCloud(this, baseCloudRadius * Mathf.Clamp(size, 0.25f, 3f), duration);
        room.AddObject(cloud);
    }
}