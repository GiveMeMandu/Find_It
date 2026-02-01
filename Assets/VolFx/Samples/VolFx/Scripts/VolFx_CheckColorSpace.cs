using UnityEngine;

namespace VolFx
{
    [AddComponentMenu("")]
    public class VolFx_CheckColorSpace : MonoBehaviour
    {
        void Awake()
        {
            var cur = QualitySettings.activeColorSpace;

            if (cur == ColorSpace.Linear)
            {
                Debug.Log(
                    "<b>[VolFx]</b> ColorSpace is <color=#ffffff>Linear</color>\n" +
                    "Recommended: <color=#ffffff>Gamma</color>, VolFx samples are authored for Gamma and may look incorrect in Linear."
                );
            }
        }
    }
}