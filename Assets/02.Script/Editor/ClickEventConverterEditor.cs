using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEditor.Events;
using System.Reflection;
using Effect;

// Utility to convert ClickEvent components to LeanClickEvent in editor time
public static class ClickEventConverterEditor
{
    [MenuItem("Tools/Convert ClickEvent -> LeanClickEvent (Selected)")]
    public static void ConvertSelected()
    {
        ConvertInTargets(Selection.gameObjects);
    }

    [MenuItem("Tools/Convert ClickEvent -> LeanClickEvent (All In Active Scene)")]
    public static void ConvertAllInScene()
    {
        var roots = SceneManager.GetActiveScene().GetRootGameObjects();
        ConvertInTargets(roots);
    }

    private static void ConvertInTargets(GameObject[] targets)
    {
        // Collect targets
        if (targets == null || targets.Length == 0)
        {
            EditorUtility.DisplayDialog("ClickEvent Converter", "No GameObjects provided.", "OK");
            return;
        }

        int converted = 0;
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();

        foreach (var root in targets)
        {
            if (root == null) continue;

            // Find all ClickEvent components in this root and children (including inactive)
            var comps = root.GetComponentsInChildren<DeskCat.FindIt.Scripts.Core.Main.Utility.ClickedFunction.ClickEvent>(true);
            foreach (var src in comps)
            {
                if (src == null) continue;
                if (ConvertComponent(src)) converted++;
            }
        }

        Undo.CollapseUndoOperations(group);
        // Mark scene dirty so user can save changes
        EditorSceneManager.MarkAllScenesDirty();

        EditorUtility.DisplayDialog("ClickEvent Converter", $"Converted {converted} component(s).", "OK");
    }

    // Returns true if conversion performed
    private static bool ConvertComponent(DeskCat.FindIt.Scripts.Core.Main.Utility.ClickedFunction.ClickEvent src)
    {
        if (src == null) return false;

        var go = src.gameObject;

        // If a LeanClickEvent already exists, we'll still copy data and remove ClickEvent
        var existingLean = go.GetComponent<LeanClickEvent>();

        Undo.RegisterCompleteObjectUndo(go, "Convert ClickEvent");

        LeanClickEvent dst = existingLean ?? Undo.AddComponent<LeanClickEvent>(go);

        // Copy simple fields via reflection
        try
        {
            var srcType = src.GetType();
            var dstType = dst.GetType();

            // Copy Enable (bool)
            var enableFieldSrc = srcType.GetField("Enable", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var enableFieldDst = dstType.GetField("Enable", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (enableFieldSrc != null && enableFieldDst != null)
            {
                enableFieldDst.SetValue(dst, enableFieldSrc.GetValue(src));
            }

            // Copy _maxClickCount if present
            var maxFieldSrc = srcType.GetField("_maxClickCount", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var maxFieldDst = dstType.GetField("_maxClickCount", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (maxFieldSrc != null && maxFieldDst != null)
            {
                maxFieldDst.SetValue(dst, maxFieldSrc.GetValue(src));
            }

            // Copy UnityEvent persistent listeners for known event fields
            string[] eventFields = new string[] { "OnMouseDownEvent", "OnMouseUpEvent", "OnClickEvent" };

            foreach (var ef in eventFields)
            {
                var srcEventField = srcType.GetField(ef, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var dstEventField = dstType.GetField(ef, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (srcEventField == null || dstEventField == null) continue;

                var srcEvent = srcEventField.GetValue(src) as UnityEngine.Events.UnityEventBase;
                var dstEvent = dstEventField.GetValue(dst) as UnityEngine.Events.UnityEventBase;
                if (srcEvent == null || dstEvent == null) continue;

                // Copy persistent listeners by reading target and method name and recreating delegate where possible
                var getCountMethod = typeof(UnityEngine.Events.UnityEventBase).GetMethod("GetPersistentEventCount", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var getTargetMethod = typeof(UnityEngine.Events.UnityEventBase).GetMethod("GetPersistentTarget", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var getMethodNameMethod = typeof(UnityEngine.Events.UnityEventBase).GetMethod("GetPersistentMethodName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                int count = 0;
                if (getCountMethod != null) count = (int)getCountMethod.Invoke(srcEvent, null);

                for (int i = 0; i < count; i++)
                {
                    var target = getTargetMethod?.Invoke(srcEvent, new object[] { i });
                    var methodName = getMethodNameMethod?.Invoke(srcEvent, new object[] { i }) as string;
                    if (target == null || string.IsNullOrEmpty(methodName)) continue;

                    // StretchVFX 컴포넌트의 PlayVFX 메서드만 연결 (소리 제외)
                    if (target is StretchVFX && methodName == "PlayVFX")
                    {
                        try
                        {
                            var del = System.Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction), target, methodName) as UnityEngine.Events.UnityAction;
                            if (del != null)
                            {
                                UnityEventTools.AddPersistentListener(dstEvent as UnityEngine.Events.UnityEvent, del);
                                Debug.Log($"PlayVFX 메서드만 연결됨: {target}.{methodName} on {go.name}");
                            }
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogWarning($"Failed to copy PlayVFX listener {methodName} on {go.name}: {ex.Message}");
                        }
                    }
                    // 다른 메서드는 무시 (소리 재생 이벤트 등 중복 방지)
                    else
                    {
                        Debug.Log($"Skipping non-PlayVFX method: {target}.{methodName} on {go.name}");
                    }
                }
            }

            EditorUtility.SetDirty(dst);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error while copying ClickEvent data: {e.Message}\n{e.StackTrace}");
        }

        // Remove original ClickEvent
        Undo.DestroyObjectImmediate(src);

        return true;
    }
}
