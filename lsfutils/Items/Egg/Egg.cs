using System;
using System.Globalization;
using MoreSlugcats;
using RWCustom;
using UnityEngine;
using Watcher;
using Random = UnityEngine.Random;

public class Egg : PlayerCarryableItem, IDrawable
{
    public Vector2 rotation;

    public Vector2 lastRotation;

    public Vector2? setRotation;

    public bool AutomaticPickUp => true;

    public Egg(AbstractPhysicalObject abstractPhysicalObject) : base(abstractPhysicalObject)
    {
        bodyChunks = new BodyChunk[1];
        bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 8f, 0.2f);
        bodyChunkConnections = new BodyChunkConnection[0];
        airFriction = 0.999f;
        gravity = 0.9f;
        bounce = 0.2f;
        surfaceFriction = 0.7f;
        collisionLayer = 1;
        waterFriction = 0.95f;
        buoyancy = 1.1f;
    }

    public override void Update(bool eu)
    {
        Update(eu);
        if (room.game.devToolsActive && Input.GetKey("b"))
        {
            BodyChunk bodyChunk = firstChunk;
            bodyChunk.vel += Custom.DirVec(firstChunk.pos, Futile.mousePosition) * 3f;
        }
        lastRotation = rotation;
        if (grabbedBy.Count > 0)
        {
            rotation = Custom.PerpendicularVector(Custom.DirVec(firstChunk.pos, grabbedBy[0].grabber.mainBodyChunk.pos));
            rotation.y = Mathf.Abs(rotation.y);
        }
        if (setRotation.HasValue)
        {
            rotation = setRotation.Value;
            setRotation = null;
        }
        if (firstChunk.ContactPoint.y < 0)
        {
            Vector2 val = rotation - Custom.PerpendicularVector(rotation) * (0.1f * firstChunk.vel.x);
            rotation = val.normalized;
            firstChunk.vel.x *= 0.8f;
        }
    }

    public override void PlaceInRoom(Room placeRoom)
    {
        PlaceInRoom(placeRoom);
        firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
        rotation = Custom.RNV();
        lastRotation = rotation;
    }

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];
        sLeaser.sprites[0] = new FSprite("eggSpriteTodo");
        AddToContainer(sLeaser, rCam, null);
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Vector2 val = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
        Vector2 val2 = Vector3.Slerp(lastRotation, rotation, timeStacker);
        sLeaser.sprites[0].x = val.x - camPos.x;
        sLeaser.sprites[0].y = val.y - camPos.y;
        sLeaser.sprites[0].rotation = Custom.VecToDeg(val2);
        if (blink > 0 && Random.value < 0.5f) sLeaser.sprites[0].color = blinkColor;
        else sLeaser.sprites[0].color = color;
        if (slatedForDeletetion || room != rCam.room) sLeaser.CleanSpritesAndRemove();
    }

    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        color = new Color(0.5f, 0.5f, 0.5f);
        sLeaser.sprites[0].color = color;
    }

    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        newContatiner ??= rCam.ReturnFContainer("Items");
        sLeaser.sprites[0].RemoveFromContainer();
        newContatiner.AddChild(sLeaser.sprites[0]);
    }
}
