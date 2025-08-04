using UnityEngine;
using UnityEditor;
using UnityEngine.Timeline;

[CustomEditor(typeof(위치변경클립))]
public class IngamePositionChangeClipEditor : Editor
{
    private SerializedProperty targetTransformProp;
    private SerializedProperty changeModeProp;
    private SerializedProperty targetPositionProp;
    private SerializedProperty targetTransformReferenceProp;
    private SerializedProperty directTargetTransformProp;
    private SerializedProperty customOffsetProp;
    private SerializedProperty changeTypeProp;
    private SerializedProperty smoothChangeProp;
    private SerializedProperty changeDurationProp;
    private SerializedProperty changeCurveProp;
    private SerializedProperty customCurveProp;  // 추가
    private SerializedProperty movementTypeProp;  // 추가
    private SerializedProperty detectAnimationStartProp;
    private SerializedProperty targetAnimationClipsProp;
    private SerializedProperty useLocalPositionProp;
    private SerializedProperty restoreOriginalPositionProp;
    private SerializedProperty restoreOriginalPositionInEditorProp;
    private SerializedProperty onlyChangeXYProp;

    private void OnEnable()
    {
        targetTransformProp = serializedObject.FindProperty("targetTransform");
        changeModeProp = serializedObject.FindProperty("changeMode");
        targetPositionProp = serializedObject.FindProperty("targetPosition");
        targetTransformReferenceProp = serializedObject.FindProperty("targetTransformReference");
        directTargetTransformProp = serializedObject.FindProperty("directTargetTransform");
        customOffsetProp = serializedObject.FindProperty("customOffset");
        changeTypeProp = serializedObject.FindProperty("changeType");
        smoothChangeProp = serializedObject.FindProperty("smoothChange");
        changeDurationProp = serializedObject.FindProperty("changeDuration");
        changeCurveProp = serializedObject.FindProperty("changeCurve");
        customCurveProp = serializedObject.FindProperty("customCurve");  // 추가
        movementTypeProp = serializedObject.FindProperty("movementType");  // 추가
        detectAnimationStartProp = serializedObject.FindProperty("detectAnimationStart");
        targetAnimationClipsProp = serializedObject.FindProperty("targetAnimationClips");
        useLocalPositionProp = serializedObject.FindProperty("useLocalPosition");
        restoreOriginalPositionProp = serializedObject.FindProperty("restoreOriginalPosition");
        restoreOriginalPositionInEditorProp = serializedObject.FindProperty("restoreOriginalPositionInEditor");
        onlyChangeXYProp = serializedObject.FindProperty("onlyChangeXY");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 헤더 스타일
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.fontSize = 12;
        headerStyle.normal.textColor = new Color(1f, 0.6f, 0.2f); // 주황색

        // 타이틀
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("🎯 2D 애니메이션 위치 변경 클립", headerStyle);
        EditorGUILayout.Space();

        // 기본 설정
        EditorGUILayout.LabelField("■ 기본 설정", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(targetTransformProp, new GUIContent("변경 대상 Transform"));
        EditorGUILayout.PropertyField(changeModeProp, new GUIContent("변경 모드"));
        EditorGUI.indentLevel--;

        // 변경 모드에 따른 추가 설정
        var changeMode = (IngamePositionChangeBehaviour.ChangeMode)changeModeProp.enumValueIndex;
        
        EditorGUI.indentLevel++;
        if (changeMode == IngamePositionChangeBehaviour.ChangeMode.ToPosition)
        {
            EditorGUILayout.PropertyField(targetPositionProp, new GUIContent("목표 위치"));
            EditorGUILayout.HelpBox("지정된 Vector3 위치로 변경합니다.", MessageType.Info);
        }
        else if (changeMode == IngamePositionChangeBehaviour.ChangeMode.ToTransform)
        {
            EditorGUILayout.LabelField("🎯 목표 Transform 설정 (우선순위 순)", EditorStyles.boldLabel);
            
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(targetTransformReferenceProp, new GUIContent("1. ExposedReference Transform"));
            EditorGUILayout.PropertyField(directTargetTransformProp, new GUIContent("2. 직접 참조 Transform"));
            EditorGUILayout.PropertyField(targetPositionProp, new GUIContent("3. Vector3 백업값"));
            EditorGUI.indentLevel--;
            
            EditorGUILayout.HelpBox(
                "📌 사용 우선순위:\n" +
                "1. ExposedReference → 2. 직접 참조 → 3. Vector3 백업\n\n" +
                "💡 ExposedReference가 Timeline에서 권장되는 방식입니다!", 
                MessageType.Info);
        }
        else if (changeMode == IngamePositionChangeBehaviour.ChangeMode.Offset)
        {
            EditorGUILayout.PropertyField(customOffsetProp, new GUIContent("오프셋"));
            EditorGUILayout.HelpBox("현재 위치에 지정된 오프셋을 적용합니다.", MessageType.Info);
        }
        else if (changeMode == IngamePositionChangeBehaviour.ChangeMode.Relative)
        {
            EditorGUILayout.PropertyField(customOffsetProp, new GUIContent("상대적 오프셋"));
            EditorGUILayout.HelpBox("원본 위치 기준으로 상대적인 위치 변경을 적용합니다.", MessageType.Info);
        }
        EditorGUI.indentLevel--;

        EditorGUILayout.Space();

        // 변경 방식
        EditorGUILayout.LabelField("■ 변경 방식", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(changeTypeProp, new GUIContent("변경 타이밍"));
        EditorGUILayout.PropertyField(smoothChangeProp, new GUIContent("부드러운 변경"));
        
        if (smoothChangeProp.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(changeDurationProp, new GUIContent("변경 시간"));
            EditorGUILayout.PropertyField(changeCurveProp, new GUIContent("변경 커브"));
            EditorGUI.indentLevel--;
        }
        
        // 이동 방식 설정 추가
        EditorGUILayout.PropertyField(movementTypeProp, new GUIContent("이동 방식"));
        
        // AnimationCurve 설정 추가
        var movementType = (IngamePositionChangeBehaviour.MovementType)movementTypeProp.enumValueIndex;
        if (movementType == IngamePositionChangeBehaviour.MovementType.AnimationCurve)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(customCurveProp, new GUIContent("커스텀 커브"));
            EditorGUILayout.HelpBox("클립 길이에 따른 위치 변경 커브를 설정합니다.", MessageType.Info);
            EditorGUI.indentLevel--;
        }
        
        EditorGUI.indentLevel--;

        EditorGUILayout.Space();

        // 애니메이션 감지 설정
        EditorGUILayout.LabelField("■ 애니메이션 감지", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(detectAnimationStartProp, new GUIContent("애니메이션 시작 감지"));
        
        if (detectAnimationStartProp.boolValue)
        {
            EditorGUILayout.PropertyField(targetAnimationClipsProp, new GUIContent("대상 애니메이션 클립"), true);
            EditorGUILayout.HelpBox("비워두면 모든 애니메이션을 감지합니다.\n특정 애니메이션만 변경하려면 클립 이름을 입력하세요.", MessageType.Info);
        }
        EditorGUI.indentLevel--;

        EditorGUILayout.Space();

        // 기타 설정
        EditorGUILayout.LabelField("■ 기타 설정", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(useLocalPositionProp, new GUIContent("로컬 위치 사용"));
        EditorGUILayout.PropertyField(onlyChangeXYProp, new GUIContent("X,Y만 변경 (2D용)"));
        EditorGUILayout.PropertyField(restoreOriginalPositionProp, new GUIContent("런타임 종료 시 원위치 복원"));
        EditorGUI.indentLevel--;

        EditorGUILayout.Space();

        // 에디터 모드 설정
        EditorGUILayout.LabelField("■ 에디터 모드 설정", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(restoreOriginalPositionInEditorProp, new GUIContent("에디터 종료 시 원위치 복원"));
        EditorGUILayout.HelpBox(
            "💡 에디터에서 플레이 중이 아닐 때 Timeline 재생 시에만 적용됩니다.\n" +
            "플레이 중이나 빌드된 게임에서는 위의 '런타임 종료 시 원위치 복원' 설정이 사용됩니다.", 
            MessageType.Info);
        EditorGUI.indentLevel--;

        EditorGUILayout.Space();

        // 변경된 위치 복사 버튼
        EditorGUILayout.LabelField("■ 유틸리티", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.normal.textColor = Color.white;
        buttonStyle.fontStyle = FontStyle.Bold;
        
        if (GUILayout.Button("📋 변경된 위치 복사", buttonStyle, GUILayout.Height(30)))
        {
            CopyChangedPosition();
        }
        
        EditorGUILayout.HelpBox(
            "💡 현재 설정에 따른 변경 위치를 계산하고 클립보드에 복사합니다.\n" +
            "다른 클립이나 스크립트에서 위치값을 재사용할 때 유용합니다.", 
            MessageType.Info);
        
        EditorGUI.indentLevel--;
        EditorGUILayout.Space();

        // 사용법 안내
        EditorGUILayout.HelpBox(
            "🎯 사용법:\n" +
            "1. Timeline에서 위치변경트랙을 추가합니다.\n" +
            "2. 변경 대상 Transform을 할당합니다.\n" +
            "3. 애니메이션 클립과 함께 재생하면 자동으로 위치가 변경됩니다.\n\n" +
            "💡 변경 모드:\n" +
            "• ToPosition: 지정된 Vector3 위치로 변경\n" +
            "• ToTransform: Transform 위치로 변경\n" +
            "• Offset: 현재 위치에 오프셋 적용\n" +
            "• Relative: 원본 위치 기준 상대적 변경\n\n" +
            "⏰ 변경 타이밍:\n" +
            "• OnStart: 클립 시작 시 즉시 변경\n" +
            "• Continuous: 매 프레임 지속 변경\n" +
            "• OnAnimationStart: 애니메이션 시작 감지 시 변경",
            MessageType.Info
        );

        serializedObject.ApplyModifiedProperties();
    }

    private void CopyChangedPosition()
    {
        Transform targetTransform = null;
        
        // 1. ExposedReference Transform 확인 (우선순위)
        var clipAsset = target as 위치변경클립;
        if (clipAsset != null && clipAsset.targetTransformReference.exposedName != null)
        {
            // ExposedReference가 설정되어 있지만 실제로는 Scene에서 찾아야 함
            // Scene에서 "Ani_potion" 이름으로 찾기
            var aniPotion = GameObject.Find("Ani_potion");
            if (aniPotion != null)
            {
                targetTransform = aniPotion.transform;
                Debug.Log($"[위치변경] ExposedReference 대상 찾음: {targetTransform.name}");
            }
        }
        // 2. 직접 참조 Transform 확인 (백업)
        else if (clipAsset != null && clipAsset.directTargetTransform != null)
        {
            targetTransform = clipAsset.directTargetTransform;
        }
        
        // 3. Scene에서 "Ani_potion" 이름으로 찾기
        if (targetTransform == null)
        {
            var aniPotion = GameObject.Find("Ani_potion");
            if (aniPotion != null)
            {
                targetTransform = aniPotion.transform;
                Debug.Log($"[위치변경] Ani_potion 찾음: {targetTransform.name}");
            }
        }
        
        // 4. 현재 선택된 GameObject 확인
        if (targetTransform == null && Selection.activeGameObject != null)
        {
            targetTransform = Selection.activeGameObject.transform;
            Debug.Log($"[위치변경] 선택된 GameObject 사용: {targetTransform.name}");
        }
        
        if (targetTransform == null)
        {
            EditorUtility.DisplayDialog("❌ 오류", 
                "변경 대상 Transform을 찾을 수 없습니다!\n\n" +
                                 "💡 해결 방법:\n" +
                 "1. Scene에서 'Ani_potion' GameObject가 있는지 확인\n" +
                 "2. 해당 GameObject를 선택한 상태에서 다시 시도\n" +
                 "3. 또는 'ExposedReference Transform' 필드에 할당", 
                "확인");
            return;
        }
        
        // 현재 위치 가져오기 (로컬/월드 위치 설정에 따라)
        bool useLocal = useLocalPositionProp.boolValue;
        Vector3 currentPosition = useLocal ? targetTransform.localPosition : targetTransform.position;
        
        // 클립보드에 복사
        string clipboardText = $"Vector3({currentPosition.x},{currentPosition.y},{currentPosition.z})";
        GUIUtility.systemCopyBuffer = clipboardText;
        
        // 사용자에게 알림
        string positionType = useLocal ? "로컬" : "월드";
        string logMessage = $"[위치변경] 📋 복사 완료!\n대상: {targetTransform.name}\n위치 타입: {positionType}\n현재 위치: {clipboardText}";
        Debug.Log(logMessage);
        
        // Unity Inspector에서도 확인할 수 있도록 일시적 팝업
        EditorUtility.DisplayDialog(
            "📋 현재 위치 복사 완료", 
            $"대상: {targetTransform.name}\n위치 타입: {positionType}\n\n현재 위치:\n{clipboardText}\n\n클립보드에 복사되었습니다!", 
            "확인"
        );
    }
} 