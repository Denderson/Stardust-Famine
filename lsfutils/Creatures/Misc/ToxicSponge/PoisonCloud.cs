using lsfUtils.CWTs;
using RWCustom;
using System.Drawing;
using UnityEngine;
using Color = UnityEngine.Color;

namespace lsfUtils.Creatures.Misc.ToxicSponge;

public class PoisonCloud : CosmeticSprite
{
    public const float poisonSpeed = 1f / 80f;

    public ToxicSponge owner;
    public float radius;
    public int life;
    public int maxLife;
    public bool followsOwner = true;

    public PoisonCloud(ToxicSponge owner, float radius, int duration)
    {
        this.owner = owner;
        this.radius = radius;
        life = maxLife = Mathf.Max(1, duration);
        pos = lastPos = owner.firstChunk.pos;
    }

    public bool CollidesWith(Creature creature, out float precentage)
    {
        precentage = 0f;
        int chunkNum = 0;
        if (creature == null) return false;
        if (creature.bodyChunks == null || creature.bodyChunks.Length <= 0) return false;
        foreach (BodyChunk chunk in creature.bodyChunks)
        {
            if (chunk?.pos == null) continue;
            if (Custom.DistLess(chunk.pos, pos, radius)) chunkNum++;
        }
        precentage = chunkNum / creature.bodyChunks.Length;
        return (chunkNum > 0);
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (room == null) return;

        if (followsOwner && owner?.room != null && !owner.slatedForDeletetion && owner.room == room)
        {
            pos = owner.firstChunk.pos;
        }

        for (int i = 0; i < room.physicalObjects.Length; i++)
        {
            for (int j = 0; j < room.physicalObjects[i].Count; j++)
            {
                if (room.physicalObjects[i][j] is Creature creature && CollidesWith(creature, out var precentage) && creature != owner && CreatureCWT.TryGetData(creature, out var data))
                {
                    data.temporaryPoison = Mathf.Min(1f, data.temporaryPoison + (poisonSpeed * precentage));
                    data.cannotRecoverPoison = 120;
                }
            }
        }

        life--;
        if (life < 0) Destroy();
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];
        sLeaser.sprites[0] = new FSprite("Futile_White")
        {
            shader = rCam.game.rainWorld.Shaders["FlatLight"],
            color = new Color(0.55f, 0.9f, 0.2f)
        };
        AddToContainer(sLeaser, rCam, null);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        FSprite s = sLeaser.sprites[0];
        Vector2 p = Vector2.Lerp(lastPos, pos, timeStacker);
        s.x = p.x - camPos.x;
        s.y = p.y - camPos.y;
        s.scale = radius / 8f;
        s.alpha = 0.4f * Mathf.Clamp01(life / (float)maxLife);
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