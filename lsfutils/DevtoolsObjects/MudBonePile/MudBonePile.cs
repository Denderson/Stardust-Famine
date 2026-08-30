using System.Linq;
using UnityEngine;
using RWCustom;
using DevInterface;
using System;

namespace lsfUtils.DevtoolsObjects.MudBonePile
{

    public class MudBonePile : UpdatableAndDeletable, IDrawable
    {
        public PlacedObject pObj;
        public MudBonePileData data => (MudBonePileData)pObj.data;

        public int mudSpriteCount;
        public int boneSpriteCount;
        public int fgClumpCount;

        private RoomPalette? currentPalette;

        private float lastFgRatio = -1f;
        private float lastMudHue = -1f;
        private float lastMudLightness = -1f;

        private Vector2[] spriteLocalPos;
        private float[] spriteScaleX;
        private float[] spriteScaleY;
        private float[] spriteRot;
        private bool[] spriteVis;
        private float[] spriteSquish;

        private int lastSeed = -1;
        private Vector2 lastHandlePos = new Vector2(-9999, -9999);
        private float lastMudAmount = -1f;
        private float lastBoneDensity = -1f;

        private int particleCooldown = 0;

        public MudBonePile(PlacedObject placedObj, Room rm)
        {
            pObj = placedObj;
            room = rm;
            GenerateLayout();
        }

