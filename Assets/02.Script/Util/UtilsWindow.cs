#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
public class UtilsWindow : EditorWindow 
{
    /// <summary>
    /// 윈도우 열기
    /// </summary>
    [MenuItem("CustomMenu/Utils", priority = 1)]
    public static void ShowWindow() => GetWindow(typeof(UtilsWindow));
    
    private void OnGUI()
    {
        if (GUILayout.Button("전체 갱신"))
        {
            AssetDatabase.ForceReserializeAssets();
        }
    }
}
#endif