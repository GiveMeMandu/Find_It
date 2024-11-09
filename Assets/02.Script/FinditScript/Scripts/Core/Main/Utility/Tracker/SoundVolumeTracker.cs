using DeskCat.FindIt.Scripts.Core.Model;
using UnityEngine;

namespace DeskCat.FindIt.Scripts.Core.Main.Utility.Tracker
{
    public class SoundVolumeTracker : MonoBehaviour
    {
        private AudioSource AudioSource;
        private void Start()
        {
            AudioSource = GetComponent<AudioSource>();
            AudioSource.volume = GlobalSetting.SoundVolume;
            GlobalSetting.SoundChangeAction += ChangeMusic;

        }
        
        private void OnDisable()
        {
            GlobalSetting.SoundChangeAction -= ChangeMusic;
        }

        private void ChangeMusic(float f)
        {
            AudioSource.volume = f;
        }
    }
}