using lsfUtils.CWTs;
using RWCustom;
using UnityEngine;

namespace lsfUtils.Creatures.Worm
{
    public class ClimbGrub : TubeWorm
    {
        private int grabHoldCounter;
        private const int SmashHoldThreshold = 40;

        public static Color yellowTintColor = new(0.75f, 0.95f, 0.25f);
        public static Color pinkStripeColor = new(1f, 0.55f, 0.75f);

        public const int freeClimbDuration = 40 * 10;
        public const int freeClimbDurationMax = 80 * 10;

        public ClimbGrub(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
        {
            grabHoldCounter = 0;
        }

        public override void InitiateGraphicsModule()
        {
            graphicsModule ??= new ClimbGrubGraphics(this);
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            if (room == null || dead)
            {
                grabHoldCounter = 0;
                return;
            }

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
                if (grabHoldCounter >= SmashHoldThreshold)
                {
                    Smash(holder);
                    return;
                }
            }
            else
            {
                grabHoldCounter = 0;
            }
        }

        private void Smash(Player player)
        {
            if (!PlayerCWT.TryGetData(player, out var data)) return;

            room.PlaySound(SoundID.Egg_Bug_Drop_Eggs, mainBodyChunk);

            for (int i = 0; i < 20; i++)
            {
                Vector2 vel = Custom.RNV() * Mathf.Lerp(4f, 8f, UnityEngine.Random.value);
                room.AddObject(new Spark(mainBodyChunk.pos, vel, Color.Lerp(yellowTintColor, Color.white, UnityEngine.Random.value), null, 14, 24));
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
}