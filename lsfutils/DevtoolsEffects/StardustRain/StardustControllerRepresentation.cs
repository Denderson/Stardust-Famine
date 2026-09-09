using DevInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace lsfUtils.DevtoolsEffects.StardustRain
{
    public class StardustControllerRepresentation : PlacedObjectRepresentation
    {
        public StardustControllerData data => (StardustControllerData)pObj.data;
        private StardustPanel settingsPanel;

        public StardustControllerRepresentation(DevUI uiOwner, string id, DevUINode parent, PlacedObject placedObj, string tag)
            : base(uiOwner, id, parent, placedObj, tag)
        {
            settingsPanel = new StardustPanel(uiOwner, "StardustPanel", this, data.panelPos);
            subNodes.Add(settingsPanel);
        }

        public override void Refresh()
        {
            base.Refresh();
            data.panelPos = settingsPanel.pos;
        }

        public class StardustPanel : Panel
        {
            public StardustControllerData data => ((StardustControllerRepresentation)parentNode).data;

            public StardustPanel(DevUI uiOwner, string id, DevUINode parent, Vector2 pos)
                : base(uiOwner, id, parent, pos, new Vector2(260f, 235f), "Stardust settings")
            {
                subNodes.Add(new WallCheckButton(uiOwner, "WallCheckToggle", this, new Vector2(5f, 210f)));
                subNodes.Add(new SpawnBelowButton(uiOwner, "SpawnBelowToggle", this, new Vector2(5f, 190f)));
                subNodes.Add(new StardustSlider(uiOwner, "ExtraWidth", this, new Vector2(5f, 170f), "Extra Width:"));
                subNodes.Add(new StardustSlider(uiOwner, "Hue1", this, new Vector2(5f, 150f), "Hue 1:"));
                subNodes.Add(new StardustSlider(uiOwner, "Hue2", this, new Vector2(5f, 130f), "Hue 2:"));
                subNodes.Add(new StardustSlider(uiOwner, "Lightness", this, new Vector2(5f, 110f), "Lightness:"));
                subNodes.Add(new StardustSlider(uiOwner, "LifeMult", this, new Vector2(5f, 90f), "Life Cycle:"));
                subNodes.Add(new StardustSlider(uiOwner, "SpeedMult", this, new Vector2(5f, 70f), "Speed:"));
                subNodes.Add(new StardustSlider(uiOwner, "Interact", this, new Vector2(5f, 50f), "Interaction:"));
                subNodes.Add(new StardustSlider(uiOwner, "Sway", this, new Vector2(5f, 30f), "Sway Dir:"));
            }

            public class SpawnBelowButton : Button
            {
                public SpawnBelowButton(DevUI uiOwner, string id, DevUINode parent, Vector2 pos)
                    : base(uiOwner, id, parent, pos, 150f, "Below geometry: ON")
                {
                    Text = "Below geometry: " + (((StardustPanel)parentNode).data.spawnBelow ? "ON" : "OFF");
                }

                public override void Clicked()
                {
                    StardustControllerData ctrlData = ((StardustPanel)parentNode).data;
                    ctrlData.spawnBelow = !ctrlData.spawnBelow;
                    Text = "Below geometry: " + (ctrlData.spawnBelow ? "ON" : "OFF");
                    parentNode.parentNode.Refresh();
                }
            }

            public class WallCheckButton : Button // naming this seriously cuz im a serious person ig
            {
                public WallCheckButton(DevUI uiOwner, string id, DevUINode parent, Vector2 pos)
                    : base(uiOwner, id, parent, pos, 150f, "Wall Check: STRICT")
                {
                    Text = "Wall Check: " + (((StardustPanel)parentNode).data.strictWallCheck ? "STRICT" : "BOUNDS");
                }

                public override void Clicked()
                {
                    StardustControllerData ctrlData = ((StardustPanel)parentNode).data;
                    ctrlData.strictWallCheck = !ctrlData.strictWallCheck;
                    Text = "Wall Check: " + (ctrlData.strictWallCheck ? "STRICT" : "BOUNDS");
                    parentNode.parentNode.Refresh();
                }
            }

            public class StardustSlider : Slider
            {
                public StardustSlider(DevUI uiOwner, string id, DevUINode parent, Vector2 pos, string title)
                    : base(uiOwner, id, parent, pos, title, false, 100f) { }

                public override void NubDragged(float sliderPos)
                {
                    StardustControllerData ctrlData = ((StardustPanel)parentNode).data;
                    if (IDstring == "LifeMult") ctrlData.lifeMult = Mathf.Lerp(0.1f, 3f, sliderPos);
                    else if (IDstring == "SpeedMult") ctrlData.speedMult = Mathf.Lerp(0.1f, 3f, sliderPos);
                    else if (IDstring == "Interact") ctrlData.pushForce = Mathf.Lerp(0f, 3f, sliderPos);
                    else if (IDstring == "Sway") ctrlData.swayDir = Mathf.Lerp(-2f, 2f, sliderPos);
                    else if (IDstring == "ExtraWidth") ctrlData.extraWidth = Mathf.Lerp(0f, 1000f, sliderPos);
                    else if (IDstring == "Hue1") ctrlData.hue1 = Mathf.Lerp(0f, 1f, sliderPos);
                    else if (IDstring == "Hue2") ctrlData.hue2 = Mathf.Lerp(0f, 1f, sliderPos);
                    else if (IDstring == "Lightness") ctrlData.lightness = Mathf.Lerp(0f, 1f, sliderPos);

                    parentNode.parentNode.Refresh();
                    Refresh();
                }

                public override void Refresh()
                {
                    base.Refresh();
                    StardustControllerData ctrlData = ((StardustPanel)parentNode).data;
                    float val = 0f;
                    float ratio = 0f;

                    if (IDstring == "LifeMult") { val = ctrlData.lifeMult; ratio = Mathf.InverseLerp(0.1f, 3f, val); }
                    else if (IDstring == "SpeedMult") { val = ctrlData.speedMult; ratio = Mathf.InverseLerp(0.1f, 3f, val); }
                    else if (IDstring == "Interact") { val = ctrlData.pushForce; ratio = Mathf.InverseLerp(0f, 3f, val); }
                    else if (IDstring == "Sway") { val = ctrlData.swayDir; ratio = Mathf.InverseLerp(-2f, 2f, val); }
                    else if (IDstring == "ExtraWidth") { val = ctrlData.extraWidth; ratio = Mathf.InverseLerp(0f, 1000f, val); }
                    else if (IDstring == "Hue1") { val = ctrlData.hue1; ratio = Mathf.InverseLerp(0f, 1f, val); }
                    else if (IDstring == "Hue2") { val = ctrlData.hue2; ratio = Mathf.InverseLerp(0f, 1f, val); }
                    else if (IDstring == "Lightness") { val = ctrlData.lightness; ratio = Mathf.InverseLerp(0f, 1f, val); }

                    if (IDstring == "ExtraWidth") NumberText = Mathf.Round(val).ToString();
                    else NumberText = val.ToString("0.00");

                    RefreshNubPos(ratio);
                }
            }
        }
    }
}
