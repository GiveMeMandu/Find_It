using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityWeld_Editor;
using UnityWeld.Binding;
using UnityWeld.Binding.Internal;
using TMPro;
using I2.Loc;

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
            DrawDefaultInspector();
            
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

            DrawLine(Color.black);

            if (GUILayout.Button("I2 로컬라이즈(폰트) 일괄 배치"))
            {
                AutoSetupI2Fonts();
            }
        }
        
        private void AutoSetupI2Fonts()
        {
            GameObject gameObject = ((Component)target).gameObject;
            var bindings = gameObject.GetComponentsInChildren<AbstractMemberBinding>(true);
            int count = 0;
            
            StringBuilder sb = new StringBuilder();
            List<GameObject> processedObjects = new List<GameObject>();
            
            foreach (var binding in bindings)
            {
                // TMP가 없으면 텍스트 UI가 아니므로 패스
                var tmp = binding.GetComponent<TextMeshProUGUI>();
                if (tmp == null) continue;

                // Localize 컴포넌트가 없으면 추가
                var localize = binding.GetComponent<Localize>();
                if (localize == null)
                {
                    localize = Undo.AddComponent<Localize>(binding.gameObject);
                }

                // Undo(Ctrl+Z) 기록
                Undo.RecordObject(localize, "Setup I2 Localize Font");
                
                // 데이터(텍스트)는 Weld가 바인딩하므로, I2의 텍스트 번역은 비활성화 처리('-')
                localize.Term = "-"; 

                EditorUtility.SetDirty(binding.gameObject);
                
                sb.AppendLine($"ㄴ{binding.gameObject.name}");
                processedObjects.Add(binding.gameObject);
                count++;
            }
            
            if (count > 0)
            {
                sb.Insert(0, $"총 {count}개의 요소에 I2 로컬라이즈 셋업을 완료했습니다.\n\n");
                Selection.objects = processedObjects.ToArray();
            }
            else
            {
                sb.AppendLine("적용할 대상(TMP가 있는 바인딩 요소)이 없습니다.");
            }
            
            EditorUtility.DisplayDialog("I2 로컬라이즈 셋업 결과", sb.ToString(), "확인");
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
