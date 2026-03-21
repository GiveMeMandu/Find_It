using UnityEngine;
using UnityEngine.UI;

namespace UI.Components
{
    [RequireComponent(typeof(CanvasScaler))]
    [ExecuteAlways]
    public class CanvasAutoRatio : MonoBehaviour
    {
        private CanvasScaler canvasScaler;
        private const float REFERENCE_RATIO = 16f / 9f;

        private void Awake()
        {
            canvasScaler = GetComponent<CanvasScaler>();
        }

        private void Start()
        {
            AdjustCanvasScaler();
        }

        private void Update()
        {
            AdjustCanvasScaler();
        }

        private void AdjustCanvasScaler()
        {
            float currentRatio = (float)Screen.width / Screen.height;

            if (currentRatio > REFERENCE_RATIO)
            {
                // 16:9보다 가로가 긴 경우 (예: 21:9) -> 세로 기준으로 맞춤
                canvasScaler.matchWidthOrHeight = 1f;
            }
            else
            {
                // 16:9보다 세로가 긴 경우 (예: 9:16) -> 가로 기준으로 맞춤
                canvasScaler.matchWidthOrHeight = 0f;
            }
        }
    }
}