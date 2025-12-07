using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System;
namespace UI.Effect
{
    public abstract class UIDotweenEffect : MonoBehaviour
    {
        protected List<Sequence> activeSequences = new List<Sequence>();
        public event Action onAnimationComplete;
        private int completedSequences = 0;

        protected virtual void OnEnable()
        {
            // 자식 클래스에서 구현할 수 있도록 virtual로 선언
            completedSequences = 0;
        }

        protected virtual void OnDisable()
        {
            KillAllSequences();
        }

        protected virtual void OnDestroy()
        {
            KillAllSequences();
        }

        /// <summary>
        /// 새로운 시퀀스를 생성하고 관리 목록에 추가
        /// </summary>
        protected Sequence CreateSequence()
        {
            var sequence = DOTween.Sequence();
            sequence.SetLink(gameObject); // GameObject와 시퀀스 연결
            
            // 시퀀스 완료 콜백 추가
            sequence.OnComplete(() => {
                completedSequences++;
                if (completedSequences >= activeSequences.Count)
                {
                    // 모든 시퀀스가 완료되면 이벤트 발생
                    onAnimationComplete?.Invoke();
                }
            });
            
            activeSequences.Add(sequence);
            return sequence;
        }

        /// <summary>
        /// 특정 시퀀스를 종료하고 목록에서 제거
        /// </summary>
        protected void KillSequence(Sequence sequence)
        {
            if (sequence != null && sequence.IsActive())
            {
                sequence.Kill();
                activeSequences.Remove(sequence);
            }
        }

        /// <summary>
        /// 모든 활성 시퀀스 종료
        /// </summary>
        protected void KillAllSequences()
        {
            foreach (var sequence in activeSequences)
            {
                if (sequence != null && sequence.IsActive())
                {
                    sequence.Kill();
                }
            }
            activeSequences.Clear();
            completedSequences = 0;
        }
    }
}
