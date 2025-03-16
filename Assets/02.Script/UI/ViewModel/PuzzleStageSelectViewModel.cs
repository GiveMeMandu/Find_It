using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityWeld.Binding;
using Data;
using UnityWeld;

namespace UI
{
    [Binding]
    public class PuzzleStageSelectViewModel : ViewModel
    {
        private string _stageName;
        private int _stageIndex;
        private SceneName _sceneName;
        private Sprite _stageImage;

        [Binding]
        public string StageName
        {
            get => _stageName;
            set
            {
                _stageName = value;
                OnPropertyChanged(nameof(StageName));
            }
        }

        [Binding]
        public Sprite StageImage
        {
            get => _stageImage;
            set
            {
                _stageImage = value;
                OnPropertyChanged(nameof(StageImage));
            }
        }

        [Binding]
        public int StageIndex
        {
            get => _stageIndex;
            set
            {
                _stageIndex = value;
                OnPropertyChanged(nameof(StageIndex));
            }
        }

        [Binding]
        public SceneName SceneName
        {
            get => _sceneName;
            set
            {
                _sceneName = value;
                OnPropertyChanged(nameof(SceneName));
            }
        }

        [Binding]
        public void Initialize(string stageName, int stageIndex, SceneName sceneName, Sprite stageImage)
        {
            StageName = stageName;
            StageIndex = stageIndex;
            SceneName = sceneName;
            StageImage = stageImage;
            
        }

        [Binding]
        public void OnClickStage()
        {
            // 스테이지 선택 시 PuzzleLevelSelectView에 알림
            PuzzleLevelSelectView.Instance.SelectStage(_sceneName, _stageIndex);
        }
    }
}
