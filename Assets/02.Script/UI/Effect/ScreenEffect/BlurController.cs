using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using VolFx;

namespace UI.Effect
{
    public class BlurController : MonoBehaviour
    {
        public Volume Volume;

        [Range(0f, 1f)]
        public float Radius;
        [Range(0f, 1f)]
        public float Radial;
        [Range(3, 18)]
        public int Samples = 9;

        private BlurVol _blurVol;

        // 블러 적용 시 Radial 값을 강제로 0으로 유지하기 위한 플래그
        private bool _enforceZeroRadial = false;

        [SerializeField]
        // 0: Radius, 1: Radial, 2: Samples
        private bool[] useField = new bool[3];

        private void Start()
        {
            TryFindVolume();
        }

        private bool TryFindVolume()
        {
            VolumeProfile profile;

            if (Volume)
            {
                profile = Volume.profile;
                return profile != null && profile.TryGet(out _blurVol);
            }

            Volume = FindFirstObjectByType<Volume>(FindObjectsInactive.Exclude);
            if (Volume)
            {
                profile = Volume.profile;
                return profile != null && profile.TryGet(out _blurVol);
            }

            // Volume이 없다면 생성
            Debug.Log($"<color=red>Volume이 존재하지 않습니다. {gameObject.name}에 Volume 컴포넌트를 추가합니다.</color>");
            Volume = gameObject.AddComponent<Volume>();
            profile = Volume.profile;
            return profile != null && profile.TryGet(out _blurVol);
        }

        public void SetRadius(float value)
        {
            if (TryFindVolume())
                _blurVol.m_Radius.Override(value);
        }

        public void SetRadial(float value)
        {
            if (TryFindVolume())
                _blurVol.m_Radial.Override(value);
        }

        public void SetSamples(int value)
        {
            if (TryFindVolume())
                _blurVol.m_Samples.Override(value);
        }

        private void FixedUpdate()
        {
            if (TryFindVolume())
            {
                if (useField[0])
                    SetRadius(Radius);
                // Radial을 강제로 0으로 유지해야할 경우 우선 적용
                if (_enforceZeroRadial)
                    SetRadial(0f);
                else if (useField[1])
                    SetRadial(Radial);
                if (useField[2])
                    SetSamples(Samples);
            }
        }

        public void ResetBlur()
        {
            SetRadius(0f);
            SetRadial(0f);
        }

        // 블러를 간단히 켜는 메서드
        // 객체가 비활성화되어 있어도 외부에서 호출될 수 있으므로
        // 먼저 게임오브젝트를 활성화한 뒤 블러 값을 적용합니다.
        public void TurnOnBlur(float targetRadius = .11f)
        {
            Volume.gameObject.SetActive(true);
            
            if (!TryFindVolume()) return;
            // 블러 적용시 Radial 값은 0으로 유지
            _enforceZeroRadial = true;
            SetRadius(targetRadius);
            SetRadial(0f);
        }

        // 블러를 간단히 끄는 메서드
        // 블러 값을 리셋한 뒤 게임오브젝트를 비활성화합니다.
        public void TurnOffBlur()
        {
            // 강제 Radial 유지 해제 후 리셋하고 비활성화
            _enforceZeroRadial = false;
            if (TryFindVolume())
                ResetBlur();
            Volume.gameObject.SetActive(false);
        }

        // 블러 상태를 토글하는 메서드
        public void ToggleBlur(bool enabled, float targetRadius = .11f)
        {
            if (enabled) TurnOnBlur(targetRadius);
            else TurnOffBlur();
        }

        public async UniTaskVoid BlurFadeIn(float duration, float targetRadius = .11f, CancellationToken cancellationToken = default)
        {
            // 활성화 후 페이드인 진행
            Volume.gameObject.SetActive(true);
            if (!TryFindVolume()) return;

            // 페이드인 중에는 Radial을 0으로 유지
            _enforceZeroRadial = true;
            SetRadial(0f);

            float startRadius = _blurVol.m_Radius.value;
            float time = 0f;

            while (time < duration)
            {
                cancellationToken.ThrowIfCancellationRequested();
                time += Time.unscaledDeltaTime;
                float t = time / duration;
                SetRadius(Mathf.Lerp(startRadius, targetRadius, t));
                await UniTask.Yield();
            }
            SetRadius(targetRadius);
        }

        public async UniTaskVoid BlurFadeOut(float duration, CancellationToken cancellationToken = default)
        {
            if (!TryFindVolume()) return;

            float startRadius = _blurVol.m_Radius.value;
            float time = 0f;

            while (time < duration)
            {
                cancellationToken.ThrowIfCancellationRequested();
                time += Time.unscaledDeltaTime;
                float t = time / duration;
                SetRadius(Mathf.Lerp(startRadius, 0f, t));
                await UniTask.Yield();
            }
            SetRadius(0f);
            // 페이드아웃 완료 후 강제 Radial 해제 및 게임오브젝트 비활성화
            _enforceZeroRadial = false;
            Volume.gameObject.SetActive(false);
        }
    }
}
