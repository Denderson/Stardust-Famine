using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace lsfUtils.DevtoolsEffects.StardustRain;

// nvm the todo i just fucking fixed the issue
public class StardustFallGraphics : UpdatableAndDeletable, IDrawable
{
    private struct Particle
    {
        //var
        public Vector2 pos;
        public Vector2 prevPos;
        public Vector2 spd;
        public float sz;
        public float baseFade;
        public float fade;
        public float age;
        public float maxAge;
        public float weight;
        public float wiggleTimer;
        public float wiggleSpd;
        public float driftPush;
        public float hueTimer;
        public Color customColor;

        public StardustLight glow;
        public float flickerTimer;
    }

    private int[] gridHeads;
    private int[] gridNext;
    private int cols;
    private int rows;
    private const float cellSize = 50f; // could be re adjusted

    private struct FlatChunk { public Vector2 pos; public float force; }
    private FlatChunk[] nearbyChunks;
    private int chunkCount;

    private Particle[] dustList;
    private int dustCount;
    private Color mainColor = new Color(0.85f, 0.9f, 1.0f);
    private readonly Color initColor = Color.Lerp(new Color(0.45f, 0.9f, 0.25f), new Color(0.85f, 0.85f, 0.2f), 0.5f);
    private const float invTileSize = 0.05f; // 1 / 20f
    private const float tau = 6.2831853f;

    public StardustFallGraphics(Room room, float amt)
    {
        this.room = room;
        // room vars
        float roomW = room.PixelWidth;
        float roomH = room.PixelHeight;
        float roomSize = roomW * roomH;
        float screenArea = 1366f * 768f;

        float dustPerScreen = 100f * amt;

        dustCount = (int)(roomSize / screenArea * dustPerScreen);
        dustCount = Mathf.Clamp(dustCount, 10, 2500);
        dustList = new Particle[dustCount];

        cols = Mathf.CeilToInt((roomW + 800f) / cellSize);
        rows = Mathf.CeilToInt((roomH + 200f) / cellSize);
        gridHeads = new int[cols * rows];
        gridNext = new int[dustCount];
        nearbyChunks = new FlatChunk[400];

        for (int i = 0; i < dustCount; i++)
        {
            ResetParticle(i, true, roomW, roomH);
            dustList[i].age = UnityEngine.Random.Range(0f, dustList[i].maxAge);
        }
    }

    public StardustFallGraphics(RoomCamera cam, float amt) : this(cam != null ? cam.room : null, amt) { }

