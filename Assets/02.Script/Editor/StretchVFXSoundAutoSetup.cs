using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using System.Collections.Generic;
using System.Linq;
using Effect;
using DeskCat.FindIt.Scripts.Core.Main.Utility.ClickedFunction;

public class StretchVFXSoundAutoSetup : EditorWindow
{
    [MenuItem("Tools/StretchVFX/사운드 자동 설정")]
    public static void ShowWindow()
    {
        StretchVFXSoundAutoSetup window = GetWindow<StretchVFXSoundAutoSetup>("StretchVFX 사운드 설정");
        window.minSize = new Vector2(400, 300);
        window.Show();
    }

    [MenuItem("Tools/StretchVFX/현재 씬의 모든 StretchVFX에 사운드 추가")]
    public static void AddSoundToAllStretchVFX()
    {
        AddSoundToAllStretchVFXInScene();
    }

    [MenuItem("Tools/StretchVFX/현재 씬의 모든 StretchVFX에서 사운드 제거")]
    public static void RemoveSoundFromAllStretchVFX()
    {
        RemoveSoundFromAllStretchVFXInScene();
    }

    [MenuItem("Tools/StretchVFX/선택된 오브젝트들에 사운드 추가")]
    public static void AddSoundToSelectedObjects()
    {
        AddSoundToSelectedStretchVFX();
    }

    // 컨텍스트 메뉴 추가
    [MenuItem("GameObject/StretchVFX Sound/사운드 추가", false, 0)]
    public static void AddSoundContextMenu()
    {
        AddSoundToSelectedStretchVFX();
    }

    [MenuItem("GameObject/StretchVFX Sound/사운드 제거", false, 1)]
    public static void RemoveSoundContextMenu()
    {
        RemoveSoundFromSelectedStretchVFX();
    }

    // 단축키 추가
    [MenuItem("Tools/StretchVFX/사운드 토글 (Ctrl+Shift+S) %#s")]
    public static void ToggleSoundOnSelected()
    {
        var selectedObjects = Selection.gameObjects;
        
        foreach (var obj in selectedObjects)
        {
            var stretchVFX = obj.GetComponent<StretchVFX>();
            if (stretchVFX == null) continue;

            var clickTouchSound = obj.GetComponent<ClickTouchSound>();
            
            if (clickTouchSound == null)
            {
                // 사운드 추가
                AddSoundToSelectedStretchVFX();
                return;
            }
            else
            {
                // 사운드 제거
                RemoveSoundFromSelectedStretchVFX();
                return;
            }
        }
    }

    // GUI 변수들
    private Data.SFXEnum selectedSoundType = Data.SFXEnum.ClickStretch;
    private bool includeChildren = true;
    private bool onlyActiveObjects = true;
    private Vector2 scrollPosition;

    void OnDestroy()
    {
        // EditorApplication.delayCall 경고 해결을 위한 정리 메서드
        // (실제로는 람다 함수로 등록된 콜백들이 자동으로 정리됨)
    }

    void OnGUI()
    {
        GUILayout.Label("StretchVFX 사운드 자동 설정", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 설정 옵션들
        EditorGUILayout.LabelField("설정 옵션", EditorStyles.boldLabel);
        selectedSoundType = (Data.SFXEnum)EditorGUILayout.EnumPopup("사운드 타입:", selectedSoundType);
        includeChildren = EditorGUILayout.Toggle("자식 오브젝트 포함:", includeChildren);
        onlyActiveObjects = EditorGUILayout.Toggle("활성화된 오브젝트만:", onlyActiveObjects);
        
        EditorGUILayout.Space();
        
        // 현재 씬 정보 표시
        EditorGUILayout.LabelField("현재 씬 정보", EditorStyles.boldLabel);
        
        var stretchVFXObjects = FindStretchVFXInScene();
        EditorGUILayout.LabelField($"StretchVFX 컴포넌트가 있는 오브젝트: {stretchVFXObjects.Count}개");
        
        var soundObjects = FindClickTouchSoundInScene();
        EditorGUILayout.LabelField($"ClickTouchSound 컴포넌트가 있는 오브젝트: {soundObjects.Count}개");

        EditorGUILayout.Space();

        // 오브젝트 목록 표시
        EditorGUILayout.LabelField("StretchVFX 오브젝트 목록", EditorStyles.boldLabel);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150));
        
