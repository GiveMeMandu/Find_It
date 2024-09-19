using System;
using System.Collections.Generic;
using DeskCat.FindIt.Scripts.Core.Model;
using UnityEngine;
using UnityEngine.UI;

namespace DeskCat.FindIt.Scripts.Core.Main.Utility.Tracker
{
    public class MultiLanguageImageTracker : MonoBehaviour
    {
        [NonReorderable] 
        public List<MultiLanguageImageModel> LanguageValue;

        private Dictionary<string, Sprite> LanguageDic;
        private Image TargetImage;
        private void Start()
        {
            TargetImage = GetComponent<Image>();
            BuildDictionary();
            OverwriteTextData(GlobalSetting.CurrentLanguage);
        }

        private void OnEnable()
        {
            GlobalSetting.LanguageChangeAction += OverwriteTextData;
        }

        private void OnDisable()
        {
            GlobalSetting.LanguageChangeAction -= OverwriteTextData;
        }

        private void OverwriteTextData(string languageKey)
        {
            if (!LanguageDic.ContainsKey(languageKey))
            {
                Debug.Log(gameObject.name + " not contain" +languageKey);
                return;
            }
            TargetImage.sprite = LanguageDic[languageKey];
        }

        private void BuildDictionary()
        {
            LanguageDic = new Dictionary<string, Sprite>();
            foreach (var languageModel in LanguageValue)
            {
                LanguageDic.TryAdd(languageModel.LanguageKey, languageModel.Value);
            }
        }
    }
}