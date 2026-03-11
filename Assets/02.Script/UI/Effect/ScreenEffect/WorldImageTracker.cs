using UnityEngine;
using Kamgam.UGUIWorldImage;

// WorldImage의 활성화/비활성화를 추적하는 컴포넌트
public class WorldImageTracker : MonoBehaviour
{
    private ScreenSplat screenSplat;
    private WorldImage worldImage;

    public void Initialize(ScreenSplat splat)
    {
        screenSplat = splat;
        worldImage = GetComponent<WorldImage>();
    }

    void OnDisable()
    {
        // WorldImage가 비활성화되면 ScreenSplat에 알림
        if (screenSplat != null && worldImage != null)
        {
            screenSplat.OnWorldImageDisabled(worldImage);
        }
    }
}