        foreach (var obj in stretchVFXObjects)
        {
            EditorGUILayout.BeginHorizontal();
            
            // 오브젝트 이름과 사운드 컴포넌트 상태 표시
            string soundStatus = obj.GetComponent<ClickTouchSound>() != null ? " [사운드 O]" : " [사운드 X]";
            EditorGUILayout.LabelField($"{obj.name}{soundStatus}");
            
            if (GUILayout.Button("선택", GUILayout.Width(50)))
            {
                Selection.activeGameObject = obj;
                EditorGUIUtility.PingObject(obj);
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndScrollView();
        
        EditorGUILayout.Space();
        
        // 버튼들
        EditorGUILayout.LabelField("일괄 작업", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("모든 StretchVFX에\n사운드 추가", GUILayout.Height(40)))
        {
            // GUI 이벤트 처리 후 실행하도록 지연
            EditorApplication.delayCall += () => AddSoundToAllStretchVFXInScene(selectedSoundType);
        }
        
        if (GUILayout.Button("모든 StretchVFX에서\n사운드 제거", GUILayout.Height(40)))
        {
            // GUI 이벤트 처리 후 실행하도록 지연
            EditorApplication.delayCall += () => RemoveSoundFromAllStretchVFXInScene();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("선택된 오브젝트에\n사운드 추가", GUILayout.Height(40)))
        {
            // GUI 이벤트 처리 후 실행하도록 지연
            EditorApplication.delayCall += () => AddSoundToSelectedStretchVFX(selectedSoundType);
        }
        
        if (GUILayout.Button("선택된 오브젝트에서\n사운드 제거", GUILayout.Height(40)))
        {
            // GUI 이벤트 처리 후 실행하도록 지연
            EditorApplication.delayCall += () => RemoveSoundFromSelectedStretchVFX();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("새로고침", GUILayout.Height(30)))
        {
            Repaint();
        }
    }

    private List<GameObject> FindStretchVFXInScene()
    {
        var objects = new List<GameObject>();
        var allStretchVFX = FindObjectsOfType<StretchVFX>(includeChildren);
        
        foreach (var stretchVFX in allStretchVFX)
        {
            if (onlyActiveObjects && !stretchVFX.gameObject.activeInHierarchy)
                continue;
                
            objects.Add(stretchVFX.gameObject);
        }
        
        return objects;
    }
    
    private List<GameObject> FindClickTouchSoundInScene()
    {
        var objects = new List<GameObject>();
        var allSounds = FindObjectsOfType<ClickTouchSound>(includeChildren);
        
        foreach (var sound in allSounds)
        {
            if (onlyActiveObjects && !sound.gameObject.activeInHierarchy)
                continue;
                
            objects.Add(sound.gameObject);
        }
        
        return objects;
    }

    public static void AddSoundToAllStretchVFXInScene(Data.SFXEnum soundType = Data.SFXEnum.ClickStretch)
    {
        // ClickEventConverterEditor 방식으로 모든 루트 오브젝트부터 재귀적으로 검사
        var rootGameObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        
        Debug.Log($"[AddSoundToAll] 활성 씬의 루트 오브젝트 {rootGameObjects.Length}개를 검사합니다.");
        
        int addedCount = 0;
        int updatedCount = 0;
        int vfxConnectedCount = 0;
        int totalFound = 0;

        // 각 루트 오브젝트와 그 모든 자식들을 검사
        foreach (var rootObj in rootGameObjects)
        {
            if (rootObj == null) continue;

            // 현재 루트와 모든 자식에서 StretchVFX 컴포넌트 찾기 (비활성화된 것도 포함)
            var stretchVFXComponents = rootObj.GetComponentsInChildren<StretchVFX>(true);
            
            Debug.Log($"[AddSoundToAll] 루트 오브젝트 '{rootObj.name}'에서 {stretchVFXComponents.Length}개의 StretchVFX를 발견했습니다.");
            totalFound += stretchVFXComponents.Length;

            foreach (var stretchVFX in stretchVFXComponents)
            {
                if (stretchVFX == null) continue;
                
                var obj = stretchVFX.gameObject;
                
                Debug.Log($"[AddSoundToAll] 처리 시작: {obj.name} | 활성화: {obj.activeInHierarchy} | 경로: {GetGameObjectPath(obj)}");
                
                // 씬에 속해있지 않은 오브젝트는 건너뛰기 (프리팹 에셋 등)
                if (string.IsNullOrEmpty(obj.scene.name))
                {
                    Debug.LogWarning($"[AddSoundToAll] {obj.name}는 씬에 속하지 않아 건너뜁니다.");
                    continue;
                }

                // Undo 지원 (ClickEventConverterEditor 방식)
                UnityEditor.Undo.RegisterCompleteObjectUndo(obj, "Add Sound to StretchVFX");
                
                // Prefab 연결 해제 (변경사항이 확실히 저장되도록)
                if (PrefabUtility.IsPartOfPrefabInstance(obj))
                {
                    // Prefab 루트를 찾아서 언팩
                    GameObject prefabRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(obj);
                    if (prefabRoot != null)
                    {
                        PrefabUtility.UnpackPrefabInstance(prefabRoot, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
                        Debug.Log($"[AddSoundToAll] {obj.name}의 Prefab 루트 '{prefabRoot.name}'의 연결을 해제했습니다.");
                    }
                    else
                    {
                        Debug.LogWarning($"[AddSoundToAll] {obj.name}의 Prefab 루트를 찾을 수 없어 언팩을 건너뜁니다.");
                    }
                }
                
                var clickTouchSound = obj.GetComponent<ClickTouchSound>();
                
                if (clickTouchSound == null)
                {
                    // 사운드 컴포넌트 추가
                    clickTouchSound = UnityEditor.Undo.AddComponent<ClickTouchSound>(obj);
                    clickTouchSound.SetSoundType(soundType);
                    Debug.Log($"[AddSoundToAll] {obj.name}에 ClickTouchSound 컴포넌트를 추가했습니다.");
                    
                    addedCount++;
                }
                else
                {
                    // 기존 사운드 타입 업데이트
                    clickTouchSound.SetSoundType(soundType);
                    Debug.Log($"[AddSoundToAll] {obj.name}의 기존 사운드 타입을 업데이트했습니다.");
                    
                    updatedCount++;
                }
                
                // VFXObject 확인 - StretchVFX는 VFXObject를 상속받으므로 자기 자신이 VFXObject입니다
                VFXObject vfxObject = stretchVFX; // 직접 캐스팅
                
                Debug.Log($"[AddSoundToAll] {obj.name}에서 VFXObject를 확인했습니다: {vfxObject != null}");
                
                if (vfxObject != null)
                {
                    Debug.Log($"[AddSoundToAll] {obj.name}의 OnEffectStart 이벤트 상태: {vfxObject.OnEffectStart != null}");
                    
                    // OnEffectStart 이벤트를 강제로 새로 초기화 (SerializeField 추가로 인한 재초기화)
                    vfxObject.OnEffectStart = new UnityEngine.Events.UnityEvent();
                    vfxObject.OnEffectEnd = new UnityEngine.Events.UnityEvent();
                    Debug.Log($"[AddSoundToAll] {obj.name}의 OnEffectStart/OnEffectEnd 이벤트를 강제로 재초기화했습니다.");
                    
                    // 새 Persistent 리스너 추가
                    try
                    {
                        UnityEventTools.AddPersistentListener(vfxObject.OnEffectStart, clickTouchSound.PlayClickSound);
                        
                        int afterCount = vfxObject.OnEffectStart.GetPersistentEventCount();
                        Debug.Log($"[AddSoundToAll] {obj.name}의 VFXObject OnEffectStart에 Persistent 사운드 이벤트를 연결했습니다. (리스너 수: {afterCount})");
                        
                        vfxConnectedCount++;
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"[AddSoundToAll] {obj.name}에서 Persistent 리스너 추가 실패: {e.Message}");
                    }
                }
                else
                {
                    Debug.LogError($"[AddSoundToAll] {obj.name}에서 VFXObject를 찾을 수 없습니다. 이는 예상되지 않은 상황입니다!");
                }
                
                // ClickEvent가 있다면 사운드 이벤트 연결 (기존 기능 유지)
                var clickEvent = obj.GetComponent<ClickEvent>();
                if (clickEvent != null && clickEvent.OnClickEvent != null)
                {
                    // 기존 사운드 리스너 제거 (중복 방지)
                    clickEvent.OnClickEvent.RemoveListener(clickTouchSound.PlayClickSound);
                    // 새 리스너 추가
                    clickEvent.OnClickEvent.AddListener(clickTouchSound.PlayClickSound);
                    Debug.Log($"[AddSoundToAll] {obj.name}의 ClickEvent에도 사운드를 연결했습니다.");
                }
                
                // 변경사항을 확실히 저장
                EditorUtility.SetDirty(obj);
                EditorUtility.SetDirty(vfxObject);
                EditorUtility.SetDirty(clickTouchSound);
                
                Debug.Log($"[AddSoundToAll] {obj.name} 처리 완료!");
            }
        }

        // ClickEventConverterEditor처럼 씬을 더티로 마킹
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        
        string resultMessage = $"StretchVFX 사운드 설정 완료!\n총 발견: {totalFound}개\n추가: {addedCount}개\n업데이트: {updatedCount}개\nVFX 이벤트 연결: {vfxConnectedCount}개";
        Debug.Log(resultMessage);
        
        // 결과를 다이얼로그로 표시 (ClickEventConverterEditor 방식)
        EditorUtility.DisplayDialog("StretchVFX Sound Setup", resultMessage, "OK");
    }

    // 게임오브젝트의 전체 경로를 가져오는 헬퍼 함수
    private static string GetGameObjectPath(GameObject obj)
    {
        if (obj == null) return "";
        
        string path = obj.name;
        Transform parent = obj.transform.parent;
        
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        
        return path;
    }

    public static void RemoveSoundFromAllStretchVFXInScene()
    {
        // ClickEventConverterEditor 방식으로 모든 루트 오브젝트부터 재귀적으로 검사
        var rootGameObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        
        Debug.Log($"[RemoveSoundFromAll] 활성 씬의 루트 오브젝트 {rootGameObjects.Length}개를 검사합니다.");
        
        int removedCount = 0;
        int totalFound = 0;

        // 각 루트 오브젝트와 그 모든 자식들을 검사
        foreach (var rootObj in rootGameObjects)
        {
            if (rootObj == null) continue;

            // 현재 루트와 모든 자식에서 StretchVFX 컴포넌트 찾기 (비활성화된 것도 포함)
            var stretchVFXComponents = rootObj.GetComponentsInChildren<StretchVFX>(true);
            
            Debug.Log($"[RemoveSoundFromAll] 루트 오브젝트 '{rootObj.name}'에서 {stretchVFXComponents.Length}개의 StretchVFX를 발견했습니다.");
            totalFound += stretchVFXComponents.Length;

            foreach (var stretchVFX in stretchVFXComponents)
            {
                if (stretchVFX == null) continue;
                
                var obj = stretchVFX.gameObject;
                var clickTouchSound = obj.GetComponent<ClickTouchSound>();
                
                if (clickTouchSound != null)
                {
                    Debug.Log($"[RemoveSoundFromAll] 처리 시작: {obj.name} | 경로: {GetGameObjectPath(obj)}");
                    
                    // Undo 지원 (ClickEventConverterEditor 방식)
                    UnityEditor.Undo.RegisterCompleteObjectUndo(obj, "Remove Sound from StretchVFX");
                    
                    // VFXObject에서 사운드 리스너 제거
                    var vfxObject = obj.GetComponent<VFXObject>();
                    if (vfxObject != null && vfxObject.OnEffectStart != null)
                    {
                        vfxObject.OnEffectStart.RemoveListener(clickTouchSound.PlayClickSound);
                        Debug.Log($"[RemoveSoundFromAll] {obj.name}의 VFXObject에서 사운드 리스너를 제거했습니다.");
                    }
                    
                    // ClickEvent에서 사운드 리스너 제거 (기존 기능 유지)
                    var clickEvent = obj.GetComponent<ClickEvent>();
                    if (clickEvent != null && clickEvent.OnClickEvent != null)
                    {
                        clickEvent.OnClickEvent.RemoveListener(clickTouchSound.PlayClickSound);
                        Debug.Log($"[RemoveSoundFromAll] {obj.name}의 ClickEvent에서 사운드 리스너를 제거했습니다.");
                    }
                    
                    // 컴포넌트 제거
                    UnityEditor.Undo.DestroyObjectImmediate(clickTouchSound);
                    removedCount++;
                    EditorUtility.SetDirty(obj);
                    
                    Debug.Log($"[RemoveSoundFromAll] {obj.name}에서 ClickTouchSound 컴포넌트를 제거했습니다.");
                }
            }
        }

        // ClickEventConverterEditor처럼 씬을 더티로 마킹
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        
        string resultMessage = $"StretchVFX 사운드 제거 완료!\n총 발견: {totalFound}개\n제거: {removedCount}개";
        Debug.Log(resultMessage);
        
        // 결과를 다이얼로그로 표시 (ClickEventConverterEditor 방식)
        EditorUtility.DisplayDialog("StretchVFX Sound Remove", resultMessage, "OK");
    }

    public static void AddSoundToSelectedStretchVFX(Data.SFXEnum soundType = Data.SFXEnum.ClickStretch)
    {
        var selectedObjects = Selection.gameObjects;
        int addedCount = 0;
        int updatedCount = 0;
        int vfxConnectedCount = 0;

        foreach (var obj in selectedObjects)
        {
            var stretchVFX = obj.GetComponent<StretchVFX>();
            if (stretchVFX == null) continue;

            // Prefab 연결 해제 (변경사항이 확실히 저장되도록)
            if (PrefabUtility.IsPartOfPrefabInstance(obj))
            {
                // Prefab 루트를 찾아서 언팩
                GameObject prefabRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(obj);
                if (prefabRoot != null)
                {
                    PrefabUtility.UnpackPrefabInstance(prefabRoot, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
                    Debug.Log($"[AddSoundToSelected] {obj.name}의 Prefab 루트 '{prefabRoot.name}'의 연결을 해제했습니다.");
                }
                else
                {
                    Debug.LogWarning($"[AddSoundToSelected] {obj.name}의 Prefab 루트를 찾을 수 없어 언팩을 건너뜁니다.");
                }
            }

            var clickTouchSound = obj.GetComponent<ClickTouchSound>();
            
            if (clickTouchSound == null)
            {
                // 사운드 컴포넌트 추가
                clickTouchSound = obj.AddComponent<ClickTouchSound>();
                clickTouchSound.SetSoundType(soundType);
                Debug.Log($"[AddSoundToSelected] {obj.name}에 ClickTouchSound 컴포넌트를 추가했습니다.");
                
                addedCount++;
            }
            else
            {
                // 기존 사운드 타입 업데이트
                clickTouchSound.SetSoundType(soundType);
                Debug.Log($"[AddSoundToSelected] {obj.name}의 기존 사운드 타입을 업데이트했습니다.");
                
                updatedCount++;
            }
            
            // VFXObject 확인 - StretchVFX는 VFXObject를 상속받으므로 자기 자신이 VFXObject입니다
            VFXObject vfxObject = stretchVFX; // 직접 캐스팅
            
            Debug.Log($"[AddSoundToSelected] {obj.name}에서 VFXObject를 확인했습니다: {vfxObject != null}");
            
            if (vfxObject != null)
            {
                Debug.Log($"[AddSoundToSelected] {obj.name}의 OnEffectStart 이벤트 상태: {vfxObject.OnEffectStart != null}");
                
                // OnEffectStart 이벤트를 강제로 새로 초기화 (SerializeField 추가로 인한 재초기화)
                vfxObject.OnEffectStart = new UnityEngine.Events.UnityEvent();
                vfxObject.OnEffectEnd = new UnityEngine.Events.UnityEvent();
                Debug.Log($"[AddSoundToSelected] {obj.name}의 OnEffectStart/OnEffectEnd 이벤트를 강제로 재초기화했습니다.");
                
                // 새 Persistent 리스너 추가
                UnityEventTools.AddPersistentListener(vfxObject.OnEffectStart, clickTouchSound.PlayClickSound);
                
                int afterCount = vfxObject.OnEffectStart.GetPersistentEventCount();
                Debug.Log($"[AddSoundToSelected] {obj.name}의 VFXObject OnEffectStart에 Persistent 사운드 이벤트를 연결했습니다. (리스너 수: {afterCount})");
                
                vfxConnectedCount++;
            }
            else
            {
                Debug.LogError($"[AddSoundToSelected] {obj.name}에서 VFXObject를 찾을 수 없습니다!");
            }
                
            // ClickEvent가 있다면 사운드 이벤트 연결 (기존 기능 유지)
            var clickEvent = obj.GetComponent<ClickEvent>();
            if (clickEvent != null && clickEvent.OnClickEvent != null)
            {
                // 기존 사운드 리스너 제거 (중복 방지)
                clickEvent.OnClickEvent.RemoveListener(clickTouchSound.PlayClickSound);
                // 새 리스너 추가
                clickEvent.OnClickEvent.AddListener(clickTouchSound.PlayClickSound);
            }
            
            // 변경사항을 확실히 저장
            EditorUtility.SetDirty(obj);
            EditorUtility.SetDirty(vfxObject);
            EditorUtility.SetDirty(clickTouchSound);
        }

        if (addedCount > 0 || updatedCount > 0)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log($"선택된 StretchVFX 사운드 설정 완료! 추가: {addedCount}개, 업데이트: {updatedCount}개, VFX 이벤트 연결: {vfxConnectedCount}개");
        }
        else
        {
            Debug.LogWarning("선택된 오브젝트 중 StretchVFX 컴포넌트가 있는 오브젝트가 없습니다.");
        }
    }

    public static void RemoveSoundFromSelectedStretchVFX()
    {
        var selectedObjects = Selection.gameObjects;
        int removedCount = 0;

        foreach (var obj in selectedObjects)
        {
            var stretchVFX = obj.GetComponent<StretchVFX>();
            if (stretchVFX == null) continue;

            var clickTouchSound = obj.GetComponent<ClickTouchSound>();
            
            if (clickTouchSound != null)
            {
                // VFXObject에서 사운드 리스너 제거
                var vfxObject = obj.GetComponent<VFXObject>();
                if (vfxObject != null && vfxObject.OnEffectStart != null)
                {
                    vfxObject.OnEffectStart.RemoveListener(clickTouchSound.PlayClickSound);
                }
                
                // ClickEvent에서 사운드 리스너 제거 (기존 기능 유지)
                var clickEvent = obj.GetComponent<ClickEvent>();
                if (clickEvent != null && clickEvent.OnClickEvent != null)
                {
                    clickEvent.OnClickEvent.RemoveListener(clickTouchSound.PlayClickSound);
                }
                
                // 컴포넌트 제거
                DestroyImmediate(clickTouchSound);
                removedCount++;
                EditorUtility.SetDirty(obj);
            }
        }

        if (removedCount > 0)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log($"선택된 StretchVFX 사운드 제거 완료! 제거: {removedCount}개");
        }
        else
        {
            Debug.LogWarning("선택된 오브젝트 중 ClickTouchSound 컴포넌트가 있는 오브젝트가 없습니다.");
        }
    }
}
