using lsfUtils.CWTs;
using RWCustom;
using Unity.Mathematics;
using UnityEngine;

namespace lsfUtils.DevtoolsObjects.BackgroundMud
{
    public class BackgroundMud : MudPit
    {
        private readonly PlacedObject myPObj;
        private Rect MyRect => new(myPObj.pos, Data.handlePos);

        public BackgroundMud(PlacedObject pObj) : base(pObj)
        {
            myPObj = pObj;
        }

        public new void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            if (newContatiner == null)
            {
                newContatiner = rCam.ReturnFContainer("Water");
            }
            newContatiner.AddChild(sLeaser.sprites[0]);
            newContatiner.AddChildAtIndex(sLeaser.sprites[1], 0);
        }
    }
}