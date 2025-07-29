using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

/// <summary>
/// 애니메이션 클립의 위치 키프레임을 일괄 처리하는 에디터 툴
/// </summary>
public class AnimationClipPositionBatchEditor : EditorWindow
{
    private List<AnimationClip> targetClips = new List<AnimationClip>();
    private Vector2 scrollPosition;
    private bool processLocalPosition = true;
    private bool onlyProcessXY = true;
    private Vector3 targetPosition = Vector3.zero;
    private bool createBackup = true;
    private string backupFolder = "Assets/BackupAnimationClips";
    
    private enum ProcessMode
    {
        SetToZero,          // (0,0,0)으로 설정
        SetToCustom,        // 커스텀 위치로 설정
        RemovePositionKeys, // 위치 키프레임 완전 제거
        OffsetCorrection    // 첫 번째 키프레임을 기준으로 오프셋 보정
    }
    
    private ProcessMode processMode = ProcessMode.OffsetCorrection;
    
    [MenuItem("Tools/2D Animation/위치 키프레임 일괄 처리")]
    public static void ShowWindow()
    {
        var window = GetWindow<AnimationClipPositionBatchEditor>("애니메이션 위치 일괄 처리");
        window.minSize = new Vector2(500, 600);
    }
    
    private void OnGUI()
    {
        EditorGUILayout.Space();
        
        // 타이틀
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 16;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.LabelField("🎯 2D 애니메이션 위치 키프레임 일괄 처리", titleStyle);
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "아트팀에서 만든 애니메이션 클립의 위치 키프레임을 일괄로 보정합니다.\n" +
            "⚠️ 작업 전 반드시 백업을 생성하세요!", 
            MessageType.Info);
        
        EditorGUILayout.Space();
        
        // 처리 모드 선택
        EditorGUILayout.LabelField("■ 처리 모드", EditorStyles.boldLabel);
        processMode = (ProcessMode)EditorGUILayout.EnumPopup("처리 방식", processMode);
        
        switch (processMode)
        {
            case ProcessMode.SetToZero:
                EditorGUILayout.HelpBox("모든 위치 키프레임을 (0,0,0)으로 설정합니다.", MessageType.Info);
                break;
            case ProcessMode.SetToCustom:
                EditorGUILayout.HelpBox("모든 위치 키프레임을 지정한 위치로 설정합니다.", MessageType.Info);
                targetPosition = EditorGUILayout.Vector3Field("목표 위치", targetPosition);
                break;
            case ProcessMode.RemovePositionKeys:
                EditorGUILayout.HelpBox("위치 키프레임을 완전히 제거합니다. (Transform은 유지)", MessageType.Warning);
                break;
            case ProcessMode.OffsetCorrection:
                EditorGUILayout.HelpBox("첫 번째 키프레임의 위치를 기준으로 상대적 오프셋을 유지하며 (0,0,0)에서 시작하도록 보정합니다.", MessageType.Info);
                break;
        }
        
        EditorGUILayout.Space();
        
        // 처리 옵션
        EditorGUILayout.LabelField("■ 처리 옵션", EditorStyles.boldLabel);
        processLocalPosition = EditorGUILayout.Toggle("Local Position 처리", processLocalPosition);
        onlyProcessXY = EditorGUILayout.Toggle("X, Y축만 처리 (2D용)", onlyProcessXY);
        
        EditorGUILayout.Space();
        
        // 백업 설정
        EditorGUILayout.LabelField("■ 백업 설정", EditorStyles.boldLabel);
        createBackup = EditorGUILayout.Toggle("백업 생성", createBackup);
        if (createBackup)
        {
            backupFolder = EditorGUILayout.TextField("백업 폴더", backupFolder);
        }
        
        EditorGUILayout.Space();
        
        // 애니메이션 클립 리스트
        EditorGUILayout.LabelField("■ 처리할 애니메이션 클립", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("프로젝트에서 모든 .anim 파일 찾기"))
        {
            FindAllAnimationClips();
        }
        if (GUILayout.Button("선택된 클립 추가"))
        {
            AddSelectedClips();
        }
        if (GUILayout.Button("리스트 비우기"))
        {
            targetClips.Clear();
        }
        EditorGUILayout.EndHorizontal();
        
