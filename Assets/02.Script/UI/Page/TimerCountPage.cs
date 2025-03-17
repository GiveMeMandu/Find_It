using Manager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityWeld.Binding;

namespace UI.Page
{
    [Binding]
    public class TimerCountPage : PageViewModel
    {
        private TimerCountViewModel _timerViewModel;
        private System.Action _onTimerComplete;

        protected override void Awake()
        {
            base.Awake();
            _timerViewModel = GetComponentInChildren<TimerCountViewModel>();
            _timerViewModel.OnTimerComplete += HandleTimerComplete;
        }

        public void SetTimer(float seconds, System.Action onComplete = null)
        {
            _onTimerComplete = onComplete;
            _timerViewModel.Initialize(seconds);
        }

        private void HandleTimerComplete()
        {
            _onTimerComplete?.Invoke();
            Global.UIManager.ClosePage(this);
        }

        private void OnDestroy()
        {
            if (_timerViewModel != null)
            {
                _timerViewModel.OnTimerComplete -= HandleTimerComplete;
            }
        }
    }
}