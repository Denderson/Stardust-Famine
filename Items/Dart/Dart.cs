using System;
using MoreSlugcats;
using RWCustom;
using Smoke;
using UnityEngine;
using Watcher;
using static lsfUtils.Plugin;

namespace lsfUtils.Items.Dart
{
    public class Dart : Weapon
    {
        public bool spinning;

        protected bool pivotAtTip;

        protected bool lastPivotAtTip;

        public PhysicalObject stuckInObject;

        public int stuckInChunkIndex;

        public Appendage.Pos stuckInAppendage;

        public float stuckRotation;

        public int stuckBodyPart;

        public int stillCounter;

        public bool hasPoisonGraphicsActive;

        public float poison;

        public bool reinitiateSpritesOnDraw;

        public AbstractDart abstractDart => abstractPhysicalObject as AbstractDart;

        public BodyChunk stuckInChunk => stuckInObject.bodyChunks[stuckInChunkIndex];

        public AbstractCreature shotFrom;

        public override bool HeavyWeapon => false;

        public int pullOutTimer;

        public int pullOutAttempts;

        public bool strongHit;

        public float pullOutChance;

        public Dart(AbstractDart abstractDart) : base(abstractDart, abstractDart.world)
        {
            Log.LogMessage("Making a dart!");
            bodyChunks = new BodyChunk[1];
            bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 5f, 0.07f);
            bodyChunkConnections = [];
            airFriction = 0.995f;
            gravity = 0.9f;
            bounce = 0.5f;
            surfaceFriction = 0.4f;
            collisionLayer = 2;
            waterFriction = 0.98f;
            buoyancy = 0.4f;
            pivotAtTip = false;
            lastPivotAtTip = false;
            stuckBodyPart = -1;
            firstChunk.loudness = 7f;
            tailPos = firstChunk.pos;
            soundLoop = new ChunkDynamicSoundLoop(firstChunk);
            pullOutAttempts = 0;
            pullOutTimer = 0;
            strongHit = false;
            poison = abstractDart.poison;
            hasPoisonGraphicsActive = poison > 0f;
            this.abstractDart.realisedDart = this;
            pullOutChance = 0.5f;
        }

        public override void ChangeMode(Mode newMode)
        {
            if (mode == Mode.StuckInCreature)
            {
                room?.PlaySound(SoundID.Spear_Dislodged_From_Creature, firstChunk);
                PulledOutOfStuckObject();
                ChangeOverlap(newOverlap: true);
            }
            else if (newMode == Mode.StuckInCreature)
            {
                pullOutAttempts = 0;
                pullOutTimer = 0;
                ChangeOverlap(newOverlap: false);
            }
            if (newMode != Mode.Free)
            {
                spinning = false;
            }
            base.ChangeMode(newMode);
        }

