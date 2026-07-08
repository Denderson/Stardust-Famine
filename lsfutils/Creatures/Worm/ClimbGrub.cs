using lsfUtils.CWTs;
using RWCustom;
using UnityEngine;

namespace lsfUtils.Creatures.Worm
{
    public class ClimbGrub : TubeWorm
    {
        private int grabHoldCounter;
        private const int SmashHoldThreshold = 40;

        public ClimbGrub(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
        {
            grabHoldCounter = 0;
        }

        public override void InitiateGraphicsModule()
        {
            if (graphicsModule == null)
            {
                graphicsModule = new ClimbGrubGraphics(this);
            }
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
            room.PlaySound(SoundID.Egg_Bug_Drop_Eggs, mainBodyChunk);
            if (!PlayerCWT.TryGetData(player, out var data)) return;
            data.freeClimbTimer = 400;

            Die();
            room.RemoveObject(this);
            abstractCreature.Destroy();
        }

        public override Color ShortCutColor()
        {
            return Color.Lerp(Color.white, new Color(1f, 0.55f, 0.75f), 0.35f);
        }
    }
}