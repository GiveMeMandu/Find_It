using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityWeld_Editor;
using UnityWeld.Binding;
using UnityWeld.Binding.Internal;

namespace UI.Editor
{
    [CustomEditor(typeof(BaseViewModel), true)]
    public class ViewModelEditor : BaseBindingEditor
    {
        protected override void OnEnabled()
        {
            
        }

        protected override void OnInspector()
        {
            DrawLine(Color.black);
            
            if (GUILayout.Button("바인딩 검사하기"))
            {
                CheckBindings();
            }
            
            if (GUILayout.Button("클래스 수정하기"))
            {
                OpenScript();
            }
            
            if (GUILayout.Button("클래스 위치 보기"))
            {
                ShowScriptLocation();
            }
        }
        
        private void CheckBindings()
        {
            GameObject gameObject = ((Component)target).gameObject;
            var bindings = gameObject.GetComponentsInChildren<AbstractMemberBinding>(true);
            StringBuilder sb = new();
            List<GameObject> errorObjects = new();
            
            foreach (var binding in bindings)
            {
                try
                {
                    if (TypeResolver.IsWrongBinding(binding))
                    {
                        sb.AppendLine($"ㄴ{binding.gameObject.name} - {binding.name}");
                        errorObjects.Add(binding.gameObject);
                    }
                }
                catch(Exception)
                {
                    sb.AppendLine($"ㄴ{binding.gameObject.name} - {binding.name}");
                    errorObjects.Add(binding.gameObject);
                }
            }
            
            if(sb.Length == 0)
            {
                sb.AppendLine("바인딩 오류가 없습니다.");
            }
            else
            {
                sb.Insert(0, "바인딩 오류가 발견되었습니다.\n");
                // 오류가 있는 오브젝트들을 Selection에 등록하여 하이라이트
                Selection.objects = errorObjects.ToArray();
            }
            
            EditorUtility.DisplayDialog("바인딩 검사 결과", sb.ToString(), "확인");
        }

        private void OpenScript()
        {
            MonoScript script = MonoScript.FromMonoBehaviour((MonoBehaviour)target);
            if (script != null)
            {
                AssetDatabase.OpenAsset(script);
            }
            else
            {
                EditorUtility.DisplayDialog("오류", "스크립트를 찾을 수 없습니다.", "확인");
            }
        }

        private void ShowScriptLocation()
        {
            MonoScript script = MonoScript.FromMonoBehaviour((MonoBehaviour)target);
            if (script != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(script);
                var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                EditorGUIUtility.PingObject(obj);
            }
            else
            {
                EditorUtility.DisplayDialog("오류", "스크립트를 찾을 수 없습니다.", "확인");
            }
        }

        void DrawLine(Color color)
        {
            GUILayout.Space(12.0f);
            Rect rect = GUILayoutUtility.GetLastRect();
            GUI.color = color;
            GUI.DrawTexture(new Rect(0.0f, rect.yMin + 6.0f, Screen.width, 4.0f), EditorGUIUtility.whiteTexture);
            GUI.DrawTexture(new Rect(0.0f, rect.yMin + 6.0f, Screen.width, 1.0f), EditorGUIUtility.whiteTexture);
            GUI.DrawTexture(new Rect(0.0f, rect.yMin + 9.0f, Screen.width, 1.0f), EditorGUIUtility.whiteTexture);
            GUI.color = Color.white;
        }
    }
}