        public bool ShouldStickInCreature(Creature creature)
        {
            return true;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            soundLoop.sound = SoundID.None;
            if (firstChunk.vel.magnitude > 5f)
            {
                if (mode == Mode.Thrown)
                {
                    soundLoop.sound = SoundID.Spear_Thrown_Through_Air_LOOP;
                }
                else if (mode == Mode.Free)
                {
                    soundLoop.sound = SoundID.Spear_Spinning_Through_Air_LOOP;
                }
                soundLoop.Volume = Mathf.InverseLerp(5f, 15f, firstChunk.vel.magnitude);
            }
            soundLoop.Update();
            lastPivotAtTip = pivotAtTip;
            pivotAtTip = mode == Mode.Thrown || mode == Mode.StuckInCreature;
            if (mode == Mode.Free)
            {
                if (spinning)
                {
                    if (Custom.DistLess(firstChunk.pos, firstChunk.lastPos, 4f * room.gravity))
                    {
                        stillCounter++;
                    }
                    else
                    {
                        stillCounter = 0;
                    }
                    if (firstChunk.ContactPoint.y < 0 || stillCounter > 20)
                    {
                        spinning = false;
                        rotationSpeed = 0f;
                        rotation = Custom.DegToVec(Mathf.Lerp(-50f, 50f, UnityEngine.Random.value) + 180f);
                        firstChunk.vel *= 0f;
                        room.PlaySound(SoundID.Spear_Stick_In_Ground, firstChunk);
                    }
                }
                else if (!Custom.DistLess(firstChunk.lastPos, firstChunk.pos, 6f))
                {
                    SetRandomSpin();
                }
            }
            else if (mode == Mode.Thrown)
            {
                firstChunk.vel.y += 0.45f;
                if (Custom.DistLess(thrownPos, firstChunk.pos, 560f * Mathf.Max(1f, 1f)) && firstChunk.ContactPoint == throwDir && room.GetTile(firstChunk.pos).Terrain == Room.Tile.TerrainType.Air && room.GetTile(firstChunk.pos + throwDir.ToVector2() * 20f).Terrain == Room.Tile.TerrainType.Solid && (UnityEngine.Random.value < (0.33f) || Custom.DistLess(thrownPos, firstChunk.pos, 140f)))
                {
                    vibrate = 10;
                    room.PlaySound(SoundID.Spear_Bounce_Off_Creauture_Shell, firstChunk);
                    for (int num = 17; num > 0; num--)
                    {
                        room.AddObject(new Spark(firstChunk.pos, Custom.RNV(), Color.white, null, 10, 20));
                    }
                }
            }
            else if (mode == Mode.StuckInCreature)
            {
                if (stuckInChunk?.owner is Creature)
                {
                    if (stuckInAppendage != null)
                    {
                        setRotation = Custom.DegToVec(stuckRotation + Custom.VecToDeg(stuckInAppendage.appendage.OnAppendageDirection(stuckInAppendage)));
                        firstChunk.pos = stuckInAppendage.appendage.OnAppendagePosition(stuckInAppendage);
                    }
                    else
                    {
                        firstChunk.vel = stuckInChunk.vel;
                        if (stuckBodyPart == -1 || !room.BeingViewed || (stuckInChunk.owner as Creature).BodyPartByIndex(stuckBodyPart) == null)
                        {
                            setRotation = Custom.DegToVec(stuckRotation + Custom.VecToDeg(stuckInChunk.Rotation));
                            firstChunk.MoveWithOtherObject(eu, stuckInChunk, new Vector2(0f, 0f));
                        }
                        else
                        {
                            setRotation = Custom.DegToVec(stuckRotation + Custom.AimFromOneVectorToAnother(stuckInChunk.pos, (stuckInChunk.owner as Creature).BodyPartByIndex(stuckBodyPart).pos));
                            firstChunk.MoveWithOtherObject(eu, stuckInChunk, Vector2.Lerp(stuckInChunk.pos, (stuckInChunk.owner as Creature).BodyPartByIndex(stuckBodyPart).pos, 0.5f) - stuckInChunk.pos);
                        }
                    }
                    if (stuckInChunk.owner.slatedForDeletetion)
                    {
                        ChangeMode(Mode.Free);
                    }
                }
            }
            for (int num3 = abstractPhysicalObject.stuckObjects.Count - 1; num3 >= 0; num3--)
            {
                if (abstractPhysicalObject.stuckObjects[num3] is AbstractPhysicalObject.ImpaledOnSpearStick)
                {
                    if (abstractPhysicalObject.stuckObjects[num3].B.realizedObject != null && (abstractPhysicalObject.stuckObjects[num3].B.realizedObject.slatedForDeletetion || abstractPhysicalObject.stuckObjects[num3].B.realizedObject.grabbedBy.Count > 0))
                    {
                        abstractPhysicalObject.stuckObjects[num3].Deactivate();
                    }
                    else if (abstractPhysicalObject.stuckObjects[num3].B.realizedObject != null && abstractPhysicalObject.stuckObjects[num3].B.realizedObject.room == room)
                    {
                        abstractPhysicalObject.stuckObjects[num3].B.realizedObject.firstChunk.MoveFromOutsideMyUpdate(eu, firstChunk.pos + rotation * Custom.LerpMap((abstractPhysicalObject.stuckObjects[num3] as AbstractPhysicalObject.ImpaledOnSpearStick).onSpearPosition, 0f, 4f, 15f, -15f));
                        abstractPhysicalObject.stuckObjects[num3].B.realizedObject.firstChunk.vel *= 0f;
                    }
                }
            }
        }

        public override void Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
        {
            base.Thrown(thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);
            room?.PlaySound(SoundID.Slugcat_Throw_Spear, firstChunk);
        }

