using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace InGame
{
    [RequireComponent(typeof(AudioSource))]
    public class FoundObj : MonoBehaviour
    {
        [LabelText("플레이할 오디오")]
        public UnityEvent OnPlayAudio;
        [LabelText("플레이할 오디오2")]
        public UnityEvent OnPlayAudio2;

        public virtual void PlayAudio()
        {
            OnPlayAudio.Invoke();
        }
        public virtual void PlayAudio2()
        {
            OnPlayAudio2.Invoke();
        }
    }
}