using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DeskCat.FindIt.Scripts.Core.Model
{
    public class MultiLanguageModel
    {
        public string LanguageKey;
    }

    [Serializable]
    public class MultiLanguageTextModel : MultiLanguageModel
    {
        [TextArea]
        public string Value;
    }

    [Serializable]
    public class MultiLanguageImageModel : MultiLanguageModel
    {
        public Sprite Value;
    }
    
    [Serializable]
    public class MultiLanguageTextListModel : MultiLanguageModel
    {
        [TextArea(3,10)] [NonReorderable]
        public List<string> Value;
    }
    
    [Serializable]
    public class MultiDialogTextListModel : MultiLanguageModel
    {
        [NonReorderable]
        public List<DialogListModel> Value;
    }
    
    [Serializable]
    public class DialogListModel
    {
        [NonReorderable]
        public Sprite CharacterSprite;
        
        [TextArea(3,10)] [NonReorderable]
        public string Value;

        public UnityEvent Event;
    }
}