using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using Manager;
using UI;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityWeld.Binding;
using SO;

namespace UnityWeld
{
    [Binding]
    public class CollectionBookScrollViewModel : ScrollView
    {
        [SerializeField]
        private PhotoInfoViewModel _photoInfoViewModel;

        private PhotoElementViewModel _selectedElement;

        public void SelectElement(PhotoElementViewModel element)
        {
            if (_selectedElement != null && _selectedElement.selectedObj != null)
            {
                _selectedElement.selectedObj.SetActive(false);
            }
            _selectedElement = element;
            if (_selectedElement != null && _selectedElement.selectedObj != null)
            {
                _selectedElement.selectedObj.SetActive(true);
            }
        }

        public void OnClickShowPhotoInfo(Sprite photoSprite, string photoName)
        {
            if (_photoInfoViewModel != null)
            {
                _photoInfoViewModel.transform.gameObject.SetActive(true);
                _photoInfoViewModel.Show(photoSprite, photoName);
            }
        }

        private string GetDisplayName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return string.Empty;
            // 파일명에서 확장자 제거
            string name = Path.GetFileNameWithoutExtension(fileName);
            // 접두사 'Mission_' 제거
            const string prefix = "Mission_";
            if (name.StartsWith(prefix, StringComparison.Ordinal))
            {
                name = name.Substring(prefix.Length);
            }
            // 뒤에 붙은 날짜/시간 패턴 제거 (예: _20260406 or _20260406_022059)
            name = Regex.Replace(name, "_(?:\\d{8}(?:_\\d{6})?)$", "");
            return name;
        }

        private void OnEnable() {
            // 카메라 연출 등에서 ES3로 저장했던 파일명 목록 불러오기
            var photoList = ES3.KeyExists("SavedPhotos") ? ES3.Load<List<string>>("SavedPhotos") : new List<string>();
            
            PrepareViewModels(photoList.Count);
            var viewModels = GetViewModels();
            
            for(int i = 0; i < viewModels.Count; i++)
            {
                var viewModel = viewModels[i] as PhotoElementViewModel;
                string fileName = photoList[i];

                if (ES3.FileExists(fileName))
                {
                    Texture2D tex = ES3.LoadImage(fileName);
                    // Texture2D를 기반으로 새로운 Sprite 생성
                    Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                    viewModel.Init(this, sprite, GetDisplayName(fileName));
                    
                    if(i == 0)
                    {
                        SelectElement(viewModel);
                        OnClickShowPhotoInfo(sprite, GetDisplayName(fileName));
                    }
                }
            }
        }
    }
}
