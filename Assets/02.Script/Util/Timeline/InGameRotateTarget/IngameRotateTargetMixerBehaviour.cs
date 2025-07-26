using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class IngameRotateTargetMixerBehaviour : PlayableBehaviour
{
    // NOTE: This function is called at runtime and edit time. Keep that in mind when setting the values of properties.
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        int inputCount = playable.GetInputCount();

        for (int i = 0; i < inputCount; i++)
        {
            float inputWeight = playable.GetInputWeight(i);
            ScriptPlayable<IngameRotateTargetBehaviour> inputPlayable = (ScriptPlayable<IngameRotateTargetBehaviour>)playable.GetInput(i);
            IngameRotateTargetBehaviour input = inputPlayable.GetBehaviour();

            // 가중치가 0보다 클 때만 처리
            if (inputWeight > 0.0f)
            {
                // 각 클립의 ProcessFrame이 자동으로 호출되므로 여기서는 추가 로직만 처리
                // 필요시 디버그 로그나 추가 기능을 여기에 구현
                
                // 예: 회전 중인 오브젝트 정보 로깅 (디버그용)
                // if (input.rotateTarget != null)
                // {
                //     Debug.Log($"Rotating {input.rotateTarget.name} with weight: {inputWeight}");
                // }
            }
        }
    }
    
    public override void OnGraphStart(Playable playable)
    {
        // 그래프 시작 시 초기화 로직
        // Debug.Log("IngameRotateTargetMixerBehaviour OnGraphStart");
    }
    
    public override void OnGraphStop(Playable playable)
    {
        // 그래프 종료 시 정리 로직
        // Debug.Log("IngameRotateTargetMixerBehaviour OnGraphStop");
    }
} 