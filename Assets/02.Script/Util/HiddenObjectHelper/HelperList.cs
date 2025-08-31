using Sirenix.OdinInspector;
using UnityEngine;

public class HelperList : MonoBehaviour
{
    [InfoBox("자동으로 세팅될 숨긴 설정할 수 있는 설정들입니다 아래 버튼 눌러주세요")]
    [Button("찾았을 때 숨기기 설정")]
    public void SetHideWhenFound()
    {
        HideWhenFoundHelper hideWhenFoundHelper = GetComponent<HideWhenFoundHelper>();
        if (hideWhenFoundHelper == null)
        {
            hideWhenFoundHelper = gameObject.AddComponent<HideWhenFoundHelper>();
        }
    }
    [Button("찾았을 때 UI 변경 설정")]
    public void SetUIChangeHelper()
    {
        UIChangeHelper uiChangeHelper = GetComponent<UIChangeHelper>();
        if (uiChangeHelper == null)
        {
            uiChangeHelper = gameObject.AddComponent<UIChangeHelper>();
        }
    }

    [Button("전체 지우기")]
    public void RemoveAllHelper()
    {
    // Remove HideWhenFoundHelper if present
    HideWhenFoundHelper hideWhenFoundHelper = GetComponent<HideWhenFoundHelper>();
    if (hideWhenFoundHelper != null)
    {
#if UNITY_EDITOR
        UnityEditor.Undo.RecordObject(gameObject, "Remove HideWhenFoundHelper");
        UnityEditor.Undo.DestroyObjectImmediate(hideWhenFoundHelper);
        hideWhenFoundHelper = null;
#endif
    }

    // Remove UIChangeHelper if present
    UIChangeHelper uiChangeHelper = GetComponent<UIChangeHelper>();
    if (uiChangeHelper != null)
    {
#if UNITY_EDITOR
        UnityEditor.Undo.RecordObject(gameObject, "Remove UIChangeHelper");
        UnityEditor.Undo.DestroyObjectImmediate(uiChangeHelper);
        uiChangeHelper = null;
#endif
    }
    }
}

