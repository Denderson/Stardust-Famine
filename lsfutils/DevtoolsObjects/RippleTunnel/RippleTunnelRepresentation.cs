using DevInterface;
using lsfUtils.DevtoolsObjects.RippleTunnel;
using UnityEngine;
using static Pom.Pom;

namespace Stardust.PlacedObjects
{
    public class RippleTunnelRepresentation : ManagedRepresentation
    {
        public RippleTunnelRepresentation(PlacedObject.Type placedType, ObjectsPage objPage, PlacedObject pObj) : base(placedType, objPage, pObj)
        {
            // Radius visualisation circle.
            fSprites.Add(new FSprite("Futile_White"));
            objPage.owner.placedObjectsContainer.AddChild(fSprites[fSprites.Count - 1]);
            fSprites[fSprites.Count - 1].shader = objPage.owner.room.game.rainWorld.Shaders["VectorCircle"];
            fSprites[fSprites.Count - 1].color = new Color(0.6f, 0.3f, 1f);

            RippleTunnelData d = pObj.data as RippleTunnelData;
            if (d.obj == null)
            {
                d.obj = new RippleTunnel(pObj, objPage.owner.room);
                objPage.owner.room.AddObject(d.obj);
            }
        }

        public override void Refresh()
        {
            base.Refresh();

            RippleTunnelData d = pObj.data as RippleTunnelData;
            int circleIndex = fSprites.Count - 1;

            MoveSprite(circleIndex, absPos);
            fSprites[circleIndex].scale = d.radius.magnitude / 8f;
            fSprites[circleIndex].alpha = 0.3f;
        }
    }
}