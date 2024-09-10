using System;
using DeskCat.FindIt.Scripts.Core.Model;
using UnityEngine;

namespace DeskCat.FindIt.Scripts.Core.Main.Utility.Region
{
    [RequireComponent(typeof(AudioSource))]
    public class MusicRegion : MonoBehaviour
    {
        private AudioSource AudioSource;

        private void Awake()
        {
            AudioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            AudioSource.volume = GlobalSetting.SoundVolume;


            GlobalSetting.SoundChangeAction += OnSoundChangeAction;
        }

        private void OnDestroy()
        {
            GlobalSetting.SoundChangeAction -= OnSoundChangeAction;
        }
        
        private void OnSoundChangeAction(float f) => AudioSource.volume = f;

        private void OnMouseDown()
        {
            AudioSource.Play();
        }
    }
}