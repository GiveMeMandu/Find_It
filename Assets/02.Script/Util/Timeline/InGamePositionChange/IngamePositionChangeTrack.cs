using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.8f, 0.4f, 0.2f)] // 주황색 계열
[TrackClipType(typeof(위치변경클립))]
public class 위치변경트랙 : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<IngamePositionChangeMixerBehaviour>.Create(graph, inputCount);
    }
} 