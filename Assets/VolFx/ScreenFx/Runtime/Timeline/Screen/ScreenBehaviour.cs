using System;
using UnityEngine;
using UnityEngine.Playables;

//  ScreenFx © NullTale - https://x.com/NullTale
namespace ScreenFx
{
    [Serializable]
    public class ScreenBehaviour : PlayableBehaviour
    {
        [Tooltip("Overlay color (tinted with sprite)")]
        public Color _color = Color.white;

        [Tooltip("Source of the sprite")]
        public SpriteSource _sprite;

        [Tooltip("Gradient for procedural sprites (used in GradHor / GradVert / GradCircle)")]
        public Gradient _gradient;

        [Tooltip("Custom sprite (used when Source = Image)")]
        public Sprite _image;

        [Tooltip("Scale of the overlay on screen")]
        public float _scale = 1f;
 
        // =======================================================================
        [Serializable]
        public enum SpriteSource
        {
            [Tooltip("Solid white pixel (used as base color)")]
            Default,
            [Tooltip("Custom sprite texture")]
            Image,
            [Tooltip("Takes a runtime screenshot of the screen")]
            ScreenShot,
            [Tooltip("Vertical gradient fill")]
            GradVert,
            [Tooltip("Horizontal gradient fill")]
            GradHor,
            [Tooltip("Radial (circular) gradient fill")]
            GradCircle,
        }
    }
}