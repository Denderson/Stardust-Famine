using DevInterface;
using RWCustom;
using UnityEngine;
using static Pom.Pom;

namespace lsfUtils.DevtoolsObjects.WaveLight
{
    public class WaveLightRepresentation : ManagedRepresentation
    {
        public FSprite topEdge;
        public FSprite bottomEdge;
        public FSprite leftEdge;
        public FSprite rightEdge;

        public WaveLightRepresentation(PlacedObject.Type placedType, ObjectsPage objPage, PlacedObject pObj) : base(placedType, objPage, pObj)
        {
            topEdge = new FSprite("pixel") { anchorY = 0f, color = Color.yellow };
            bottomEdge = new FSprite("pixel") { anchorY = 0f, color = Color.yellow };
            leftEdge = new FSprite("pixel") { anchorY = 0f, color = Color.yellow };
            rightEdge = new FSprite("pixel") { anchorY = 0f, color = Color.yellow };

            owner.placedObjectsContainer.AddChild(topEdge);
            owner.placedObjectsContainer.AddChild(bottomEdge);
            owner.placedObjectsContainer.AddChild(leftEdge);
            owner.placedObjectsContainer.AddChild(rightEdge);
        }

        public static void PositionLine(FSprite line, Vector2 start, Vector2 end)
        {
            Vector2 diff = end - start;
            line.x = start.x;
            line.y = start.y;
            line.scaleY = diff.magnitude;
            line.rotation = Custom.VecToDeg(diff);
        }

        public override void Update()
        {
            base.Update();

            WaveLightData data = pObj.data as WaveLightData;
            if (data == null) return;

            float halfMinWidth = data.minWidth * 0.5f;
            float halfMaxWidth = data.maxWidth * 0.5f;
            float halfHeight = data.height * 0.5f;

            Vector2 center = absPos;
            Vector2 topLeft = center + new Vector2(-halfMinWidth, halfHeight);
            Vector2 topRight = center + new Vector2(halfMinWidth, halfHeight);
            Vector2 bottomLeft = center + new Vector2(-halfMaxWidth, -halfHeight);
            Vector2 bottomRight = center + new Vector2(halfMaxWidth, -halfHeight);

            PositionLine(topEdge, topLeft, topRight);
            PositionLine(bottomEdge, bottomLeft, bottomRight);
            PositionLine(leftEdge, topLeft, bottomLeft);
            PositionLine(rightEdge, topRight, bottomRight);
        }
    }
}