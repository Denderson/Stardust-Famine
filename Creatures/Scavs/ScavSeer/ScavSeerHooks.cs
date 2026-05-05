using lsfUtils.Creatures.Scavs.ScavSeer;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lsfUtils.Creatures.Scavs.ScavSeer
{
    public static class ScavSeerHooks
    {
        public static void ScavengerGraphics_ctor(On.ScavengerGraphics.orig_ctor orig, ScavengerGraphics self, PhysicalObject ow)
        {
            orig(self, ow);
            if (false) //self?.scavenger?.abstractCreature != null)
            {
                if (self.scavenger.abstractCreature.IsSeer())
                {
                    int num = self.TotalSprites;
                    self.blindfold = new ScavengerCosmetic.TemplarBlindfold(self, num);
                    num += self.blindfold.totalSprites;
                    self.totalSprites = num;
                    self.maskGfx = new VultureMaskGraphics(self.scavenger, VultureMask.MaskType.SCAVTEMPLAR, self.MaskSprite, null);
                    self.maskGfx.GenerateColor(self.scavenger.abstractCreature.ID.RandomSeed);
                    self.maskGfx.ignoreDarkness = true;
                    self.maskGfx.glimmer = true;
                }
            }
        }

        public static void ScavengerAI_Update(On.ScavengerAI.orig_Update orig, ScavengerAI self)
        {
            orig(self);
            if (self?.scavenger?.abstractCreature != null)
            {
                if (self.scavenger.abstractCreature.IsSeer())
                {
                    if (self.PingCD > 0)
                    {
                        self.PingCD--;
                    }
                    else
                    {
                        self.scavenger.SeerPing();
                    }
                }
            }
        }

        public static void ScavengerSquad_UpdateLeader(On.ScavengerAbstractAI.ScavengerSquad.orig_UpdateLeader orig, ScavengerAbstractAI.ScavengerSquad self)
        {
            orig(self);
            for (int i = 0; i < self.members.Count; i++)
            {
                if (self.members[i]?.realizedCreature != null && self.members[i].realizedCreature is Scavenger)
                {
                    if (self.members[i].realizedCreature is Scavs.ScavSeer.ScavSeer)
                    {
                        self.leader = self.members[i];
                        break;
                    }
                }
            }
        }

        public static void SeerPing(this Scavenger source)
        {
            if (source?.room != null && source.AI != null)
            {
                if (source.AI.behavior == ScavengerAI.Behavior.Flee || source.AI.behavior == ScavengerAI.Behavior.Attack || source.AI.scared > 0.75f)
                {
                    source.AI.PingCD = 80;
                    source.room.AddObject(new Scavs.ScavSeer.SeerPing(source, source.mainBodyChunk.pos, source.karmicArmor?.rad ?? 0f, 0.4f, 0.4f, 60));
                }
                else if (source.AI.behavior == ScavengerAI.Behavior.Idle)
                {
                    source.AI.PingCD = 240;
                    source.room.AddObject(new Scavs.ScavSeer.SeerPing(source, source.mainBodyChunk.pos, source.karmicArmor?.rad ?? 0f, 0.2f, 0.2f, 200));
                }
                else
                {
                    source.AI.PingCD = 160;
                    source.room.AddObject(new Scavs.ScavSeer.SeerPing(source, source.mainBodyChunk.pos, source.karmicArmor?.rad ?? 0f, 0.3f, 0.3f, 100));
                }


            }
        }
        public static void ScavengerGraphics_ShockReaction(On.ScavengerGraphics.orig_ShockReaction orig, ScavengerGraphics self, float intensity)
        {
            //if (self?.scavenger?.abstractCreature == null || self.scavenger.abstractCreature.IsFlank()) return;
            orig(self, intensity);
        }

        public static void ScavengerAI_ReactToNoise(On.ScavengerAI.orig_ReactToNoise orig, ScavengerAI self, NoiseTracker.TheorizedSource source, Noise.InGameNoise noise)
        {
            orig(self, source, noise);
            if (self?.scavenger?.abstractCreature != null && self.scavenger.abstractCreature.IsSeer() && self.PingCD == 0)
            {
                self.scavenger.SeerPing();
            }
        }
    }
}
