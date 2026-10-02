using System.Net.NetworkInformation;
using UnityEngine;

namespace lsfUtils.Items.WarpSpears;

public class WarpPortal : UpdatableAndDeletable, IDrawable
{
    public WarpPortal linkedPortal;
    public bool whyareyoublue;

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {

    }

    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {

    }

    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {

    }
}
