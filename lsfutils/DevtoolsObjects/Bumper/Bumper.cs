using Pom;
using RWCustom;
using System.Collections.Generic;
using UnityEngine;
using static Pom.Pom;

namespace lsfUtils.DevtoolsObjects.Bumper
{
    public class Bumper : UpdatableAndDeletable, IDrawable
    {
        public PlacedObject placedObject;
        public BumperData Data => placedObject.data as BumperData;

        public readonly Dictionary<BodyChunk, int> cooldowns = [];
        
        public const int bounceDuration = 20;
        public const int bounceCooldown = 30;

        public int bounceTimer = bounceDuration;
        public Vector2 lastBounceDir = Vector2.right;
        private static Vector2? bumperSpriteSize;

        public Bumper(PlacedObject placedObject, Room room)
        {
            this.placedObject = placedObject;
            this.room = room;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            if (slatedForDeletetion || room == null) return;

            if (bounceTimer < bounceDuration) bounceTimer++;

            TickCooldowns();

            Vector2 center = placedObject.pos;
            float radius = Data.radius.magnitude;

            List<BodyChunk> toLaunch = null;

            foreach (var updatable in room.updateList)
            {
                if (updatable is not PhysicalObject phys) continue;
                if (phys is Player p && p.dead) continue;

                foreach (var chunk in phys.bodyChunks)
                {
                    if (cooldowns.ContainsKey(chunk)) continue;
                    if (Vector2.Distance(chunk.pos, center) > radius + chunk.rad) continue;

                    (toLaunch ??= []).Add(chunk);
                }
            }

            if (toLaunch != null)
            {
                foreach (var chunk in toLaunch)
                {
                    Launch(chunk, center);
                }
            }
        }

        public void TickCooldowns()
        {
            List<BodyChunk> expired = null;
            var keys = new List<BodyChunk>(cooldowns.Keys);
            foreach (var chunk in keys)
            {
                cooldowns[chunk]--;
                if (cooldowns[chunk] <= 0f) (expired ??= []).Add(chunk);
            }
            if (expired != null) foreach (var c in expired) cooldowns.Remove(c);
        }

        public void Launch(BodyChunk chunk, Vector2 center)
        {
            Vector2 dir = chunk.pos - center;
            dir = dir == Vector2.zero ? Custom.RNV() : dir.normalized;

            float incomingSpeed = chunk.vel.magnitude;
            float launchSpeed = Mathf.Max(Data.force, incomingSpeed * Data.bounciness);

            chunk.vel = dir * launchSpeed;
            chunk.pos += dir * 2f;

            if (chunk.owner is Player player)
            {
                player.standing = false;
            }

            cooldowns[chunk] = bounceCooldown;
            bounceTimer = 0;
            lastBounceDir = dir;

            room.PlaySound(SoundID.Rock_Bounce_Off_Creature_Shell, chunk.pos, 0.35f, 1.7f);

            for (int i = 0; i < 6; i++)
            {
                room.AddObject(new Spark(chunk.pos, Custom.RNV() * Random.value * 6f, Color.white, null, 8, 20));
            }
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite("atlases/Bumper1", true);

            if (bumperSpriteSize == null)
            {
                var element = Futile.atlasManager.GetElementWithName("atlases/Bumper1");
                bumperSpriteSize = element != null ? element.sourcePixelSize : new Vector2(100f, 100f);
            }

            AddToContainer(sLeaser, rCam, null);
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 pos = placedObject.pos - camPos;

            float diameter = Data.radius.magnitude * 2f;
            Vector2 size = bumperSpriteSize ?? new Vector2(100f, 100f);
            float baseScaleX = diameter / size.x;
            float baseScaleY = diameter / size.y;

            float t = Mathf.Clamp01((bounceTimer + timeStacker) / bounceDuration);
            (float axis, float perp) = SquashStretchCurve(t);

            sLeaser.sprites[0].rotation = Custom.VecToDeg(lastBounceDir);
            sLeaser.sprites[0].x = pos.x;
            sLeaser.sprites[0].y = pos.y;
            sLeaser.sprites[0].scaleX = baseScaleX * axis;
            sLeaser.sprites[0].scaleY = baseScaleY * perp;
        }

        public static (float axis, float perp) SquashStretchCurve(float t)
        {
            if (t < 0.25f)
            {
                float s = EaseOut(Mathf.InverseLerp(0f, 0.25f, t));
                return (Mathf.Lerp(1f, 0.55f, s), Mathf.Lerp(1f, 1.5f, s));
            }
            if (t < 0.6f)
            {
                float s = EaseInOut(Mathf.InverseLerp(0.25f, 0.6f, t));
                return (Mathf.Lerp(0.55f, 1.5f, s), Mathf.Lerp(1.5f, 0.6f, s));
            }
            {
                float s = EaseOut(Mathf.InverseLerp(0.6f, 1f, t));
                return (Mathf.Lerp(1.5f, 1f, s), Mathf.Lerp(0.6f, 1f, s));
            }
        }

        public static float EaseOut(float x)
        {
            return 1f - (1f - x) * (1f - x);
        }
        public static float EaseInOut(float x)
        {
            return x < 0.5f ? 2f * x * x : 1f - Mathf.Pow(-2f * x + 2f, 2f) / 2f;
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
        {
            newContainer ??= rCam.ReturnFContainer("Background");
            foreach (var s in sLeaser.sprites)
            {
                s.RemoveFromContainer();
                newContainer.AddChild(s);
            }
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) 
        {

        }
    }
}