        // 클립 리스트 표시
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
        for (int i = 0; i < targetClips.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            targetClips[i] = (AnimationClip)EditorGUILayout.ObjectField($"클립 {i + 1}", targetClips[i], typeof(AnimationClip), false);
            if (GUILayout.Button("제거", GUILayout.Width(50)))
            {
                targetClips.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
        
        EditorGUILayout.Space();
        
        // 처리 실행 버튼
        EditorGUILayout.LabelField("■ 실행", EditorStyles.boldLabel);
        
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("⚠️ 위치 키프레임 일괄 처리 실행", GUILayout.Height(40)))
        {
            if (EditorUtility.DisplayDialog(
                "위치 키프레임 일괄 처리", 
                $"총 {targetClips.Count}개의 애니메이션 클립을 처리합니다.\n" +
                $"처리 모드: {processMode}\n" +
                $"백업 생성: {(createBackup ? "예" : "아니오")}\n\n" +
                "계속하시겠습니까?", 
                "실행", "취소"))
            {
                ProcessAnimationClips();
            }
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("미리보기 (로그만 출력)", GUILayout.Height(30)))
        {
            PreviewProcessing();
        }
    }
    
    private void FindAllAnimationClips()
    {
        string[] guids = AssetDatabase.FindAssets("t:AnimationClip");
        targetClips.Clear();
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip != null && !clip.name.StartsWith("__preview__"))
            {
                targetClips.Add(clip);
            }
        }
        
        Debug.Log($"총 {targetClips.Count}개의 애니메이션 클립을 찾았습니다.");
    }
    
    private void AddSelectedClips()
    {
        foreach (Object obj in Selection.objects)
        {
            if (obj is AnimationClip clip && !targetClips.Contains(clip))
            {
                targetClips.Add(clip);
            }
        }
    }
    
    private void PreviewProcessing()
    {
        Debug.Log("=== 애니메이션 클립 처리 미리보기 ===");
        
        foreach (var clip in targetClips)
        {
            if (clip == null) continue;
            
            var bindings = AnimationUtility.GetCurveBindings(clip);
            var positionBindings = GetPositionBindings(bindings);
            
            if (positionBindings.Count > 0)
            {
                Debug.Log($"[{clip.name}] 위치 키프레임 발견: {positionBindings.Count}개 바인딩");
                
                foreach (var binding in positionBindings)
                {
                    var curve = AnimationUtility.GetEditorCurve(clip, binding);
                    if (curve != null && curve.keys.Length > 0)
                    {
                        Debug.Log($"  - {binding.propertyName}: {curve.keys.Length}개 키프레임, 첫 번째 값: {curve.keys[0].value}");
                    }
                }
            }
            else
            {
                Debug.Log($"[{clip.name}] 위치 키프레임 없음");
            }
        }
    }
    
    private void ProcessAnimationClips()
    {
        if (createBackup)
        {
            CreateBackupFolder();
        }
        
        int processedCount = 0;
        int errorCount = 0;
        
        try
        {
            AssetDatabase.StartAssetEditing();
            
            foreach (var clip in targetClips)
            {
                if (clip == null) continue;
                
                try
                {
                    if (createBackup)
                    {
                        CreateBackup(clip);
                    }
                    
                    ProcessSingleClip(clip);
                    processedCount++;
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"클립 처리 중 오류 발생: {clip.name} - {e.Message}");
                    errorCount++;
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        string resultMessage = $"처리 완료!\n성공: {processedCount}개\n오류: {errorCount}개";
        
        if (errorCount > 0)
        {
            EditorUtility.DisplayDialog("처리 완료 (오류 있음)", resultMessage, "확인");
        }
        else
        {
            EditorUtility.DisplayDialog("처리 완료", resultMessage, "확인");
        }
        
        Debug.Log($"=== 애니메이션 클립 일괄 처리 완료 ===\n{resultMessage}");
    }
    
    private void ProcessSingleClip(AnimationClip clip)
    {
        var bindings = AnimationUtility.GetCurveBindings(clip);
        var positionBindings = GetPositionBindings(bindings);
        
        if (positionBindings.Count == 0)
        {
            Debug.Log($"[{clip.name}] 위치 키프레임이 없어 건너뜁니다.");
            return;
        }
        
        Vector3 firstFrameOffset = Vector3.zero;
        
        // OffsetCorrection 모드일 때 첫 번째 프레임의 오프셋 계산
        if (processMode == ProcessMode.OffsetCorrection)
        {
            firstFrameOffset = GetFirstFrameOffset(clip, positionBindings);
        }
        
        foreach (var binding in positionBindings)
        {
            var curve = AnimationUtility.GetEditorCurve(clip, binding);
            if (curve == null || curve.keys.Length == 0) continue;
            
            ProcessCurve(curve, binding.propertyName, firstFrameOffset);
            AnimationUtility.SetEditorCurve(clip, binding, curve);
        }
        
        EditorUtility.SetDirty(clip);
        Debug.Log($"[{clip.name}] 위치 키프레임 처리 완료");
    }
    
    private List<EditorCurveBinding> GetPositionBindings(EditorCurveBinding[] bindings)
    {
        var positionBindings = new List<EditorCurveBinding>();
        
        foreach (var binding in bindings)
        {
            string propName = binding.propertyName;
            bool isPosition = processLocalPosition ? 
                propName.StartsWith("m_LocalPosition") : 
                propName.StartsWith("m_Position");
            
            if (isPosition)
            {
                if (onlyProcessXY)
                {
                    if (propName.EndsWith(".x") || propName.EndsWith(".y"))
                    {
                        positionBindings.Add(binding);
                    }
                }
                else
                {
                    positionBindings.Add(binding);
                }
            }
        }
        
        return positionBindings;
    }
    
    private Vector3 GetFirstFrameOffset(AnimationClip clip, List<EditorCurveBinding> positionBindings)
    {
        Vector3 offset = Vector3.zero;
        
        foreach (var binding in positionBindings)
        {
            var curve = AnimationUtility.GetEditorCurve(clip, binding);
            if (curve != null && curve.keys.Length > 0)
            {
                float firstValue = curve.keys[0].value;
                
                if (binding.propertyName.EndsWith(".x"))
                    offset.x = firstValue;
                else if (binding.propertyName.EndsWith(".y"))
                    offset.y = firstValue;
                else if (binding.propertyName.EndsWith(".z"))
                    offset.z = firstValue;
            }
        }
        
        return offset;
    }
    
    private void ProcessCurve(AnimationCurve curve, string propertyName, Vector3 offset)
    {
        for (int i = 0; i < curve.keys.Length; i++)
        {
            Keyframe key = curve.keys[i];
            
            switch (processMode)
            {
                case ProcessMode.SetToZero:
                    key.value = 0f;
                    break;
                    
                case ProcessMode.SetToCustom:
                    if (propertyName.EndsWith(".x"))
                        key.value = targetPosition.x;
                    else if (propertyName.EndsWith(".y"))
                        key.value = targetPosition.y;
                    else if (propertyName.EndsWith(".z"))
                        key.value = targetPosition.z;
                    break;
                    
                case ProcessMode.OffsetCorrection:
                    if (propertyName.EndsWith(".x"))
                        key.value -= offset.x;
                    else if (propertyName.EndsWith(".y"))
                        key.value -= offset.y;
                    else if (propertyName.EndsWith(".z"))
                        key.value -= offset.z;
                    break;
            }
            
            curve.keys[i] = key;
        }
    }
    
    private void CreateBackupFolder()
    {
        if (!AssetDatabase.IsValidFolder(backupFolder))
        {
            string parentFolder = Path.GetDirectoryName(backupFolder);
            string folderName = Path.GetFileName(backupFolder);
            AssetDatabase.CreateFolder(parentFolder, folderName);
        }
    }
    
    private void CreateBackup(AnimationClip clip)
    {
        string originalPath = AssetDatabase.GetAssetPath(clip);
        string fileName = Path.GetFileNameWithoutExtension(originalPath);
        string extension = Path.GetExtension(originalPath);
        string backupPath = $"{backupFolder}/{fileName}_backup{extension}";
        
        AssetDatabase.CopyAsset(originalPath, backupPath);
    }
} 