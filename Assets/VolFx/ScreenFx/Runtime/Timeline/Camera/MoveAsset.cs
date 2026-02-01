using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

//  ScreenFx © NullTale - https://x.com/NullTale
namespace ScreenFx
{
    [Serializable]
    public class MoveAsset : PlayableAsset
    {
        public ClipCaps clipCaps => ClipCaps.Blending;
        
        [InplaceField(nameof(CameraBehaviour._offset), nameof(CameraBehaviour._fov), nameof(CameraBehaviour._roll))]
        public CameraBehaviour _template;

        // =======================================================================
        public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
        {
#if UNITY_EDITOR
            #pragma warning disable CS0618
            if (UnityEngine.Object.FindObjectOfType<ScreenFx>() == null)
                Debug.LogWarning($"ScreenFx : the Scene has no CinemachineCamera with ScreenFx component on it");
#endif
            var playable = ScriptPlayable<CameraBehaviour>.Create(graph, _template);

            return playable;
        }
    }
}