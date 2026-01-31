using System.Collections.Generic;
using DeskCat.FindIt.Scripts.Core.Main.System;
using DeskCat.FindIt.Scripts.Core.Main.Utility.ClickedFunction;
using DeskCat.FindIt.Scripts.Core.Main.Utility.DragObj;
using DeskCat.FindIt.Scripts.Core.Model;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UI;
using Helper;

namespace DeskCat.FindIt.Scripts.Editor
{
    [CustomEditor(typeof(HiddenObj))]
    [CanEditMultipleObjects]
    public class HiddenObjEditor : UnityEditor.Editor
    {
        private HiddenObj HiddenObjTarget;
        private Sprite DefaultSprite;

        private AnimBool baseInfoAnimBool;
        private AnimBool tooltipsAnimBool;
        private AnimBool actionAnimBool;
        private AnimBool bgAnimBool;
        
        private int actionToolbarInt = 0;

        // Serialized Properties
        SerializedProperty hiddenObjFoundTypeProp;
        SerializedProperty UISpriteProp;
        SerializedProperty uiChangeHelperProp;
        SerializedProperty HideOnStartProp;
        SerializedProperty HideWhenFoundProp;
        SerializedProperty PlaySoundWhenFoundProp;
        SerializedProperty AudioWhenClickProp;
        
        SerializedProperty EnableTooltipProp;
        SerializedProperty TooltipsTypeProp;
        SerializedProperty tooltipsProperty;

        SerializedProperty EnableBGAnimationProp;
        SerializedProperty BGAnimationPrefabProp;
        SerializedProperty BgAnimationTransformProp;
        
        SerializedProperty baseInfoBoolProp;
        SerializedProperty tooltipsBoolProp;
        SerializedProperty bgAnimBoolProp;
        SerializedProperty actionFoldoutBoolProp;

