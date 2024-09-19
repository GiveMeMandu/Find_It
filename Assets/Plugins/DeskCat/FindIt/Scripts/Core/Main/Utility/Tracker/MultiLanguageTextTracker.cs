using System;
using System.Collections.Generic;
using DeskCat.FindIt.Scripts.Core.Model;
using UnityEngine;
using UnityEngine.UI;

namespace DeskCat.FindIt.Scripts.Core.Main.Utility.Tracker
{
    
    public class MultiLanguageTextTracker : MonoBehaviour
    {
        [NonReorderable] 
        public List<MultiLanguageTextModel> LanguageValue;

        private Dictionary<string, string> LanguageDic;
        private Text TargetText;
        private void Start()
        {
            TargetText = GetComponent<Text>();
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
            TargetText.text = LanguageDic[languageKey];
        }

        private void BuildDictionary()
        {
            LanguageDic = new Dictionary<string, string>();
            foreach (var languageModel in LanguageValue)
            {
                LanguageDic.TryAdd(languageModel.LanguageKey, languageModel.Value);
            }
        }
    }
}