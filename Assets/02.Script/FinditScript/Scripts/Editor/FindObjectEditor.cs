using UnityEditor;
using UnityEngine;

namespace DeskCat.FindIt.Scripts.Editor
{
    public class FindObjectEditor : EditorWindow
    {
        private const string MenuItemPath = "FindIt/DefaultSettingWindow";

        [MenuItem(MenuItemPath)]
        private static void OpenGUILayout()
        {
            GetWindow<FindObjectEditor>().Show();
        }

        private Object objFolder;
        public static string data;

        private void OnGUI()
        {
            objFolder = EditorGUILayout.ObjectField(objFolder, typeof(ScriptableObject), true);
            var objPath = AssetDatabase.GetAssetPath(objFolder);
            EditorGUILayout.LabelField(objPath);
            

            //C# Do Some Directory File Searching
        }
    }
}