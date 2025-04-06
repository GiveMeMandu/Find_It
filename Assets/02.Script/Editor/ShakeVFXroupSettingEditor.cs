using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using System.Reflection;
using DeskCat.FindIt.Scripts.Core.Main.Utility.ClickedFunction;
using Effect;
using UnityEditor.SceneManagement;
using Sirenix.OdinInspector.Editor;

// Odin Inspector 버튼이 표시되도록 커스텀 에디터를 수정
// OdinEditor를 상속받아 Odin Inspector의 버튼이 정상 표시되도록 함
[CustomEditor(typeof(ShakeVFXGroupSetting))]
public class ShakeVFXGroupSettingEditor : OdinEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        // 인스펙터에 버튼 추가
        EditorGUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("이벤트 영구 연결 (Runtime Only)", GUILayout.Height(30), GUILayout.Width(250)))
        {
            RegisterPersistentListenersStatic((ShakeVFXGroupSetting)target);
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(10);
    }

    [MenuItem("GameObject/ShakeVFXGroupSetting/이벤트 영구 연결 (Runtime Only)", false, 0)]
    private static void RegisterPersistentListenersMenuItem()
    {
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject == null)
        {
            Debug.LogWarning("게임 오브젝트를 선택한 후 다시 시도해주세요.");
            return;
        }

        ShakeVFXGroupSetting setting = selectedObject.GetComponent<ShakeVFXGroupSetting>();
        if (setting == null)
        {
            Debug.LogWarning("선택한 오브젝트에 ShakeVFXGroupSetting 컴포넌트가 없습니다.");
            return;
        }

        RegisterPersistentListenersStatic(setting);
    }

    [MenuItem("CONTEXT/ShakeVFXGroupSetting/이벤트 영구 연결 (Runtime Only)")]
    private static void RegisterPersistentListenersContextMenu(MenuCommand command)
    {
        ShakeVFXGroupSetting setting = (ShakeVFXGroupSetting)command.context;
        RegisterPersistentListenersStatic(setting);
    }

    public static void RegisterPersistentListenersStatic(ShakeVFXGroupSetting setting)
    {
        if (setting == null) return;

        // 자기 자신과 자식 오브젝트 목록 생성
        var targetObjects = new System.Collections.Generic.List<GameObject>();
        targetObjects.Add(setting.gameObject);
        
        if (setting.applyToChildren)
        {
            var childTransforms = setting.gameObject.GetComponentsInChildren<Transform>(true);
            foreach (var child in childTransforms)
            {
                if (child != null && child.gameObject != setting.gameObject)
                {
                    targetObjects.Add(child.gameObject);
                }
            }
        }
        
        // 각 오브젝트에 영구 리스너 등록
        foreach (var obj in targetObjects)
        {
            if (obj == null) continue;
            
            // 컴포넌트를 재생성하는 대신 기존 컴포넌트를 활용합니다
            ClickEvent clickEvent = obj.GetComponent<ClickEvent>();
            if (clickEvent == null)
            {
                clickEvent = obj.AddComponent<ClickEvent>();
                Debug.Log($"{obj.name}에 ClickEvent 컴포넌트를 추가했습니다.");
            }
            
            ShakeVFX shakeVFX = obj.GetComponent<ShakeVFX>();
            if (shakeVFX == null)
            {
                shakeVFX = obj.AddComponent<ShakeVFX>();
                Debug.Log($"{obj.name}에 ShakeVFX 컴포넌트를 추가했습니다.");
                
                // 이펙트 설정 적용
                setting.ApplyEffectSettings(shakeVFX);
            }
            
            // BoxCollider2D 확인
            BoxCollider2D boxCollider = obj.GetComponent<BoxCollider2D>();
            if (boxCollider == null)
            {
                boxCollider = obj.AddComponent<BoxCollider2D>();
                boxCollider.isTrigger = true;
                
                // 스프라이트 크기로 콜라이더 설정
                if (setting.useSpriteBounds)
                {
                    SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null && spriteRenderer.sprite != null)
                    {
                        Bounds bounds = spriteRenderer.sprite.bounds;
                        boxCollider.size = new Vector2(bounds.size.x, bounds.size.y);
                        Debug.Log($"{obj.name}의 콜라이더가 스프라이트 크기({boxCollider.size})로 설정되었습니다.");
                    }
                    else
                    {
                        boxCollider.size = new Vector2(1f, 1f);
                        Debug.LogWarning($"{obj.name}에 SpriteRenderer 또는 Sprite가 없어 기본 콜라이더 크기(1,1)로 설정되었습니다.");
                    }
                }
                else
                {
                    boxCollider.size = setting.colliderSize;
                }
                
                boxCollider.offset = setting.colliderOffset;
                Debug.Log($"{obj.name}에 BoxCollider2D 컴포넌트를 추가했습니다.");
            }
            
            try
            {
                // 이벤트가 있는지 확인하고 초기화
                if (clickEvent.OnClickEvent == null)
                {
                    clickEvent.OnClickEvent = new UnityEvent();
                }
                
                // 기존 리스너 제거
                clickEvent.OnClickEvent.RemoveAllListeners();
                
                // ClickEvent에 PlayVFX 메서드 직접 연결
                UnityAction action = new UnityAction(shakeVFX.PlayVFX);
                clickEvent.OnClickEvent.AddListener(action);
                
                // SerializedObject를 통한 등록
                SerializedObject serializedClickEvent = new SerializedObject(clickEvent);
                serializedClickEvent.Update();
                
                SerializedProperty onClickEventProperty = serializedClickEvent.FindProperty("OnClickEvent");
                if (onClickEventProperty != null)
                {
                    // m_PersistentCalls.m_Calls 초기화 및 설정
                    SerializedProperty persistentCallsProperty = onClickEventProperty.FindPropertyRelative("m_PersistentCalls.m_Calls");
                    persistentCallsProperty.ClearArray();
                    persistentCallsProperty.arraySize = 1;
                    
                    SerializedProperty call = persistentCallsProperty.GetArrayElementAtIndex(0);
                    call.FindPropertyRelative("m_Target").objectReferenceValue = shakeVFX;
                    call.FindPropertyRelative("m_MethodName").stringValue = "PlayVFX";
                    call.FindPropertyRelative("m_Mode").enumValueIndex = 1; // PersistentListenerMode.Void
                    call.FindPropertyRelative("m_Arguments.m_ObjectArgumentAssemblyTypeName").stringValue = "UnityEngine.Object, UnityEngine";
                    call.FindPropertyRelative("m_CallState").enumValueIndex = 2; // UnityEventCallState.RuntimeOnly
                    
                    serializedClickEvent.ApplyModifiedProperties();
                    Debug.Log($"{obj.name}에 ShakeVFX.PlayVFX 메서드가 Runtime Only 상태로 등록되었습니다.");
                }
                
                // 변경사항을 에디터에 알리기
                EditorUtility.SetDirty(clickEvent);
                EditorUtility.SetDirty(shakeVFX);
                EditorUtility.SetDirty(obj);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"{obj.name}의 이벤트 등록 중 오류 발생: {e.Message}");
            }
        }
        
        // 씬 변경 저장
        EditorUtility.SetDirty(setting);
        if (!Application.isPlaying)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
        }
        
        Debug.Log("모든 영구 리스너가 Runtime Only 상태로 등록되었습니다!");
    }
}
