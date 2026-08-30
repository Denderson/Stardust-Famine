using DevInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace lsfUtils.DevtoolsObjects.MudBonePile
{
    public class MudBonePileRepresentation : PlacedObjectRepresentation
    {
        public MudBonePileData data => (MudBonePileData)pObj.data;
        private Handle handle;
        private MudBonePilePanel panel;

        public MudBonePileRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
            : base(owner, IDstring, parentNode, pObj, name)
        {
            panel = new MudBonePilePanel(owner, "MudBonePanel", this, data.panelPos);
            subNodes.Add(panel);

            handle = new Handle(owner, "MudBoneTarget", this, data.handlePos);
            subNodes.Add(handle);
        }

        public override void Refresh()
        {
            base.Refresh();
            if (data.handlePos != handle.pos)
            {
                data.handlePos = handle.pos;
                data.randomSeed = UnityEngine.Random.Range(0, 10000);
            }
            data.panelPos = panel.pos;
        }

        public class MudBonePilePanel : Panel
        {
            public MudBonePileData data => ((MudBonePileRepresentation)parentNode).data;

            public MudBonePilePanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos) : base(owner, IDstring, parentNode, pos, new Vector2(260f, 155f), "MudBonePile settings")
            {
                subNodes.Add(new SquishyButton(owner, "SquishyToggle", this, new Vector2(5f, 130f)));
                subNodes.Add(new MudBoneSlider(owner, "MudLightness", this, new Vector2(5f, 110f), "Mud lightness:"));
                subNodes.Add(new MudBoneSlider(owner, "MudHue", this, new Vector2(5f, 90f), "Mud hue:"));
                subNodes.Add(new MudBoneSlider(owner, "FgRatio", this, new Vector2(5f, 70f), "Fg/Bg blend:"));
                subNodes.Add(new MudBoneSlider(owner, "BoneDensity", this, new Vector2(5f, 50f), "Bone density:"));
                subNodes.Add(new MudBoneSlider(owner, "MudAmount", this, new Vector2(5f, 30f), "Mud amount:"));
                subNodes.Add(new ReseedButton(owner, "ReseedButton", this, new Vector2(5f, 5f), "Randomize seed"));
            }

            public class SquishyButton : Button
            {
                public SquishyButton(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos) : base(owner, IDstring, parentNode, pos, 120f, "Squish: ON")
                {
                    Text = "Squish: " + (((MudBonePilePanel)parentNode).data.isSquishy ? "ON" : "OFF");
                }

                public override void Clicked()
                {
                    MudBonePileData data = ((MudBonePilePanel)parentNode).data;
                    data.isSquishy = !data.isSquishy;
                    Text = "Squish: " + (data.isSquishy ? "ON" : "OFF");
                    parentNode.parentNode.Refresh();
                }
            }

            public class ReseedButton : Button
            {
                public ReseedButton(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string text) : base(owner, IDstring, parentNode, pos, 120f, text) { }

                public override void Clicked()
                {
                    MudBonePileData data = ((MudBonePilePanel)parentNode).data;
                    data.randomSeed = UnityEngine.Random.Range(0, 10000);
                    parentNode.parentNode.Refresh();
                }
            }

            public class MudBoneSlider : Slider
            {
                public MudBoneSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
                    : base(owner, IDstring, parentNode, pos, title, false, 100f) { }

                public override void NubDragged(float nubPos)
                {
                    MudBonePileData data = ((MudBonePilePanel)parentNode).data;

                    if (IDstring == "FgRatio") data.fgRatio = Mathf.Lerp(0f, 1f, nubPos);
                    else if (IDstring == "BoneDensity") data.boneDensity = Mathf.Lerp(0f, 1f, nubPos);
                    else if (IDstring == "MudAmount") data.mudAmount = Mathf.Lerp(0f, 1f, nubPos);
                    else if (IDstring == "MudHue") data.mudHue = Mathf.Lerp(0f, 1f, nubPos);
                    else if (IDstring == "MudLightness") data.mudLightness = Mathf.Lerp(0f, 1f, nubPos);

                    parentNode.parentNode.Refresh();
                    Refresh();
                }

                public override void Refresh()
                {
                    base.Refresh();
                    MudBonePileData data = ((MudBonePilePanel)parentNode).data;
                    float num = 0f;

                    if (IDstring == "FgRatio") num = data.fgRatio;
                    else if (IDstring == "BoneDensity") num = data.boneDensity;
                    else if (IDstring == "MudAmount") num = data.mudAmount;
                    else if (IDstring == "MudHue") num = data.mudHue;
                    else if (IDstring == "MudLightness") num = data.mudLightness;

                    NumberText = (num * 100f).ToString("0") + "%";
                    RefreshNubPos(num);
                }
            }
        }
    }
}
