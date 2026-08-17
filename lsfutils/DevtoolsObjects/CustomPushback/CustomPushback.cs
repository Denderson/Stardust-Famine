using lsfUtils.CWTs;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static lsfUtils.Plugin;
using static Pom.Pom;

namespace lsfUtils.DevtoolsObjects.CustomPushback
{
    public class CustomPushback : UpdatableAndDeletable
    {
        public CustomPushbackData data;
        Vector2 pos;

        public CustomPushback(PlacedObject placedObject, Room room)
        {
            CustomPushbackData maybedata = placedObject.data as CustomPushbackData ?? throw new ArgumentException($"{nameof(PlacedObject)} was null or didn't contain a {nameof(CustomPushback)} instance");
            data = maybedata;
            pos = placedObject.pos;
            this.room = room;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            if (data.playerOnly)
            {
                for (int a = 0; a < room.game.Players.Count; a++)
                {
                    if (room.game.Players[a].realizedCreature == null || room.game.Players[a].realizedCreature.room != room) continue;
                    if (room.game.Players[a].realizedCreature is not Player player) continue;

                    for (int b = 0; b < player.bodyChunks.Length; b++)
                    {
                        if (InRange(player.bodyChunks[b].pos))
                        {
                            Push(player.bodyChunks[b]);
                        }
                    }
                }
            }
            else
            {
                for (int c = 0; c < room.physicalObjects.Length; c++)
                {
                    for (int d = 0; d < room.physicalObjects[c].Count; d++)
                    {
                        PhysicalObject physicalObject = room.physicalObjects[c][d];
                        if (physicalObject?.bodyChunks != null && physicalObject.bodyChunks.Length > 0)
                        {
                            for (int e = 0; e < physicalObject.bodyChunks.Length; e++)
                            {
                                if (InRange(physicalObject.bodyChunks[e].pos))
                                {
                                    Push(physicalObject.bodyChunks[e]);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void Push(BodyChunk chunk)
        {
            chunk.vel += PushDirection(chunk.pos) * data.strength;
        }

        public Vector2 PushDirection(Vector2 pos)
        {
            if (!data.directed) return data.radius.normalized;
            return Custom.DirVec(this.pos, pos);
        }

        public bool InRange(Vector2 pos)
        {
            return Custom.DistLess(pos, this.pos, data.radius.magnitude);
        }
    }
}