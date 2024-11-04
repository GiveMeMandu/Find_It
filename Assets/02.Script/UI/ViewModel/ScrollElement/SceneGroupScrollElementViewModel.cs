using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityWeld.Binding;
namespace UI
{
    [Binding]
    public class SceneGroupScrollElementViewModel : ScrollElementViewModel
    {
        private string _sceneName;

        [Binding]
        public string SceneName
        {
            get => _sceneName;
            set
            {
                _sceneName = value;
                OnPropertyChanged(nameof(SceneName));
            }
        }
    }
}