        public override void PickedUp(Creature upPicker)
        {
            ChangeMode(Mode.Carried);
            room.PlaySound(SoundID.Slugcat_Pick_Up_Spear, firstChunk);
        }

        public void LodgeInCreature(SharedPhysics.CollisionResult result, bool eu)
        {
            stuckInObject = result.obj;
            ChangeMode(Mode.StuckInCreature);
            if (result.chunk != null)
            {
                stuckInChunkIndex = result.chunk.index;
                if (stuckBodyPart == -1)
                {
                    stuckRotation = Custom.Angle(throwDir.ToVector2(), stuckInChunk.Rotation);
                }
                firstChunk.MoveWithOtherObject(eu, stuckInChunk, new Vector2(0f, 0f));
                new AbstractPhysicalObject.AbstractSpearStick(abstractPhysicalObject, (result.obj as Creature).abstractCreature, stuckInChunkIndex, stuckBodyPart, stuckRotation);
            }
            else if (result.onAppendagePos != null)
            {
                stuckInChunkIndex = 0;
                stuckInAppendage = result.onAppendagePos;
                stuckRotation = Custom.VecToDeg(rotation) - Custom.VecToDeg(stuckInAppendage.appendage.OnAppendageDirection(stuckInAppendage));
                new AbstractPhysicalObject.AbstractSpearAppendageStick(abstractPhysicalObject, (result.obj as Creature).abstractCreature, result.onAppendagePos.appendage.appIndex, result.onAppendagePos.prevSegment, result.onAppendagePos.distanceToNext, stuckRotation);
            }
            if (room.BeingViewed)
            {
                for (int i = 0; i < 8; i++)
                {
                    room.AddObject(new WaterDrip(result.collisionPoint, -firstChunk.vel * (UnityEngine.Random.value * 0.5f) + Custom.DegToVec(360f * UnityEngine.Random.value) * (firstChunk.vel.magnitude * UnityEngine.Random.value * 0.5f), waterColor: false));
                }
            }
        }

        public override void RecreateSticksFromAbstract()
        {
            for (int i = 0; i < abstractPhysicalObject.stuckObjects.Count; i++)
            {
                if (abstractPhysicalObject.stuckObjects[i] is AbstractPhysicalObject.AbstractSpearStick && (abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearStick).Spear == abstractPhysicalObject && (abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearStick).LodgedIn.realizedObject != null)
                {
                    AbstractPhysicalObject.AbstractSpearStick abstractSpearStick = abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearStick;
                    stuckInObject = abstractSpearStick.LodgedIn.realizedObject;
                    stuckInChunkIndex = abstractSpearStick.chunk;
                    stuckBodyPart = abstractSpearStick.bodyPart;
                    stuckRotation = abstractSpearStick.angle;
                    ChangeMode(Mode.StuckInCreature);
                }
                else if (abstractPhysicalObject.stuckObjects[i] is AbstractPhysicalObject.AbstractSpearAppendageStick && (abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearAppendageStick).Spear == abstractPhysicalObject && (abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearAppendageStick).LodgedIn.realizedObject != null)
                {
                    AbstractPhysicalObject.AbstractSpearAppendageStick abstractSpearAppendageStick = abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearAppendageStick;
                    stuckInObject = abstractSpearAppendageStick.LodgedIn.realizedObject;
                    stuckInAppendage = new Appendage.Pos(stuckInObject.appendages[abstractSpearAppendageStick.appendage], abstractSpearAppendageStick.prevSeg, abstractSpearAppendageStick.distanceToNext);
                    stuckRotation = abstractSpearAppendageStick.angle;
                    ChangeMode(Mode.StuckInCreature);
                }
            }
        }

        public void PulledOutOfStuckObject()
        {
            for (int i = 0; i < abstractPhysicalObject.stuckObjects.Count; i++)
            {
                if (abstractPhysicalObject.stuckObjects[i] is AbstractPhysicalObject.AbstractSpearStick && (abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearStick).Spear == abstractPhysicalObject)
                {
                    abstractPhysicalObject.stuckObjects[i].Deactivate();
                    break;
                }
                if (abstractPhysicalObject.stuckObjects[i] is AbstractPhysicalObject.AbstractSpearAppendageStick && (abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearAppendageStick).Spear == abstractPhysicalObject)
                {
                    abstractPhysicalObject.stuckObjects[i].Deactivate();
                    break;
                }
            }
            stuckInObject = null;
            stuckInAppendage = null;
            stuckInChunkIndex = 0;
            pullOutAttempts = 0;
            pullOutTimer = 0;
            strongHit = false;
        }

