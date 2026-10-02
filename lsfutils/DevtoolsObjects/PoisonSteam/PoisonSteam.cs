using lsfUtils.CWTs;
using RWCustom;
using Unity.Mathematics;
using UnityEngine;

namespace lsfUtils.DevtoolsObjects.PoisonSteam
{
    public class PoisonSteam : HarmfulSteam
    {
        private readonly PlacedObject myPObj;

        public PoisonSteam(PlacedObject pObj, Room room) : base(pObj, room)
        {
            myPObj = pObj;
            this.room = room;
        }
        public override void Update(bool eu)
        {
            base.Update(eu);

            for (int i = 0; i < this.steam.particles.Count; i++)
            {
                if (this.steam.particles[i].life > this.dangerRange)
                {
                    for (int j = 0; j < this.room.physicalObjects.Length; j++)
                    {
                        for (int k = 0; k < this.room.physicalObjects[j].Count; k++)
                        {
                            for (int l = 0; l < this.room.physicalObjects[j][k].bodyChunks.Length; l++)
                            {
                                Vector2 a = this.room.physicalObjects[j][k].bodyChunks[l].ContactPoint.ToVector2();
                                Vector2 b = this.room.physicalObjects[j][k].bodyChunks[l].pos + a * (this.room.physicalObjects[j][k].bodyChunks[l].rad + 30f);
                                if (CreatureCWT.TryGetData(this.room.physicalObjects[j][k] as Creature, out var data))
                                {
                                    if (Vector2.Distance(this.steam.particles[i].pos, b) < 10f && this.room.physicalObjects[j][k] is Creature && (this.room.physicalObjects[j][k] as Creature).abstractCreature.rippleLayer == 0)
                                    {
                                        data.isInPoisonSteam = true;
                                        this.room.AddObject(new CreatureSpasmer(this.room.physicalObjects[j][k] as Creature, false, (this.room.physicalObjects[j][k] as Creature).stun));
                                        this.room.PlaySound(SoundID.Gate_Water_Steam_Puff, (this.room.physicalObjects[j][k] as Creature).mainBodyChunk, false, 1.1f, 1f);
                                        this.room.PlaySound(SoundID.Big_Spider_Spit_Warning_Rustle, (this.room.physicalObjects[j][k] as Creature).mainBodyChunk, false, 1f, 1f);
                                        return;
                                    }
                                    else data.isInPoisonSteam = false;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
