using Sirenix.OdinInspector;
using UnityEngine;
using InGame;

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

    [Button("찾았을 때 이벤트 설정")]
    public void SetWhenFoundEventHelper()
    {
        WhenFoundEventHelper whenFoundEventHelper = GetComponent<WhenFoundEventHelper>();
        if (whenFoundEventHelper == null)
        {
            whenFoundEventHelper = gameObject.AddComponent<WhenFoundEventHelper>();
        }
    }

    [Button("게임 시작시 이벤트 설정")]
    public void SetOnEnableHelper()
    {
        OnEnableHelper onEnableHelper = GetComponent<OnEnableHelper>();
        if (onEnableHelper == null)
        {
            onEnableHelper = gameObject.AddComponent<OnEnableHelper>();
        }
    }

    [Button("스트레치 VFX 그룹 설정")]
    public void SetStretchVFXGroupSetting()
    {
        StretchVFXGroupSetting stretchVFXGroupSetting = GetComponent<StretchVFXGroupSetting>();
        if (stretchVFXGroupSetting == null)
        {
            stretchVFXGroupSetting = gameObject.AddComponent<StretchVFXGroupSetting>();
        }
    }

    [Button("흔들림 VFX 그룹 설정")]
    public void SetShakeVFXGroupSetting()
    {
        ShakeVFXGroupSetting shakeVFXGroupSetting = GetComponent<ShakeVFXGroupSetting>();
        if (shakeVFXGroupSetting == null)
        {
            shakeVFXGroupSetting = gameObject.AddComponent<ShakeVFXGroupSetting>();
        }
    }

    [Button("애니메이션 오브젝트 설정")]
    public void SetAnimationObj()
    {
        AnimationObj animationObj = GetComponent<AnimationObj>();
        if (animationObj == null)
        {
            animationObj = gameObject.AddComponent<AnimationObj>();
        }
    }

    [Button("위치 설정 오브젝트 설정")]
    public void SetPositionHelperSetting()
    {
        SetPositionHelper setPositionHelper = GetComponent<SetPositionHelper>();
        if (setPositionHelper == null)
        {
            setPositionHelper = gameObject.AddComponent<SetPositionHelper>();
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

    // Remove WhenFoundEventHelper if present
    WhenFoundEventHelper whenFoundEventHelper = GetComponent<WhenFoundEventHelper>();
    if (whenFoundEventHelper != null)
    {
#if UNITY_EDITOR
        UnityEditor.Undo.RecordObject(gameObject, "Remove WhenFoundEventHelper");
        UnityEditor.Undo.DestroyObjectImmediate(whenFoundEventHelper);
        whenFoundEventHelper = null;
#endif
    }

    // Remove OnEnableHelper if present
    OnEnableHelper onEnableHelper = GetComponent<OnEnableHelper>();
    if (onEnableHelper != null)
    {
#if UNITY_EDITOR
        UnityEditor.Undo.RecordObject(gameObject, "Remove OnEnableHelper");
        UnityEditor.Undo.DestroyObjectImmediate(onEnableHelper);
        onEnableHelper = null;
#endif
    }

    // Remove StretchVFXGroupSetting if present
    StretchVFXGroupSetting stretchVFXGroupSetting = GetComponent<StretchVFXGroupSetting>();
    if (stretchVFXGroupSetting != null)
    {
#if UNITY_EDITOR
        UnityEditor.Undo.RecordObject(gameObject, "Remove StretchVFXGroupSetting");
        UnityEditor.Undo.DestroyObjectImmediate(stretchVFXGroupSetting);
        stretchVFXGroupSetting = null;
#endif
    }

    // Remove ShakeVFXGroupSetting if present
    ShakeVFXGroupSetting shakeVFXGroupSetting = GetComponent<ShakeVFXGroupSetting>();
    if (shakeVFXGroupSetting != null)
    {
#if UNITY_EDITOR
        UnityEditor.Undo.RecordObject(gameObject, "Remove ShakeVFXGroupSetting");
        UnityEditor.Undo.DestroyObjectImmediate(shakeVFXGroupSetting);
        shakeVFXGroupSetting = null;
#endif
    }

    // Remove AnimationObj if present
    AnimationObj animationObj = GetComponent<AnimationObj>();
    if (animationObj != null)
    {
#if UNITY_EDITOR
        UnityEditor.Undo.RecordObject(gameObject, "Remove AnimationObj");
        UnityEditor.Undo.DestroyObjectImmediate(animationObj);
        animationObj = null;
#endif
    }

    // Remove SetPositionHelper if present
    SetPositionHelper setPositionHelper = GetComponent<SetPositionHelper>();
    if (setPositionHelper != null)
    {
#if UNITY_EDITOR
        UnityEditor.Undo.RecordObject(gameObject, "Remove SetPositionHelper");
        UnityEditor.Undo.DestroyObjectImmediate(setPositionHelper);
        setPositionHelper = null;
#endif
    }
    }
}