        public override void Update(bool eu) // considering to rewrite this huge ass nest but eeeeh fuck it
        {
            base.Update(eu);

            float pileWidth = Mathf.Max(15f, Mathf.Abs(data.handlePos.x));
            float pileHeight = Mathf.Max(10f, Mathf.Abs(data.handlePos.y));

            if (particleCooldown > 0) particleCooldown--;

            if (data.isSquishy)
            {
                for (int i = 0; i < mudSpriteCount; i++)
                {
                    if (spriteSquish[i] > 0f)
                    {
                        spriteSquish[i] = Mathf.Max(0f, spriteSquish[i] - 0.05f); //adj
                    }
                }

                if (room != null && room.abstractRoom != null)
                {
                    foreach (AbstractCreature absCrit in room.abstractRoom.creatures)
                    {
                        if (absCrit.realizedCreature != null && absCrit.realizedCreature.room == room)
                        {
                            Creature crit = absCrit.realizedCreature;
                            foreach (BodyChunk chunk in crit.bodyChunks)
                            {
                                if (Mathf.Abs(chunk.pos.x - pObj.pos.x) < pileWidth + 50f &&
                                    chunk.pos.y > pObj.pos.y - 20f && chunk.pos.y < pObj.pos.y + pileHeight + 50f)
                                {
                                    for (int i = 0; i < mudSpriteCount; i++)
                                    {
                                        if (!spriteVis[i]) continue;

                                        Vector2 spriteWorldPos = pObj.pos + spriteLocalPos[i];
                                        if (Custom.DistLess(chunk.pos, spriteWorldPos, 15f + chunk.rad))
                                        {
                                            float force = Mathf.Clamp01(chunk.vel.magnitude * chunk.mass * 0.05f);

                                            if (force > 0.05f && spriteSquish[i] < 0.8f)
                                            {
                                                spriteSquish[i] = Mathf.Min(1f, spriteSquish[i] + force * 0.5f);

                                                if (particleCooldown <= 0 && chunk.vel.magnitude > 2.5f && UnityEngine.Random.value < 0.4f) // doesnt work properly
                                                {
                                                    particleCooldown = UnityEngine.Random.Range(4, 12);
                                                    Color mudCol = currentPalette.HasValue ? Color.Lerp(currentPalette.Value.fogColor, Custom.HSL2RGB(data.mudHue, 0.4f, data.mudLightness), 0.7f) : new Color(0.2f, 0.15f, 0.1f);
                                                    room.AddObject(new MudParticle(spriteWorldPos + new Vector2(0f, 5f), chunk.vel * 0.3f + new Vector2(UnityEngine.Random.Range(-1.5f, 1.5f), UnityEngine.Random.Range(1f, 4f)), mudCol));
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
            else
            {
                for (int i = 0; i < mudSpriteCount; i++)
                {
                    if (spriteSquish[i] > 0f) spriteSquish[i] = 0f;
                }
            }
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) // sets up the sprites
        {
            int totalSprites = mudSpriteCount + boneSpriteCount;
            sLeaser.sprites = new FSprite[totalSprites];

            for (int i = 0; i < mudSpriteCount; i++)
            {
                sLeaser.sprites[i] = new FSprite("Circle20", true); // only using circle20 for the mud
            }
            // for the bones im using my own atlas and model citizen's bones
            string[] boneSprites = { "hip3", "bacculum", "boneshard1", "boneshard2", "boneshard3", "fatbone2", "bonefragment1", "bonefragment2" };
            UnityEngine.Random.State state = UnityEngine.Random.state;
            UnityEngine.Random.InitState(data.randomSeed);

            for (int i = 0; i < boneSpriteCount; i++) // this just randomly picks the bones
            {
                int spriteIdx = mudSpriteCount + i;
                string randomBone = boneSprites[UnityEngine.Random.Range(0, boneSprites.Length)];
                sLeaser.sprites[spriteIdx] = new FSprite(randomBone, true);
            }

            UnityEngine.Random.state = state;
            AddToContainer(sLeaser, rCam, null);
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (slatedForDeletetion || room != rCam.room) //clean up after deleting or going in another room
            {
                sLeaser.CleanSpritesAndRemove();
                return;
            }

            bool layoutChanged = false;
            bool colorsChanged = false;
            // check if any values are changed and generate a new layout or just update the colors
            if (lastSeed != data.randomSeed || lastHandlePos != data.handlePos || lastMudAmount != data.mudAmount || lastBoneDensity != data.boneDensity)
            {
                GenerateLayout();
                layoutChanged = true;
            }

            if (lastMudHue != data.mudHue || lastMudLightness != data.mudLightness || lastFgRatio != data.fgRatio)
            {
                colorsChanged = true;
            }

            int totalSprites = mudSpriteCount + boneSpriteCount;

            if (sLeaser.sprites == null || sLeaser.sprites.Length != totalSprites)
            {
                int savedBgIndex = -1;
                FContainer bg = rCam.ReturnFContainer("Midground"); // fix for the fg bug

                if (sLeaser.sprites != null)
                {
                    for (int i = 0; i < sLeaser.sprites.Length; i++)
                    {
                        if (sLeaser.sprites[i] != null && sLeaser.sprites[i].container == bg)
                        {
                            savedBgIndex = bg.GetChildIndex(sLeaser.sprites[i]);
                            break;
                        }
                    }

                    foreach (var sprite in sLeaser.sprites)
                    {
                        sprite.RemoveFromContainer();
                    }
                }

                InitiateSprites(sLeaser, rCam);

                if (savedBgIndex != -1)
                {
                    int currentIndex = savedBgIndex;
                    for (int i = 0; i < totalSprites; i++)
                    {
                        if (sLeaser.sprites[i].container == bg)
                        {
                            sLeaser.sprites[i].RemoveFromContainer();
                            bg.AddChildAtIndex(sLeaser.sprites[i], currentIndex);
                            currentIndex++;
                        }
                    }
                }

                layoutChanged = true;
            }

            if ((layoutChanged || colorsChanged) && currentPalette.HasValue) // if either layout or colors are flagged apply new colors
            {
                ApplyColors(sLeaser, rCam);
            }

            Vector2 basePos = pObj.pos;

            for (int i = 0; i < totalSprites; i++) //loops through every sprite, cals their pos, adds squish to the mud and finally adds designated scale and rotation
            {
                sLeaser.sprites[i].isVisible = spriteVis[i];
                if (spriteVis[i])
                {
                    sLeaser.sprites[i].x = basePos.x + spriteLocalPos[i].x - camPos.x;
                    sLeaser.sprites[i].y = basePos.y + spriteLocalPos[i].y - camPos.y;

                    float squishX = 1f;
                    float squishY = 1f;

                    if (i < mudSpriteCount)
                    {
                        squishX = 1f + spriteSquish[i] * 0.4f;
                        squishY = 1f - spriteSquish[i] * 0.4f;
                    }

                    sLeaser.sprites[i].scaleX = spriteScaleX[i] * squishX;
                    sLeaser.sprites[i].scaleY = spriteScaleY[i] * squishY;
                    sLeaser.sprites[i].rotation = spriteRot[i];
                }
            }
        }


        private void GenerateLayout()
        {
            // store current state vars
            lastSeed = data.randomSeed;
            lastHandlePos = data.handlePos;
            lastMudAmount = data.mudAmount;
            lastBoneDensity = data.boneDensity;

            Vector2 spread = data.handlePos;
            float pileWidth = Mathf.Max(15f, Mathf.Abs(spread.x));
            float pileHeight = Mathf.Max(10f, Mathf.Abs(spread.y));
            float ySign = Mathf.Sign(spread.y); // added this because now you could make a ceiling pile 

            float actualArea = pileWidth * pileHeight;
            float maxArea = 20000f;
            float effectiveArea = Mathf.Min(actualArea, maxArea);
            float scaleFix = actualArea > maxArea ? Mathf.Sqrt(actualArea / maxArea) : 1f;

            // alr this should calculate all the sprite counts including foreground sprites
            mudSpriteCount = Mathf.Max(10, Mathf.RoundToInt(effectiveArea * 0.05f * data.mudAmount));
            boneSpriteCount = Mathf.Max(0, Mathf.RoundToInt(effectiveArea * 0.016f * data.boneDensity));
            fgClumpCount = Mathf.Max(2, Mathf.RoundToInt(mudSpriteCount * 0.15f));

            int total = mudSpriteCount + boneSpriteCount;

            if (spriteLocalPos == null || spriteLocalPos.Length != total)
            {
                spriteLocalPos = new Vector2[total];
                spriteScaleX = new float[total];
                spriteScaleY = new float[total];
                spriteRot = new float[total];
                spriteVis = new bool[total];
                spriteSquish = new float[total];
            }
            // prepare the random seed and width
            UnityEngine.Random.State state = UnityEngine.Random.state;
            UnityEngine.Random.InitState(data.randomSeed);

            float spreadMultiplier = Mathf.Lerp(0.4f, 1.0f, data.mudAmount);

            float moundWidth = pileWidth * spreadMultiplier;

            float GetMoundHeightAt(float nx) // helper
            {
                float moundCurve = Mathf.Pow(Mathf.Clamp01(1f - nx * nx), 1.3f);
                return moundCurve * pileHeight * Mathf.Lerp(0.5f, 1.0f, data.mudAmount);
            }

            float GetTerrainLift(float localX) //helper that hopefully checks 3 tiles above or below the current pos to locate solid ground, older helper did not work properly
            {
                if (room == null) return 0f;
                float dir = ySign != 0f ? ySign : 1f;
                Vector2 baseWorldPos = pObj.pos + new Vector2(localX, 0f);
                float originalY = baseWorldPos.y;

                int maxSearchTiles = 3;
                bool foundSurface = false;
                float bestY = originalY;

                if (room.GetTile(baseWorldPos).Solid)
                {
                    for (int i = 0; i <= maxSearchTiles; i++)
                    {
                        Vector2 testPos = baseWorldPos + new Vector2(0f, i * 20f * dir);
                        if (!room.GetTile(testPos).Solid)
                        {
                            bestY = room.MiddleOfTile(testPos - new Vector2(0f, 20f * dir)).y + 10f * dir;
                            foundSurface = true;
                            break;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i <= maxSearchTiles; i++)
                    {
                        Vector2 testPos = baseWorldPos - new Vector2(0f, i * 20f * dir);
                        if (room.GetTile(testPos).Solid)
                        {
                            bestY = room.MiddleOfTile(testPos).y + 10f * dir;
                            foundSurface = true;
                            break;
                        }
                    }
                }

                if (foundSurface) return bestY - originalY;
                return 0f;
            }

            float GetSurfaceHeightAt(float localX) // another helper, combines mound height and terrain lift
            {
                float nx = Mathf.Clamp(localX / (moundWidth > 0f ? moundWidth : 1f), -1f, 1f);
                float moundH = GetMoundHeightAt(nx) * ySign;
                float lift = GetTerrainLift(localX);
                return moundH + lift;
            }

            float GetSlopeAngleAt(float localX) // another new helper, samples two points left and right and uses atan2 to calculate angle slope
            {
                float delta = 6f;
                float h1 = GetSurfaceHeightAt(localX - delta);
                float h2 = GetSurfaceHeightAt(localX + delta);
                float dy = h2 - h1;
                float dx = delta * 2f;
                return -Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
            }

            for (int i = 0; i < mudSpriteCount; i++) // mud generation loop, works fine, should update
            {
                bool isForeground = i < fgClumpCount;
                int bgIndex = isForeground ? 0 : i - fgClumpCount;
                int bgTotal = Mathf.Max(1, mudSpriteCount - fgClumpCount);

                spriteVis[i] = true;

                float depthProgress = isForeground ? 1f : (float)bgIndex / bgTotal;

                float nx = UnityEngine.Random.Range(-1f, 1f);
                nx = Mathf.Sign(nx) * Mathf.Pow(Mathf.Abs(nx), Mathf.Lerp(1.5f, 1.0f, data.mudAmount));

                if (!isForeground && depthProgress < 0.4f) nx *= 0.65f;
                if (isForeground) nx = UnityEngine.Random.Range(-0.85f, 0.85f);

                float peakHeight = GetMoundHeightAt(nx);

                float ny;

                if (isForeground)
                {
                    ny = UnityEngine.Random.Range(-0.02f, 0.05f);
                }
                else
                {
                    float targetY = Mathf.Lerp(0.85f, 0.05f, depthProgress);
                    ny = targetY * UnityEngine.Random.Range(0.8f, 1.1f);
                }

                float xPos = nx * moundWidth + UnityEngine.Random.Range(-1.5f, 1.5f);
                float baseLift = GetTerrainLift(xPos);

                float yPos = ny * peakHeight * ySign + baseLift + (isForeground ? UnityEngine.Random.Range(-0.5f, 0.5f) : UnityEngine.Random.Range(-1.5f, 1.5f));

                if (room != null)
                {
                    Vector2 worldPos = pObj.pos + new Vector2(xPos, yPos);

                    int tries = 0;
                    float dir = ySign != 0f ? ySign : 1f;

                    while (room.GetTile(worldPos).Solid && tries < 20)
                    {
                        worldPos.y += dir * 2f;
                        tries++;
                    }
                    xPos = worldPos.x - pObj.pos.x;
                    yPos = worldPos.y - pObj.pos.y;
                }

                spriteLocalPos[i] = new Vector2(xPos, yPos);

                float baseScale = Mathf.Lerp(1.5f, 0.9f, depthProgress) * scaleFix;
                if (isForeground) baseScale = UnityEngine.Random.Range(0.3f, 0.7f) * scaleFix;

                float stretchX = UnityEngine.Random.Range(1.1f, 1.5f);
                float stretchY = UnityEngine.Random.Range(0.5f, 0.8f);

                spriteScaleX[i] = baseScale * stretchX;
                spriteScaleY[i] = baseScale * stretchY;

                float slopeAngle = GetSlopeAngleAt(xPos);
                spriteRot[i] = Mathf.Clamp(slopeAngle * 0.6f, -45f, 45f) + UnityEngine.Random.Range(-10f, 10f); //adj
            }

            for (int i = 0; i < boneSpriteCount; i++) // bone generation loop
            {
                int spriteIdx = mudSpriteCount + i;
                spriteVis[spriteIdx] = true;

                float nx = UnityEngine.Random.Range(-0.9f, 0.9f);
                nx = Mathf.Sign(nx) * Mathf.Pow(Mathf.Abs(nx), Mathf.Lerp(1.5f, 1.0f, data.mudAmount));
                float surfaceHeight = GetMoundHeightAt(nx);
                float ny = UnityEngine.Random.Range(0.2f, 1.0f);

                float xPos = nx * moundWidth + UnityEngine.Random.Range(-3f, 3f);
                float baseLift = GetTerrainLift(xPos);
                float yPos = ny * surfaceHeight * ySign + baseLift;

                if (room != null)
                {
                    Vector2 worldPos = pObj.pos + new Vector2(xPos, yPos);

                    int tries = 0;
                    float dir = ySign != 0f ? ySign : 1f;
                    while (room.GetTile(worldPos).Solid && tries < 20)
                    {
                        worldPos.y += dir * 2f;
                        tries++;
                    }
                    xPos = worldPos.x - pObj.pos.x;
                    yPos = worldPos.y - pObj.pos.y;
                }

                spriteLocalPos[spriteIdx] = new Vector2(xPos, yPos);
                float slopeAngle = GetSlopeAngleAt(xPos);

                if (i % 6 == 0) // fix and added a 1/6 chance of bone to randomly rotate, while most just stay +/- 20 degrees of the slope angle
                {
                    float scale = UnityEngine.Random.Range(0.6f, 1.0f) * scaleFix;
                    spriteScaleX[spriteIdx] = scale;
                    spriteScaleY[spriteIdx] = scale * UnityEngine.Random.Range(0.9f, 1.1f);
                    spriteRot[spriteIdx] = slopeAngle + UnityEngine.Random.Range(0f, 360f);
                }
                else
                {
                    float baseScale = UnityEngine.Random.Range(0.7f, 1.2f) * scaleFix;
                    spriteScaleX[spriteIdx] = baseScale;
                    spriteScaleY[spriteIdx] = baseScale * UnityEngine.Random.Range(0.9f, 1.2f);
                    spriteRot[spriteIdx] = UnityEngine.Random.value < 0.75f ? slopeAngle + UnityEngine.Random.Range(-20f, 20f) : UnityEngine.Random.Range(0f, 360f);
                }
            }

            UnityEngine.Random.state = state; // restore
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            currentPalette = palette;
            ApplyColors(sLeaser, rCam);
        }

        private void ApplyColors(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            // make sure room has a palette and the sprites have been generated properly
            if (!currentPalette.HasValue || sLeaser.sprites == null || sLeaser.sprites.Length != mudSpriteCount + boneSpriteCount) return;

            RoomPalette palette = currentPalette.Value;

            lastFgRatio = data.fgRatio;
            lastMudHue = data.mudHue;
            lastMudLightness = data.mudLightness;

            // fog calculations for the fg/bg dev slider
            float blend = data.fgRatio;
            Color shadow = Color.Lerp(palette.fogColor, palette.blackColor, blend);
            Color fog = palette.fogColor;
            float fogAmount = palette.fogAmount * Mathf.Lerp(1.2f, 0.1f, blend);

            // base mud calculations, takes hue and lightness and creates a dark and normal version
            Color rawDeep = Custom.HSL2RGB(data.mudHue, 0.45f, data.mudLightness * 0.4f);
            Color rawSurface = Custom.HSL2RGB(data.mudHue, 0.40f, data.mudLightness);
            Color deepMud = Color.Lerp(shadow, rawDeep, 0.6f);
            Color surfaceMud = Color.Lerp(shadow, rawSurface, 0.8f);

            int bgTotal = Mathf.Max(1, mudSpriteCount - fgClumpCount);

            for (int i = 0; i < mudSpriteCount; i++) // loop through every mud sprite
            {
                if (!spriteVis[i]) continue;

                // calculate depth progress
                bool isForeground = i < fgClumpCount;
                int bgIndex = isForeground ? 0 : i - fgClumpCount;
                float depthProgress = isForeground ? 1f : (float)bgIndex / bgTotal;

                Color mudColor = Color.Lerp(deepMud, surfaceMud, depthProgress); // combines the colors and depending on how far its sitting it gets darker, so it has that gradient look
                float spriteFog = Mathf.Lerp(fogAmount * 1.5f, fogAmount * 0.4f, depthProgress); // rewrote this due to bug

                if (isForeground) spriteFog *= 0.5f;

                sLeaser.sprites[i].color = Color.Lerp(mudColor, fog, spriteFog); // mix mud color and fog
            }

            // paints the bone sprites
            Color deepBone = Color.Lerp(shadow, new Color(0.40f, 0.38f, 0.32f), 0.7f);
            Color brightBone = Color.Lerp(Color.white, shadow, 0.25f);

            for (int i = 0; i < boneSpriteCount; i++)
            {
                int spriteIdx = mudSpriteCount + i;
                if (!spriteVis[spriteIdx]) continue;

                float depthProgress = (float)i / Mathf.Max(1, boneSpriteCount - 1);
                Color boneColor = Color.Lerp(deepBone, brightBone, depthProgress);
                boneColor = Color.Lerp(boneColor, surfaceMud, 0.25f);

                float spriteFog = Mathf.Lerp(fogAmount * 1.2f, fogAmount * 0.4f, depthProgress);
                sLeaser.sprites[spriteIdx].color = Color.Lerp(boneColor, fog, spriteFog);
            }
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
        {
            FContainer bgContainer = rCam.ReturnFContainer("Midground");
            FContainer fgContainer = rCam.ReturnFContainer("Foreground");

            int totalSprites = mudSpriteCount + boneSpriteCount;
            if (sLeaser.sprites == null || sLeaser.sprites.Length != totalSprites) return;

            for (int i = 0; i < totalSprites; i++)
            {
                sLeaser.sprites[i].RemoveFromContainer();
                if (i < fgClumpCount) fgContainer.AddChild(sLeaser.sprites[i]);
                else bgContainer.AddChild(sLeaser.sprites[i]);
            }
        }
    }
}