    private void ResetParticle(int idx, bool randY, float rW, float rH, bool underCeilings = true, float extraW = 100f)
    {
        ref Particle p = ref dustList[idx];

        float x = UnityEngine.Random.Range(-extraW, rW + extraW);
        float y = randY ? UnityEngine.Random.Range(0f, rH) : rH + UnityEngine.Random.Range(50f, 200f);

        if (room != null)
        {
            if (randY)
            {
                for (int k = 0; k < 4; k++)
                {
                    int tileX = (int)(x * invTileSize);
                    int tileY = (int)(y * invTileSize);

                    bool spawnOk = false;

                    if (tileX < 0 || tileX >= room.TileWidth || tileY >= room.TileHeight)
                    {
                        spawnOk = true;
                    }
                    else if (tileY >= 0 && !room.GetTile(tileX, tileY).Solid)
                    {
                        spawnOk = true;
                    }

                    if (spawnOk)
                        break;

                    x = UnityEngine.Random.Range(-extraW, rW + extraW);
                    y = UnityEngine.Random.Range(0f, rH);
                }
            }
            else
            {
                int tileX = Mathf.Clamp((int)(x * invTileSize), 0, room.TileWidth - 1);
                int tileY = room.TileHeight - 1;

                if (underCeilings)
                {
                    while (tileY >= 0 && room.GetTile(tileX, tileY).Solid)
                    {
                        tileY--;
                    }

                    if (tileY < room.TileHeight - 1)
                    {
                        y = tileY * 20f + UnityEngine.Random.Range(0f, 18f);
                    }
                }
                else
                {
                    for (int k = 0; k < 10; k++)
                    {
                        if (!room.GetTile(tileX, tileY).Solid)
                            break;

                        x = UnityEngine.Random.Range(-extraW, rW + extraW);
                        tileX = Mathf.Clamp((int)(x * invTileSize), 0, room.TileWidth - 1);
                    }
                }
            }
        }

        p.pos = new Vector2(x, y);
        p.prevPos = p.pos;

        p.spd = new Vector2(UnityEngine.Random.Range(-0.4f, 0.4f), UnityEngine.Random.Range(-1.2f, -0.3f));
        p.sz = UnityEngine.Random.Range(2.5f, 5.0f);
        p.baseFade = UnityEngine.Random.Range(0.5f, 1.0f) * 0.9f;
        p.fade = 0f;
        p.age = 0f;
        p.maxAge = UnityEngine.Random.Range(250f, 800f);
        p.weight = UnityEngine.Random.Range(0.5f, 1.2f);

        p.wiggleTimer = UnityEngine.Random.value * tau;
        p.wiggleSpd = UnityEngine.Random.Range(0.008f, 0.03f);
        p.driftPush = UnityEngine.Random.Range(0.01f, 0.05f);

        p.hueTimer = UnityEngine.Random.value * tau;
        p.customColor = new Color(
            UnityEngine.Random.Range(0.85f, 1.15f),
            UnityEngine.Random.Range(0.85f, 1.15f),
            UnityEngine.Random.Range(0.70f, 1.30f)
        );

        p.flickerTimer = UnityEngine.Random.value * tau;

        if (p.glow == null)
        {
            p.glow = new StardustLight(p.pos, initColor, this)
            {
                paletteDarkness = 0.4f
            };
            if (room != null) room.AddObject(p.glow);
        }
        else
        {
            p.glow.nextPos = p.pos;
            p.glow.prevPos = p.pos;
            p.glow.nextRadius = 0f;
            p.glow.prevRadius = 0f;
        }
    }

