using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace VoxelLabs.UltimateThumbnails.Lib
{
    [Serializable]
    public class ToolbarSettings
    {
        [Tooltip("Shows or hides the entire toolbar.")]
        public bool showToolbar = true;
        
        [Tooltip("Shows or hides the toggle button for enabling/disabling Ultimate Thumbnails' custom icons.")]
        public bool showPreviewToggle = true;
        
        [Tooltip("Shows or hides the toggle button for enhancing visibility.")]
        public bool showVisibilityEnhancerToggle = true;
        
        [Tooltip("Shows or hides the background color picker tool on the toolbar.")]
        public bool showBgColorPicker = true;
    }
}