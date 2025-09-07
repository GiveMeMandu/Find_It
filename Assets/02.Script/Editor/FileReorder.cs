using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class FileReorder : EditorWindow
{
    private string targetFolderName = "Weapons_and_shields_Clear";
    private string sourceFolder = "Weapons_And_Shields";
    private bool showDebugLogs = false;
    
    private Dictionary<string, string> folderMappings = new Dictionary<string, string>()
    {
        // 3D 에셋
        { "Mesh", "" },
        { "GameObject", "" },
        { "Prefab", "" },
        
        // 머티리얼 & 셰이더
        { "Material", "" },
        { "Shader", "" },
        { "PhysicMaterial", "" },
        
        // 텍스처 & 이미지
        { "Texture2D", "" },
        { "Sprite", "" },
        { "Cubemap", "" },
        { "RenderTexture", "" },
        
        // UI
        { "Font", "" },
        { "GUISkin", "" },
        
        // 애니메이션
        { "AnimationClip", "" },
        { "AnimatorController", "" },
        { "Avatar", "" },
        
        // 씬 & 레벨
        { "Scene", "" },
        { "SceneAsset", "" },
        
        // 오디오
        { "AudioClip", "" },
        { "AudioMixer", "" },
        { "AudioMixerGroup", "" },
        
        // 스크립트 & 데이터
        { "MonoScript", "" },
        { "ScriptableObject", "" },
        { "TextAsset", "" },
        
        // 기타
        { "Lightmap", "" },
        { "ComputeShader", "" },
        { "VideoClip", "" }
    };
    
    private Vector2 scrollPosition = Vector2.zero;
    private bool showHelp = false;
    private bool enableDirectInput = false;
    
    [MenuItem("Tools/파일 정리툴")]
    public static void ShowWindow()
    {
        GetWindow<FileReorder>("파일 정리툴");
    }
    
    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("파일 정리도구", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("도움말", GUILayout.Width(80), GUILayout.Height(20)))
        {
            showHelp = !showHelp;
        }
        EditorGUILayout.EndHorizontal();
        
        // Help InfoBox
        if (showHelp)
        {
            EditorGUILayout.HelpBox(
                "📋 사용법:\n" +
                "1. 메인 폴더 이름: 정리된 파일들이 들어갈 새 폴더명\n" +
                "2. 대상 폴더: Project 창에서 폴더를 드래그해서 넣기\n" +
                "3. 각 에셋 타입별로 들어갈 하위 폴더명 입력\n" +
                "4. '정리 실행' 버튼 클릭\n\n" +
                "🎯 지원하는 데이터 타입:\n" +
                "• 3D: Mesh, GameObject, Prefab\n" +
                "• 텍스처: Texture2D, Sprite, Cubemap\n" +
                "• 오디오: AudioClip, AudioMixer\n" +
                "• 애니메이션: AnimationClip, AnimatorController\n" +
                "• 씬: Scene, SceneAsset\n" +
                "• 스크립트: MonoScript, ScriptableObject\n" +
                "• 기타: Material, Shader, Font, VideoClip 등\n\n" +
                "팁: 빈 칸은 해당 타입 파일을 건너뜁니다", 
                MessageType.Info);
            EditorGUILayout.Space();
        }
        
        // Target/scene section
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("메인 폴더 이름:", GUILayout.Width(150));
        targetFolderName = EditorGUILayout.TextField(targetFolderName);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // Source folder section
        GUILayout.Label("정리할 폴더를 드래그하거나 직접 입력하세요");
        
        // Drag and Drop area
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("대상 폴더:", GUILayout.Width(150));
        
        // Drag and drop area
        Rect dropArea = GUILayoutUtility.GetRect(0, 50, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, string.IsNullOrEmpty(sourceFolder) ? 
            "여기에 폴더를 드래그하세요" : sourceFolder, EditorStyles.helpBox);
        
        // Handle drag and drop
        Event currentEvent = Event.current;
        if (dropArea.Contains(currentEvent.mousePosition))
        {
            if (currentEvent.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                currentEvent.Use();
            }
            else if (currentEvent.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                
                foreach (string draggedPath in DragAndDrop.paths)
                {
                    if (showDebugLogs)
                        Debug.Log($"드래그된 경로: {draggedPath}");
                        
                    if (AssetDatabase.IsValidFolder(draggedPath))
                    {
                        sourceFolder = draggedPath;
                        if (showDebugLogs)
                            Debug.Log($"대상 폴더 설정됨: {sourceFolder}");
                        break;
                    }
                }
                currentEvent.Use();
            }
        }
        
        // Control buttons section
        EditorGUILayout.Space(5);
        EditorGUILayout.BeginHorizontal();
        enableDirectInput = EditorGUILayout.Toggle("직접 입력", enableDirectInput);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("찾아보기", GUILayout.Width(100)))
        {
            BrowseForFolder();
        }
        EditorGUILayout.EndHorizontal();
        
        // Direct input field
        if (enableDirectInput)
        {
            EditorGUILayout.Space(3);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("경로:", GUILayout.Width(40));
            sourceFolder = EditorGUILayout.TextField(sourceFolder);
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // Folder mappings section with header and grouping
        EditorGUILayout.BeginVertical("box");
        
        // Group header
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("📁 하위 폴더 설정", EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        // Column headers
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("데이터 타입", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(150));
        GUILayout.Label("|", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(20));
        GUILayout.Label("폴더 이름", EditorStyles.centeredGreyMiniLabel);
        EditorGUILayout.EndHorizontal();
        
        // Separator line
        EditorGUILayout.Space(3);
        Rect lineRect = GUILayoutUtility.GetRect(GUIContent.none, GUI.skin.horizontalSlider, GUILayout.Height(1));
        EditorGUI.DrawRect(lineRect, Color.grey);
        EditorGUILayout.Space(5);
        
        // Folder mappings with scroll
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
        
        var keys = folderMappings.Keys.ToList();
        string currentCategory = "";
        
        foreach (var key in keys)
        {
            // 카테고리 구분
            string category = GetCategoryForKey(key);
            if (category != currentCategory)
            {
                if (currentCategory != "") // 첫 번째 카테고리가 아니면 구분선 추가
                {
                    EditorGUILayout.Space(3);
                    Rect categoryLineRect = GUILayoutUtility.GetRect(GUIContent.none, GUI.skin.horizontalSlider, GUILayout.Height(1));
                    EditorGUI.DrawRect(categoryLineRect, new Color(0.5f, 0.5f, 0.5f, 0.3f));
                    EditorGUILayout.Space(3);
                }
                
                // 카테고리 헤더
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label($"● {category}", EditorStyles.miniLabel, GUILayout.Width(150));
                GUILayout.Label("", GUILayout.Width(20));
                GUILayout.Label("", EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(2);
                
                currentCategory = category;
            }
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"  {key}", GUILayout.Width(150));
            GUILayout.Label("|", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(20));
            folderMappings[key] = EditorGUILayout.TextField(folderMappings[key]);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(1);
        }
        
        EditorGUILayout.EndScrollView();
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // Order button
        if (GUILayout.Button("정리 실행", GUILayout.Height(30)))
        {
            ExecuteReorder();
        }
        
        EditorGUILayout.Space();
        
        // Debug logs
        showDebugLogs = EditorGUILayout.Toggle("디버그 로그", showDebugLogs);
    }
    
    private string GetCategoryForKey(string key)
    {
        // 3D 에셋
        if (key == "Mesh" || key == "GameObject" || key == "Prefab")
            return "3D 에셋";
        
        // 머티리얼 & 셰이더
        if (key == "Material" || key == "Shader" || key == "PhysicMaterial")
            return "머티리얼 & 셰이더";
        
        // 텍스처 & 이미지
        if (key == "Texture2D" || key == "Sprite" || key == "Cubemap" || key == "RenderTexture")
            return "텍스처 & 이미지";
        
        // UI
        if (key == "Font" || key == "GUISkin")
            return "UI";
        
        // 애니메이션
        if (key == "AnimationClip" || key == "AnimatorController" || key == "Avatar")
            return "애니메이션";
        
        // 씬 & 레벨
        if (key == "Scene" || key == "SceneAsset")
            return "씬 & 레벨";
        
        // 오디오
        if (key == "AudioClip" || key == "AudioMixer" || key == "AudioMixerGroup")
            return "오디오";
        
        // 스크립트 & 데이터
        if (key == "MonoScript" || key == "ScriptableObject" || key == "TextAsset")
            return "스크립트 & 데이터";
        
        // 기타
        return "기타";
    }
    
    private void BrowseForFolder()
    {
        string path = EditorUtility.OpenFolderPanel("대상 폴더 선택", "Assets", "");
        if (!string.IsNullOrEmpty(path))
        {
            // Convert absolute path to relative path
            if (path.StartsWith(Application.dataPath))
            {
                sourceFolder = "Assets" + path.Substring(Application.dataPath.Length);
            }
            else
            {
                sourceFolder = path;
            }
        }
    }
    
    private void ExecuteReorder()
    {
        if (string.IsNullOrEmpty(targetFolderName) || string.IsNullOrEmpty(sourceFolder))
        {
            EditorUtility.DisplayDialog("오류", "모든 필수 필드를 입력해주세요.", "확인");
            return;
        }
        
        // sourceFolder이 이미 Assets/로 시작하는지 확인
        string sourcePath = sourceFolder;
        if (!sourcePath.StartsWith("Assets/"))
        {
            sourcePath = Path.Combine("Assets", sourceFolder);
        }
        
        string targetPath = Path.Combine("Assets", targetFolderName);
        
        if (showDebugLogs)
            Debug.Log($"대상 경로 확인: {sourcePath}");
        
        if (!AssetDatabase.IsValidFolder(sourcePath))
        {
            if (showDebugLogs)
                Debug.LogError($"대상 폴더가 존재하지 않습니다: {sourcePath}");
            EditorUtility.DisplayDialog("오류", $"대상 폴더가 존재하지 않습니다: {sourcePath}", "확인");
            return;
        }
        
        // Create target folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder(targetPath))
        {
            string parentFolder = Path.GetDirectoryName(targetPath);
            string folderName = Path.GetFileName(targetPath);
            AssetDatabase.CreateFolder(parentFolder, folderName);
            if (showDebugLogs)
                Debug.Log($"대상 폴더를 생성했습니다: {targetPath}");
        }
        
        // Create subfolders and organize assets
        int movedCount = 0;
        foreach (var mapping in folderMappings)
        {
            if (string.IsNullOrEmpty(mapping.Value)) continue;
            
            string subFolderPath = Path.Combine(targetPath, mapping.Value);
            
            // Create subfolder if it doesn't exist
            if (!AssetDatabase.IsValidFolder(subFolderPath))
            {
                AssetDatabase.CreateFolder(targetPath, mapping.Value);
                if (showDebugLogs)
                    Debug.Log($"하위 폴더를 생성했습니다: {subFolderPath}");
            }
            
            // Find and move assets of this type
            string[] assetGUIDs = AssetDatabase.FindAssets($"t:{mapping.Key}", new[] { sourcePath });
            
            foreach (string guid in assetGUIDs)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = Path.GetFileName(assetPath);
                string newPath = Path.Combine(subFolderPath, fileName);
                
                string result = AssetDatabase.MoveAsset(assetPath, newPath);
                if (string.IsNullOrEmpty(result))
                {
                    movedCount++;
                    if (showDebugLogs)
                        Debug.Log($"{fileName}를 {mapping.Value} 폴더로 이동했습니다");
                }
                else
                {
                    if (showDebugLogs)
                        Debug.LogError($"{fileName} 이동 실패: {result}");
                }
            }
        }
        
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("완료", $"파일 정리가 완료되었습니다! {movedCount}개의 에셋을 이동했습니다.", "확인");
        
        if (showDebugLogs)
            Debug.Log($"파일 정리 완료! 총 이동된 에셋: {movedCount}개");
    }
}
