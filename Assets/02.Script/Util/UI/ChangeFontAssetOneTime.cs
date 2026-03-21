using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;

public class ChangeFontAssetOneTime : MonoBehaviour
{
    [Tooltip("변경할 대상 TMP 폰트 에셋")]
    public TMP_FontAsset targetFontAsset;

    void Start()
    {
        ChangeAllFonts();
    }

    [Button("Change All Fonts")]
    public void ChangeAllFonts()
    {
        if (targetFontAsset == null)
        {
            Debug.LogWarning("Target Font Asset이 할당되지 않았습니다.", this);
            return;
        }

        // 비활성화된 자식 객체까지 모두 포함하여 TMP_Text 컴포넌트를 찾습니다.
        TMP_Text[] textComponents = GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text textComponent in textComponents)
        {
            textComponent.font = targetFontAsset;
        }
    }
}
