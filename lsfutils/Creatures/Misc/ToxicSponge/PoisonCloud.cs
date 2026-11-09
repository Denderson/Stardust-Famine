using lsfUtils.CWTs;
using RWCustom;
using System.Collections.Generic;
using UnityEngine;

namespace lsfUtils.Creatures.Misc.ToxicSponge;

public class PoisonCloud : CosmeticSprite
{
    public struct Particle
    {
        public Vector2 pos;
        public Vector2 lastPos;
        public Vector2 vel;
        public int age;
        public int lifeTime;
        public float size;
        public float flickerPhase;
        public Color color;

        public bool Alive => age < lifeTime;
        public float Progress => lifeTime > 0 ? age / (float)lifeTime : 1f;

        public Particle(Vector2 pos, Vector2 vel, int lifeTime, float size, float flickerPhase, Color color)
        {
            this.pos = pos;
            lastPos = pos;
            this.vel = vel;
            age = 0;
            this.lifeTime = lifeTime;
            this.size = size;
            this.flickerPhase = flickerPhase;
            this.color = color;
        }
    }

    public const float poisonSpeed = 1f / 120f;
    public const int minParticles = 20;
    public const int maxParticles = 250;
    public const float minLifeMultiplier = 0.6f;
    public const float maxLifeFraction = 1f;
    public const float wallBounce = 0.3f;
    public const float particleAlpha = 0.7f;
    public const float floatPhaseStart = 0.7f;
    public const float floatAcceleration = 0.08f;
    public const float endSizeMultiplier = 0.2f;
    public static Color particleDarkColor = new(0.35f, 0.7f, 0.1f);
    public static Color particleLightColor = new(0.65f, 1f, 0.3f);

    public float radius;
    public float poisonRadius;
    public int life;
    public int duration;

    public List<Particle> particles;

    public PoisonCloud(Vector2 pos, float radius, int duration)
    {
        this.radius = radius;
        this.duration = duration;
        this.life = duration;
        this.pos = pos;
        lastPos = pos;

        int particleCount = Mathf.Clamp(Mathf.RoundToInt(Mathf.PI * radius * radius / 100f), minParticles, maxParticles);
        particles = new List<Particle>(particleCount);
        for (int i = 0; i < particleCount; i++)
        {
            particles.Add(CreateParticle(pos));
        }
    }

    private Particle CreateParticle(Vector2 emitPos)
    {
        Vector2 vel = Random.insideUnitCircle.normalized * radius * Mathf.Sqrt(Random.value) * 0.1f;
        int lifetime = Mathf.Max(1, Mathf.RoundToInt(duration * Random.Range(minLifeMultiplier, maxLifeFraction)));

        return new Particle(emitPos, vel, lifetime, Random.value + 2f, Random.value * Mathf.PI * 2f, Color.Lerp(particleDarkColor, particleLightColor, Random.value));
    }

    public bool ShouldPoison(Creature creature, out float precentage)
    {
        precentage = 0f;
        int chunkNum = 0;
        if (creature == null) return false;
        if (creature is ToxicSponge) return false;
        if (creature.dead) return false;
        if (creature.bodyChunks == null || creature.bodyChunks.Length <= 0) return false;
        foreach (BodyChunk chunk in creature.bodyChunks)
        {
            if (chunk?.pos == null) continue;
            if (Custom.DistLess(chunk.pos, pos, poisonRadius + chunk.rad)) chunkNum++;
        }
        precentage = (float)chunkNum / creature.bodyChunks.Length;
        return (chunkNum > 0);
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (room == null) return;

        UpdateParticles();

        for (int i = 0; i < room.physicalObjects.Length; i++)
        {
            for (int j = 0; j < room.physicalObjects[i].Count; j++)
            {
                if (room.physicalObjects[i][j] is Creature creature && ShouldPoison(creature, out var precentage) && CreatureCWT.TryGetData(creature, out var data))
                {
                    data.temporaryPoison = Mathf.Min(1f, data.temporaryPoison + (poisonSpeed * precentage));
                    data.cannotRecoverPoison = 80;
                    if (Random.value < 0.005f) creature.Stun(10);
                }
            }
        }

        life--;
        if (life < 0) Destroy();
    }

