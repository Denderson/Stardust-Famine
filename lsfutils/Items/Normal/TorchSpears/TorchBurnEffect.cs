using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace lsfUtils.Items.Normal.TorchSpears
{
    public class TorchBurnEffect : UpdatableAndDeletable // patoma fire logic, pissing me off early in the morning, removed unnecesarry 2 am hook
    {
        public Creature target;
        public BodyChunk attachedChunk;
        public PhysicalObject.Appendage.Pos attachedAppendage;

        public int burnDuration = 1600;
        public int tickCounter = 0;

        public LightSource lightSource;
        public float[,] flicker;
        public TorchFlameParticle flamePool;

        public TorchBurnEffect(Creature target, BodyChunk chunk, PhysicalObject.Appendage.Pos appendage)
        {
            this.target = target;
            attachedChunk = chunk ?? target.mainBodyChunk;
            attachedAppendage = appendage;

            flicker = new float[2, 3];
            for (int i = 0; i < 2; i++)
            {
                flicker[i, 0] = 1f;
                flicker[i, 1] = 1f;
                flicker[i, 2] = 1f;
            }
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            if (target == null || target.slatedForDeletetion || burnDuration <= 0 || attachedChunk == null || attachedChunk.submersion > 0.2f)
            {
                if (lightSource != null)
                {
                    lightSource.Destroy();
                    lightSource = null;
                }
                Destroy();
                return;
            }

            int decayRate = (attachedChunk.vel.magnitude > 2.5f) ? 2 : 1;
            burnDuration -= decayRate;
            tickCounter++;

            // le flick
            for (int i = 0; i < flicker.GetLength(0); i++)
            {
                flicker[i, 1] = flicker[i, 0];
                flicker[i, 0] += Mathf.Pow(UnityEngine.Random.value, 3f) * 0.1f * ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f);
                flicker[i, 0] = Custom.LerpAndTick(flicker[i, 0], flicker[i, 2], 0.05f, 0.033333335f);

                if (UnityEngine.Random.value < 0.2f)
                {
                    flicker[i, 2] = 1f + Mathf.Pow(UnityEngine.Random.value, 3f) * 0.2f * ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f);
                }
                flicker[i, 2] = Mathf.Lerp(flicker[i, 2], 1f, 0.01f);
            }

            float effectiveness = Mathf.Max(0f, burnDuration / 1600f);

            if (room != null && !target.inShortcut)
            {
                if (UnityEngine.Random.value < effectiveness)
                {
                    if (flamePool == null || flamePool.slatedForDeletetion || flamePool.room != room)
                    {
                        flamePool = new TorchFlameParticle();
                        room.AddObject(flamePool);
                    }

                    float spreadRad = Mathf.Min(attachedChunk.rad * 0.8f, 15f);
                    Vector2 particlePos = attachedChunk.pos + UnityEngine.Random.insideUnitCircle * spreadRad;
                    Vector2 flameVel = (attachedChunk.vel * 0.5f) + new Vector2(0f, 2.5f) + (Custom.RNV() * UnityEngine.Random.value * 2f);

                    flamePool.Emit(particlePos, flameVel, 1.5f + effectiveness);
                }

                if (lightSource != null && (lightSource.slatedForDeletetion || lightSource.room != room))
                {
                    lightSource = null;
                }

                if (lightSource == null)
                {
                    lightSource = new LightSource(attachedChunk.pos, false, new Color(1f, 0.4f, 0.1f), null);
                    lightSource.requireUpKeep = true;
                    lightSource.affectedByPaletteDarkness = 0f;
                    lightSource.HardSetAlpha(1f);
                    lightSource.HardSetRad(150f);
                    room.AddObject(lightSource);
                }

                lightSource.stayAlive = true;
                lightSource.setPos = attachedChunk.pos;
                lightSource.setRad = (150f + (60f * effectiveness)) * flicker[0, 0];
                lightSource.color = new Color(1f, 0.4f, 0.1f);
                lightSource.setAlpha = effectiveness;
            }
            else if (lightSource != null)
            {
                lightSource.Destroy();
                lightSource = null;
            }

            if (tickCounter % 20 == 0 && !target.dead)
            {
                target.Violence(
                    source: null,
                    directionAndMomentum: null,
                    hitChunk: attachedChunk,
                    hitAppendage: attachedAppendage,
                    type: Creature.DamageType.Explosion,
                    damage: 0.05f,
                    stunBonus: 0.5f
                );
            }
        }
    }
}
