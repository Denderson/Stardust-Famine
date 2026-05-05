using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Stardust.Plugin;
using static Stardust.SaveFile.SaveFileMain;

namespace Stardust.SaveFile
{
    public static class SaveFileGates
    {
        public static void TickGates(RainWorldGame self, bool malnourished)
        {
            if (malnourished)
            {
                Log.LogMessage("Not ticking gates due to starving!");
                return;
            }
            string gates = self.GetStorySession.saveState.GetString(SaveFileMain.gates);
            if (!string.IsNullOrEmpty(gates))
            {
                Log.LogMessage($"Before change: {gates}");
                string newGates = string.Empty;
                string[] arrayGates;
                if (gates != null && gates.Length > 2 && gates.Contains('+'))
                {
                    arrayGates = gates.Split('+');
                }
                else arrayGates = [gates];
                for (int i = 0; i < arrayGates.Length; i++)
                {
                    string newValue = arrayGates[i];
                    if (newValue != null && newValue.Length > 1 && newValue.Contains('/') && newValue.Split('/').Length > 1 && Int32.TryParse(newValue.Split('/')[1], out int cyclesUntilOpen))
                    {
                        cyclesUntilOpen--;
                        if (cyclesUntilOpen > 0)
                        {
                            if (newGates != string.Empty) newGates += "+";
                            newGates += newValue.Split('/')[0] + $"/{cyclesUntilOpen}";
                        }
                    }
                }
                Log.LogMessage($"After change: {newGates}");
                self.GetStorySession.saveState.Set<string>(SaveFileMain.gates, newGates);
            }
        }

        public static bool IsGateLocked(this Room room)
        {
            string regions = room.game.GetStorySession.saveState.GetString(gates);
            bool value = regions != null && regions.Contains(room.abstractRoom.name);
            return value;
        }
        public static void LockGate(this Room room)
        {
            Log.LogMessage($"Locking gate: {room.abstractRoom.name}");
            string regions = room.game.GetStorySession.saveState.GetString(gates);
            if (regions != null) room.game.GetStorySession.saveState.Set<string>(gates, $"{regions}+{room.abstractRoom.name}/3");
            else room.game.GetStorySession.saveState.Set<string>(gates, $"{room.abstractRoom.name}/3");
            Log.LogMessage($"Gates locked: {room.game.GetStorySession.saveState.GetString(gates)}");
        }
    }
}