    private void UpdateParticles()
    {
        float maxDist = 0f;

        for (int i = 0; i < particles.Count; i++)
        {
            Particle particle = particles[i];
            particle.lastPos = particle.pos;
            particle.age++;

            if (!particle.Alive)
            {
                particles[i] = particle;
                continue;
            }

            float floatProgress = Mathf.InverseLerp(floatPhaseStart, 1f, particle.Progress);
            particle.vel.y += floatAcceleration * floatProgress;

            particle.vel += Random.insideUnitCircle * 0.1f;
            particle.pos += particle.vel;
            particle.vel *= 0.92f;

            BounceOffWalls(ref particle);
            particles[i] = particle;

            if (particle.Alive)
            {
                float dist = (particle.pos - pos).sqrMagnitude;
                if (dist > maxDist) maxDist = dist;
            }
        }
        poisonRadius = Mathf.Min(Mathf.Sqrt(maxDist), radius);
    }

    private void BounceOffWalls(ref Particle particle)
    {
        if (!room.GetTile(particle.pos).Solid || room.GetTile(particle.lastPos).Solid) return;

        IntVector2? hit = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(room, room.GetTilePosition(particle.lastPos), room.GetTilePosition(particle.pos));
        if (!hit.HasValue)
        {
            particle.pos = particle.lastPos;
            particle.vel *= -wallBounce;
            return;
        }

        FloatRect rect = Custom.RectCollision(particle.pos, particle.lastPos, room.TileRect(hit.Value).Grow(2f));
        particle.pos = rect.GetCorner(FloatRect.CornerLabel.D);

        float bounceX = rect.GetCorner(FloatRect.CornerLabel.B).x;
        if (bounceX < 0f) particle.vel.x = Mathf.Abs(particle.vel.x);
        else if (bounceX > 0f) particle.vel.x = -Mathf.Abs(particle.vel.x);

        float bounceY = rect.GetCorner(FloatRect.CornerLabel.B).y;
        if (bounceY < 0f) particle.vel.y = Mathf.Abs(particle.vel.y);
        else if (bounceY > 0f) particle.vel.y = -Mathf.Abs(particle.vel.y);

        particle.vel *= wallBounce;
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[particles.Count];

        for (int i = 0; i < particles.Count; i++)
        {
            sLeaser.sprites[i] = new FSprite("pixel")
            {
                color = particles[i].color,
                scale = particles[i].size,
                isVisible = false
            };
        }
        AddToContainer(sLeaser, rCam, null);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (particles.Count == 0 || slatedForDeletetion || room == null) return;

        for (int i = 0; i < particles.Count && i < sLeaser.sprites.Length; i++)
        {
            Particle particle = particles[i];
            FSprite sprite = sLeaser.sprites[i];

            float floatProgress = Mathf.InverseLerp(floatPhaseStart, 1f, particle.Progress);

            Vector2 drawPos = Vector2.Lerp(particle.lastPos, particle.pos, timeStacker);
            sprite.x = drawPos.x - camPos.x;
            sprite.y = drawPos.y - camPos.y;
            sprite.color = particle.color;
            sprite.scale = particle.size * Mathf.Lerp(1f, endSizeMultiplier, floatProgress);

            float flicker = 0.6f + 0.4f * Mathf.Sin(particle.age * 0.15f + particle.flickerPhase);
            sprite.alpha = particleAlpha * Mathf.Lerp(1f, 0f, floatProgress) * flicker;
            sprite.isVisible = true;
        }

        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        newContatiner ??= rCam.ReturnFContainer("Midground");
        foreach (FSprite sprite in sLeaser.sprites)
        {
            sprite.RemoveFromContainer();
            newContatiner.AddChild(sprite);
        }
    }
}