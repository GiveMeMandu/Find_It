using System;
using UnityEngine;
using UnityEngine.Playables;

//  ScreenFx © NullTale - https://x.com/NullTale
namespace ScreenFx
{
    [Serializable]
    public class NoiseAsset : PlayableAsset
    {
        [InplaceField(nameof(NoiseBehaviour._freq), nameof(NoiseBehaviour._move), nameof(NoiseBehaviour._torque), nameof(NoiseBehaviour._noise))]
        public NoiseBehaviour _template;

        // =======================================================================
        public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
        {
            var playable = ScriptPlayable<NoiseBehaviour>.Create(graph, _template);

            return playable;
        }
    }
}