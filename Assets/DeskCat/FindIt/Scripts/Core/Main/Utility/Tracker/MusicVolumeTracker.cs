using DeskCat.FindIt.Scripts.Core.Model;
using UnityEngine;

namespace DeskCat.FindIt.Scripts.Core.Main.Utility.Tracker
{
    public class MusicVolumeTracker : MonoBehaviour
    {
        private AudioSource AudioSource;

        private void Start()
        {
            AudioSource = GetComponent<AudioSource>();
            AudioSource.volume = GlobalSetting.MusicVolume;
            GlobalSetting.MusicChangeAction += ChangeMusic;
        }

        private void OnDisable()
        {
            GlobalSetting.MusicChangeAction -= ChangeMusic;
        }

        private void ChangeMusic(float f)
        {
            AudioSource.volume = f;
        }
    }
}