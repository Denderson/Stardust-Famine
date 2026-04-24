using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Menu;
using RWCustom;
using UnityEngine;
using static Stardust.Plugin;

namespace Stardust.Slugcats.Scholar.ThreadsSequence
{
    public class ThreadsCheckbox : CheckBox
    {
        private struct Simple : IOwnCheckBox
        {
            private bool on;

            bool IOwnCheckBox.GetChecked(CheckBox box)
            {
                return on;
            }

            void IOwnCheckBox.SetChecked(CheckBox box, bool c)
            {
                on = c;
            }
        }

        private new readonly SlugcatSelectMenu menu;

        public ThreadsCheckbox(SlugcatSelectMenu menu, float posX) : base(menu, menu.pages[0], default(Simple), new Vector2(posX, -30f), 40f, Custom.ToTitleCase(menu.Translate("CHOOSE THREAD").ToLower()), "", textOnRight: true)
        {
            this.menu = menu;
        }

        public override void Clicked()
        {
            base.Clicked();
            if (Checked)
            {
                menu.startButton.fillTime = 40f;
                menu.startButton.menuLabel.text = menu.Translate("CHOOSE THREAD");
            }
            else
            {
                menu.UpdateStartButtonText();
            }
        }

        public override void Update()
        {
            SlugcatStats.Name name = menu.colorFromIndex(menu.slugcatPageIndex);
            bool hidden = name != Enums.SlugcatStatsName.sfscholar || !OptionsMenu.scholarSeenPermadeath.Value;
            GetButtonBehavior.greyedOut = hidden || menu.restartChecked;
            selectable = !hidden && !menu.restartChecked;
            pos.y = hidden ? -40 : 40;
            base.Update();
            if (menu.restartChecked)
            {
                Checked = false;
            }
        }
    }

}