        private void OnEnable()
        {
            HiddenObjTarget = (HiddenObj)target;

            // Initialize Properties
            hiddenObjFoundTypeProp = serializedObject.FindProperty("hiddenObjFoundType");
            UISpriteProp = serializedObject.FindProperty("UISprite");
            uiChangeHelperProp = serializedObject.FindProperty("uiChangeHelper");
            HideOnStartProp = serializedObject.FindProperty("HideOnStart");
            HideWhenFoundProp = serializedObject.FindProperty("HideWhenFound");
            PlaySoundWhenFoundProp = serializedObject.FindProperty("PlaySoundWhenFound");
            AudioWhenClickProp = serializedObject.FindProperty("AudioWhenClick");
            
            EnableTooltipProp = serializedObject.FindProperty("EnableTooltip");
            TooltipsTypeProp = serializedObject.FindProperty("TooltipsType");
            tooltipsProperty = serializedObject.FindProperty("Tooltips");
            
            EnableBGAnimationProp = serializedObject.FindProperty("EnableBGAnimation");
            BGAnimationPrefabProp = serializedObject.FindProperty("BGAnimationPrefab");
            BgAnimationTransformProp = serializedObject.FindProperty("BgAnimationTransform");
            
            baseInfoBoolProp = serializedObject.FindProperty("baseInfoBool");
            tooltipsBoolProp = serializedObject.FindProperty("tooltipsBool");
            bgAnimBoolProp = serializedObject.FindProperty("bgAnimBool");
            actionFoldoutBoolProp = serializedObject.FindProperty("actionFoldoutBool");

            // 레이어가 HiddenObjectLayer가 아니면 자동으로 설정
            if (HiddenObjTarget.gameObject.layer != LayerManager.HiddenObjectLayer)
            {
                Undo.RecordObject(HiddenObjTarget.gameObject, "Set HiddenObject Layer");
                HiddenObjTarget.gameObject.layer = LayerManager.HiddenObjectLayer;
                EditorUtility.SetDirty(HiddenObjTarget.gameObject);
            }

            baseInfoAnimBool = new AnimBool(baseInfoBoolProp.boolValue);
            baseInfoAnimBool.valueChanged.AddListener(Repaint);

            tooltipsAnimBool = new AnimBool(tooltipsBoolProp.boolValue);
            tooltipsAnimBool.valueChanged.AddListener(Repaint);

            bgAnimBool = new AnimBool(bgAnimBoolProp.boolValue);
            bgAnimBool.valueChanged.AddListener(Repaint);

            actionAnimBool = new AnimBool(actionFoldoutBoolProp.boolValue);
            actionAnimBool.valueChanged.AddListener(Repaint);

            HiddenObjTarget.TryGetComponent<SpriteRenderer>(out var defaultSprite);
            if (defaultSprite != null)
            {
                DefaultSprite = defaultSprite.sprite;
                if (UISpriteProp.objectReferenceValue == null)
                {
                    UISpriteProp.objectReferenceValue = DefaultSprite;
                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // 레이어 상태 확인 및 정보 표시
            if (HiddenObjTarget.gameObject.layer != LayerManager.HiddenObjectLayer)
            {
                EditorGUILayout.HelpBox($"HiddenObj는 자동으로 HiddenObjectLayer({LayerManager.HiddenObjectLayer})로 설정됩니다.", MessageType.Info);
                
                // 자동으로 레이어 설정
                if (GUILayout.Button("Fix Layer"))
                {
                    Undo.RecordObject(HiddenObjTarget.gameObject, "Set HiddenObject Layer");
                    HiddenObjTarget.gameObject.layer = LayerManager.HiddenObjectLayer;
                    EditorUtility.SetDirty(HiddenObjTarget.gameObject);
                }
            }

            DrawBaseInfo();
            DrawTooltips();
            DrawBGAnimation();
            DrawActionUtility();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawBaseInfo()
        {
            baseInfoBoolProp.boolValue = EditorGUILayout.BeginFoldoutHeaderGroup(baseInfoBoolProp.boolValue, "General Setting");
            baseInfoAnimBool.target = baseInfoBoolProp.boolValue;

            if (EditorGUILayout.BeginFadeGroup(baseInfoAnimBool.faded))
            {
                EditorGUI.indentLevel++;
                {
                    EditorGUILayout.PropertyField(hiddenObjFoundTypeProp, new GUIContent("Found Type"));
                    EditorGUILayout.PropertyField(UISpriteProp, new GUIContent("UI Sprite"));
                    EditorGUILayout.PropertyField(uiChangeHelperProp, new GUIContent("UI Change Helper"));
                    EditorGUILayout.PropertyField(HideOnStartProp, new GUIContent("Hide Object On Start"));
                    EditorGUILayout.PropertyField(HideWhenFoundProp, new GUIContent("Hide Object When Found"));
                    EditorGUILayout.PropertyField(PlaySoundWhenFoundProp, new GUIContent("Play Sound When Found"));

                    if (PlaySoundWhenFoundProp.boolValue)
                    {
                        EditorGUILayout.PropertyField(AudioWhenClickProp, new GUIContent("Specified Sound"));
                    }
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFadeGroup();
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawTooltips()
        {
            tooltipsBoolProp.boolValue = EditorGUILayout.BeginFoldoutHeaderGroup(tooltipsBoolProp.boolValue, "Tooltips Setting");
            tooltipsAnimBool.target = tooltipsBoolProp.boolValue;
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (EditorGUILayout.BeginFadeGroup(tooltipsAnimBool.faded))
            {
                EditorGUI.indentLevel++;
                {
                    EditorGUILayout.PropertyField(EnableTooltipProp, new GUIContent("Enable Tooltips"));

                    if (EnableTooltipProp.boolValue)
                    {
                        EditorGUILayout.PropertyField(TooltipsTypeProp, new GUIContent("Tooltips Type"));

                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Space(EditorGUIUtility.labelWidth);
                            if (GUILayout.Button("Generate Default Tooltips", EditorStyles.miniButtonRight))
                            {
                                Undo.RecordObject(HiddenObjTarget, "Generate Default Tooltips");
                                HiddenObjTarget.Tooltips =
                                    GlobalSetting.GetDefaultLanguageKey<MultiLanguageTextListModel>();
                                foreach (var tooltip in HiddenObjTarget.Tooltips)
                                {
                                    tooltip.Value ??= new List<string>() { "" };
                                }
                                EditorUtility.SetDirty(HiddenObjTarget);
                                serializedObject.Update(); // Update serializedObject to reflect changes
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.PropertyField(tooltipsProperty, new GUIContent("Tooltips Value"));
                    }
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFadeGroup();
        }

        private void DrawBGAnimation()
        {
            bgAnimBoolProp.boolValue = EditorGUILayout.BeginFoldoutHeaderGroup(bgAnimBoolProp.boolValue, "Clicked Background Animation");
            bgAnimBool.target = bgAnimBoolProp.boolValue;

            if (EditorGUILayout.BeginFadeGroup(bgAnimBool.faded))
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.PropertyField(EnableBGAnimationProp, new GUIContent("Enable Background Animation"));
                    
                    if (GUILayout.Button("Clean", EditorStyles.miniButtonRight))
                    {
                        Undo.RecordObject(HiddenObjTarget, "Clean BG Animation");
                        BGAnimationPrefabProp.objectReferenceValue = null;
                        if(HiddenObjTarget.BgAnimationTransform) 
                        {
                            Undo.DestroyObjectImmediate(HiddenObjTarget.BgAnimationTransform.gameObject);
                        }
                        BgAnimationTransformProp.objectReferenceValue = null;
                        // No SetDirty needed as we updated properties, but DestroyObjectImmediate handles scene dirtying
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                
                if (EnableBGAnimationProp.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.PropertyField(BGAnimationPrefabProp, new GUIContent("Background Prefab"));
                        
                        if (GUILayout.Button("Use Default", EditorStyles.miniButtonRight))
                        {
                            BGAnimationPrefabProp.objectReferenceValue = (GameObject) Resources.Load("Prefabs/BGAnimPrefab");
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.PropertyField(BgAnimationTransformProp, new GUIContent("BG Object"));
                        
                        if (GUILayout.Button("Add BG Object", EditorStyles.miniButtonRight))
                        {
                            // Need to handle object instantiation which isn't a property change
                            if (BGAnimationPrefabProp.objectReferenceValue != null)
                            {
                                GameObject prefab = (GameObject)BGAnimationPrefabProp.objectReferenceValue;
                                GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab, HiddenObjTarget.transform);
                                Undo.RegisterCreatedObjectUndo(obj, "Create BG Animation Object");
                                
                                SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                                SpriteRenderer targetSr = HiddenObjTarget.GetComponent<SpriteRenderer>();
                                if (sr != null && targetSr != null)
                                {
                                    Undo.RecordObject(sr, "Set Sorting Order");
                                    sr.sortingOrder = targetSr.sortingOrder - 1;
                                }
                                
                                BgAnimationTransformProp.objectReferenceValue = obj.transform;
                            }
                        }
                        
                        if (GUILayout.Button("Hide/Show", EditorStyles.miniButtonRight))
                        {
                            Transform t = (Transform)BgAnimationTransformProp.objectReferenceValue;
                            if (t != null)
                            {
                                Undo.RecordObject(t.gameObject, "Toggle Active");
                                t.gameObject.SetActive(!t.gameObject.activeSelf);
                            }
                        }
                        
                        if (GUILayout.Button("AutoScale", EditorStyles.miniButtonRight))
                        {
                            Transform t = (Transform)BgAnimationTransformProp.objectReferenceValue;
                            if (t != null)
                            {
                                Undo.RecordObject(t, "Auto Scale");
                                var currentScale = t.localScale;
                                float maxScale = Mathf.Max(currentScale.x, currentScale.y);
                                t.localScale = new Vector3(maxScale, maxScale, currentScale.z);
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    if (HideWhenFoundProp.boolValue)
                    {
                        EditorGUILayout.HelpBox("Background animation may not work properly when 'Hide When Found' is enabled.", MessageType.Warning);
                    }
                    
                    EditorGUI.indentLevel--;
                }
                
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFadeGroup();
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawActionUtility()
        {
            actionFoldoutBoolProp.boolValue = EditorGUILayout.BeginFoldoutHeaderGroup(actionFoldoutBoolProp.boolValue, "Show Action Utility");
            actionAnimBool.target = actionFoldoutBoolProp.boolValue;

            if (EditorGUILayout.BeginFadeGroup(actionAnimBool.faded))
            {
                EditorGUI.indentLevel++;
                string[] toolbarStrings = {"General", "2D", "3D"};
                actionToolbarInt = GUILayout.Toolbar(actionToolbarInt, toolbarStrings);
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                if (actionToolbarInt == 0)
                {
                    if (GUILayout.Button("Add Click Event"))
                    {
                        Undo.AddComponent<ClickEvent>(HiddenObjTarget.gameObject);
                    }

                    if (GUILayout.Button("Add Multiple Click Event"))
                    {
                        Undo.AddComponent<MultipleClickEvent>(HiddenObjTarget.gameObject);
                    }

                    if (GUILayout.Button("Add Drag Event"))
                    {
                        Undo.AddComponent<DragObj>(HiddenObjTarget.gameObject);
                    }

                    if (GUILayout.Button("Add Clickable Function"))
                    {
                        Undo.AddComponent<ClickableFunction>(HiddenObjTarget.gameObject);
                    }
                }

                if (actionToolbarInt == 1)
                {
                    if (GUILayout.Button("Add/Reset Box Collider 2D"))
                    {
                        HiddenObjTarget.TryGetComponent(typeof(BoxCollider2D), out var boxCollider2D);
                        if (boxCollider2D != null)
                        {
                            Undo.DestroyObjectImmediate(boxCollider2D);
                        }
                        Undo.AddComponent<BoxCollider2D>(HiddenObjTarget.gameObject);
                    }
                }

                if (actionToolbarInt == 2)
                {
                    if (GUILayout.Button("Add/Reset Mesh Collider 3D"))
                    {
                        HiddenObjTarget.TryGetComponent(typeof(MeshCollider), out var meshCollider);
                        if (meshCollider != null)
                        {
                            Undo.DestroyObjectImmediate(meshCollider);
                        }
                        Undo.AddComponent<MeshCollider>(HiddenObjTarget.gameObject);
                    }
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFadeGroup();
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}