using BepInEx;
using RWCustom;
using System;
using UnityEngine;

namespace lsfUtils.Items.WarpSpears;

public class WarpSpear : Spear
{
    public WarpSpear connectedSpear;

    public bool hasPortal;
    public WarpPortal portal;

    public WarpSpear(AbstractSpear abstractObject, World world) : base(abstractObject, world)
    {
        this.connectedSpear = null;
    }

    public WarpSpear(AbstractSpear abstractObject, World world, WarpSpear connectedSpear) : base(abstractObject, world)
    {
        this.connectedSpear = connectedSpear;
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
    }

    public override void Destroy()
    {
        WarpSpear connectedSpear = this.connectedSpear;
        this.connectedSpear = null;

        base.Destroy();

        if (connectedSpear != null && !connectedSpear.slatedForDeletetion)
        {
            connectedSpear.connectedSpear = null;
            connectedSpear.Destroy();
        }
    }

    public override void NewRoom(Room newRoom)
    {
        base.NewRoom(newRoom);
        if (connectedSpear != null && connectedSpear.room != newRoom)
        {
            connectedSpear.NewRoom(newRoom);
        }
    }

    public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
    {
        return base.HitSomething(result, eu);
    }

    public override void HitWall()
    {
        base.HitWall();

    }

    public void SpawnPortal()
    {

    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam,timeStacker,camPos);
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        base.AddToContainer(sLeaser, rCam, newContainer);
    }
}