using UnityEngine;

namespace SnowRabbit
{
    /// <summary>
    /// Legacy Animation을 Time.timeScale의 영향 없이 재생하는 컴포넌트
    /// </summary>
    public class UnscaledAnimationPlayer : MonoBehaviour
    {
        private Animation anim;
        private AnimationState state;

        private void Awake()
        {
            anim = GetComponent<Animation>();
            anim.playAutomatically = false;
            
            // 0번째 애니메이션 가져오기
            foreach (AnimationState s in anim)
            {
                state = s;
                break;
            }
            
            if (state != null)
            {
                state.speed = 0f; // 자동 재생 방지
            }
        }

        private void OnEnable()
        {
            if (state != null)
            {
                state.time = 0f;
                anim.Play();
                anim.Sample();
            }
        }

        private void Update()
        {
            if (state == null) return;

            state.time += Time.unscaledDeltaTime;
            anim.Sample();
        }
    }
}
