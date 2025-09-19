using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using UnityWeld;
using UnityWeld.Binding;
using UI.Page;
using Manager;

[Binding]
public class TutorialViewModel : ViewModel
{
    [System.Serializable]
    public class TutorialPage
    {
        public Sprite Image;
        public string InfoText;
    }

    [SerializeField]
    private List<TutorialPage> _tutorialPages = new List<TutorialPage>();

    public List<TutorialPage> TutorialPages
    {
        get => _tutorialPages;
        set
        {
            _tutorialPages = value;
            OnPropertyChanged(nameof(TutorialPages));
        }
    }

    private Sprite _currentImage;

    [Binding]
    public Sprite CurrentImage
    {
        get => _currentImage;
        set
        {
            _currentImage = value;
            OnPropertyChanged(nameof(CurrentImage));
        }
    }

    private int _currentIndex;

    [Binding]
    public int CurrentIndex
    {
        get => _currentIndex;
        set
        {
            _currentIndex = value;
            OnPropertyChanged(nameof(CurrentIndex));
        }
    }

    private string _nextButtonText;
    [Binding]
    public string NextButtonText
    {
        get => _nextButtonText;
        set
        {
            _nextButtonText = value;
            OnPropertyChanged(nameof(NextButtonText));
        }
    }

    private string _curInfoText;
    [Binding]
    public string CurInfoText
    {
        get => _curInfoText;
        set
        {
            _curInfoText = value;
            OnPropertyChanged(nameof(CurInfoText));
        }
    }

    private CancellationTokenSource _cts;
    public void OnEnable()
    {
        _cts = new CancellationTokenSource();
        Global.InputManager.DisableGameInputOnly();
        
        CurrentIndex = 0;
        if (_tutorialPages != null && _tutorialPages.Count > 0)
        {
            CurrentImage = _tutorialPages[0].Image;
            CurInfoText = _tutorialPages[0].InfoText;
        }
        else
        {
            CurrentImage = null;
            CurInfoText = string.Empty;
        }
        NextButtonText = "다음";
    }

    [Binding]
    public void Next()
    {
        if (_tutorialPages == null || _tutorialPages.Count == 0)
        {
            NextButtonText = "네!!!";
            Skip();
            return;
        }

        if (CurrentIndex < _tutorialPages.Count - 1)
        {
            CurrentIndex++;
            CurrentImage = _tutorialPages[CurrentIndex].Image;
            CurInfoText = _tutorialPages[CurrentIndex].InfoText;
            if (CurrentIndex == _tutorialPages.Count - 1)
            {
                NextButtonText = "네!!!";
            }
        }
        else
        {
            Skip();
        }
    }

    [Binding]
    public void Previous()
    {
        if (_tutorialPages == null || _tutorialPages.Count == 0) return;

        if (CurrentIndex > 0)
        {
            CurrentIndex--;
            CurrentImage = _tutorialPages[CurrentIndex].Image;
            CurInfoText = _tutorialPages[CurrentIndex].InfoText;
        }
    }

    [Binding]
    public void Skip()
    {
        Global.InputManager.EnableGameInputOnly();
        GetComponentInParent<InGameTutorialPage>().ClosePage();
    }

    private void OnDisable()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        Global.InputManager.EnableGameInputOnly();
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }
}
