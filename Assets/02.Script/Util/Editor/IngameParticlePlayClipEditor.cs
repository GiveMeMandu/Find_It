using UnityEngine;
using UnityEditor;
using UnityEngine.Timeline;

[CustomEditor(typeof(파티클재생클립))]
public class IngameParticlePlayClipEditor : Editor
{
    private SerializedProperty targetParticleSystemProp;
    private SerializedProperty particleTransformProp;
    private SerializedProperty playModeProp;
    private SerializedProperty autoStopProp;
    private SerializedProperty playDurationProp;
    private SerializedProperty delayTimeProp;
    private SerializedProperty loopProp;
    private SerializedProperty useCustomPositionProp;
    private SerializedProperty customPositionProp;
    private SerializedProperty positionReferenceProp;
    private SerializedProperty directPositionReferenceProp;
    private SerializedProperty useCustomScaleProp;
    private SerializedProperty customScaleProp;
    private SerializedProperty useCustomRotationProp;
    private SerializedProperty customRotationProp;
    private SerializedProperty detectAnimationStartProp;
    private SerializedProperty targetAnimationClipsProp;
    private SerializedProperty restoreOriginalSettingsProp;
    private SerializedProperty onlyPlayOnceProp;
    private SerializedProperty restoreOriginalSettingsInEditorProp;

