using Data;
using Manager;
using UI.Effect;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityWeld.Binding;

namespace UI.Page
{
    [Binding]
    public class PuzzleInGamePage : PageViewModel
    {
        private PuzzleGameViewModel puzzleGameViewModel;

        private void OnEnable()
        {
            puzzleGameViewModel = GetComponentInChildren<PuzzleGameViewModel>();
            puzzleGameViewModel.Initialize(PuzzleGameManager.Instance.CurrentPuzzleData);
        }

        [Binding]
        public void OnClickOptionButton()
        {
            Global.SoundManager.PlaySFX(SFXEnum.ClickUI);
            Global.UIManager.OpenPage<OptionPage>();
        }

        [Binding]
        public void OnClickExitButton()
        {
            Global.SoundManager.PlaySFX(SFXEnum.ClickUI);
            // 게임 종료 전에 필요한 정리 작업 수행
            Global.UIManager.OpenPage<PuzzlePage>();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }
    }
}