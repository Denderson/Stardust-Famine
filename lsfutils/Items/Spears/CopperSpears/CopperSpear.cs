using MoreSlugcats;
using RWCustom;
using UnityEngine;
using static lsfUtils.Plugin;

namespace lsfUtils.Items.Spears.CopperSpears;

public class CopperSpear : Spear
{
    public float intensity;
    public int timer;
    public int Oxidization => (abstractSpear as CopperSpearAbstract).oxidization;

    public int charges = 0;
    public bool overcharged = false;

    public float randomVariation;

    public const int warningShockSpeed = 20;
    public const int secondWarningTime = 40;
    public const int warningTime = 120;
    public static readonly Color copperColor = new(0.72f, 0.45f, 0.2f);
    public static readonly Color oxidisedColor = new(0.29f, 0.66f, 0.58f);

    public bool struck = false;

    public CopperSpear(AbstractSpear abstractObject, World world) : base(abstractObject, world)
    {
        intensity = 0f;
        timer = -1;
        randomVariation = Random.value;
    }

    public override void Update(bool eu)
    {
        Update(eu);
        if (timer > -1)
        {
            if (mode != Mode.Thrown || overcharged) timer--;
            if (Random.value < 1 / 30f) room.AddObject(new Spark(firstChunk.pos + Custom.RNV() * (Random.value * 10f), Custom.RNV() * Mathf.Lerp(6f, 18f, Random.value), GetElectricityColor(), null, 4, 18));
            if (timer <= warningTime)
            {
                if (timer == 0)
                {
                    if (grabbedBy.Count > 0 && grabbedBy[0].grabber is Creature creature && !CheckElectricCreature(creature))
                    {
                        if (creature is not Player player)
                        {
                            //if a non player creature (scavengers mainly) is still holding onto a copper spear when the timer runs out, they take electric damage based on the charge
                            creature.Violence(
                            source: firstChunk,
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
                        room.AddObject(new CreatureSpasmer(creature, true, (int)(intensity * 80)));
                    }
                    room.PlaySound(SoundID.Zapper_Zap, firstChunk.pos, 1f, 1.5f, abstractSpear);
                    room.AddObject(new Explosion.ExplosionLight(firstChunk.pos, 40f * intensity, 1f, 8, GetElectricityColor()));
                    for (int i = 0; i < 15; i++)
                    {
                        Vector2 a = Custom.DegToVec(360f * Random.value);
                        room.AddObject(new MouseSpark(firstChunk.pos + a * 9f, firstChunk.vel + a * (36f * Random.value), 20f, GetElectricityColor()));
                    }
                    if (overcharged)
                    {
                        //:leditoroverload:
                        if (this?.room == null || firstChunk.pos == null) return;
                        room.AddObject(new Explosion(room, this, firstChunk.pos, 7, 40f * intensity, 8.3f, 3f, 280f, 0.4f, (mode == Mode.Thrown) ? thrownBy : null, 0.7f, 160f, 1f));
                        room.AddObject(new ExplosionSpikes(room, firstChunk.pos, 14, 5f * intensity, 9f, 7f, 170f, GetElectricityColor()));
                        room.PlaySound(SoundID.Bomb_Explode, firstChunk.pos, 0.8f, 0.4f);
                        Destroy();
                    }

                    timer = -1;
                    intensity = 0f;
                    charges = 0;
                    struck = false;
                }

                if (timer % (timer <= secondWarningTime ? warningShockSpeed / 2 : warningShockSpeed) == 0)
                {
                    room.PlaySound(SoundID.Zapper_Zap); Log.LogMessage("warning shock!");
                    room.AddObject(new Spark(firstChunk.pos + Custom.RNV() * (Random.value * 10f), Custom.RNV() * Mathf.Lerp(6f, 18f, Random.value), GetElectricityColor(), null, 4, 18));
                    room.AddObject(new Explosion.ExplosionLight(firstChunk.pos, 20f * intensity, 1f, 8, GetElectricityColor()));
                }
            }
        }
        if (intensity > 0f && Submersion > 0.5f)
        {
            //if submerged while charged, instantly discharges underwater
            room.AddObject(new UnderwaterShock(room, null, firstChunk.pos, 10, intensity * 250f, intensity * 2, (mode == Mode.Thrown) ? thrownBy : null, GetElectricityColor()));
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
        Log.LogMessage($"new timer: {timer}. new charge: {intensity}");

        room.PlaySound(SoundID.Zapper_Zap, firstChunk.pos, 1f, 1.5f, abstractSpear);
        room.AddObject(new Spark(firstChunk.pos + Custom.RNV() * (Random.value * 10f), Custom.RNV() * Mathf.Lerp(6f, 18f, Random.value), GetElectricityColor(), null, 4, 18));
    }

    public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
    {
        bool hit = HitSomething(result, eu);
        Electrocute(result.obj);
        return hit;
    }
    public void Electrocute(PhysicalObject otherObject)
    {
        if (otherObject is not Creature)
        {
            return;
        }
        if (otherObject.abstractPhysicalObject.rippleLayer != abstractPhysicalObject.rippleLayer && !otherObject.abstractPhysicalObject.rippleBothSides && !abstractPhysicalObject.rippleBothSides)
        {
            return;
        }
        bool flag = CheckElectricCreature(otherObject as Creature);
        if (flag && (timer < 0 || timer > warningTime))
        {
            //if it's an electric creature, charges the copper spear based on its mass
            Charge(otherObject.TotalMass / (float)(charges + 2));
            Log.LogMessage($"mass of electric creature: {otherObject.TotalMass}. charge added: {otherObject.TotalMass / (float)(charges + 1)}. total charge: {intensity}" );
            return;
        }
        if (intensity == 0)
        {
            return;
        }
        if (otherObject is not BigEel && !flag && !overcharged)
        {
            //if not an electric creature, deals electric damage to it based on the current charge
            (otherObject as Creature).Violence(
            source: firstChunk,
            directionAndMomentum: new Vector2?(Custom.DirVec(firstChunk.pos, otherObject.firstChunk.pos) * 5f),
            hitChunk: otherObject.firstChunk,
            hitAppendage: null,
            type: Creature.DamageType.Electric,
            damage: intensity,
            stunBonus: intensity * 80
            );
            room.PlaySound(SoundID.Zapper_Zap, firstChunk.pos, 1f, 1.5f, abstractSpear);
            room.AddObject(new Explosion.ExplosionLight(firstChunk.pos, 30f * intensity, 1f, 8, GetElectricityColor()));
            room.AddObject(new CreatureSpasmer(otherObject as Creature, true, (int)(intensity * 80)));
            for (int i = 0; i < 15; i++)
            {
                Vector2 a = Custom.DegToVec(360f * Random.value);
                room.AddObject(new MouseSpark(firstChunk.pos + a * 9f, firstChunk.vel + a * (36f * Random.value), 20f, GetElectricityColor()));
            }
            intensity = 0f;
            timer = -1;
            charges = 0;
        }
    }

    public bool CheckElectricCreature(Creature otherObject)
    {
        return (null as ElectricSpear).CheckElectricCreature(otherObject);
    }

    public override void ChangeMode(Mode newMode)
    {
        if (newMode == Mode.StuckInWall || mode == Mode.StuckInWall)
        {
            foreach (ZapCoil zapCoil in room.zapCoils)
            {
                if (zapCoil.turnedOn > 0.5f && zapCoil.GetFloatRect.Vector2Inside(firstChunk.pos + rotation * 30f))
                {
                    //charges (and unlodges) copper spears if they hit an active zapper. they still get stuck if they hit an inactive zapper, even after the zapper reactivates
                    float randomCharge = Random.Range(3f, 5f) / (charges + 1);
                    Charge(randomCharge);
                    Log.LogMessage($"Charge: {randomCharge}");
                    newMode = Mode.Free;
                }
            }
        }
        ChangeMode(newMode);
    }
    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        ApplyPalette(sLeaser, rCam, palette);
        color = Color.Lerp(Color.Lerp(copperColor, oxidisedColor, Oxidization / 5f + (-0.1f + 0.2f * randomVariation)), palette.blackColor, 0.2f);
    }

   
    public void Oxidize() 
    {
        if (Oxidization < 6)
        {
            if (abstractSpear is CopperSpearAbstract absSpear)
            {
                absSpear.oxidization++;
            }
        }
    }

    public Color GetElectricityColor()
    {
        //color of sparks and lights for the electricity. green by default, but will be of the color of lightning if it's present in the room
        RoomSettings.RoomEffect effect = room.roomSettings.GetEffect(Watcher.WatcherEnums.RoomEffectType.SKLightningHue);
        RoomSettings.RoomEffect effect2 = room.roomSettings.GetEffect(Watcher.WatcherEnums.RoomEffectType.SKLightning);
        return Custom.HSL2RGB((effect2 != null) ? (effect != null) ? effect.amount : 0.535f : 1/3f, 1f, 0.85f);
    }
}
