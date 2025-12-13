using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VoxelLabs.UltimateThumbnails.Lib;
using Toolbar = UnityEditor.UIElements.Toolbar;

namespace VoxelLabs.UltimateThumbnails.UIElements
{
    public static class ToolbarBottom
    {
        private static List<ToolbarData> toolbarDataList = new List<ToolbarData>();

        public static ToolbarData BottomToolbarGUI(bool isEnable, bool isEnhanceVisibilityOn, Color backgroundColor,
            Texture2D enhancedVisibilityEnabledIcon, Texture2D enhancedVisibilityDisabledIcon, ToolbarSettings toolbarSettings,
            Action<Color> OnColorFieldValueChanged, Action<bool> OnVisibilityEnhancerToggleChanged, Action<bool> OnEnableToggleChanged)
        {
            var toolbar = new Toolbar();
            var toolbarData = new ToolbarData();
            
            toolbar.style.position = Position.Absolute;
            toolbar.style.bottom = 0;  
            toolbar.style.height = 20.5f;                  
            toolbar.style.right = 73;
            toolbar.style.paddingRight = 5;
            toolbar.style.flexDirection = FlexDirection.Row;
            toolbar.style.justifyContent = Justify.FlexStart;
            toolbar.style.borderBottomColor = EditorGUIUtility.isProSkin ? new Color(0.25f, 0.25f, 0.25f, 1) : new Color(0.811f, 0.811f, 0.811f, 1.0f);
            toolbar.style.backgroundColor = EditorGUIUtility.isProSkin ? new Color(0.25f, 0.25f, 0.25f, 1) : new Color(0.811f, 0.811f, 0.811f, 1.0f);
            
            var colorField = new ColorField()
            {
                value = backgroundColor,
                style =
                {
                    width = 50,
                    display = isEnhanceVisibilityOn ? DisplayStyle.None : DisplayStyle.Flex,
                }
            };

            colorField.RegisterValueChangedCallback(evt =>
            {
                OnColorFieldValueChanged?.Invoke(evt.newValue);
            });
            
            var visibilityEnhancerImage = new Image
            {
                image = isEnhanceVisibilityOn ? enhancedVisibilityEnabledIcon : enhancedVisibilityDisabledIcon 
            };
            
            // ToolbarToggle is the UI Toolkit equivalent of GUILayout.Toggle with toolbarButton
            var toggleVisibilityEnhancer = new ToolbarToggle {
                value = isEnhanceVisibilityOn, 
                style =
                {
                    width = 30,
                }
            };
            
            toggleVisibilityEnhancer.Add(visibilityEnhancerImage);

            toggleVisibilityEnhancer.RegisterValueChangedCallback(evt =>
            {
                OnVisibilityEnhancerToggleChanged(evt.newValue);
            });
            
            var toggleEnable = new ToolbarToggle
            {
                text = "",
                value = isEnable
            };
            
            var enableIcon = new Image
            {
                image = EditorGUIUtility.IconContent("animationvisibilitytoggleon").image
            };
            
            toggleEnable.Add(enableIcon);

            toggleEnable.RegisterValueChangedCallback(evt =>
            {
                OnEnableToggleChanged(evt.newValue);
            });

            toolbar.Add(colorField);
            toolbar.Add(toggleVisibilityEnhancer);
            toolbar.Add(toggleEnable);

            toolbarData.toolbar = toolbar;
            toolbarData.colorField = colorField;
            toolbarData.toggleVisibilityEnhancer = toggleVisibilityEnhancer;
            toolbarData.visibilityEnhancerImage = visibilityEnhancerImage;
            toolbarData.toggleEnable = toggleEnable;
            
            toolbarDataList.Add(toolbarData);

            return toolbarData;
        }
        
        public static void UpdateToolbar(bool isEnable, bool isEnhanceVisibilityOn, Color backgroundColor, 
            Texture2D enhancedVisibilityEnabledIcon, Texture2D enhancedVisibilityDisabledIcon, ToolbarSettings toolbarSettings)
        {
            foreach (var toolbarData in toolbarDataList)
            {
                if (toolbarData == null)
                    return;

                var toolbar = toolbarData.toolbar as Toolbar;
                var toggleEnable = toolbarData.toggleEnable as ToolbarToggle;
                var toggleVisibilityEnhancer = toolbarData.toggleVisibilityEnhancer as ToolbarToggle;
                var colorField = toolbarData.colorField as ColorField;
                var visibilityEnhancerImage = toolbarData.visibilityEnhancerImage as Image;
            
                toolbar.style.display = toolbarSettings.showToolbar ? DisplayStyle.Flex : DisplayStyle.None;
            
                toggleEnable.SetValueWithoutNotify(isEnable);
                colorField.SetValueWithoutNotify(backgroundColor);
                toggleVisibilityEnhancer.SetValueWithoutNotify(isEnhanceVisibilityOn);
                visibilityEnhancerImage.image = isEnhanceVisibilityOn ? enhancedVisibilityEnabledIcon : enhancedVisibilityDisabledIcon;
            
                toggleEnable.style.display = toolbarSettings.showPreviewToggle ? DisplayStyle.Flex : DisplayStyle.None;
                toggleVisibilityEnhancer.style.display = isEnable ? DisplayStyle.Flex : DisplayStyle.None;
                colorField.style.display = (!isEnable || (isEnable && isEnhanceVisibilityOn)) ? DisplayStyle.None : DisplayStyle.Flex;
                
                if (!toolbarSettings.showVisibilityEnhancerToggle)
                    toggleVisibilityEnhancer.style.display = DisplayStyle.None;
                
                if (!toolbarSettings.showBgColorPicker)
                    colorField.style.display = DisplayStyle.None;   
            }
        }
    }
}