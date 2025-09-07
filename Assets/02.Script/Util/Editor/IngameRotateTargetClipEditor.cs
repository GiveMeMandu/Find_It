using UnityEngine;
using UnityEditor;
using UnityEngine.Timeline;

[CustomEditor(typeof(회전클립))]
public class IngameRotateTargetClipEditor : Editor
{
    private SerializedProperty rotateTargetProperty;
    private SerializedProperty rotationModeProperty;
    
    // 각 모드별 프로퍼티들
    private SerializedProperty instantRotationAngleProperty;
    private SerializedProperty rotationDurationProperty;
    private SerializedProperty targetRotationAngleProperty;
    private SerializedProperty flipCountProperty;
    private SerializedProperty flipDurationProperty;
    private SerializedProperty instantFlipCountProperty;
    private SerializedProperty useLocalRotationProperty;
    private SerializedProperty restoreOriginalRotationProperty;
    private SerializedProperty rotationCurveProperty;

    private void OnEnable()
    {
        // target이 null이거나 파괴된 경우 serializedObject 생성을 건너뜀
        if (target == null)
            return;
            
        rotateTargetProperty = serializedObject.FindProperty("rotateTarget");
        rotationModeProperty = serializedObject.FindProperty("rotationMode");
        
        instantRotationAngleProperty = serializedObject.FindProperty("instantRotationAngle");
        rotationDurationProperty = serializedObject.FindProperty("rotationDuration");
        targetRotationAngleProperty = serializedObject.FindProperty("targetRotationAngle");
        flipCountProperty = serializedObject.FindProperty("flipCount");
        flipDurationProperty = serializedObject.FindProperty("flipDuration");
        instantFlipCountProperty = serializedObject.FindProperty("instantFlipCount");
        useLocalRotationProperty = serializedObject.FindProperty("useLocalRotation");
        restoreOriginalRotationProperty = serializedObject.FindProperty("restoreOriginalRotation");
        rotationCurveProperty = serializedObject.FindProperty("rotationCurve");
    }

