using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.2f, 0.8f, 0.4f)] // 녹색 계열
[TrackClipType(typeof(위치보정클립))]
public class 위치보정트랙 : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<IngamePositionCorrectionMixerBehaviour>.Create(graph, inputCount);
    }
} 