    public override void Update(bool evenUpdate)
    {
        base.Update(evenUpdate);
        if (room == null) { slatedForDeletetion = true; return; }

        float lifeSpeed = 1f;
        float moveSpeed = 1f;
        float pushForce = 1f;
        float sway = 0f;
        bool spawnBelow = true;
        bool strictWallCheck = true;
        float extraWidth = 100f;
        float currentHue1 = 0.30f;
        float currentHue2 = 0.15f;
        float currentLightness = 0.5f;

        foreach (PlacedObject pThing in room.roomSettings.placedObjects)
        {
            if (pThing.active && pThing.type == StardustEnums.StardustController && pThing.data is StardustControllerData ctrlData)
            {
                lifeSpeed = ctrlData.lifeMult;
                moveSpeed = ctrlData.speedMult;
                pushForce = ctrlData.pushForce;
                sway = ctrlData.swayDir;
                spawnBelow = ctrlData.spawnBelow;
                strictWallCheck = ctrlData.strictWallCheck;
                extraWidth = ctrlData.extraWidth;
                currentHue1 = ctrlData.hue1;
                currentHue2 = ctrlData.hue2;
                currentLightness = ctrlData.lightness;
                break;
            }
        }

        float rW = room.PixelWidth;
        float rH = room.PixelHeight;

        // working le fine
        CacheBodyChunks();
        UpdateSpatialGrid();
        ApplyPtPGrid();

        Color greenish = Color.HSVToRGB(currentHue1, 1f, currentLightness);
        Color yellowish = Color.HSVToRGB(currentHue2, 1f, currentLightness);
        // stardust color withing shader, too lazy to switch the names xd
        Shader.SetGlobalColor("_StardustColor1", greenish);
        Shader.SetGlobalColor("_StardustColor2", yellowish);

        for (int i = 0; i < dustCount; i++)
        {
            ref Particle p = ref dustList[i];
            p.prevPos = p.pos;

            p.spd.y -= 0.035f * p.weight * moveSpeed;

            p.wiggleTimer += p.wiggleSpd * moveSpeed;
            p.spd.x += (Mathf.Sin(p.wiggleTimer) * p.driftPush + sway * 0.05f) * moveSpeed;

            p.spd.x *= 0.92f;
            p.spd.y *= 0.92f;

            ApplyPhyslInt(ref p, pushForce);

            p.pos.x += p.spd.x;
            p.pos.y += p.spd.y;

            p.age += 1f / lifeSpeed;

            int tileX = Mathf.FloorToInt(p.pos.x / 20f);
            int tileY = Mathf.FloorToInt(p.pos.y / 20f);

            bool hitWall = false;
            // switching to PixelWidth kind of worked but made them spawn way to far
            if (tileX >= 0 && tileX < room.TileWidth && tileY >= 0 && tileY < room.TileHeight) // updating the update how ironic
            {
                hitWall = room.GetTile(tileX, tileY).Solid;
            }

            bool dampCondition = false;
            if (strictWallCheck)
            {
                dampCondition = tileY < 0 || hitWall;
            }
            else
            {
                dampCondition = tileX < 0 || tileX >= room.TileWidth || tileY < 0 || hitWall;
            }

            if (dampCondition)
            {
                bool hitCeiling = !spawnBelow && hitWall && tileY >= room.TileHeight - 2;

                if (p.age <= 30f || hitCeiling) // adj
                {
                    p.age = p.maxAge;
                    p.fade = 0f;
                }
                else if (p.age < p.maxAge - 60f)
                {
                    p.age = p.maxAge - 60f;
                }

                p.spd.x *= 0.6f;
                p.spd.y *= 0.6f;
            }

            float fadeMult = 1f;
            if (p.age < 30f) fadeMult = p.age / 30f;
            else if (p.age > p.maxAge - 60f) fadeMult = Mathf.Max(0f, p.maxAge - p.age) / 60f;

            float groundFade = Mathf.Clamp01((p.pos.y + 20f) / 60f);
            p.fade = p.baseFade * fadeMult * groundFade;

            if (p.glow != null)
            {
                p.flickerTimer += 0.25f;
                float flicker = Mathf.Sin(p.flickerTimer * 2.1f) * 0.15f;
                float hueMix = (Mathf.Sin(p.wiggleTimer * 0.4f) + 1f) * 0.5f;

                Color lightCol = Color.Lerp(greenish, yellowish, Mathf.Lerp(0.35f, 0.65f, hueMix));
                float wantRad = Mathf.Max(3f, p.sz * 3.5f + flicker * 2.5f);
                float wantAlpha = p.fade * Mathf.Clamp01(0.55f + flicker * 0.25f);

                p.glow.nextPos = p.pos;
                p.glow.nextRadius = wantRad;
                p.glow.nextFade = wantAlpha;
                p.glow.color = lightCol;
            }

            if (p.pos.x < -extraWidth) // remaking this with extraWidth fixed the issue
            {
                p.pos.x += rW + extraWidth * 2f;
                p.prevPos.x += rW + extraWidth * 2f;
            }
            else if (p.pos.x > rW + extraWidth)
            {
                p.pos.x -= rW + extraWidth * 2f;
                p.prevPos.x -= rW + extraWidth * 2f;
            }

            if (p.pos.y < -50f || p.age >= p.maxAge)
            {
                ResetParticle(i, false, rW, rH, spawnBelow, extraWidth);
            }
        }
    }

    private void CacheBodyChunks()
    {
        chunkCount = 0;
        var physThings = room.physicalObjects;

        for (int i = 0; i < physThings.Length; i++)
        {
            var subList = physThings[i];
            for (int j = 0; j < subList.Count; j++)
            {
                var bodyParts = subList[j].bodyChunks;
                for (int k = 0; k < bodyParts.Length; k++)
                {
                    if (chunkCount >= nearbyChunks.Length) return;

                    nearbyChunks[chunkCount].pos = bodyParts[k].pos;
                    nearbyChunks[chunkCount].force = bodyParts[k].vel.magnitude * 0.1f + 0.04f;
                    chunkCount++;
                }
            }
        }
    }