        public override void HitSomethingWithoutStopping(PhysicalObject obj, BodyChunk chunk, Appendage appendage)
        {
            if (obj.abstractPhysicalObject.rippleLayer != abstractPhysicalObject.rippleLayer && !obj.abstractPhysicalObject.rippleBothSides && !abstractPhysicalObject.rippleBothSides)
            {
                return;
            }
            base.HitSomethingWithoutStopping(obj, chunk, appendage);
        }

        public void ProvideRotationBodyPart(BodyChunk chunk, BodyPart bodyPart)
        {
            stuckBodyPart = bodyPart.bodyPartArrayIndex;
            stuckRotation = Custom.Angle(firstChunk.vel, (bodyPart.pos - chunk.pos).normalized);
            bodyPart.vel += firstChunk.vel;
        }

        public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
        {
            if (result.obj == null)
            {
                return false;
            }
            if (result.obj.abstractPhysicalObject.rippleLayer != abstractPhysicalObject.rippleLayer && !result.obj.abstractPhysicalObject.rippleBothSides && !abstractPhysicalObject.rippleBothSides)
            {
                return false;
            }
            bool flag2 = false;
            if (result.obj is Creature)
            {
                if (!(result.obj is Player) || (result.obj as Creature).SpearStick(this, Mathf.Lerp(0.55f, 0.62f, UnityEngine.Random.value), result.chunk, result.onAppendagePos, firstChunk.vel))
                {
                    (result.obj as Creature).Violence(firstChunk, firstChunk.vel * (firstChunk.mass * 2f), result.chunk, result.onAppendagePos, Creature.DamageType.Stab, 0.1f, 20f);
                }
            }
            else if (result.chunk != null)
            {
                result.chunk.vel += firstChunk.vel * firstChunk.mass / result.chunk.mass;
            }
            else if (result.onAppendagePos != null)
            {
                (result.obj as IHaveAppendages).ApplyForceOnAppendage(result.onAppendagePos, firstChunk.vel * firstChunk.mass);
            }
            if (result.obj is Creature && (result.obj as Creature).SpearStick(this, Mathf.Lerp(0.55f, 0.62f, UnityEngine.Random.value), result.chunk, result.onAppendagePos, firstChunk.vel))
            {
                Creature creature = result.obj as Creature;
                room.PlaySound(SoundID.Spear_Stick_In_Creature, firstChunk);
                LodgeInCreature(result, eu);
                if (flag2)
                {
                    abstractPhysicalObject.world.game.GetArenaGameSession.PlayerLandSpear(thrownBy as Player, stuckInObject as Creature);
                }
                return true;
            }
            room.PlaySound(SoundID.Spear_Bounce_Off_Creauture_Shell, firstChunk);
            vibrate = 20;
            ChangeMode(Mode.Free);
            firstChunk.vel = firstChunk.vel * -0.5f + Custom.DegToVec(UnityEngine.Random.value * 360f) * (Mathf.Lerp(0.1f, 0.4f, UnityEngine.Random.value) * firstChunk.vel.magnitude);
            SetRandomSpin();
            return false;
        }

