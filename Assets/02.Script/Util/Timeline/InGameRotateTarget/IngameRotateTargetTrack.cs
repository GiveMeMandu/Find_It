using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.8f, 0.4f, 0.2f)] // 주황색 계열
[TrackClipType(typeof(IngameRotateTargetClip))]
public class IngameRotateTargetTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<IngameRotateTargetMixerBehaviour>.Create(graph, inputCount);
    }
} 