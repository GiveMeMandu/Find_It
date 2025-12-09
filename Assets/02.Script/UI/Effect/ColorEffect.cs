using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Effect
{
    public class ColorEffect : ChainEffect
    {
        [BoxGroup("컬러 이펙트 설정")][LabelText("타깃 색상")][SerializeField] private Color targetColor = Color.white;
        [BoxGroup("컬러 이펙트 설정")][LabelText("변환 시간")][SerializeField] private float duration = 1f;
        [BoxGroup("컬러 이펙트 설정")][LabelText("Ease 타입")][SerializeField] private Ease easeType = Ease.Linear;
        [BoxGroup("컬러 이펙트 설정")][LabelText("자동 시작")][SerializeField] private bool autoStart = true;

        private Color startColor = Color.white;
        private bool isColoring = false;
        private Sequence colorSequence;

        // target components
        private Graphic uiGraphic;
        private SpriteRenderer spriteRenderer;
        private Renderer meshRenderer;

        protected override void Start()
        {
            base.Start();
            Initialize();

            if (autoStart)
            {
                StartColor();
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            StopColor();
        }

        private void Initialize()
        {
            uiGraphic = GetComponent<Graphic>();
            if (uiGraphic != null)
            {
                startColor = uiGraphic.color;
                return;
            }

            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                startColor = spriteRenderer.color;
                return;
            }

            meshRenderer = GetComponent<Renderer>();
            if (meshRenderer != null && meshRenderer.material != null)
            {
                startColor = meshRenderer.material.color;
                return;
            }

            startColor = Color.white;
        }

        [Button("컬러 변경 시작")]
        public void StartColor()
        {
            if (isColoring) return;
            isColoring = true;
            StartColorTask().Forget();
        }

        [Button("컬러 변경 정지")]
        public void StopColor()
        {
            isColoring = false;
            destroyCancellation.Cancel();
            colorSequence?.Kill();
        }

        private async UniTaskVoid StartColorTask()
        {
            while (isColoring)
            {
                if (isUIEffect)
                {
                    await ColorUITask();
                }
                else
                {
                    await ColorGameObjectTask();
                }

                await UniTask.Yield(destroyCancellation.Token);
            }
        }

        private async UniTask ColorGameObjectTask()
        {
            destroyCancellation.Cancel();
            destroyCancellation = new CancellationTokenSource();

            try
            {
                if (spriteRenderer != null)
                {
                    await spriteRenderer.DOColor(targetColor, duration).SetEase(easeType).WithCancellation(destroyCancellation.Token);
                    await spriteRenderer.DOColor(startColor, duration).SetEase(easeType).WithCancellation(destroyCancellation.Token);
                }
                else if (meshRenderer != null && meshRenderer.material != null)
                {
                    var mat = meshRenderer.material;
                    await mat.DOColor(targetColor, "_Color", duration).SetEase(easeType).WithCancellation(destroyCancellation.Token);
                    await mat.DOColor(startColor, "_Color", duration).SetEase(easeType).WithCancellation(destroyCancellation.Token);
                }
            }
            catch (System.OperationCanceledException)
            {
                // canceled
            }
        }

        private async UniTask ColorUITask()
        {
            destroyCancellation.Cancel();
            destroyCancellation = new CancellationTokenSource();

            if (uiGraphic == null)
            {
                // No UI graphic found, attempt to initialize again
                Initialize();
            }

            try
            {
                if (uiGraphic != null)
                {
                    await uiGraphic.DOColor(targetColor, duration).SetEase(easeType).WithCancellation(destroyCancellation.Token);
                    await uiGraphic.DOColor(startColor, duration).SetEase(easeType).WithCancellation(destroyCancellation.Token);
                }
            }
            catch (System.OperationCanceledException)
            {
                // canceled
            }
        }

        protected override async UniTask VFXOnceInGame()
        {
            await ColorGameObjectTask();
        }

        protected override async UniTask VFXOnceUI()
        {
            await ColorUITask();
        }
    }
}
