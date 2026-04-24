using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Stardust.Anchors
{
    public class Anchor : UpdatableAndDeletable//, IDrawable
    {
        public PlacedObject placedObject;

        public AnchorData Data
        {
            get
            {
                if (placedObject?.data == null || placedObject.data is not AnchorData)
                {
                    throw new ArgumentException($"{nameof(PlacedObject)} was null or didn't contain a {nameof(AnchorData)} instance");
                }
                return placedObject.data as AnchorData;
            }
        }

        public readonly int firstBodySprite;

        public readonly int firstAttachmentSprite;

        public readonly int firstEyeSprite;

        public readonly int distortionSprite;

        public readonly int totalSprites;

        public Vector2 targetPos;

        public Vector2 targetDir;

        public Vector2 pos;

        public AnchorFade fadeOut;

        public int fadeOutCounter;

        public PositionedSoundEmitter voice;

        public float talking;

        public float lastTalking;

        public AnchorBehavior behavior;

        public float[] freqSamples = new float[64];

        public Anchor(PlacedObject placedObject, Room room)
        {
            this.room = room;

            // here should be the main part of Anchor code, including its graphics and stuff
        }

        public void AnchorMeetingFinished()
        {
            if (this?.room?.game?.GetStorySession?.saveState?.deathPersistentSaveData != null)
            {
                this.room.game.GetStorySession.saveState.deathPersistentSaveData.SetAnchorMeeting(this.Data.type);
            }
            // do the dissapearing code here
        }

        public void DeactivateAnchor()
        {
            this.Destroy();
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            if (base.slatedForDeletetion)
            {
                return;
            }
            if (placedObject != null)
            {
                targetPos = placedObject.pos;
                //targetDir = -(placedObject.data as PlacedObject.ResizableObjectData).handlePos.normalized;
            }
            /*behavior?.Update();
            if (fadeOutCounter == 0 && behavior != null && behavior.Vanish)
            {
                StartDeactivate();
            }*/
            if (voice != null)
            {
                voice.alive = true;
                voice.pos = pos;
                if (voice.slatedForDeletetion)
                {
                    voice = null;
                }
            }
            lastTalking = talking;
            talking = Mathf.MoveTowards(talking, (voice != null) ? 1f : 0f, 1f / 30f);
            foreach (AbstractCreature player2 in room.game.Players)
            {
                if (player2.realizedCreature is Player player && player.room == room && player.graphicsModule is PlayerGraphics playerGraphics)
                {
                    playerGraphics.LookAtPoint(pos, 10000f);
                }
            }
            if (fadeOutCounter > 0)
            {
                fadeOutCounter++;
            }
            if (fadeOutCounter > 80)
            {
                if (fadeOut == null)
                {
                    //fadeOut = new AnchorFade(pos);
                    //room.AddObject(fadeOut);
                }
                //fadeOut.setPos = body[0].pos;
                if (true) //fadeOut.RampingDown)
                {
                    DeactivateAnchor();
                }
            }
            // TODO: Graphics and moving code
        }


        /* void IDrawable.AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            if (newContatiner == null)
            {
                newContatiner = rCam.ReturnFContainer("Items");
            }
            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                sLeaser.sprites[i].RemoveFromContainer();
                if (i == DistortionSprite)
                {
                    rCam.ReturnFContainer("Bloom").AddChild(sLeaser.sprites[i]);
                }
                else if (i == LightSprite)
                {
                    rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[i]);
                }
                else if (i == FadeSprite)
                {
                    rCam.ReturnFContainer("Bloom").AddChild(sLeaser.sprites[i]);
                }
                else
                {
                    newContatiner.AddChild(sLeaser.sprites[i]);
                }
            }
        }

        void IDrawable.ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            
        }

        void IDrawable.DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            
        }

        void IDrawable.InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            
        } */
    }
}
