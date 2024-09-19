using System.Collections.Generic;
using DeskCat.FindIt.Scripts.Core.Main.System;
using DeskCat.FindIt.Scripts.Core.Main.Utility.ClickedFunction;
using DeskCat.FindIt.Scripts.Core.Main.Utility.DragObj;
using DeskCat.FindIt.Scripts.Core.Model;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

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
        SerializedProperty tooltipsProperty;

        private AnimBool actionAnimBool;
        private AnimBool bgAnimBool;
        
        private int actionToolbarInt = 0;

        private void OnEnable()
        {
            HiddenObjTarget = (HiddenObj)target;

            baseInfoAnimBool = new AnimBool(true);
            baseInfoAnimBool.valueChanged.AddListener(Repaint);

            tooltipsAnimBool = new AnimBool(true);
            tooltipsAnimBool.valueChanged.AddListener(Repaint);
            tooltipsProperty = serializedObject.FindProperty("Tooltips");

            bgAnimBool = new AnimBool(true);
            bgAnimBool.valueChanged.AddListener(Repaint);

            actionAnimBool = new AnimBool(true);
            actionAnimBool.valueChanged.AddListener(Repaint);

            HiddenObjTarget.TryGetComponent<SpriteRenderer>(out var defaultSprite);
            if (defaultSprite != null)
            {
                DefaultSprite = defaultSprite.sprite;
                if (HiddenObjTarget.UISprite == null)
                {
                    HiddenObjTarget.UISprite = DefaultSprite;
                }
            }
        }

        public override void OnInspectorGUI()
        {
            DrawBaseInfo();
            DrawTooltips();
            DrawBGAnimation();
            DrawActionUtility();
        }

        private void DrawBaseInfo()
        {
            HiddenObjTarget.baseInfoBool = EditorGUILayout.BeginFoldoutHeaderGroup(HiddenObjTarget.baseInfoBool, "General Setting");
            baseInfoAnimBool.target = HiddenObjTarget.baseInfoBool;

            if (EditorGUILayout.BeginFadeGroup(baseInfoAnimBool.faded))
            {
                EditorGUI.indentLevel++;
                {
                    HiddenObjTarget.hiddenObjFoundType = (HiddenObjFoundType)EditorGUILayout.EnumPopup("Found Type",
                        HiddenObjTarget.hiddenObjFoundType);

                    HiddenObjTarget.UISprite = (Sprite)EditorGUILayout.ObjectField("UI Sprite",
                        HiddenObjTarget.UISprite, typeof(Sprite), true);

                    HiddenObjTarget.HideOnStart =
                        EditorGUILayout.ToggleLeft("Hide Object On Start", HiddenObjTarget.HideOnStart);

                    HiddenObjTarget.HideWhenFound =
                        EditorGUILayout.ToggleLeft("Hide Object When Found", HiddenObjTarget.HideWhenFound);

                    HiddenObjTarget.PlaySoundWhenFound = EditorGUILayout.ToggleLeft("Play Sound When Found",
                        HiddenObjTarget.PlaySoundWhenFound);

                    if (HiddenObjTarget.PlaySoundWhenFound)
                    {
                        HiddenObjTarget.AudioWhenClick = (AudioClip)EditorGUILayout.ObjectField("Specified Sound",
                            HiddenObjTarget.AudioWhenClick, typeof(AudioClip), true);
                    }
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFadeGroup();
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawTooltips()
        {
            HiddenObjTarget.tooltipsBool = EditorGUILayout.BeginFoldoutHeaderGroup(HiddenObjTarget.tooltipsBool, "Tooltips Setting");
            tooltipsAnimBool.target = HiddenObjTarget.tooltipsBool;
            EditorGUILayout.EndFoldoutHeaderGroup();


            if (EditorGUILayout.BeginFadeGroup(tooltipsAnimBool.faded))
            {
                EditorGUI.indentLevel++;
                {
                    HiddenObjTarget.EnableTooltip =
                        EditorGUILayout.ToggleLeft("Enable Tooltips", HiddenObjTarget.EnableTooltip);

                    if (HiddenObjTarget.EnableTooltip)
                    {
                        HiddenObjTarget.TooltipsType = (TooltipsType)EditorGUILayout.EnumPopup("Tooltips Type",
                            HiddenObjTarget.TooltipsType);

                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Space(EditorGUIUtility.labelWidth);
                            if (GUILayout.Button("Generate Default Tooltips", EditorStyles.miniButtonRight))
                            {
                                HiddenObjTarget.Tooltips =
                                    GlobalSetting.GetDefaultLanguageKey<MultiLanguageTextListModel>();
                                foreach (var tooltip in HiddenObjTarget.Tooltips)
                                {
                                    tooltip.Value ??= new List<string>() { "" };
                                }
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.PropertyField(tooltipsProperty, new GUIContent("Tooltips Value"));
                        serializedObject.ApplyModifiedProperties();
                    }
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFadeGroup();
        }

        private void DrawBGAnimation()
        {
            HiddenObjTarget.bgAnimBool = EditorGUILayout.BeginFoldoutHeaderGroup(HiddenObjTarget.bgAnimBool, "Clicked Background Animation");
            bgAnimBool.target = HiddenObjTarget.bgAnimBool;

            if (EditorGUILayout.BeginFadeGroup(bgAnimBool.faded))
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.BeginHorizontal();
                {
                    HiddenObjTarget.EnableBGAnimation =
                        EditorGUILayout.ToggleLeft("Enable Background Animation", HiddenObjTarget.EnableBGAnimation);
                    
                    if (GUILayout.Button("Clean", EditorStyles.miniButtonRight))
                    {
                        HiddenObjTarget.BGAnimationPrefab = null;
                        if(HiddenObjTarget.BgAnimationTransform) DestroyImmediate(HiddenObjTarget.BgAnimationTransform.gameObject);
                        HiddenObjTarget.BgAnimationTransform = null;
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                
                if (HiddenObjTarget.EnableBGAnimation)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        HiddenObjTarget.BGAnimationPrefab = (GameObject)EditorGUILayout.ObjectField("Background Prefab", 
                            HiddenObjTarget.BGAnimationPrefab, typeof(GameObject), false);
                        
                        if (GUILayout.Button("Use Default", EditorStyles.miniButtonRight))
                        {
                            HiddenObjTarget.BGAnimationPrefab = (GameObject) Resources.Load("Prefabs/BGAnimPrefab");
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.BeginHorizontal();
                    {
                        
                        HiddenObjTarget.BgAnimationTransform = (Transform)EditorGUILayout.ObjectField("BG Object", 
                            HiddenObjTarget.BgAnimationTransform, typeof(Transform), true);
                        
                        if (GUILayout.Button("Add BG Object", EditorStyles.miniButtonRight))
                        {
                            var obj = Instantiate(HiddenObjTarget.BGAnimationPrefab, HiddenObjTarget.transform);
                            obj.GetComponent<SpriteRenderer>().sortingOrder =
                                HiddenObjTarget.GetComponent<SpriteRenderer>().sortingOrder - 1;
                            HiddenObjTarget.BgAnimationTransform = obj.transform;
                        }
                        
                        if (GUILayout.Button("Hide/Show", EditorStyles.miniButtonRight))
                        {
                            HiddenObjTarget.BgAnimationTransform.gameObject.SetActive(
                                !HiddenObjTarget.BgAnimationTransform.gameObject.activeSelf);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    HiddenObjTarget.HideWhenFound = false;
                }
                
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFadeGroup();
            EditorGUILayout.EndFoldoutHeaderGroup();
        
        }

        private void DrawActionUtility()
        {
            HiddenObjTarget.actionFoldoutBool = EditorGUILayout.BeginFoldoutHeaderGroup(HiddenObjTarget.actionFoldoutBool, "Show Action Utility");
            actionAnimBool.target = HiddenObjTarget.actionFoldoutBool;

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
                        HiddenObjTarget.AddComponent<ClickEvent>();
                    }

                    if (GUILayout.Button("Add Multiple Click Event"))
                    {
                        HiddenObjTarget.AddComponent<MultipleClickEvent>();
                    }

                    if (GUILayout.Button("Add Drag Event"))
                    {
                        HiddenObjTarget.AddComponent<DragObj>();
                    }

                    if (GUILayout.Button("Add Clickable Function"))
                    {
                        HiddenObjTarget.AddComponent<ClickableFunction>();
                    }
                }

                if (actionToolbarInt == 1)
                {
                    if (GUILayout.Button("Add/Reset Box Collider 2D"))
                    {
                        HiddenObjTarget.TryGetComponent(typeof(BoxCollider2D), out var boxCollider2D);
                        if (boxCollider2D != null)
                        {
                            DestroyImmediate(boxCollider2D);
                        }

                        HiddenObjTarget.AddComponent<BoxCollider2D>();
                    }
                }

                if (actionToolbarInt == 2)
                {
                    if (GUILayout.Button("Add/Reset Mesh Collider 3D"))
                    {
                        HiddenObjTarget.TryGetComponent(typeof(MeshCollider), out var meshCollider);
                        if (meshCollider != null)
                        {
                            DestroyImmediate(meshCollider);
                        }

                        HiddenObjTarget.AddComponent<MeshCollider>();
                    }
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFadeGroup();
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}