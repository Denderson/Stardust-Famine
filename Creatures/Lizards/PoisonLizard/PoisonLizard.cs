using UnityEngine;
using Watcher;
using RWCustom;

namespace lsfUtils.Creatures.Lizards.PoisonLizard;

public class PoisonLizard : Lizard
{
    public WorldCoordinate? lurkPos;
    public AbstractCreature trackedCreature;
    public int trackTimeRemaining;
    public int holdCounter;
    public PoisonLizard(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        Debug.Log("Poison Lizard ctor: ");
        var state = Random.state;
        Random.InitState(abstractCreature.ID.RandomSeed);
        effectColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(120 / 360f, 10 / 360f, .5f), .55f, Custom.ClampedRandomVariation(.30f, .05f, .5f));
        Random.state = state;
        lurkPos = null;
        trackedCreature = null;
        trackTimeRemaining = 0;
        holdCounter = 0;
    }

    public override void InitiateGraphicsModule() => graphicsModule ??= new PoisonLizardGraphics(this);

    public override void LoseAllGrasps() => ReleaseGrasp(0);

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (grasps != null && grasps.Length > 0 && grasps[0] != null && grasps[0].grabbed is Creature creature)
        {
            holdCounter++;
            if (holdCounter == 1)
            {
                EnterAnimation(Animation.ShakePrey, true);
                timeToRemainInAnimation = 40;
            }
            if (holdCounter > 40)
            {
                holdCounter = 0;
                LoseAllGrasps();
                creature.Stun(60);
                for (int i = 0; i < 6; i++)
                {
                    Vector2 vector = Custom.RNV();
                    room.AddObject(new Spark(firstChunk.pos + vector * 40f, vector * Mathf.Lerp(4f, 30f, Random.value), effectColor, null, 8, 24));
                }
                for (int j = 0; j < 3; j++)
                {
                    room.AddObject(new LizardBubble(graphicsModule as LizardGraphics, 0f, 1f, 10f));
                }
                foreach (BodyChunk chunk in creature.bodyChunks)
                {
                    chunk.vel += Custom.DirVec(bodyChunks[1].pos, bodyChunks[0].pos) * 15;
                }
                JawOpen = 0.5f;
                room.PlaySound(SoundID.Red_Lizard_Spit, firstChunk.pos, abstractCreature);
            }
        }
    }
}
