using Menu.Remix.MixedUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Stardust
{
    public class OptionsMenu : OptionInterface
    {
        public OpCheckBox opCheckBox(Configurable<bool> config, int x, int y, bool isUnfinished = false)
        {
            OpCheckBox checkBox = new(config, x * 160, 503 - y * 80) { description = config.info.description };
            if (isUnfinished) checkBox.colorEdge = new(0.85f, 0.35f, 0.4f);
            return checkBox;
        }

        public OpLabel opLabel(string text, float x, float y, bool isUnfinished = false)
        {
            OpLabel label = new(x * 160 + 30, 500 - y * 80, text);
            if (isUnfinished) label.color = new(0.85f, 0.35f, 0.4f);
            return label;
        }

        public OpLabel opBigLabel(string text, float y, bool isUnfinished = false)
        {
            OpLabel label = new(410, 480 - y * 80, text, true);
            if (isUnfinished) label.color = new(0.85f, 0.35f, 0.4f);
            return label;
        }

        public OpLabel opSliderLabel(string text, int y, bool isUnfinished = false)
        {
            OpLabel label = new(110, 460 - y * 80, text) { description = text };
            if (isUnfinished) label.color = new(0.85f, 0.35f, 0.4f);
            return label;
        }
        public OptionsMenu(Plugin plugin)
        {
            unlockScholar = config.Bind("stardustfamine_unlockScholar", false, new ConfigurableInfo("Unlocks Scholar"));
            scholarSeenPermadeath = config.Bind("stardustfamine_scholarSeenPermadeath", false, new ConfigurableInfo("Scholar seen permadeath"));
        }

        public override void Initialize()
        {
            base.Initialize();

            Color unfinishedColor = new(0.85f, 0.35f, 0.4f);
            this.Tabs = new[] {
                new OpTab(this, "General options"),
                new OpTab(this, "Mechanics 1")
            };
            // Tab 1
            Tabs[0].AddItems(new[] {
                new OpLabel(0, 550, "General options", true), new OpLabel(160, 550, "(red means not implemeted yet)", true){color = unfinishedColor}
            });
            // Tab 2
            Tabs[1].AddItems(new UIelement[] {
                new OpLabel(0, 550, "Mechanics", true), new OpLabel(110, 550, "(red means not implemented yet)", true){color = unfinishedColor},

                opLabel("Unlock Scholar", 0, 0),
                opCheckBox(unlockScholar, 0, 0),

                opLabel("Seen Scholar permadeath", 0, 1),
                opCheckBox(scholarSeenPermadeath, 0, 1)
            });
        }

        public static Configurable<bool> unlockScholar, scholarSeenPermadeath;
    }
}
