using System;
using System.Collections.Generic;
using UnityEngine;

namespace DeskCat.FindIt.Scripts.Core.Model
{
    public static class GlobalSetting
    {
        public static float MusicVolume = 1;
        public static float SoundVolume = 1;
        public static string CurrentLanguage = "English";

        public static Action<float> MusicChangeAction;
        public static Action<float> SoundChangeAction;
        public static Action<string> LanguageChangeAction;

        private static int currentLanguageIndex;
        private static List<string> languageKeyList;
        private static FindItSetting DefaultFindItSetting;
        public static readonly string GlobalFindItSettingPath = "FindItSetting/DefaultFindItSetting";

        public static void InitializeSetting()
        {
            if (languageKeyList != null) return;
            
            DefaultFindItSetting = (FindItSetting)Resources.Load(GlobalFindItSettingPath);
            MusicVolume = DefaultFindItSetting.DefaultBackgroundMusic;
            SoundVolume = DefaultFindItSetting.DefaultSoundFxMusic;
            currentLanguageIndex = DefaultFindItSetting.DefaultLanguagesIndex;
            languageKeyList = DefaultFindItSetting.LanguagesKey;
            CurrentLanguage = DefaultFindItSetting.LanguagesKey[currentLanguageIndex];
        }

        public static void ChangeMusicVolume(float volume)
        {
            if(languageKeyList==null) 
                InitializeSetting();
            
            MusicVolume = volume;
            MusicChangeAction?.Invoke(volume);
        }

        public static void ChangeSoundVolume(float volume)
        {
            if(languageKeyList==null) 
                InitializeSetting();
            
            SoundVolume = volume;
            SoundChangeAction?.Invoke(volume);
        }

        public static void NextLanguage()
        {
            if(languageKeyList==null) 
                InitializeSetting();

            currentLanguageIndex++;
            if (currentLanguageIndex >= languageKeyList.Count)
            {
                currentLanguageIndex = 0;
            }
            ChangeCurrentLanguage(languageKeyList[currentLanguageIndex]);
        }
        
        public static void PrevLanguage()
        {
            if(languageKeyList==null) 
                InitializeSetting();

            currentLanguageIndex--;
            if (currentLanguageIndex < 0 )
            {
                currentLanguageIndex = languageKeyList.Count -1;
            }
            ChangeCurrentLanguage(languageKeyList[currentLanguageIndex]);
        }

        public static void ChangeCurrentLanguage(string language)
        {
            if(languageKeyList==null) 
                InitializeSetting();
            
            CurrentLanguage = language;
            LanguageChangeAction?.Invoke(language);
        }

        public static List<TModel> GetDefaultLanguageKey<TModel>() where TModel : MultiLanguageModel, new()
        {
            var multiLanguageTextModels = new List<TModel>();
            
            if(languageKeyList==null) 
                InitializeSetting();
            
            foreach (var language in languageKeyList)
            {
                multiLanguageTextModels.Add(new TModel()
                {
                    LanguageKey = language
                });
            }

            return multiLanguageTextModels;
        }
    }
}