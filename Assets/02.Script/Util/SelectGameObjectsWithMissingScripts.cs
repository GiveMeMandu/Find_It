#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Experimental.SceneManagement;
using System.Collections.Generic;

public class SelectGameObjectsWithMissingScripts : Editor
{
    [MenuItem("Tools/Missing Script Components 싹다 지우기 &d", false, 101)]
    private static void RemoveAllMissingScriptComponents()
    {
        // 현재 프리팹 스테이지 확인
        PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        List<GameObject> rootObjects = new List<GameObject>();

        if (prefabStage != null)
        {
            // 프리팹 모드일 경우, 해당 프리팹 루트 오브젝트만 처리
            rootObjects.Add(prefabStage.prefabContentsRoot);
        }
        else
        {
            // 일반 씬에서 선택된 게임 오브젝트들 처리
            rootObjects.AddRange(Selection.gameObjects);
        }

        Object[] deepSelectedObjects = EditorUtility.CollectDeepHierarchy(rootObjects.ToArray());

        Debug.Log($"검사 대상 오브젝트 수: {deepSelectedObjects.Length}");

        int componentCount = 0;
        int gameObjectCount = 0;

        foreach (Object obj in deepSelectedObjects)
        {
            if (obj is GameObject go)
            {
                int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);

                if (count > 0)
                {
                    Undo.RegisterCompleteObjectUndo(go, "Remove Missing Scripts");
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                    componentCount += count;
                    gameObjectCount++;
                }
            }
        }

        Debug.Log($"<color=green>Missing Script 제거 완료</color>\nGameObjects: {gameObjectCount}, Removed Components: {componentCount}");
    }
}
#endif
