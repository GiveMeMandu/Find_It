using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UI.Effect;
namespace Manager
{
    public class UIEffectManager : MonoBehaviour
    {

        public EyeBlinkScreen EyeBlinkScreen => eyeBlinkScreen;
        public LetterBox LetterBox => letterBox;
        public bool IsEffectPlaying => _isEffectPlaying || 
            (eyeBlinkScreen != null && eyeBlinkScreen.IsPlaying) || 
            (letterBox != null && letterBox.IsPlaying);

        public BlurController BlurController => blurController;
        private bool _isEffectPlaying;

        [SerializeField] private RectTransform _Fader;
        [SerializeField] private FadeScript _WhiteOut;
        [SerializeField] private EyeBlinkScreen eyeBlinkScreen;
        [SerializeField] private LetterBox letterBox;
        [SerializeField] private BlurController blurController;

        private CancellationTokenSource _fadeToken;

        void Awake()
        {
            _Fader.parent.gameObject.SetActive(false);
            _Fader.localScale = Vector3.one * 15;
        }

        public async UniTask FadeOut()
        {
            _fadeToken?.Cancel();
            _fadeToken = new CancellationTokenSource();
            _isEffectPlaying = true;
            _Fader.parent.gameObject.SetActive(true);
            _Fader.localScale = Vector3.one * 15;
            await _Fader.DOScale(Vector3.zero, 0.5f)
            .SetEase(Ease.Linear)
            .WithCancellation(_fadeToken.Token);
            _isEffectPlaying = false;
            if (_fadeToken.IsCancellationRequested)
            {
                return;
            }
        }

        public async UniTask FadeIn()
        {
            _fadeToken?.Cancel();
            _fadeToken = new CancellationTokenSource();
            _isEffectPlaying = true;
            _Fader.parent.gameObject.SetActive(true);
            _Fader.localScale = Vector3.zero;
            await _Fader.DOScale(Vector3.one * 15, 0.5f)
            .SetEase(Ease.Linear)
            .WithCancellation(_fadeToken.Token);
            _Fader.parent.gameObject.SetActive(false);
            _isEffectPlaying = false;
        }

        public async UniTask WhiteOut(float duration = 1)
        {
            if (_WhiteOut == null) return;

            _WhiteOut.SetFadeColor(Color.white);
            await _WhiteOut.FadeOutAsync(duration);
            _isEffectPlaying = false;
        }

        public async UniTask WhiteIn(float duration = 1)
        {
            if (_WhiteOut == null) return;

            _isEffectPlaying = true;
            _WhiteOut.SetFadeColor(Color.white);
            await _WhiteOut.FadeInAsync(duration);
        }

        public async UniTask BlackOut(float duration = 1)
        {
            if (_WhiteOut == null) return;

            _WhiteOut.SetFadeColor(Color.black);
            await _WhiteOut.FadeOutAsync(duration);
            _isEffectPlaying = false;
        }

        public async UniTask BlackIn(float duration = 1)
        {
            if (_WhiteOut == null) return;

            _isEffectPlaying = true;
            _WhiteOut.SetFadeColor(Color.black);
            await _WhiteOut.FadeInAsync(duration);
        }

        public void CancelFade()
        {
            _fadeToken?.Cancel();
            _fadeToken = new CancellationTokenSource();
            _isEffectPlaying = false;
        }
    }
}
