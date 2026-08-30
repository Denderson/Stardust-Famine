using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace lsfUtils.DevtoolsObjects.MudBonePile
{
    public class MudBonePileData : PlacedObject.Data
    {
        public Vector2 handlePos;
        public Vector2 panelPos;
        public float boneDensity;
        public float mudAmount;
        public float fgRatio;
        public int randomSeed;
        public float mudHue;
        public float mudLightness;
        public bool isSquishy;

        public MudBonePileData(PlacedObject owner) : base(owner)
        {
            handlePos = new Vector2(80f, 45f);
            panelPos = new Vector2(0f, 20f);
            boneDensity = 0.5f;
            mudAmount = 0.6f;
            fgRatio = 0.5f;
            randomSeed = UnityEngine.Random.Range(0, 10000);
            mudHue = 0.08f;
            mudLightness = 0.19f;
            isSquishy = true;
        }

        public override string ToString()
        {
            return $"{handlePos.x}~{handlePos.y}~{panelPos.x}~{panelPos.y}~{boneDensity}~{mudAmount}~{fgRatio}~{randomSeed}~{mudHue}~{mudLightness}~{isSquishy}";
        }

        public override void FromString(string s)
        {
            string[] array = s.Split('~');
            if (array.Length >= 6)
            {
                try
                {
                    handlePos = new Vector2(float.Parse(array[0]), float.Parse(array[1]));
                    panelPos = new Vector2(float.Parse(array[2]), float.Parse(array[3]));
                    boneDensity = float.Parse(array[4]);
                    mudAmount = float.Parse(array[5]);

                    if (array.Length >= 11)
                    {
                        fgRatio = float.Parse(array[6]);
                        randomSeed = int.Parse(array[7]);
                        mudHue = float.Parse(array[8]);
                        mudLightness = float.Parse(array[9]);
                        isSquishy = bool.Parse(array[10]);
                    }
                    else if (array.Length >= 10)
                    {
                        fgRatio = float.Parse(array[6]);
                        randomSeed = int.Parse(array[7]);
                        mudHue = float.Parse(array[8]);
                        mudLightness = float.Parse(array[9]);
                        isSquishy = true;
                    }
                    else if (array.Length >= 8)
                    {
                        fgRatio = float.Parse(array[6]);
                        randomSeed = int.Parse(array[7]);
                        mudHue = 0.08f;
                        mudLightness = 0.19f;
                        isSquishy = true;
                    }
                    else if (array.Length >= 7)
                    {
                        fgRatio = 0.5f;
                        randomSeed = int.Parse(array[6]);
                        mudHue = 0.08f;
                        mudLightness = 0.19f;
                        isSquishy = true;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"MudBonePile FromString Parse Error: {e.Message}");
                }
            }
        }
    }
}