        public override void SetRandomSpin()
        {
            if (room != null)
            {
                rotationSpeed = ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f) * Mathf.Lerp(50f, 150f, UnityEngine.Random.value) * Mathf.Lerp(0.05f, 1f, room.gravity);
            }
            spinning = true;
        }
        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            string dartSprite = null;
            switch (abstractDart.dartType)
            {
                case Enums.DartType.Poison: dartSprite = "atlases/PoisonDart"; break;
                default: dartSprite = "atlases/Dart"; break;
            }
            if (abstractDart.poison > 0f)
            {
                hasPoisonGraphicsActive = true;
                sLeaser.sprites = new FSprite[2];
                sLeaser.sprites[1] = new FSprite(dartSprite);
                float width = 4f;
                float length = 11f;
                sLeaser.sprites[0] = TriangleMesh.MakeLongMesh(1, pointyTip: false, customColor: true);
                (sLeaser.sprites[0] as TriangleMesh).MoveVertice(0, new Vector2(-width, 0f));
                (sLeaser.sprites[0] as TriangleMesh).MoveVertice(1, new Vector2(width, 0f));
                (sLeaser.sprites[0] as TriangleMesh).MoveVertice(2, new Vector2(width / 1.25f, -length));
                (sLeaser.sprites[0] as TriangleMesh).MoveVertice(3, new Vector2(-width / 1.25f, -length));

                sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["PoisonSpearTip"];
            }
            else
            {
                sLeaser.sprites = new FSprite[1];
                sLeaser.sprites[0] = new FSprite(dartSprite);
            }
            AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (reinitiateSpritesOnDraw)
            {
                reinitiateSpritesOnDraw = false;
                sLeaser.RemoveAllSpritesFromContainer();
                InitiateSprites(sLeaser, rCam);
                ApplyPalette(sLeaser, rCam, rCam.currentPalette);
                if (mode == Mode.StuckInCreature)
                {
                    ChangeOverlap(newOverlap: true);
                    ChangeOverlap(newOverlap: false);
                }
            }
            Vector2 vector = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
            if (vibrate > 0)
            {
                vector += Custom.DegToVec(UnityEngine.Random.value * 360f) * (2f * UnityEngine.Random.value);
            }
            Vector3 vector2 = Vector3.Slerp(lastRotation, rotation, timeStacker);
            for (int num = ((hasPoisonGraphicsActive) ? 1 : 0); num >= 0; num--)
            {
                sLeaser.sprites[num].x = vector.x - camPos.x;
                sLeaser.sprites[num].y = vector.y - camPos.y;
                sLeaser.sprites[num].anchorY = Mathf.Lerp(lastPivotAtTip ? 0.85f : 0.5f, pivotAtTip ? 0.85f : 0.5f, timeStacker);
                sLeaser.sprites[num].rotation = Custom.AimFromOneVectorToAnother(new Vector2(0f, 0f), vector2);
            }
            if (hasPoisonGraphicsActive)
            {
                Vector2 vector3 = Custom.DirVec(new Vector2(0f, 0f), vector2);
                float num2 = Mathf.Lerp(lastPivotAtTip ? 6f : 21f, pivotAtTip ? 6f : 21f, timeStacker);
                Vector2 vector4 = vector + vector3 * num2;
                sLeaser.sprites[0].x = vector4.x - camPos.x;
                sLeaser.sprites[0].y = vector4.y - camPos.y;
                //Color rgb = new HSLColor(Enums.Colors.PoisonColor, 1f, 0.5f).rgb;
                Color rgb = Enums.Colors.PoisonColor;
                rgb = new Color(rgb.r, rgb.g, rgb.b, abstractDart.poison);
                Color b = new Color(color.r, color.g, color.b, abstractDart.poison);
                for (int i = 0; i < 2; i++)
                {
                    float num3 = Mathf.InverseLerp(0f, 1f, i);
                    (sLeaser.sprites[0] as TriangleMesh).verticeColors[i * 2] = Color.Lerp(rgb, b, num3 * 0.4f);
                    (sLeaser.sprites[0] as TriangleMesh).verticeColors[i * 2 + 1] = Color.Lerp(rgb, b, num3 * 0.4f);
                }
                if (blink > 0 && UnityEngine.Random.value < 0.5f)
                {
                    sLeaser.sprites[1].color = blinkColor;
                }
                else
                {
                    sLeaser.sprites[1].color = color;
                }
                if (abstractDart.poison <= 0f)
                {
                    hasPoisonGraphicsActive = false;
                    reinitiateSpritesOnDraw = true;
                }
            }
            else if (blink > 0 && UnityEngine.Random.value < 0.5f)
            {
                sLeaser.sprites[0].color = blinkColor;
            }
            else
            {
                sLeaser.sprites[0].color = color;
            }
            if (slatedForDeletetion || room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            color = palette.blackColor;
            sLeaser.sprites[0].color = color;
        }
    }
}