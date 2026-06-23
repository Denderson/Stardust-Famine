using lsfUtils.CWTs;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;
using Watcher;
using static lsfUtils.Plugin;
using Stardust.PlacedObjects;

namespace lsfUtils.DevtoolsObjects.RippleTunnel
{
    public class RippleTunnel : UpdatableAndDeletable, IDrawable
    {
        private readonly RippleTunnelData data;
        private readonly PlacedObject placedObject;

        public Vector2 pos => placedObject.pos;

        private WarpTear warpTear;

        public RippleTunnel(PlacedObject pObj, Room room)
        {
            placedObject = pObj;
            data = pObj.data as RippleTunnelData;
            base.room = room;
        }

        private int ActiveLayer => room.game.ActiveRippleLayer;
        private bool IsRelevantLayer => ActiveLayer == data.layerA || ActiveLayer == data.layerB;

        private int OtherLayer(int fromLayer) => fromLayer == data.layerA ? data.layerB : data.layerA;
        public override void Update(bool eu)
        {
            UpdateTear();
            if (IsRelevantLayer) UpdateTransport();
        }

        private void UpdateTear()
        {
            if (!room.BeingViewed || !IsRelevantLayer)
            {
                if (warpTear != null)
                {
                    warpTear.FadeOut(40f);
                    if (warpTear.fadeAnim <= 0f) warpTear = null;
                }
                return;
            }

            if (warpTear == null)
            {
                warpTear = new WarpTear(room, pos, branchingChance: 0.2f, rippleSide: true, seed: null, shaderOverride: null, spawnBigRift: false);
                room.AddObject(warpTear);
            }

            warpTear.FadeIn(120f);
            warpTear.openAnimation = 1f;
        }

        private void UpdateTransport()
        {
            for (int i = 0; i < room.physicalObjects.Length; i++)
            {
                for (int j = 0; j < room.physicalObjects[i].Count; j++)
                {
                    if (room.physicalObjects[i][j] is not Creature creature) continue;

                    int creatureLayer = creature.abstractCreature.rippleLayer;
                    if (creatureLayer != this.data.layerA && creatureLayer != this.data.layerB) continue;

                    if (!CreatureCWT.TryGetData(creature, out var data))
                    {
                        Log.LogMessage("Could get the creature CWT!");
                        continue;
                    }

                    if (data.rippleTunnelCooldown > 0)
                    {
                        data.rippleTunnelCooldown--;
                        continue;
                    }

                    if (Custom.DistLess(creature.firstChunk.pos, pos, this.data.radius.magnitude))
                    {
                        data.rippleTunnelTimer++;

                        if (data.rippleTunnelTimer >= this.data.transportFrames)
                        {
                            TransportCreature(creature, creatureLayer);
                        }
                    }
                    else
                    {
                        data.rippleTunnelTimer = 0;
                    }
                }
            }
        }

        private void TransportCreature(Creature creature, int fromLayer)
        {
            if (!CreatureCWT.TryGetData(creature, out var data))
            {
                Log.LogMessage("Could get the creature CWT!");
                return;
            }

            creature.ChangeRippleLayer(OtherLayer(fromLayer));

            room.PlaySound(WatcherEnums.WatcherSoundID.Ripple_Creature_Swap_Dimensions, creature.firstChunk.pos);
            room.AddObject(new ShockWave(creature.firstChunk.pos, 250f, 0.1f, 40));

            data.rippleTunnelTimer = 0;
            data.rippleTunnelCooldown = 800;
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) { }
        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) { }
        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) { }
        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer) { }
    }
}