    public override void OnInspectorGUI()
    {
        // target이 null이거나 serializedObject가 유효하지 않은 경우 처리 중단
        if (target == null || serializedObject == null)
        {
            EditorGUILayout.HelpBox("Target object is null or invalid.", MessageType.Error);
            return;
        }
        
        serializedObject.Update();

        // 회전 타겟
        EditorGUILayout.LabelField("회전 타겟", EditorStyles.boldLabel);
        DrawRotateTargetSection();
        
        EditorGUILayout.Space();
        
        // 회전 모드
        EditorGUILayout.LabelField("2D 회전 모드", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(rotationModeProperty);
        
        EditorGUILayout.Space();
        
        // 선택된 모드에 따른 설정들
        IngameRotateTargetBehaviour.RotationMode2D currentMode = 
            (IngameRotateTargetBehaviour.RotationMode2D)rotationModeProperty.enumValueIndex;
        
        DrawModeSpecificSettings(currentMode);
        
        EditorGUILayout.Space();
        
        // 공통 설정
        EditorGUILayout.LabelField("기타 설정", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(useLocalRotationProperty, new GUIContent("로컬 회전 사용"));
        EditorGUILayout.PropertyField(restoreOriginalRotationProperty, new GUIContent("원래 각도로 복구"));
        
        // 즉시 회전과 즉시 반전은 애니메이션 커브가 필요없음
        if (currentMode != IngameRotateTargetBehaviour.RotationMode2D.Instant && 
            currentMode != IngameRotateTargetBehaviour.RotationMode2D.InstantFlip)
        {
            EditorGUILayout.PropertyField(rotationCurveProperty, new GUIContent("회전 애니메이션 커브"));
        }

        serializedObject.ApplyModifiedProperties();
    }
    
    private void DrawRotateTargetSection()
    {
        // ExposedReference PropertyField 직접 그리기 (드래그 앤 드롭 가능)
        EditorGUILayout.PropertyField(rotateTargetProperty, new GUIContent("회전 타겟"));
        
        // 현재 할당된 Transform 표시
        var currentTarget = rotateTargetProperty.exposedReferenceValue as Transform;
        string targetName = currentTarget != null ? currentTarget.name : "None";
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("현재 타겟:", GUILayout.Width(80));
        EditorGUILayout.LabelField(targetName, EditorStyles.textField);
        EditorGUILayout.EndHorizontal();

        // 할당 버튼들
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("선택된 오브젝트 할당"))
        {
            var selected = Selection.activeGameObject?.transform;
            if (selected != null)
            {
                rotateTargetProperty.exposedReferenceValue = selected;
            }
            else
            {
                EditorUtility.DisplayDialog("알림", "씬에서 할당할 오브젝트를 선택해주세요.", "확인");
            }
        }
        
        if (GUILayout.Button("클리어"))
        {
            rotateTargetProperty.exposedReferenceValue = null;
        }
        
        EditorGUILayout.EndHorizontal();
        
        // 도움말
        EditorGUILayout.HelpBox(
            "ExposedReference 할당 방법:\n" +
            "1. 위 필드에 직접 드래그 앤 드롭\n" +
            "2. 씬에서 오브젝트 선택 후 '선택된 오브젝트 할당' 버튼\n" +
            "3. Timeline에서 ExposedReference 바인딩", 
            MessageType.Info);
    }
    
    private void DrawModeSpecificSettings(IngameRotateTargetBehaviour.RotationMode2D mode)
    {
        switch (mode)
        {
            case IngameRotateTargetBehaviour.RotationMode2D.Instant:
                DrawInstantRotationSettings();
                break;
                
            case IngameRotateTargetBehaviour.RotationMode2D.TimeBased:
                DrawTimeBasedRotationSettings();
                break;
                
            case IngameRotateTargetBehaviour.RotationMode2D.Flip:
                DrawFlipRotationSettings();
                break;
                
            case IngameRotateTargetBehaviour.RotationMode2D.InstantFlip:
                DrawInstantFlipSettings();
                break;
        }
    }
    
    private void DrawInstantRotationSettings()
    {
        EditorGUILayout.LabelField("즉시 회전 설정 (Z축)", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(instantRotationAngleProperty, new GUIContent("회전 각도 (도)"));
        
        // 도움말
        EditorGUILayout.HelpBox("오브젝트를 즉시 지정된 각도만큼 Z축으로 회전시킵니다.", MessageType.Info);
    }
    
    private void DrawTimeBasedRotationSettings()
    {
        EditorGUILayout.LabelField("시간 기반 회전 설정 (Z축)", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(rotationDurationProperty, new GUIContent("회전 시간 (초)"));
        EditorGUILayout.PropertyField(targetRotationAngleProperty, new GUIContent("목표 회전 각도 (도)"));
        
        // 도움말
        EditorGUILayout.HelpBox("지정된 시간에 걸쳐 목표 각도까지 Z축으로 회전시킵니다.\n애니메이션 커브를 통해 회전 속도를 조절할 수 있습니다.", MessageType.Info);
    }
    
    private void DrawFlipRotationSettings()
    {
        EditorGUILayout.LabelField("반전 회전 설정 (Y축)", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(flipCountProperty, new GUIContent("반전 횟수"));
        EditorGUILayout.PropertyField(flipDurationProperty, new GUIContent("반전당 시간 (초)"));
        
        // 반전 미리보기 정보
        int flipCount = flipCountProperty.intValue;
        float flipDuration = flipDurationProperty.floatValue;
        float totalTime = flipCount * flipDuration;
        
        EditorGUILayout.Space();
        EditorGUI.indentLevel++;
        EditorGUILayout.LabelField("미리보기:", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"총 시간: {totalTime:F1}초", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"최종 각도: Y축 {flipCount * 180}도", EditorStyles.miniLabel);
        EditorGUI.indentLevel--;
        
        // 도움말
        EditorGUILayout.HelpBox("오브젝트를 Y축으로 180도씩 반전시킵니다.\n반전 횟수만큼 좌우가 뒤바뀝니다.\n(홀수: 뒤집힘, 짝수: 원래대로)", MessageType.Info);
    }
    
    private void DrawInstantFlipSettings()
    {
        EditorGUILayout.LabelField("클립 길이 기반 즉시 반전 설정 (Y축)", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(instantFlipCountProperty, new GUIContent("반전 횟수"));
        
        // 클립 길이 기반 즉시 반전 미리보기 정보
        int instantFlipCount = instantFlipCountProperty.intValue;
        
        EditorGUILayout.Space();
        EditorGUI.indentLevel++;
        EditorGUILayout.LabelField("미리보기:", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"클립을 {instantFlipCount}구간으로 나누어 각 구간마다 즉시 반전", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"최종 각도: Y축 {instantFlipCount * 180}도", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"결과: {(instantFlipCount % 2 == 0 ? "원래대로" : "뒤집힌 상태")}", EditorStyles.miniLabel);
        EditorGUI.indentLevel--;
        
        // 도움말
        EditorGUILayout.HelpBox("클립 길이를 반전 횟수로 나누어 각 타이밍마다 즉시 180도씩 점프합니다.\n예: 3초 클립 + 3회 반전 = 1초, 2초, 3초에 즉시 반전\n부드러운 애니메이션 없이 순간적으로 각도가 변경됩니다.", MessageType.Info);
    }
} 