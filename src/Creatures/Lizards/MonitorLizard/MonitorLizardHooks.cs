using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace lsfUtils.Creatures.Lizards.MonitorLizard
{
    internal class MonitorLizardHooks
    {
        public static void LizardGraphics_ColorBody(On.LizardGraphics.orig_ColorBody orig, LizardGraphics self, RoomCamera.SpriteLeaser sLeaser, Color col)
        {
            if (self?.lizard?.Template?.type != null && self.lizard.Template.type == Enums.CreatureTemplateType.MonitorLizard && (self.lizard as MonitorLizard).IsAlbino())
            {
                col = Color.white;
            }
            orig(self, sLeaser, col);
        }

        public static Color LizardGraphics_BodyColor(On.LizardGraphics.orig_BodyColor orig, LizardGraphics self, float f)
        {
            if (self?.lizard?.Template?.type != null && self.lizard.Template.type == Enums.CreatureTemplateType.MonitorLizard && (self.lizard as MonitorLizard).IsAlbino())
            {
                return Color.Lerp(Color.white, self.effectColor, Mathf.Clamp(Mathf.InverseLerp(self.lizard.lizardParams.tailColorationStart, 0.95f, Mathf.InverseLerp(self.bodyLength / self.BodyAndTailLength, 1f, f)), 0f, 1f));
            }
            return orig(self, f);
        }

        public static void MudPit_ChunkSlowdown(On.MudPit.orig_ChunkSlowdown orig, MudPit self, BodyChunk chunk, float factor)
        {
            if (chunk?.owner != null && chunk.owner is MonitorLizard)
            {
                return;
            }
            orig(self, chunk, factor);
        }

        public static void Water_Update(On.Water.orig_Update orig, Water self)
        {
            orig(self);
            for (int j = 0; j < self.room.physicalObjects.Length; j++)
            {
                foreach (PhysicalObject item in self.room.physicalObjects[j])
                {
                    if (item is MonitorLizard monitorLizard)
                    {
                        BodyChunk[] bodyChunks = monitorLizard.bodyChunks;
                        foreach (BodyChunk bodyChunk in bodyChunks)
                        {
                            if (self.viscosity > 0f && bodyChunk.submersion > 0f)
                            {
                                if (bodyChunk.submersion < 1f)
                                {
                                    bodyChunk.vel.x /= 1f - 0.75f * self.viscosity;
                                    if (bodyChunk.vel.y > 0f)
                                    {
                                        bodyChunk.vel.y /= 1f - 0.075f * self.viscosity;
                                    }
                                    else
                                    {
                                        bodyChunk.vel.y /= 1f - 0.15f * self.viscosity;
                                    }
                                }
                                else
                                {
                                    bodyChunk.vel.x /= 1f - 0.225f * self.viscosity;
                                    if (bodyChunk.vel.y > 0f)
                                    {
                                        bodyChunk.vel.y /= 1f - 0.1f * self.viscosity;
                                    }
                                    else
                                    {
                                        bodyChunk.vel.y /= 1f - 0.15f * self.viscosity;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