    private void UpdateSpatialGrid()
    {
        Array.Clear(gridHeads, 0, gridHeads.Length);
        for (int i = 0; i < gridHeads.Length; i++) gridHeads[i] = -1;

        for (int i = 0; i < dustCount; i++)
        {
            if (dustList[i].age >= dustList[i].maxAge - 20f) continue;

            int gridX = Mathf.Clamp((int)((dustList[i].pos.x + 100f) / cellSize), 0, cols - 1);
            int gridY = Mathf.Clamp((int)((dustList[i].pos.y + 50f) / cellSize), 0, rows - 1);

            int cellId = gridY * cols + gridX;
            gridNext[i] = gridHeads[cellId];
            gridHeads[cellId] = i;
        }
    }

    private void ApplyPtPGrid()
    {
        for (int gridY = 0; gridY < rows; gridY++)
        {
            for (int gridX = 0; gridX < cols; gridX++)
            {
                int dustA = gridHeads[gridY * cols + gridX];
                if (dustA == -1) continue;

                bool canGoRight = gridX + 1 < cols;
                bool canGoDown = gridY + 1 < rows;
                bool canGoDownLeft = canGoDown && gridX - 1 >= 0;

                while (dustA != -1)
                {
                    CheckCellInt(dustA, gridX, gridY, true);

                    if (canGoRight) CheckCellInt(dustA, gridX + 1, gridY, false);
                    if (canGoDownLeft) CheckCellInt(dustA, gridX - 1, gridY + 1, false);
                    if (canGoDown) CheckCellInt(dustA, gridX, gridY + 1, false);
                    if (canGoRight && canGoDown) CheckCellInt(dustA, gridX + 1, gridY + 1, false);

                    dustA = gridNext[dustA];
                }
            }
        }
    }

    private void CheckCellInt(int dustA, int targetX, int targetY, bool sameCell) // working properly
    {
        float touchDist = 45f;
        float combineDistSq = 4f;
        float touchDistSq = 2025f;

        int dustB = gridHeads[targetY * cols + targetX];
        while (dustB != -1)
        {
            if (sameCell && dustA >= dustB)
            {
                dustB = gridNext[dustB];
                continue;
            }

            ref Particle dA = ref dustList[dustA];
            ref Particle dB = ref dustList[dustB];

            float diffX = dA.pos.x - dB.pos.x;
            if (diffX > touchDist || diffX < -touchDist) { dustB = gridNext[dustB]; continue; }

            float diffY = dA.pos.y - dB.pos.y;
            if (diffY > touchDist || diffY < -touchDist) { dustB = gridNext[dustB]; continue; }

            float gapSq = diffX * diffX + diffY * diffY;
            if (gapSq < touchDistSq && gapSq > 0.001f)
            {
                float invGap = 1f / Mathf.Sqrt(gapSq);
                float gap = gapSq * invGap;
                float normX = diffX * invGap;
                float normY = diffY * invGap;

                float power = 1f - gap / touchDist;
                int bumpType = (dustA + dustB) % 3;

                if (bumpType == 0)
                {
                    float pull = 0.25f * power;
                    dA.spd.x -= normX * pull;
                    dA.spd.y -= normY * pull;
                    dB.spd.x += normX * pull;
                    dB.spd.y += normY * pull;

                    if (gapSq < combineDistSq)
                    {
                        float combinedWeight = dA.weight + dB.weight;
                        float bWeight = dB.weight / combinedWeight;

                        dA.hueTimer = Mathf.Lerp(dA.hueTimer, dB.hueTimer, bWeight);
                        dA.weight += dB.weight * 0.4f;
                        dA.sz = Mathf.Min(dA.sz + dB.sz * 0.5f, 18f);
                        dA.spd.x = (dA.spd.x + dB.spd.x) * 0.5f;
                        dA.spd.y = (dA.spd.y + dB.spd.y) * 0.5f;

                        dB.age = dB.maxAge - 5f;
                        dB.spd.x = dA.spd.x;
                        dB.spd.y = dA.spd.y;
                    }
                }
                else if (bumpType == 1)
                {
                    float pullIn = 0.08f * power;
                    float sideX = -normY;
                    float sideY = normX;

                    dA.spd.x += -normX * pullIn + sideX * pullIn * 2.5f;
                    dA.spd.y += -normY * pullIn + sideY * pullIn * 2.5f;
                    dB.spd.x += normX * pullIn + -sideX * pullIn * 2.5f;
                    dB.spd.y += normY * pullIn + -sideY * pullIn * 2.5f;
                }
                else
                {
                    float pushAway = 0.06f * power;
                    dA.spd.x += normX * pushAway;
                    dA.spd.y += normY * pushAway;
                    dB.spd.x -= normX * pushAway;
                    dB.spd.y -= normY * pushAway;
                }
            }
            dustB = gridNext[dustB];
        }
    }

