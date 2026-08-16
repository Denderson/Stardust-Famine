using lsfUtils.Creatures.Scavs.ScavSeer;
using MoreSlugcats;
using System;
using UnityEngine;

public class ScavSeerGraphics : ScavengerGraphics
{
    public SeerHalo halo;
    public VultureMaskGraphics seerMask;

    private readonly int haloFirstSprite;
    private readonly int maskFirstSprite;

    public ScavSeerGraphics(ScavSeer Seer) : base(Seer)
    {
        maskGfx = null;

        haloFirstSprite = TotalSprites;
        halo = new SeerHalo(this, haloFirstSprite);
        maskFirstSprite = haloFirstSprite + halo.totalSprites;

        seerMask = new VultureMaskGraphics(scavenger, VultureMask.MaskType.SCAVTEMPLAR, maskFirstSprite, "SeerMask");
        seerMask.GenerateColor(scavenger.abstractCreature.ID.RandomSeed);
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        maskGfx = null;
        base.InitiateSprites(sLeaser, rCam);
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        maskGfx = seerMask;

        int required = maskFirstSprite + maskGfx.TotalSprites;
        if (sLeaser.sprites.Length < required)
        {
            Array.Resize(ref sLeaser.sprites, required);
            halo.InitiateSprites(sLeaser, rCam);
            maskGfx.InitiateSprites(sLeaser, rCam);
        }

        base.AddToContainer(sLeaser, rCam, newContatiner);
        halo.AddToContainer(sLeaser, rCam, newContatiner);
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        base.ApplyPalette(sLeaser, rCam, palette);
        halo.ApplyPalette(sLeaser, rCam, palette);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPosV2)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPosV2);
        halo.DrawSprites(sLeaser, rCam, timeStacker, camPosV2);
    }

    public override void Update()
    {
        base.Update();
        halo.Update();
    }
}