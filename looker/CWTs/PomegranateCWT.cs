using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Looker.CWTs
{
    public static class PomegranateCWT
    {

        public static readonly ConditionalWeakTable<Pomegranate, DataClass> melonCWT = new();
        public static bool TryGetData(Pomegranate key, out DataClass data)
        {
            if (key != null)
            {
                data = melonCWT.GetOrCreateValue(key);
            }
            else data = null;

            return data != null;
        }
        public class DataClass
        {
            public int cooldown = 0;
            public Vector2 target = Vector2.Zero;
        }
    }
}
