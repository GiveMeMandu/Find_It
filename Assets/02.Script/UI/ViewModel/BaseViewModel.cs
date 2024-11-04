using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityWeld.Binding;

namespace UI
{
    /// <summary>
    /// 비즈니스 로직과 UI 로직을 분리하기 위한 기본 클래스입니다.
    /// UnityWeld를 사용하여 데이터 바인딩을 지원합니다.
    /// 참고 1: https://www.what-could-possibly-go-wrong.com/bringing-mvvm-to-unity-part-1-about-mvvm-and-unity-weld
    /// 참고 2: https://learn.microsoft.com/ko-kr/windows/uwp/data-binding/data-binding-and-mvvm
    /// </summary>
    [Binding]
    [HelpURL("https://www.what-could-possibly-go-wrong.com/bringing-mvvm-to-unity-part-1-about-mvvm-and-unity-weld")]
    public class BaseViewModel : MonoBehaviour, INotifyPropertyChanged
    {
        /// <summary>
        /// Event to raise when a property's value has changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">Name of the property that has changed.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected virtual void Awake()
        {
            ViewModelTypeName = GetType().Name;
            GameObjectName = gameObject.name;
        }

        private string _viewModelTypeName;

        [Binding]
        public string ViewModelTypeName
        {
            get => _viewModelTypeName;
            set
            {
                _viewModelTypeName = value;
                OnPropertyChanged(nameof(ViewModelTypeName));
            }
        }

        private string _gameObjectName;

        [Binding]
        public string GameObjectName
        {
            get => _gameObjectName;
            set
            {
                _gameObjectName = value;
                OnPropertyChanged(nameof(GameObjectName));
            }
        }

        
    }
}