    private void ApplyPhyslInt(ref Particle dust, float forceMult)
    {
        float pushRadius = 60f;
        float pushRadiusSq = 3600f;

        for (int i = 0; i < chunkCount; i++)
        {
            float diffX = dust.pos.x - nearbyChunks[i].pos.x;
            float diffY = dust.pos.y - nearbyChunks[i].pos.y;
            float gapSq = diffX * diffX + diffY * diffY;

            if (gapSq < pushRadiusSq && gapSq > 0.001f)
            {
                float invGap = 1f / Mathf.Sqrt(gapSq);
                float gap = gapSq * invGap;

                float applyForce = (1f - gap / pushRadius) * nearbyChunks[i].force * forceMult;
                dust.spd.x += diffX * invGap * applyForce;
                dust.spd.y += diffY * invGap * applyForce;
            }
        }
    }

    public void InitiateSprites(RoomCamera.SpriteLeaser sprites, RoomCamera cam)
    {
        sprites.sprites = new FSprite[dustCount];
        for (int i = 0; i < dustCount; i++)
        {
            sprites.sprites[i] = new FSprite("Futile_White", true)
            {
                scale = dustList[i].sz / 16f,
                alpha = 0f,
                isVisible = true,
                color = dustList[i].customColor
            };

            if (cam.room != null && cam.room.game.rainWorld.Shaders.ContainsKey("CustomStardust"))
            {
                sprites.sprites[i].shader = cam.room.game.rainWorld.Shaders["CustomStardust"];
            }
            else
            {
                sprites.sprites[i].shader = cam.room.game.rainWorld.Shaders["Basic"];
            }
        }
        AddToContainer(sprites, cam, null);
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sprites, RoomCamera cam, float lerpTime, Vector2 camOffset)
    {
        var spriteList = sprites.sprites;

        for (int i = 0; i < dustCount; i++)
        {
            ref Particle p = ref dustList[i];

            spriteList[i].x = Mathf.Lerp(p.prevPos.x, p.pos.x, lerpTime) - camOffset.x;
            spriteList[i].y = Mathf.Lerp(p.prevPos.y, p.pos.y, lerpTime) - camOffset.y;

            spriteList[i].alpha = p.fade;
            spriteList[i].scale = p.sz / 16f;
        }

        if (slatedForDeletetion || room != cam.room)
        {
            sprites.CleanSpritesAndRemove();
        }
    }

    public void ApplyPalette(RoomCamera.SpriteLeaser sprites, RoomCamera cam, RoomPalette pal)
    {
        Color skyColor = Color.Lerp(pal.skyColor, Color.white, 0.4f);
        Color fogColor = Color.Lerp(pal.fogColor, Color.white, 0.3f);
        Color fgColor = Color.Lerp(Color.white, skyColor, 0.3f);
        mainColor = Color.Lerp(fgColor, fogColor, 0.1f);
    }

    public void AddToContainer(RoomCamera.SpriteLeaser sprites, RoomCamera cam, FContainer container)
    {
        FContainer foreGroup = cam.ReturnFContainer("Foreground");
        for (int i = 0; i < dustCount; i++)
        {
            sprites.sprites[i].RemoveFromContainer();
            foreGroup.AddChild(sprites.sprites[i]);
        }
    }

    public override void Destroy()
    {
        if (dustList != null)
        {
            for (int i = 0; i < dustList.Length; i++)
            {
                if (dustList[i].glow != null)
                {
                    dustList[i].glow.Destroy();
                    dustList[i].glow = null;
                }
            }
        }
        base.Destroy();
    }
}
