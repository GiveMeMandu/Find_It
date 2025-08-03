using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.8f, 0.4f, 0.8f)] // 보라색 계열
[TrackClipType(typeof(파티클재생클립))]
public class 파티클재생트랙 : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<IngameParticlePlayMixerBehaviour>.Create(graph, inputCount);
    }
} 