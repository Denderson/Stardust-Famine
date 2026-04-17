using BepInEx;
using BepInEx.Logging;
using Fisobs.Core;
using Menu.Remix.MixedUI;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MoreSlugcats;
using Music;
using RWCustom;
using SlugBase;
using SlugBase.Features;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Contexts;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Threading;
using UnityEngine;
using UnityEngine.Playables;
using Watcher;
using lsfUtils.Creatures.Spiders.PoisonSpider;
using static SlugBase.Features.FeatureTypes;

namespace lsfUtils.Creatures.Spiders
{
    internal class SpiderCode
    {
        public static void SpiderSpitModule_SpiderHasSpit(On.BigSpiderAI.SpiderSpitModule.orig_SpiderHasSpit orig, BigSpiderAI.SpiderSpitModule self)
        {
            orig(self);
            if (self?.AI?.creature?.creatureTemplate?.type != null && self.AI.creature.creatureTemplate.type == Enums.CreatureTemplateType.PoisonSpider)
            {
                orig(self);
                orig(self);
            }
        }

        public static void DartMaggot_Update(On.DartMaggot.orig_Update orig, DartMaggot self, bool eu)
        {
            orig(self, eu);
            if (self?.shotBy != null && self.shotBy is PoisonSpider.PoisonSpider && self?.stuckInChunk != null && self.stuckInChunk.owner is Creature)
            {
                self.sleepCounter = 1000;
                (self.stuckInChunk.owner as Creature).InjectPoison(1 / 3000f, self.shotBy.ShortCutColor());
            }
        }

        public static void DartMaggot_Shoot(On.DartMaggot.orig_Shoot orig, DartMaggot self, Vector2 pos, Vector2 dir, Creature shotBy)
        {
            if (shotBy is PoisonSpider.PoisonSpider)
            {
                self.sizeFac = Custom.ClampedRandomVariation(0.95f, 0.05f, 0.5f);
                self.lifeTime = Mathf.Lerp(2500f, 3500f, UnityEngine.Random.value);
                self.firstChunk.HardSetPosition(pos);
                self.firstChunk.vel = dir * 100f;
                self.ResetBody(dir);
                self.ChangeMode(DartMaggot.Mode.Shot);
                self.shotBy = shotBy;
                self.needleDir = dir;
                self.lastNeedleDir = dir;
                self.sleepCounter = 1023;
                self.newAndPink = 1f;
                self.room.PlaySound(SoundID.Dart_Maggot_Whizz_By, self.firstChunk, false, 1.5f, 1f);
                self.room.AddObject(new DartMaggot.Umbilical(self.room, self, shotBy as BigSpider, self.firstChunk.vel));
                for (int num = UnityEngine.Random.Range(2, 5); num >= 0; num--)
                {
                    self.room.AddObject(new WaterDrip(pos, dir * (UnityEngine.Random.value * 15f) + Custom.RNV() * (UnityEngine.Random.value * 5f), waterColor: false));
                }
            }
            else orig(self, pos, dir, shotBy);
        }

        public static bool SpiderSpitModule_CanSpit(On.BigSpiderAI.SpiderSpitModule.orig_CanSpit orig, BigSpiderAI.SpiderSpitModule self)
        {
            if (self?.AI?.creature?.creatureTemplate?.type != null && self.AI.creature.creatureTemplate.type == Enums.CreatureTemplateType.PoisonSpider && self.spitAtCrit?.representedCreature?.realizedCreature != null && self.spitAtCrit?.representedCreature?.realizedCreature.injectedPoison >= 0.1)
            {
                return false;
            }
            return orig(self);
        }

        public static void DartMaggot_ChangeMode(On.DartMaggot.orig_ChangeMode orig, DartMaggot self, DartMaggot.Mode newMode)
        {
            orig(self, newMode);
            if (newMode == DartMaggot.Mode.StuckInChunk && self?.stuckInChunk?.owner != null && self.stuckInChunk.owner is Creature && self.shotBy != null && self.shotBy is PoisonSpider.PoisonSpider)
            {
                (self.stuckInChunk.owner as Creature).InjectPoison(0.1f, self.shotBy.ShortCutColor());
                (self.stuckInChunk.owner as Creature).Stun(40);
            }

        }
    }
}
