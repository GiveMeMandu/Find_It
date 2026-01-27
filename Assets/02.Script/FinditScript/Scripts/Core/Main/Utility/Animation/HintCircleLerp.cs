using UnityEngine;
using DeskCat.FindIt.Scripts.Core.Main.System;

namespace DeskCat.FindIt.Scripts.Core.Main.Utility.Animation
{
    public class HintCircleLerp : FindItLerp
    {
        [Header("Circle Settings")]
        public bool UseCustomRadius = false;
        public float CustomRadius = 1f;
        public float RadiusMultiplier = 1.2f; // 대상 크기의 배수
        
        [Header("Animation Settings")]
        public bool PlayOnEnable = false; // OnEnable 시 자동 재생 여부
        public bool AutoRestart = false; // 기본값을 false로 변경
        public float RotationSpeed = 360f; // 초당 회전 각도
        public bool Clockwise = true; // 시계방향 회전
        
        // 초기 정보 저장
        public Bounds InitialBounds { get; private set; }
        
        private Vector3 centerPosition;
        public float circleRadius;
        private float currentAngle = 0f;
        private Vector3 originalPosition;

        private void OnEnable()
        {
            // 부모 클래스의 자동 애니메이션 비활성화
            PlayOnStart = false;
            HideHiddenObjAfterDone = false;
            
            // 부모에 HiddenObj가 있고, Collider가 있는지 확인하여 중심 위치 설정
            if (transform.parent != null && transform.parent.TryGetComponent<HiddenObj>(out var parentHiddenObj))
            {
                // Collider 체크 및 중심 위치 설정
                if (parentHiddenObj.TryGetComponent<Collider>(out var collider))
                {
                    centerPosition = collider.bounds.center;
                    InitialBounds = collider.bounds;
                }
                else if (parentHiddenObj.TryGetComponent<Collider2D>(out var collider2D))
                {
                    centerPosition = collider2D.bounds.center;
                    InitialBounds = collider2D.bounds;
                }
                else
                {
                    centerPosition = parentHiddenObj.transform.position;
                }
            }
            else
            {
                centerPosition = transform.position;
            }

            // 원의 반지름 계산
            CalculateCircleRadius();
            
            // 힌트 스프라이트의 sorting order 설정
            var hintSprite = GetComponent<SpriteRenderer>();
            if (hintSprite != null && transform.parent != null && transform.parent.TryGetComponent<HiddenObj>(out var hiddenObj))
            {
                if (hiddenObj.spriteRenderer != null)
                {
                    // 대상보다 위에 보이도록
                    hintSprite.sortingOrder = hiddenObj.spriteRenderer.sortingOrder + 1;
                }
            }
            
            // 시작 위치 설정 (0도 위치)
            originalPosition = transform.position;
            currentAngle = 0f;
            SetCirclePosition(currentAngle);
            
            // PlayOnEnable이 true면 자동으로 애니메이션 시작
            if (PlayOnEnable)
            {
                AutoRestart = true; // PlayOnEnable일 때만 무한 반복 활성화
                StartHintEffect();
            }
            
            transform.parent.parent = null; // 부모에서 분리하여 독립적으로 스케일 조정
            transform.parent.localScale = Vector3.one;
            transform.localScale = Vector3.one;
        }

        private void Update()
        {
            if (!AutoRestart)
                return;

            // 각도 업데이트
            float deltaAngle = RotationSpeed * Time.deltaTime;
            if (!Clockwise)
                deltaAngle = -deltaAngle;
                
            currentAngle += deltaAngle;
            
            // 360도를 넘으면 리셋
            if (currentAngle >= 360f)
                currentAngle -= 360f;
            else if (currentAngle < 0f)
                currentAngle += 360f;
            
            // 원형 위치 설정
            SetCirclePosition(currentAngle);
        }
        
        private void CalculateCircleRadius()
        {
            if (UseCustomRadius)
            {
                circleRadius = CustomRadius;
                return;
            }
            
            // 대상 오브젝트의 크기에 따라 반지름 계산
            if (transform.parent != null && transform.parent.TryGetComponent<HiddenObj>(out var hiddenObj))
            {
                // 우선 콜라이더(InitialBounds)가 있으면 이를 사용
                if (InitialBounds.size != Vector3.zero)
                {
                    Debug.Log($"InitialBounds Size: {InitialBounds.size} for {hiddenObj.name}");
                    circleRadius = Mathf.Max(InitialBounds.size.x, InitialBounds.size.y) * 0.5f * RadiusMultiplier;
                }
                else if (hiddenObj.spriteRenderer != null && hiddenObj.spriteRenderer.sprite != null)
                {
                    // 콜라이더가 없으면 스프라이트 크기 사용
                    Debug.Log($"Sprite Size: {hiddenObj.spriteRenderer.sprite.bounds.size} for {hiddenObj.name}");
                    var spriteSize = hiddenObj.spriteRenderer.sprite.bounds.size;
                    circleRadius = Mathf.Max(spriteSize.x, spriteSize.y) * 0.5f * RadiusMultiplier;
                }
                else
                {
                    // 기본값
                    circleRadius = 1f;
                }
            }
            else
            {
                // 부모가 없을 때 기본값
                circleRadius = 1f;
            }
        }
        
        private void SetCirclePosition(float angle)
        {
            // 각도를 라디안으로 변환
            float radian = angle * Mathf.Deg2Rad;
            
            // 원형 위치 계산 (0도는 오른쪽, 90도는 위쪽)
            float x = centerPosition.x + Mathf.Cos(radian) * circleRadius;
            float y = centerPosition.y + Mathf.Sin(radian) * circleRadius;
            
            transform.position = new Vector3(x, y, transform.position.z);
        }
        
        // 힌트 효과를 시작하는 메서드
        public void StartHintEffect()
        {
            // 오브젝트가 비활성화되어 있다면 활성화
            if (!gameObject.activeInHierarchy)
            {
                gameObject.SetActive(true);
            }
            
            // 애니메이션 상태 초기화 (AutoRestart는 강제로 변경하지 않음)
            currentAngle = 0f;
            SetCirclePosition(currentAngle);
            
            Debug.Log($"HintCircleLerp 애니메이션 시작 - Radius: {circleRadius}, Center: {centerPosition}, AutoRestart: {AutoRestart}");
        }
        
        // 힌트 효과를 중지하는 메서드
        public void StopHintEffect()
        {
            // 애니메이션만 중지
            AutoRestart = false;
            // 원래 위치로 복원
            transform.position = originalPosition;
            Debug.Log("HintCircleLerp 애니메이션 중지");
        }
        
        // 특정 각도로 위치 설정 (외부에서 호출 가능)
        public void SetAngle(float angle)
        {
            currentAngle = angle;
            SetCirclePosition(currentAngle);
        }
        
        // 현재 각도 반환
        public float GetCurrentAngle()
        {
            return currentAngle;
        }
    }
}