    private void OnEnable()
    {
        targetParticleSystemProp = serializedObject.FindProperty("targetParticleSystem");
        particleTransformProp = serializedObject.FindProperty("particleTransform");
        playModeProp = serializedObject.FindProperty("playMode");
        autoStopProp = serializedObject.FindProperty("autoStop");
        playDurationProp = serializedObject.FindProperty("playDuration");
        delayTimeProp = serializedObject.FindProperty("delayTime");
        loopProp = serializedObject.FindProperty("loop");
        useCustomPositionProp = serializedObject.FindProperty("useCustomPosition");
        customPositionProp = serializedObject.FindProperty("customPosition");
        positionReferenceProp = serializedObject.FindProperty("positionReference");
        directPositionReferenceProp = serializedObject.FindProperty("directPositionReference");
        useCustomScaleProp = serializedObject.FindProperty("useCustomScale");
        customScaleProp = serializedObject.FindProperty("customScale");
        useCustomRotationProp = serializedObject.FindProperty("useCustomRotation");
        customRotationProp = serializedObject.FindProperty("customRotation");
        detectAnimationStartProp = serializedObject.FindProperty("detectAnimationStart");
        targetAnimationClipsProp = serializedObject.FindProperty("targetAnimationClips");
        restoreOriginalSettingsProp = serializedObject.FindProperty("restoreOriginalSettings");
        onlyPlayOnceProp = serializedObject.FindProperty("onlyPlayOnce");
        restoreOriginalSettingsInEditorProp = serializedObject.FindProperty("restoreOriginalSettingsInEditor");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 헤더 스타일
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.fontSize = 12;
        headerStyle.normal.textColor = new Color(0.8f, 0.4f, 0.8f);

        // 타이틀
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("✨ 파티클 재생 클립", headerStyle);
        EditorGUILayout.Space();

        // 기본 설정
        EditorGUILayout.LabelField("■ 기본 설정", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(targetParticleSystemProp, new GUIContent("파티클 시스템"));
        EditorGUILayout.PropertyField(particleTransformProp, new GUIContent("파티클 Transform"));
        EditorGUILayout.PropertyField(playModeProp, new GUIContent("재생 모드"));
        EditorGUI.indentLevel--;

        EditorGUILayout.Space();

        // 재생 설정
        EditorGUILayout.LabelField("■ 재생 설정", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(autoStopProp, new GUIContent("자동 정지"));
        EditorGUILayout.PropertyField(playDurationProp, new GUIContent("재생 시간"));
        EditorGUILayout.PropertyField(delayTimeProp, new GUIContent("지연 시간"));
        EditorGUILayout.PropertyField(loopProp, new GUIContent("루프"));
        EditorGUILayout.PropertyField(onlyPlayOnceProp, new GUIContent("한 번만 재생"));
        EditorGUI.indentLevel--;

        EditorGUILayout.Space();

        // 위치 설정
        EditorGUILayout.LabelField("■ 위치 설정", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(useCustomPositionProp, new GUIContent("커스텀 위치 사용"));
        
        if (useCustomPositionProp.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("🎯 위치 설정 (우선순위 순)", EditorStyles.boldLabel);
            
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(positionReferenceProp, new GUIContent("1. ExposedReference Transform"));
            EditorGUILayout.PropertyField(directPositionReferenceProp, new GUIContent("2. 직접 참조 Transform"));
            EditorGUILayout.PropertyField(customPositionProp, new GUIContent("3. Vector3 백업값"));
            EditorGUI.indentLevel--;
            
            EditorGUILayout.HelpBox(
                "📌 사용 우선순위:\n" +
                "1. ExposedReference → 2. 직접 참조 → 3. Vector3 백업\n\n" +
                "💡 ExposedReference가 Timeline에서 권장되는 방식입니다!", 
                MessageType.Info);
            EditorGUI.indentLevel--;
        }
        EditorGUI.indentLevel--;

        EditorGUILayout.Space();

        // 스케일 설정
        EditorGUILayout.LabelField("■ 스케일 설정", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(useCustomScaleProp, new GUIContent("커스텀 스케일 사용"));
        if (useCustomScaleProp.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(customScaleProp, new GUIContent("스케일"));
            EditorGUI.indentLevel--;
        }
        EditorGUI.indentLevel--;

        EditorGUILayout.Space();

        // 회전 설정
        EditorGUILayout.LabelField("■ 회전 설정", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(useCustomRotationProp, new GUIContent("커스텀 회전 사용"));
        if (useCustomRotationProp.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(customRotationProp, new GUIContent("회전 (Euler)"));
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
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(targetAnimationClipsProp, new GUIContent("대상 애니메이션 클립"));
            EditorGUILayout.HelpBox("특정 애니메이션 클립 이름을 포함하는 경우에만 파티클을 재생합니다.", MessageType.Info);
            EditorGUI.indentLevel--;
        }
        EditorGUI.indentLevel--;

        EditorGUILayout.Space();

        // 기타 설정
        EditorGUILayout.LabelField("■ 기타 설정", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(restoreOriginalSettingsProp, new GUIContent("원래 설정 복원"));
        EditorGUILayout.PropertyField(restoreOriginalSettingsInEditorProp, new GUIContent("에디터에서 원래 설정 복원"));
        EditorGUI.indentLevel--;

        EditorGUILayout.Space();

        // 도움말
        EditorGUILayout.LabelField("■ 사용법", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "🎮 파티클 재생 모드:\n" +
            "• PlayOnStart: 클립 시작 시 즉시 재생\n" +
            "• PlayOnAnimationStart: 애니메이션 시작 감지 시 재생\n" +
            "• PlayOnCustomTime: 지연 시간 후 재생\n\n" +
            "💡 팁:\n" +
            "• 자동 정지가 활성화되면 재생 시간 후 자동으로 정지됩니다\n" +
            "• 지연 시간은 PlayOnCustomTime 모드에서만 사용됩니다\n" +
            "• 루프가 활성화되면 계속 재생됩니다\n" +
            "• 한 번만 재생이 활성화되면 클립이 다시 시작되어도 재생되지 않습니다", 
            MessageType.Info);

        // 미리보기 버튼
        EditorGUILayout.Space();
        if (GUILayout.Button("🎬 파티클 미리보기", GUILayout.Height(30)))
        {
            PreviewParticle();
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void PreviewParticle()
    {
        var clip = target as 파티클재생클립;
        if (clip != null && clip.targetParticleSystem != null)
        {
            var particleSystem = clip.targetParticleSystem.Resolve(UnityEditor.Timeline.TimelineEditor.inspectedDirector);
            if (particleSystem != null)
            {
                // 미리보기 재생
                particleSystem.Play();
                Debug.Log($"[파티클미리보기] {particleSystem.name} 미리보기 재생");
                
                // 3초 후 자동 정지
                EditorApplication.delayCall += () =>
                {
                    if (particleSystem != null)
                    {
                        particleSystem.Stop();
                        Debug.Log($"[파티클미리보기] {particleSystem.name} 미리보기 정지");
                    }
                };
            }
        }
    }
} 