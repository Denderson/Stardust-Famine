using HUD;
using System.Collections.Generic;
using UnityEngine;

namespace Stardust.Anchors
{
    public class AnchorDialogueBox : DialogBox
    {
        public Vector2 posOffset;

        public AnchorDialogueBox(HUD.HUD hud, Vector2 posOffset) : base(hud)
        {
            this.posOffset = posOffset;
        }

        public AnchorDialogueBox(HUD.HUD hud, Vector2 posOffset, Color currentColor) : this(hud, posOffset)
        {
            this.currentColor = currentColor;
        }

        public static Vector2 DialogBox_DrawPos(On.HUD.DialogBox.orig_DrawPos orig, DialogBox self, float timeStacker)
        {
            Vector2 value = orig(self, timeStacker);
            if (self != null && self is AnchorDialogueBox box && box.posOffset != null)
            {
                return value + box.posOffset;
            }
            return value;
        }
    }

    public class AnchorDialogueBoxes
    {
        public List<AnchorDialogueBox> Dialogs = [];
        public AnchorDialogueBox CreateDialog(HUD.HUD hud, string text, Vector2 posOffset, int extraLinger = 0)
        {
            AnchorDialogueBox dialog = new(hud, posOffset);

            dialog.NewMessage(text, extraLinger);

            hud.parts.Add(dialog);
            Dialogs.Add(dialog);

            return dialog;
        }

        public void RemoveDialog(HUD.HUD hud, AnchorDialogueBox dialog)
        {
            if (dialog == null) return;

            dialog.ClearSprites();

            hud.parts.Remove(dialog);
            Dialogs.Remove(dialog);
        }

        public void ClearAll(HUD.HUD hud)
        {
            foreach (AnchorDialogueBox dialog in Dialogs)
            {
                dialog.ClearSprites();
                hud.parts.Remove(dialog);
            }
            Dialogs.Clear();
        }
    }
}