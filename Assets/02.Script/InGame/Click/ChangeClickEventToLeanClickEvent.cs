using System.Reflection;
using UnityEngine;
using DeskCat.FindIt.Scripts.Core.Main.Utility.ClickedFunction;
using Sirenix.OdinInspector;

public class ChangeClickEventToLeanClickEvent : MonoBehaviour
{
    [Button("Click Event 린클릭으로 교체")]
    public void ChangeClickEvent()
    {
        var click = GetComponent<ClickEvent>();

        if (click == null)
        {
            Debug.LogWarning("No ClickEvent found on this GameObject.");
            return;
        }

        // Add or reuse LeanClickEvent
        var lean = GetComponent<LeanClickEvent>();
        if (lean == null)
        {
            lean = gameObject.AddComponent<LeanClickEvent>();
        }

        // Copy basic fields
        lean.IsEnable(click.Enable);
        lean.SetMaxClickCount(click._maxClickCount);

        // Copy UnityEvents (assigning the event instances)
        lean.OnMouseDownEvent = click.OnMouseDownEvent;
        lean.OnMouseUpEvent = click.OnMouseUpEvent;
        lean.OnClickEvent = click.OnClickEvent;

        // Copy current click count via reflection if possible
        try
        {
            var clickCount = click.GetClickCount();
            var fi = typeof(LeanClickEvent).GetField("_clickCount", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fi != null)
            {
                fi.SetValue(lean, clickCount);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("Failed to copy click count: " + ex.Message);
        }

        // Remove old ClickEvent component
#if UNITY_EDITOR
        UnityEditor.Undo.RecordObject(gameObject, "Replace ClickEvent with LeanClickEvent");
        UnityEditor.Undo.DestroyObjectImmediate(click);
#else
        Destroy(click);
#endif

        Debug.Log("Replaced ClickEvent with LeanClickEvent on " + gameObject.name);
    }

    [Button("LeanClick -> Click Event으로 교체")]
    public void ChangeLeanToClickEvent()
    {
        var lean = GetComponent<LeanClickEvent>();

        if (lean == null)
        {
            Debug.LogWarning("No LeanClickEvent found on this GameObject.");
            return;
        }

        var click = GetComponent<ClickEvent>();
        if (click == null)
        {
            click = gameObject.AddComponent<ClickEvent>();
        }

        // Copy basic fields
        click.IsEnable(lean.Enable);
        click.SetMaxClickCount(lean._maxClickCount);

        // Copy UnityEvents
        click.OnMouseDownEvent = lean.OnMouseDownEvent;
        click.OnMouseUpEvent = lean.OnMouseUpEvent;
        click.OnClickEvent = lean.OnClickEvent;

        // Copy current click count via reflection
        try
        {
            var clickCount = lean.GetClickCount();
            var fi = typeof(ClickEvent).GetField("_clickCount", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fi != null)
            {
                fi.SetValue(click, clickCount);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("Failed to copy click count: " + ex.Message);
        }

        // Remove LeanClickEvent
#if UNITY_EDITOR
        UnityEditor.Undo.RecordObject(gameObject, "Replace LeanClickEvent with ClickEvent");
        UnityEditor.Undo.DestroyObjectImmediate(lean);
#else
        Destroy(lean);
#endif

        Debug.Log("Replaced LeanClickEvent with ClickEvent on " + gameObject.name);
    }

    [Button("클릭 컴포넌트 제거 (ClickEvent, LeanClickEvent)")]
    public void RemoveClickComponents()
    {
        var click = GetComponent<ClickEvent>();
        var lean = GetComponent<LeanClickEvent>();

        if (click == null && lean == null)
        {
            Debug.LogWarning("No ClickEvent or LeanClickEvent found on this GameObject.");
            return;
        }

#if UNITY_EDITOR
        UnityEditor.Undo.RecordObject(gameObject, "Remove Click Components");
        if (click != null) UnityEditor.Undo.DestroyObjectImmediate(click);
        if (lean != null) UnityEditor.Undo.DestroyObjectImmediate(lean);
#else
        if (click != null) Destroy(click);
        if (lean != null) Destroy(lean);
#endif

        Debug.Log("Removed ClickEvent/LeanClickEvent from " + gameObject.name);
    }
    [Button("해당 컴포넌트 제거")]
    public void RemoveThisComponent()
    {
        try
        {
            // 모든 대상 오브젝트에서 컴포넌트 제거
            GameObject obj = this.gameObject;
            if (obj == null) return;
            ChangeClickEventToLeanClickEvent changeClickEventToLeanClickEvent = obj.GetComponent<ChangeClickEventToLeanClickEvent>();
            if (changeClickEventToLeanClickEvent != null)
            {
                DestroyImmediate(changeClickEventToLeanClickEvent);
            }
            Debug.Log("모든 관련 컴포넌트가 제거되었습니다.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"컴포넌트 제거 중 오류 발생: {e.Message}\n{e.StackTrace}");
        }
    }
}
