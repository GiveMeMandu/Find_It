using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

//  ScreenFx © NullTale - https://x.com/NullTale
namespace ScreenFx
{
    [TrackColor(0.09803922f, 0.09803922f, 0.09803922f)]
    [TrackClipType(typeof(ScreenAsset))]
    public class ScreenTrack : TrackAsset
    {
        public RenderMode _renderMode   = RenderMode.ScreenSpaceCamera;
        public int        _sortingOrder = 10000;
        [Tooltip("Alpha multiplayer for track assets, to control general flashes intensity")]
        public float      _planeDist    = 10f;
        [Tooltip("Alpha multiplayer for track assets, to control general flashes intensity")]
        [Range(0, 1)]
        public float      _alphaMul = 1f;
        [Layer]
        public int        _layer;
        
        // =======================================================================
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var mixerTrack = ScriptPlayable<ScreenMixer>.Create(graph, inputCount);
            
            var mixer  = mixerTrack.GetBehaviour();
            mixer._sortingOrder = _sortingOrder;
            mixer._renderMode   = _renderMode;
            mixer._weight       = _alphaMul;
            mixer._layer        = _layer;
            mixer._planeDist    = _planeDist;
            mixer._name         = name;

            return mixerTrack;
        }
    }
}