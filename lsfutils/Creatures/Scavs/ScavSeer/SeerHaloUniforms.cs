using UnityEngine;
using static lsfUtils.Plugin;

namespace lsfUtils.Creatures.Scavs.ScavSeer
{
    public class SeerHaloUniforms
    {
        public const int RingCount = 4;
        public const int SlotCount = 12;

        public struct Slot(int layer, int cell, bool crossed)
        {
            public int layer = layer;
            public int cell = cell;
            public bool crossed = crossed;
        }

        public readonly Slot[] slots = new Slot[SlotCount];
        public int activeSlotCount;
        public float activateTime = float.MaxValue;

        public void Activate()
        {
            activateTime = Shader.GetGlobalFloat("_RAIN");
        }

        public void Deactivate()
        {
            activateTime = float.MaxValue;
        }

        public void SetSlot(int index, int layer, int cell, bool crossed)
        {
            if (index < 0 || index >= SlotCount)
            {
                return;
            }
            slots[index] = new Slot(layer, cell, crossed);
        }

        public void SetActiveSlotCount(int count)
        {
            activeSlotCount = Mathf.Clamp(count, 0, SlotCount);
        }

        public void ClearSlots()
        {
            for (int i = 0; i < SlotCount; i++)
            {
                slots[i] = new Slot(0, 0, false);
            }
            activeSlotCount = 0;
        }

        public void PushGlobals()
        {
            Shader.SetGlobalFloat("_ActivateTime", activateTime);
            Shader.SetGlobalFloat("_SlotCount", activeSlotCount);

            Vector4[] slotData = new Vector4[SlotCount];
            for (int i = 0; i < SlotCount; i++)
            {
                Slot slot = slots[i];
                slotData[i] = new Vector4(slot.layer, slot.cell, slot.crossed ? 1f : 0f, 0f);
            }
            Shader.SetGlobalVectorArray("_SlotData", slotData);
        }
    }
}