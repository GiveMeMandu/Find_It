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
[CustomEditor(typeof(StretchVFXGroupSetting))]
public class StretchVFXGroupSettingEditor : OdinEditor
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
            RegisterPersistentListenersStatic((StretchVFXGroupSetting)target);
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(10);
    }

    [MenuItem("GameObject/StretchVFXGroupSetting/이벤트 영구 연결 (Runtime Only)", false, 0)]
    private static void RegisterPersistentListenersMenuItem()
    {
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject == null)
        {
            Debug.LogWarning("게임 오브젝트를 선택한 후 다시 시도해주세요.");
            return;
        }

        StretchVFXGroupSetting setting = selectedObject.GetComponent<StretchVFXGroupSetting>();
        if (setting == null)
        {
            Debug.LogWarning("선택한 오브젝트에 StretchVFXGroupSetting 컴포넌트가 없습니다.");
            return;
        }

        RegisterPersistentListenersStatic(setting);
    }

    [MenuItem("CONTEXT/StretchVFXGroupSetting/이벤트 영구 연결 (Runtime Only)")]
    private static void RegisterPersistentListenersContextMenu(MenuCommand command)
    {
        StretchVFXGroupSetting setting = (StretchVFXGroupSetting)command.context;
        RegisterPersistentListenersStatic(setting);
    }

    public static void RegisterPersistentListenersStatic(StretchVFXGroupSetting setting)
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
            
            // 기존 컴포넌트들을 모두 제거 후 재생성 (중복 방지)
            ClickEvent existingClickEvent = obj.GetComponent<ClickEvent>();
            if (existingClickEvent != null)
            {
                Object.DestroyImmediate(existingClickEvent);
                Debug.Log($"{obj.name}에서 기존 ClickEvent 컴포넌트를 제거했습니다.");
            }
            
            LeanClickEvent existingLeanClickEvent = obj.GetComponent<LeanClickEvent>();
            if (existingLeanClickEvent != null)
            {
                Object.DestroyImmediate(existingLeanClickEvent);
                Debug.Log($"{obj.name}에서 기존 LeanClickEvent 컴포넌트를 제거했습니다.");
            }
            
            StretchVFX existingStretchVFX = obj.GetComponent<StretchVFX>();
            if (existingStretchVFX != null)
            {
                Object.DestroyImmediate(existingStretchVFX);
                Debug.Log($"{obj.name}에서 기존 StretchVFX 컴포넌트를 제거했습니다.");
            }
            
            ClickTouchSound existingClickTouchSound = obj.GetComponent<ClickTouchSound>();
            if (existingClickTouchSound != null)
            {
                Object.DestroyImmediate(existingClickTouchSound);
                Debug.Log($"{obj.name}에서 기존 ClickTouchSound 컴포넌트를 제거했습니다.");
            }
            
            // LeanClickEvent 컴포넌트 새로 추가 (기본으로 LeanClickEvent 사용)
            LeanClickEvent leanClickEvent = obj.AddComponent<LeanClickEvent>();
            Debug.Log($"{obj.name}에 LeanClickEvent 컴포넌트를 추가했습니다.");
            
            // StretchVFX 컴포넌트 새로 추가
            StretchVFX stretchVFX = obj.AddComponent<StretchVFX>();
            Debug.Log($"{obj.name}에 StretchVFX 컴포넌트를 추가했습니다.");
            
            // 이펙트 설정 적용
            setting.ApplyEffectSettings(stretchVFX);
            
            // ClickTouchSound 컴포넌트 확인 및 추가
            ClickTouchSound clickTouchSound = null;
            if (setting.addClickSound)
            {
                clickTouchSound = obj.AddComponent<ClickTouchSound>();
                clickTouchSound.SetSoundType(setting.clickSoundType);
                Debug.Log($"{obj.name}에 ClickTouchSound 컴포넌트를 추가했습니다.");
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
                if (leanClickEvent.OnClickEvent == null)
                {
                    leanClickEvent.OnClickEvent = new UnityEvent();
                }
                
                // 기존 리스너 제거
                leanClickEvent.OnClickEvent.RemoveAllListeners();
                
                // LeanClickEvent에 PlayVFX 메서드만 직접 연결 (PlayVFX만!)
                UnityAction vfxAction = new UnityAction(stretchVFX.PlayVFX);
                leanClickEvent.OnClickEvent.AddListener(vfxAction);
                
                // 사운드 이벤트는 StretchVFX의 OnEffectStart에 연결 (있는 경우)
                if (setting.addClickSound && clickTouchSound != null)
                {
                    // StretchVFX의 OnEffectStart에 사운드 연결
                    if (stretchVFX.OnEffectStart != null)
                    {
                        stretchVFX.OnEffectStart.RemoveListener(clickTouchSound.PlayClickSound);
                        UnityAction soundAction = new UnityAction(clickTouchSound.PlayClickSound);
                        stretchVFX.OnEffectStart.AddListener(soundAction);
                        Debug.Log($"{obj.name}의 StretchVFX.OnEffectStart에 사운드가 연결되었습니다.");
                    }
                }
                
                // SerializedObject를 통한 등록
                SerializedObject serializedLeanClickEvent = new SerializedObject(leanClickEvent);
                serializedLeanClickEvent.Update();
                
                SerializedProperty onClickEventProperty = serializedLeanClickEvent.FindProperty("OnClickEvent");
                if (onClickEventProperty != null)
                {
                    // m_PersistentCalls.m_Calls 초기화 및 설정
                    SerializedProperty persistentCallsProperty = onClickEventProperty.FindPropertyRelative("m_PersistentCalls.m_Calls");
                    persistentCallsProperty.ClearArray();
                    
                    // LeanClickEvent에는 오직 PlayVFX만 연결
                    persistentCallsProperty.arraySize = 1;
                    
                    // VFX 이벤트 설정 (PlayVFX만!)
                    SerializedProperty vfxCall = persistentCallsProperty.GetArrayElementAtIndex(0);
                    vfxCall.FindPropertyRelative("m_Target").objectReferenceValue = stretchVFX;
                    vfxCall.FindPropertyRelative("m_MethodName").stringValue = "PlayVFX";
                    vfxCall.FindPropertyRelative("m_Mode").enumValueIndex = 1; // PersistentListenerMode.Void
                    vfxCall.FindPropertyRelative("m_Arguments.m_ObjectArgumentAssemblyTypeName").stringValue = "UnityEngine.Object, UnityEngine";
                    vfxCall.FindPropertyRelative("m_CallState").enumValueIndex = 2; // UnityEventCallState.RuntimeOnly
                    
                    serializedLeanClickEvent.ApplyModifiedProperties();
                    Debug.Log($"{obj.name}에 PlayVFX 이벤트가 Runtime Only 상태로 등록되었습니다.");
                }
                
                // 사운드 이벤트는 StretchVFX의 OnEffectStart에 별도로 등록
                if (setting.addClickSound && clickTouchSound != null)
                {
                    SerializedObject serializedStretchVFX = new SerializedObject(stretchVFX);
                    serializedStretchVFX.Update();
                    
                    SerializedProperty onEffectStartProperty = serializedStretchVFX.FindProperty("OnEffectStart");
                    if (onEffectStartProperty != null)
                    {
                        SerializedProperty soundPersistentCallsProperty = onEffectStartProperty.FindPropertyRelative("m_PersistentCalls.m_Calls");
                        soundPersistentCallsProperty.ClearArray();
                        soundPersistentCallsProperty.arraySize = 1;
                        
                        // 사운드 이벤트 설정
                        SerializedProperty soundCall = soundPersistentCallsProperty.GetArrayElementAtIndex(0);
                        soundCall.FindPropertyRelative("m_Target").objectReferenceValue = clickTouchSound;
                        soundCall.FindPropertyRelative("m_MethodName").stringValue = "PlayClickSound";
                        soundCall.FindPropertyRelative("m_Mode").enumValueIndex = 1; // PersistentListenerMode.Void
                        soundCall.FindPropertyRelative("m_Arguments.m_ObjectArgumentAssemblyTypeName").stringValue = "UnityEngine.Object, UnityEngine";
                        soundCall.FindPropertyRelative("m_CallState").enumValueIndex = 2; // UnityEventCallState.RuntimeOnly
                        
                        serializedStretchVFX.ApplyModifiedProperties();
                        Debug.Log($"{obj.name}의 StretchVFX.OnEffectStart에 사운드 이벤트가 Runtime Only 상태로 등록되었습니다.");
                    }
                }
                
                // 변경사항을 에디터에 알리기
                EditorUtility.SetDirty(leanClickEvent);
                EditorUtility.SetDirty(stretchVFX);
                if (clickTouchSound != null)
                    EditorUtility.SetDirty(clickTouchSound);
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
        
        Debug.Log("모든 영구 리스너가 Runtime Only 상태로 등록되었습니다! (LeanClickEvent: PlayVFX만, StretchVFX.OnEffectStart: 사운드만)");
    }
}
