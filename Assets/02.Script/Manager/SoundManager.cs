using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using UnityEngine;

namespace Manager
{
    [System.Serializable]
    public class BGMSound{
        public BGMEnum name;
        public AudioClip clip;
    }
    [System.Serializable]
    public class SFXSound{
        public SFXEnum name;
        public AudioClip clip;
    }
    public class SoundManager : MonoBehaviour
    {
        public BGMSound[] bgmArr;
        public SFXSound[] sfxArr;
        public AudioSource bgmSource, sfxSource;

        public void PlayMusic(BGMEnum name, float volume = 0.15f, bool isLoop = true)
        {
            BGMSound s = FindBGM(name, bgmArr);
            if(s == null || s.name == BGMEnum.NONE) return;
            bgmSource.loop = isLoop;
            bgmSource.clip = s.clip;
            bgmSource.volume = volume;
            bgmSource.Play();
        }
        public void PlaySFX(SFXEnum name, float volume = 1)
        {
            SFXSound s = FindSFX(name, sfxArr);
            if(s == null || s.name == SFXEnum.NONE) return;
            bgmSource.PlayOneShot(s.clip, volume);
        }
        public AudioSource PlaySFXInstance(SFXEnum name, float volume = 1, AudioSource audioSource = null)
        {
            SFXSound s = FindSFX(name, sfxArr);
            if(s == null || s.name == SFXEnum.NONE) return null;

            if(audioSource != null)
                audioSource.PlayOneShot(s.clip, volume);
            else
            {
                GameObject instanceSFX = new GameObject("InstanceSFX_" + name);
                instanceSFX.transform.SetParent(transform);
                audioSource = instanceSFX.AddComponent<AudioSource>(); // AudioSource 컴포넌트를 추가하여 audioSource에 할당
                audioSource.clip = s.clip; // 클립을 설정
                audioSource.PlayOneShot(s.clip, volume); // 사운드 재생
            }

            return audioSource;
        }

        public BGMSound FindBGM(BGMEnum name, BGMSound[] soundsArr)
        {
            BGMSound s = Array.Find(bgmArr, x => x.name == name);
            if(s == null)
                Debug.Log("해당 사운드 찾기 실패");
            return s;
        }
        public SFXSound FindSFX(SFXEnum name, SFXSound[] soundsArr)
        {
            SFXSound s = Array.Find(sfxArr, x => x.name == name);
            if(s == null)
                Debug.Log("해당 사운드 찾기 실패");
            return s;
        }
        public bool MuteSFX(bool? muteSFX = null) => sfxSource.mute = muteSFX ?? !sfxSource.mute;
        public bool MuteBGM(bool? muteBGM = null) => bgmSource.mute = muteBGM ?? !bgmSource.mute;


    }
}
