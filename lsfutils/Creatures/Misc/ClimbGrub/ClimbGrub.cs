using lsfUtils.CWTs;
using RWCustom;
using UnityEngine;

namespace lsfUtils.Creatures.Misc.ClimbGrub;

public class ClimbGrub : Creature
{
    public const int headIndex = 0;
    public const int bodyIndex = 1;
    public const int tailIndex = 2;
    public const int chunkCount = 3;
    
    public const float headSize = 4.5f;
    public const float bodySize = 7f;
    public const float tailSize = 3f;

    public const float headMass = 0.1f;
    public const float bodyMass = 0.15f;
    public const float tailMass = 0.05f;

    public const float headBodyDistance = 14f;
    public const float bodyTailDistance = 16f;
    
    public const float sqeezeSizeIncrease = 0.6f;
    
    public const float lookSpeed = 0.15f;

    public int grabHoldCounter;
    public const int smashHoldDuration = 20;

    public static Color yellowTintColor = new(0.75f, 0.95f, 0.25f);
    public static Color pinkStripeColor = new(1f, 0.55f, 0.75f);

    public const int freeClimbDuration = 40 * 20;
    public const int freeClimbDurationMax = 80 * 20;

    public ClimbGrubAI AI;

    public float squeezeProgress;
    public float squeezeScale = 1f;
    public float lastSqueezeScale = 1f;
    
    public Vector2 lookDirection;
    public Vector2 lastLookDirection;

    public ClimbGrub(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        grabHoldCounter = 0;

        bodyChunks = new BodyChunk[chunkCount];
        bodyChunks[headIndex] = new BodyChunk(this, headIndex, new Vector2(0f, 0f), headSize, headMass);
        bodyChunks[bodyIndex] = new BodyChunk(this, bodyIndex, new Vector2(0f, 0f), bodySize, bodyMass);
        bodyChunks[tailIndex] = new BodyChunk(this, tailIndex, new Vector2(0f, 0f), tailSize, tailMass);

        bodyChunkConnections =
        [
            new BodyChunkConnection(bodyChunks[headIndex], bodyChunks[bodyIndex], headBodyDistance, BodyChunkConnection.Type.Normal, 1f, 0.5f),
            new BodyChunkConnection(bodyChunks[bodyIndex], bodyChunks[tailIndex], bodyTailDistance, BodyChunkConnection.Type.Normal, 1f, 0.5f)
        ];
        
        airFriction = 0.999f;
        gravity = 0.9f;
        bounce = 0.1f;
        surfaceFriction = 0.4f;
        collisionLayer = 1;
        waterFriction = 0.96f;
        buoyancy = 0.95f;
    }

    public override void InitiateGraphicsModule()
    {
        graphicsModule ??= new ClimbGrubGraphics(this);
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        if (room == null)
        {
            grabHoldCounter = 0;
            squeezeProgress = 0f;
            UpdateSqueezeSize();
            return;
        }

        AI?.Update();
        UpdateLook();

        Player holder = null;
        for (int i = 0; i < grabbedBy.Count; i++)
        {
            if (grabbedBy[i].grabber is Player p)
            {
                holder = p;
                break;
            }
        }

        if (holder != null && holder.input[0].pckp)
        {
            grabHoldCounter++;
            (holder.graphicsModule as PlayerGraphics).LookAtObject(this);
            if (grabHoldCounter >= smashHoldDuration)
            {
                Smash(holder);
                return;
            }
        }
        else
        {
            grabHoldCounter = 0;
        }

        squeezeProgress = Mathf.Clamp01((float)grabHoldCounter / smashHoldDuration);
        UpdateSqueezeSize();
    }

    public void UpdateLook()
    {
        lastLookDirection = lookDirection;
        Vector2 targetDirection = Vector2.zero;
        if (graphicsModule is ClimbGrubGraphics graphics && graphics?.looker?.lookCreature?.representedCreature?.realizedCreature != null)
        {
            targetDirection = Custom.DirVec(bodyChunks[headIndex].pos, graphics.looker.lookCreature.representedCreature.realizedCreature.mainBodyChunk.pos);
        }
        lookDirection = Vector2.Lerp(lookDirection, targetDirection, lookSpeed);
    }

    public void UpdateSqueezeSize()
    {
        lastSqueezeScale = squeezeScale;

        float targetScale = 1f + squeezeProgress * sqeezeSizeIncrease;
        squeezeScale = Mathf.Lerp(squeezeScale, targetScale, 0.5f);

        bodyChunks[headIndex].rad = headSize * squeezeScale;
        bodyChunks[bodyIndex].rad = bodySize / squeezeScale;
        bodyChunks[tailIndex].rad = tailSize * squeezeScale;

        if (squeezeProgress >= 0.7f &&  squeezeProgress <= 0.8f)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = Custom.RNV() * Mathf.Lerp(4f, 8f, Random.value);
                room.AddObject(new Spark(mainBodyChunk.pos, vel, Color.Lerp(yellowTintColor, Color.white, Random.value), null, 14, 24));
            }
        }
    }

    public void Smash(Player player)
    {
        if (!PlayerCWT.TryGetData(player, out var data)) return;

        room.PlaySound(SoundID.Egg_Bug_Drop_Eggs, mainBodyChunk);

        for (int i = 0; i < 20; i++)
        {
            Vector2 vel = Custom.RNV() * Mathf.Lerp(4f, 8f, Random.value);
            room.AddObject(new Spark(mainBodyChunk.pos, vel, Color.Lerp(yellowTintColor, Color.white, Random.value), null, 14, 24));
        }

        data.freeClimbTimer += freeClimbDuration;
        if (data.freeClimbTimer > freeClimbDurationMax) data.freeClimbTimer = freeClimbDurationMax;

        player.AddMud(freeClimbDuration / 3, freeClimbDurationMax, yellowTintColor);
        player.AddMud(freeClimbDuration / 3, freeClimbDurationMax, yellowTintColor);
        player.AddMud(freeClimbDuration / 3, freeClimbDurationMax, yellowTintColor);

        Die();
        room.RemoveObject(this);
        abstractCreature.Destroy();
    }

    public override Color ShortCutColor()
    {
        return Color.Lerp(Color.white, pinkStripeColor, 0.35f);
    }
}