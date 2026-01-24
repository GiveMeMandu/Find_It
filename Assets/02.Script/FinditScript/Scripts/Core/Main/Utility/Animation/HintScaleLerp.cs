using UnityEngine;
using DeskCat.FindIt.Scripts.Core.Main.System;

namespace DeskCat.FindIt.Scripts.Core.Main.Utility.Animation
{
    public class HintScaleLerp : FindItLerp
    {
        [Header("Scale Settings")]
        public bool UseCustomScale = false;
        public Vector3 MinScale = Vector3.one * 0.9f; // 완전히 작아지지 않도록
        public Vector3 MaxScale = Vector3.one * 1.3f;
        
        [Header("Animation Settings")]
        public bool AutoRestart = true;
        public float PauseTime = 0.1f; // 최대/최소 크기에서 잠깐 멈추는 시간
        
        // 초기 bounds 정보 저장
        public Bounds InitialBounds { get; private set; }
        
        private bool isIncreasing = true; // 커지는 방향인지 확인
        private float pauseTimer = 0f;
        private bool isPausing = false;

        private void OnEnable()
        {
            // 부모 클래스의 자동 애니메이션 비활성화
            PlayOnStart = false;
            HideHiddenObjAfterDone = false;
            
            // 부모에 HiddenObj가 있고, Collider가 있는지 확인하여 위치 설정
            if (transform.parent != null && transform.parent.TryGetComponent<HiddenObj>(out var parentHiddenObj))
            {
                // Collider 체크 및 위치 설정
                if (parentHiddenObj.TryGetComponent<Collider>(out var collider))
                {
                    transform.position = collider.bounds.center;
                    InitialBounds = collider.bounds;
                }
                else if (parentHiddenObj.TryGetComponent<Collider2D>(out var collider2D))
                {
                    transform.position = collider2D.bounds.center;
                    InitialBounds = collider2D.bounds;
                }
            }

            if (UseCustomScale == false)
            {
                // HiddenObj의 spriteRenderer 사용하여 적절한 크기 계산
                if (transform.parent != null && transform.parent.TryGetComponent<HiddenObj>(out var hiddenObj))
                {
                    var hintSprite = GetComponent<SpriteRenderer>();
                    if (hintSprite != null)
                    {
                        // Ensure prefab's local scale is neutral for consistent size calculations
                        transform.localScale = Vector3.one;
                        // 우선 콜라이더(InitialBounds)가 있으면 이를 사용
                        if (InitialBounds.size != Vector3.zero)
                        {
                            float targetWidth = InitialBounds.size.x;
                            float targetHeight = InitialBounds.size.y;
                            float hintWidth = hintSprite.sprite.bounds.size.x;
                            float hintHeight = hintSprite.sprite.bounds.size.y;

                            float scaleX = targetWidth / hintWidth;
                            float scaleY = targetHeight / hintHeight;
                            float baseScale = Mathf.Max(scaleX, scaleY) * 1.0f;

                            MinScale = new Vector3(baseScale * 0.85f, baseScale * 0.85f, 1f);
                            MaxScale = new Vector3(baseScale * 1.25f, baseScale * 1.25f, 1f);
                        }
                        else if (hiddenObj.spriteRenderer != null && hiddenObj.spriteRenderer.sprite != null)
                        {
                            // 콜라이더가 없으면 스프라이트 실제 크기 기준으로 힌트 크기 설정
                            float targetWidth = hiddenObj.spriteRenderer.sprite.bounds.size.x;
                            float targetHeight = hiddenObj.spriteRenderer.sprite.bounds.size.y;
                            float hintWidth = hintSprite.sprite.bounds.size.x;
                            float hintHeight = hintSprite.sprite.bounds.size.y;

                            float scaleX = targetWidth / hintWidth;
                            float scaleY = targetHeight / hintHeight;
                            float baseScale = Mathf.Max(scaleX, scaleY) * 1.0f; // 기본 크기

                            // 펄스 효과: 기본 크기에서 약간 작게/크게
                            MinScale = new Vector3(baseScale * 0.85f, baseScale * 0.85f, 1f);
                            MaxScale = new Vector3(baseScale * 1.25f, baseScale * 1.25f, 1f);

                            // sorting order 설정 (대상보다 위에 보이도록)
                            hintSprite.sortingOrder = hiddenObj.spriteRenderer.sortingOrder + 1;
                        }
                        else
                        {
                            // 기본값 설정 (완전히 작아지지 않도록)
                            MinScale = new Vector3(0.85f, 0.85f, 1f);
                            MaxScale = new Vector3(1.25f, 1.25f, 1f);
                        }
                    }
                }
                else
                {
                    // 부모가 없을 때 기본값
                    MinScale = new Vector3(0.85f, 0.85f, 1f);
                    MaxScale = new Vector3(1.25f, 1.25f, 1f);
                }
            }
            
            // 시작 크기를 MinScale로 설정
            transform.localScale = MinScale;
            currentTime = 0f;
            isIncreasing = true;
            isPausing = false;
            pauseTimer = 0f;
        }

        private void Update()
        {
            if (!AutoRestart)
                return;

            if (isPausing)
            {
                pauseTimer += Time.deltaTime;
                if (pauseTimer >= PauseTime)
                {
                    isPausing = false;
                    pauseTimer = 0f;
                    currentTime = 0f; // 애니메이션 리셋
                }
                return;
            }

            currentTime += Time.deltaTime; // 시간 증가 추가
            var value = GetLerpValue(currentTime);
            
            if (isIncreasing)
            {
                transform.localScale = Vector3.Lerp(MinScale, MaxScale, value);
                
                if (value >= 1f)
                {
                    isIncreasing = false;
                    isPausing = true;
                    transform.localScale = MaxScale;
                    currentTime = 0f; // 즉시 리셋
                }
            }
            else
            {
                transform.localScale = Vector3.Lerp(MaxScale, MinScale, value);
                
                if (value >= 1f)
                {
                    isIncreasing = true;
                    isPausing = true;
                    transform.localScale = MinScale;
                    currentTime = 0f; // 즉시 리셋
                }
            }
        }
        
        // 힌트 효과를 시작하는 메서드
        public void StartHintEffect()
        {
            // 오브젝트가 비활성화되어 있다면 활성화
            if (!gameObject.activeInHierarchy)
            {
                gameObject.SetActive(true);
            }
            
            // 애니메이션 상태 초기화
            AutoRestart = true; // 확실히 무한 반복 활성화
            currentTime = 0f;
            isIncreasing = true;
            isPausing = false;
            pauseTimer = 0f;
            transform.localScale = MinScale;
            
            // Debug.Log($"HintScaleLerp 애니메이션 시작 - MinScale: {MinScale}, MaxScale: {MaxScale}, AutoRestart: {AutoRestart}");
        }
        
        // 힌트 효과를 중지하는 메서드
        public void StopHintEffect()
        {
            // 오브젝트를 비활성화하지 않고 애니메이션만 중지
            AutoRestart = false;
            // 원래 크기로 복원
            transform.localScale = Vector3.one;
            // Debug.Log("HintScaleLerp 애니메이션 중지");
        }
    }
}
