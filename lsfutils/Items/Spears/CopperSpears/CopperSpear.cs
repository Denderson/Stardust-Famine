using lsfUtils.CWTs;
using lsfUtils.Mechanics.Fire;
using MoreSlugcats;
using RWCustom;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace lsfUtils.Items.Spears.CopperSpears
{
    public class CopperSpear : Spear
    {
        public float intensity;
        public int timer;
        public int oxidization;
        public int charges = 0;
        public bool overcharged = false;

        public float randomVariation = Random.value;

        public bool struck = false;

        public CopperSpear(AbstractSpear abstractObject, World world) : base(abstractObject, world)
        {
            intensity = 0f;
            timer = -1;
            if (abstractObject is CopperSpearAbstract absSpear)
            {
                oxidization = absSpear.oxidization;
            }
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            if (timer > -1)
            {
                if (this.mode != Weapon.Mode.Thrown || overcharged) timer--;
                if (Random.value < 1 / 30f) room.AddObject(new Spark(base.firstChunk.pos + RWCustom.Custom.RNV() * (UnityEngine.Random.value * 10f), RWCustom.Custom.RNV() * Mathf.Lerp(6f, 18f, UnityEngine.Random.value), GetElectricityColor(), null, 4, 18));
                if (timer <= 120)
                {
                    if ((timer % 20 == 0 || (timer <= 40 && timer % 10 == 0)) && timer != 0)
                    {
                        this.room.PlaySound(SoundID.Zapper_Zap); Plugin.Log.LogMessage("warning shock!");
                        room.AddObject(new Spark(base.firstChunk.pos + RWCustom.Custom.RNV() * (UnityEngine.Random.value * 10f), RWCustom.Custom.RNV() * Mathf.Lerp(6f, 18f, UnityEngine.Random.value), GetElectricityColor(), null, 4, 18));
                        this.room.AddObject(new Explosion.ExplosionLight(this.firstChunk.pos, 20f * intensity, 1f, 8, GetElectricityColor()));
                    }
                    if (timer < 1)
                    {
                        if (grabbedBy.Count > 0 && grabbedBy[0].grabber is Creature creature && !CheckElectricCreature(creature))
                        {
                            if (creature is not Player player)
                            {
                                //if a non player creature (scavengers mainly) is still holding onto a copper spear when the timer runs out, they take electric damage based on the charge
                                creature.Violence(
                                source: this.firstChunk,
                                directionAndMomentum: null,
                                hitChunk: creature.mainBodyChunk,
                                hitAppendage: null,
                                type: Creature.DamageType.Electric,
                                damage: intensity,
                                stunBonus: intensity * 80
                                );
                            }
                            else if (intensity < 1f)
                            {
                                //same applies to the player, with it obviously being lethal past 1 intensity
                                player.playerState.permanentDamageTracking += intensity;
                                player.Stun((int)(intensity * 80));
                            }
                            else
                            {
                                player.Die();
                            }
                            this.room.AddObject(new CreatureSpasmer(creature, true, (int)(intensity * 80)));
                        }
                        this.room.PlaySound(SoundID.Zapper_Zap, this.firstChunk.pos, 1f, 1.5f, this.abstractSpear);
                        this.room.AddObject(new Explosion.ExplosionLight(this.firstChunk.pos, 40f * intensity, 1f, 8, GetElectricityColor()));
                        for (int i = 0; i < 15; i++)
                        {
                            Vector2 a = RWCustom.Custom.DegToVec(360f * UnityEngine.Random.value);
                            this.room.AddObject(new MouseSpark(base.firstChunk.pos + a * 9f, base.firstChunk.vel + a * (36f * UnityEngine.Random.value), 20f, GetElectricityColor()));
                        }
                        if (overcharged)
                        {
                            //:leditoroverload:
                            if (this?.room == null || this.firstChunk.pos == null) return;
                            room.AddObject(new Explosion(room, this, this.firstChunk.pos, 7, 40f * intensity, 8.3f, 3f, 280f, 0.4f, (this.mode == Weapon.Mode.Thrown) ? this.thrownBy : null, 0.7f, 160f, 1f));
                            room.AddObject(new ExplosionSpikes(room, this.firstChunk.pos, 14, 5f * intensity, 9f, 7f, 170f, GetElectricityColor()));
                            room.PlaySound(SoundID.Bomb_Explode, this.firstChunk.pos, 0.8f, 0.4f);
                            this.Destroy();
                        }

                        timer = -1;
                        intensity = 0f;
                        charges = 0;
                        struck = false;
                    }
                }
            }
            if (intensity > 0f && this.Submersion > 0.5f)
            {
                //if submerged while charged, instantly discharges underwater
                this.room.AddObject(new UnderwaterShock(this.room, null, base.firstChunk.pos, 10, intensity * 250f, intensity * 2, (this.mode == Weapon.Mode.Thrown) ? this.thrownBy : null, GetElectricityColor()));
                intensity = 0f;
                timer = -1;
                charges = 0;
            }

            
        }

        


        public void Charge(float amount)
        {
            intensity += amount;
            if (timer == -1) timer = 1320 - (int)(120 * amount);
            else timer = (timer + 1320 - (int)(120 * amount)) / (charges + 3);
            charges++;

            if (intensity >= 10f)
            {
                //overcharged means it will explode from, well, overcharge
                if (intensity > 10f) overcharged = true;
                timer = 40;
            }
            Plugin.Log.LogMessage($"new timer: {timer}. new charge: {intensity}");

            this.room.PlaySound(SoundID.Zapper_Zap, this.firstChunk.pos, 1f, 1.5f, this.abstractSpear);
            room.AddObject(new Spark(base.firstChunk.pos + RWCustom.Custom.RNV() * (UnityEngine.Random.value * 10f), RWCustom.Custom.RNV() * Mathf.Lerp(6f, 18f, UnityEngine.Random.value), GetElectricityColor(), null, 4, 18));
        }

        public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
        {
            bool hit = base.HitSomething(result, eu);
            Electrocute(result.obj);
            return hit;
        }
        public void Electrocute(PhysicalObject otherObject)
        {
            if (!(otherObject is Creature))
            {
                return;
            }
            if (otherObject.abstractPhysicalObject.rippleLayer != this.abstractPhysicalObject.rippleLayer && !otherObject.abstractPhysicalObject.rippleBothSides && !this.abstractPhysicalObject.rippleBothSides)
            {
                return;
            }
            bool flag = CheckElectricCreature(otherObject as Creature);
            if (flag && (timer < 0 || timer > 120))
            {
                //if it's an electric creature, charges the copper spear based on its mass
                this.Charge(otherObject.TotalMass / (float)(charges + 2));
                Plugin.Log.LogMessage($"mass of electric creature: {otherObject.TotalMass}. charge added: {otherObject.TotalMass / (float)(charges + 1)}. total charge: {intensity}" );
                return;
            }
            if (intensity == 0)
            {
                return;
            }
            if (!(otherObject is BigEel) && !flag && !overcharged)
            {
                //if not an electric creature, deals electric damage to it based on the current charge
                (otherObject as Creature).Violence(
                source: this.firstChunk,
                directionAndMomentum: new Vector2?(RWCustom.Custom.DirVec(base.firstChunk.pos, otherObject.firstChunk.pos) * 5f),
                hitChunk: otherObject.firstChunk,
                hitAppendage: null,
                type: Creature.DamageType.Electric,
                damage: intensity,
                stunBonus: intensity * 80
                );
                this.room.PlaySound(SoundID.Zapper_Zap, this.firstChunk.pos, 1f, 1.5f, this.abstractSpear);
                this.room.AddObject(new Explosion.ExplosionLight(this.firstChunk.pos, 30f * intensity, 1f, 8, GetElectricityColor()));
                this.room.AddObject(new CreatureSpasmer(otherObject as Creature, true, (int)(intensity * 80)));
                for (int i = 0; i < 15; i++)
                {
                    Vector2 a = RWCustom.Custom.DegToVec(360f * UnityEngine.Random.value);
                    this.room.AddObject(new MouseSpark(base.firstChunk.pos + a * 9f, base.firstChunk.vel + a * (36f * UnityEngine.Random.value), 20f, GetElectricityColor()));
                }
                    intensity = 0f;
                    timer = -1;
                    charges = 0;
            }
        }

        public bool CheckElectricCreature(Creature otherObject)
        {
            //return ElectricSpear.CheckElectricCreature(otherObject);      //unsure of how to make it inherit from the list that electric spears use, would be important for mod compatability
            return otherObject is Centipede || otherObject is BigJellyFish || otherObject is Inspector;
        }

        public override void ChangeMode(Mode newMode)
        {
            if (newMode == Mode.StuckInWall || mode == Mode.StuckInWall)
            {
                for (int i = 0; i < this.room.zapCoils.Count; i++)
                {
                    ZapCoil zapCoil = this.room.zapCoils[i];
                    if (zapCoil.turnedOn > 0.5f && zapCoil.GetFloatRect.Vector2Inside(base.firstChunk.pos + this.rotation * 30f))
                    {
                        //charges (and unlodges) copper spears if they hit an active zapper. they still get stuck if they hit an inactive zapper, even after the zapper reactivates
                        float randomCharge = Random.Range(3f, 5f) / (charges + 1);
                        this.Charge(randomCharge);
                        Plugin.Log.LogMessage($"Charge: {randomCharge}");
                        newMode = Mode.Free;
                    }
                }
            }
            base.ChangeMode(newMode);
        }
        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            base.ApplyPalette(sLeaser, rCam, palette);
            //color based on oxidization. for arena all colors can appear since oxidization isn't possible
            if (this.abstractPhysicalObject.world.game.IsArenaSession) this.color = Color.Lerp(Color.Lerp(new(0.72f, 0.45f, 0.2f), new(0.29f, 0.66f, 0.58f), randomVariation), palette.blackColor, 0.2f);
            else this.color = Color.Lerp(Color.Lerp(new(0.72f, 0.45f, 0.2f), new(0.29f, 0.66f, 0.58f), oxidization / 5f + (-0.1f + 0.2f * randomVariation)), palette.blackColor, 0.2f);

        }

       
        public void Oxidize() 
        {
            if (oxidization < 6)
            {
                oxidization++;
                if (this.abstractSpear is CopperSpearAbstract absSpear)
                {
                    absSpear.oxidization++;
                }
            }
        }

        public Color GetElectricityColor()
        {
            //color of sparks and lights for the electricity. green by default, but will be of the color of lightning if it's present in the room
            RoomSettings.RoomEffect effect = this.room.roomSettings.GetEffect(Watcher.WatcherEnums.RoomEffectType.SKLightningHue);
            RoomSettings.RoomEffect effect2 = this.room.roomSettings.GetEffect(Watcher.WatcherEnums.RoomEffectType.SKLightning);
            return Custom.HSL2RGB((effect2 != null) ? (effect != null) ? effect.amount : 0.535f : 1/3f, 1f, 0.85f);
        }
    }
}
