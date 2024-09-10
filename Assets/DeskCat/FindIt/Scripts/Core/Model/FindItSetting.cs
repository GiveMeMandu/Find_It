using System.Collections.Generic;
using UnityEngine;

namespace DeskCat.FindIt.Scripts.Core.Model
{
    public class FindItSetting : ScriptableObject
    {
        [Range(0.0f,1.0f)]
        public float DefaultBackgroundMusic = 0.5f;
        
        [Range(0.0f,1.0f)]
        public float DefaultSoundFxMusic = 0.5f;
        
        [Tooltip("The Index Value Of Languages Key Below")]
        public int DefaultLanguagesIndex = 0;
        public List<string> LanguagesKey = new();

        public void OnValidate() {
            GlobalSetting.InitializeSetting();
        }
    }
}