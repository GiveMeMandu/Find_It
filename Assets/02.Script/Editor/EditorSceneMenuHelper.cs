using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class EditorSceneMenuHelper
{
    [MenuItem("Assets/모든 씬 빌드에 추가하기", true)]
    private static bool ValidateAddAllScenesToBuild()
    {
        // 선택된 항목이 폴더인 경우에만 메뉴 활성화
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        return AssetDatabase.IsValidFolder(path);
    }

    [MenuItem("Assets/모든 씬 빌드에 추가하기", false, 20)]
    private static void AddAllScenesToBuild()
    {
        string folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        
        // 선택된 폴더 하위의 모든 .unity 파일 찾기
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { folderPath });
        List<EditorBuildSettingsScene> newScenes = new List<EditorBuildSettingsScene>();
        
        // 기존 빌드 세팅에 있는 씬들을 먼저 추가
        newScenes.AddRange(EditorBuildSettings.scenes);
        
        foreach (string guid in sceneGuids)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(guid);
            
            // 이미 빌드 세팅에 있는 씬은 건너뛰기
            if (newScenes.Any(scene => scene.path == scenePath))
                continue;
                
            newScenes.Add(new EditorBuildSettingsScene(scenePath, true));
        }
        
        // 빌드 세팅 업데이트
        EditorBuildSettings.scenes = newScenes.ToArray();
        
        Debug.Log($"'{folderPath}' 폴더의 모든 씬이 빌드 세팅에 추가되었습니다.");
    }
}
