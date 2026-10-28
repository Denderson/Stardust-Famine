using MoreSlugcats;
using System;
using UnityEngine;
using Watcher;

namespace lsfUtils.Creatures.Misc.ToxicSponge;

public class ToxicSpongeGraphics : BarnacleGraphics
{

    public ToxicSpongeGraphics(ToxicSponge sponge) : base(sponge)
    {

    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        base.AddToContainer(sLeaser, rCam, newContatiner);
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        base.ApplyPalette(sLeaser, rCam, palette);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPosV2)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPosV2);
    }

    public override void Update()
    {
        base.Update();
    }
}