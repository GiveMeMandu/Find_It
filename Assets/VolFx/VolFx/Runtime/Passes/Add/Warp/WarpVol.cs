using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  VolFx © NullTale - https://x.com/NullTale
namespace VolFx
{
	[Serializable, VolumeComponentMenu("VolFx/Warp (Speedlines)")]
    public sealed class WarpVol : VolumeComponent, IPostProcessComponent
    {
        [Tooltip("Blend amount of effect")]
        public ClampedFloatParameter _intensity = new ClampedFloatParameter(0f, 0f, 1f);
		[Tooltip("Color distortion strength")]
        public ClampedFloatParameter _depth = new ClampedFloatParameter(2f, 0f, 2f);
		[Tooltip("Gradient used for light streak coloring")]
        public GradientParameter _color = new GradientParameter(new GradientValue(new Gradient()), false);
		[Tooltip("Extra tint added to final color")]
        public ColorParameter _emission = new ColorParameter(Color.clear);
		[Tooltip("Image warping strength")]
        public ClampedFloatParameter _distort = new ClampedFloatParameter(0f, 0f, 1f);

        [Tooltip("Radius of visible area")]
        public NoInterpClampedFloatParameter _size = new NoInterpClampedFloatParameter(.7f, 0f, 10f);
		[Tooltip("Edge falloff softness")]
        public NoInterpClampedFloatParameter _hardness = new NoInterpClampedFloatParameter(0.583f, -2f, 1f);
		[Tooltip("Remap threshold of noise")]
        public NoInterpClampedFloatParameter _count = new NoInterpClampedFloatParameter(.5f, 0f, 1f);
		[Tooltip("Noise texture tiling")]
        public NoInterpClampedFloatParameter _density = new NoInterpClampedFloatParameter(2.3f, 0f, 17f);
		[Tooltip("Scrolling speed of effect")]
        public NoInterpClampedFloatParameter _speed = new NoInterpClampedFloatParameter(3f, -7f, 7f);

        // =======================================================================
        public bool IsActive() => active && (_intensity.value > 0f || _distort.value > 0f);

        public bool IsTileCompatible() => true;
    }
}