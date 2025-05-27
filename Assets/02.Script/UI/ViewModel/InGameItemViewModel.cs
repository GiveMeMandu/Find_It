using System;
using Manager;
using UnityEngine;
using UnityWeld.Binding;
using Cysharp.Threading.Tasks;
using DeskCat.FindIt.Scripts.Core.Main.System;

namespace UI
{
    [Binding]
    public class InGameItemViewModel : BaseViewModel
    {
        // 아이템 관련 필드들
        private string _compassCountText;
        private string _stopwatchCountText;
        private string _hintCountText;
        private bool _isCompassActive;
        private bool _isStopwatchActive;
        private bool _isHintActive;
        
        // 나침판 관련 필드들
        private Quaternion _compassDirection;
        private int _compassHintCount;
        private int _compassMaxHints = 5;
        private float _compassDuration = 12f;
        
        // 돋보기 관련 필드들
        private HiddenObj _currentMagnifierTarget;
        private bool _isMagnifierEffectActive;
        private System.Guid _magnifierTargetGuid;
        private Vector3 _magnifierUIPosition;
        
        // 아이템 수량 텍스트 프로퍼티들
        [Binding]
        public string CompassCountText
        {
            get => _compassCountText;
            set
            {
                _compassCountText = value;
                OnPropertyChanged(nameof(CompassCountText));
            }
        }

        [Binding]
        public string StopwatchCountText
        {
            get => _stopwatchCountText;
            set
            {
                _stopwatchCountText = value;
                OnPropertyChanged(nameof(StopwatchCountText));
            }
        }

        [Binding]
        public string HintCountText
        {
            get => _hintCountText;
            set
            {
                _hintCountText = value;
                OnPropertyChanged(nameof(HintCountText));
            }
        }

        // 아이템 활성화 상태 프로퍼티들
        [Binding]
        public bool IsCompassActive
        {
            get => _isCompassActive;
            set
            {
                _isCompassActive = value;
                OnPropertyChanged(nameof(IsCompassActive));
            }
        }

        [Binding]
        public bool IsStopwatchActive
        {
            get => _isStopwatchActive;
            set
            {
                _isStopwatchActive = value;
                OnPropertyChanged(nameof(IsStopwatchActive));
            }
        }

        [Binding]
        public bool IsHintActive
        {
            get => _isHintActive;
            set
            {
                _isHintActive = value;
                OnPropertyChanged(nameof(IsHintActive));
            }
        }

        // 나침판 방향 바인딩 (Quaternion)
        [Binding]
        public Quaternion CompassDirection
        {
            get => _compassDirection;
            set
            {
                _compassDirection = value;
                OnPropertyChanged(nameof(CompassDirection));
            }
        }

        // 나침판 힌트 카운트 바인딩
        [Binding]
        public int CompassHintCount
        {
            get => _compassHintCount;
            set
            {
                _compassHintCount = value;
                OnPropertyChanged(nameof(CompassHintCount));
            }
        }

        // 돋보기 효과 활성화 상태 바인딩
        [Binding]
        public bool IsMagnifierEffectActive
        {
            get => _isMagnifierEffectActive;
            set
            {
                _isMagnifierEffectActive = value;
                OnPropertyChanged(nameof(IsMagnifierEffectActive));
            }
        }

        // 돋보기 UI 위치 바인딩 (스크린 좌표)
        [Binding]
        public Vector3 MagnifierUIPosition
        {
            get => _magnifierUIPosition;
            set
            {
                _magnifierUIPosition = value;
                OnPropertyChanged(nameof(MagnifierUIPosition));
            }
        }

        private void Start()
        {
            // 아이템 관련 이벤트 구독
            if (Global.ItemManager != null)
            {
                Global.ItemManager.OnItemCountChanged += OnItemCountChanged;
                UpdateAllItemCounts();
            }
            
            // 초기 상태 설정
            InitializeItemStates();
        }

        private void OnDisable()
        {
            // 아이템 관련 이벤트 구독 해제
            if (Global.ItemManager != null)
            {
                Global.ItemManager.OnItemCountChanged -= OnItemCountChanged;
            }
        }
        
        private void InitializeItemStates()
        {
            // 모든 아이템 효과 비활성화
            IsCompassActive = false;
            IsStopwatchActive = false;
            IsHintActive = false;
            
            // 돋보기 상태 초기화
            ResetMagnifierState();
        }

        // 아이템 관련 메서드들
        private void OnItemCountChanged(object sender, ItemType itemType)
        {
            UpdateItemCount(itemType);
        }

        private void UpdateItemCount(ItemType itemType)
        {
            if (Global.ItemManager == null) return;

            int count = Global.ItemManager.GetItemCount(itemType);
            
            switch (itemType)
            {
                case ItemType.Compass:
                    CompassCountText = count.ToString();
                    break;
                case ItemType.Stopwatch:
                    StopwatchCountText = count.ToString();
                    break;
                case ItemType.Hint:
                    HintCountText = count.ToString();
                    break;
            }
        }

        private void UpdateAllItemCounts()
        {
            UpdateItemCount(ItemType.Compass);
            UpdateItemCount(ItemType.Stopwatch);
            UpdateItemCount(ItemType.Hint);
        }

        // 아이템 사용 메서드들
        [Binding]
        public void UseCompass()
        {
            if (Global.ItemManager != null && Global.ItemManager.UseItem(ItemType.Compass))
            {
                ActivateCompass();
            }
        }

        [Binding]
        public void UseStopwatch()
        {
            if (Global.ItemManager != null && Global.ItemManager.UseItem(ItemType.Stopwatch))
            {
                ActivateStopwatch();
            }
        }

        [Binding]
        public void UseHint()
        {
            if (Global.ItemManager != null && Global.ItemManager.UseItem(ItemType.Hint))
            {
                ActivateHint();
            }
        }

        // 아이템 효과 활성화 메서드들
        private void ActivateCompass()
        {
            // 이미 나침반 효과가 활성화되어 있으면 무시
            if (IsCompassActive)
            {
                Debug.Log("나침반 효과가 이미 활성화되어 있습니다.");
                return;
            }
            
            IsCompassActive = true;
            CompassHintCount = 0;
            Debug.Log("나침반 활성화: 가장 가까운 오브젝트 방향 안내 시작");
            
            // 나침판 효과 시작
            CompassEffectAsync().Forget();
        }

        private void ActivateStopwatch()
        {
            // 이미 초시계 효과가 활성화되어 있으면 무시
            if (IsStopwatchActive)
            {
                Debug.Log("초시계 효과가 이미 활성화되어 있습니다.");
                return;
            }
            
            IsStopwatchActive = true;
            Debug.Log("초시계 활성화: 시간 추가");
            
            // TimeChallengeViewModel에 시간 추가 효과 적용
            var timeChallengeViewModel = FindAnyObjectByType<TimeChallengeViewModel>();
            if (timeChallengeViewModel != null)
            {
                // 15초 시간 추가
                timeChallengeViewModel.AddTime(15f);
                Debug.Log("15초 시간 추가됨");
            }
            
            // 효과는 즉시 적용되고 비활성화
            DeactivateStopwatchAsync(1f).Forget();
        }

        private void ActivateHint()
        {
            // 이미 돋보기 효과가 활성화되어 있으면 무시
            if (IsHintActive || IsMagnifierEffectActive)
            {
                Debug.Log("돋보기 효과가 이미 활성화되어 있습니다.");
                return;
            }
            
            IsHintActive = true;
            Debug.Log("돋보기 활성화: 무작위 오브젝트 힌트 시작");
            
            // 돋보기 효과 시작
            MagnifierEffectAsync().Forget();
        }

        private async UniTaskVoid CompassEffectAsync()
        {
            var timeChallengeManager = TimeChallengeManager.Instance;
            if (timeChallengeManager == null)
            {
                Debug.LogError("TimeChallengeManager를 찾을 수 없습니다.");
                IsCompassActive = false;
                return;
            }

            float elapsedTime = 0f;
            var hintedObjects = new System.Collections.Generic.HashSet<System.Guid>();
            HiddenObj currentTargetObject = null;
            System.Guid currentTargetGuid = System.Guid.Empty;

            Debug.Log("나침판 활성화: 실시간 방향 안내 시작");

            while (elapsedTime < _compassDuration && CompassHintCount < _compassMaxHints && IsCompassActive)
            {
                // 현재 타겟이 없거나 발견되었으면 새로운 타겟 찾기
                if (currentTargetObject == null || currentTargetObject.IsFound)
                {
                    if (currentTargetObject != null && currentTargetObject.IsFound)
                    {
                        Debug.Log($"힌트된 오브젝트 {currentTargetObject.name}가 발견됨!");
                        hintedObjects.Add(currentTargetGuid);
                    }

                    var closestObject = FindClosestUnfoundObject(timeChallengeManager, hintedObjects);
                    
                    if (closestObject.HasValue)
                    {
                        currentTargetObject = closestObject.Value.rabbit;
                        currentTargetGuid = closestObject.Value.guid;
                        CompassHintCount++;
                        
                        Debug.Log($"나침판 힌트 {CompassHintCount}: {currentTargetObject.name} 추적 시작");
                    }
                    else
                    {
                        Debug.Log("모든 오브젝트를 찾았거나 힌트할 수 있는 오브젝트가 없습니다.");
                        break;
                    }
                }

                // 현재 타겟이 있으면 실시간으로 방향 업데이트
                if (currentTargetObject != null && !currentTargetObject.IsFound)
                {
                    Quaternion direction = CalculateDirectionToObject(currentTargetObject);
                    CompassDirection = direction;
                }

                await UniTask.Delay(100); // 0.1초마다 업데이트
                elapsedTime += 0.1f;
            }

            // 나침판 효과 종료
            IsCompassActive = false;
            CompassDirection = Quaternion.identity;
            Debug.Log($"나침판 효과 종료. 총 {CompassHintCount}개 힌트 제공됨");
        }

        private (System.Guid guid, HiddenObj rabbit)? FindClosestUnfoundObject(TimeChallengeManager manager, System.Collections.Generic.HashSet<System.Guid> excludeGuids)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null) return null;

            // 카메라의 현재 위치를 기준으로 계산
            Vector3 cameraPosition = mainCamera.transform.position;

            var rabbitDict = GetRabbitDictionary(manager);
            if (rabbitDict == null) return null;

            float closestDistance = float.MaxValue;
            System.Guid closestGuid = System.Guid.Empty;
            HiddenObj closestRabbit = null;

            foreach (var kvp in rabbitDict)
            {
                if (kvp.Value.IsFound || excludeGuids.Contains(kvp.Key)) continue;

                // 카메라 위치에서 오브젝트까지의 실제 거리 계산
                float distance = Vector3.Distance(cameraPosition, kvp.Value.transform.position);
                
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestGuid = kvp.Key;
                    closestRabbit = kvp.Value;
                }
            }

            if (closestRabbit != null)
            {
                return (closestGuid, closestRabbit);
            }

            return null;
        }

        private Quaternion CalculateDirectionToObject(HiddenObj targetObject)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null) return Quaternion.identity;

            // 카메라의 현재 위치를 기준으로 계산
            Vector3 cameraPosition = mainCamera.transform.position;
            Vector3 targetPosition = targetObject.transform.position;
            
            // 카메라에서 타겟으로의 방향 벡터 (월드 좌표계)
            Vector3 directionVector = targetPosition - cameraPosition;
            
            // 카메라의 회전을 고려하여 로컬 방향으로 변환
            Vector3 localDirection = mainCamera.transform.InverseTransformDirection(directionVector);
            
            // 2D 평면에서의 방향 계산 (X, Y 성분만 사용)
            float angle = Mathf.Atan2(localDirection.x, localDirection.y) * Mathf.Rad2Deg;
            
            // Quaternion으로 회전 생성 (Z축 회전)
            return Quaternion.Euler(0f, 0f, angle);
        }

        private System.Collections.Generic.Dictionary<System.Guid, HiddenObj> GetRabbitDictionary(TimeChallengeManager manager)
        {
            // TimeChallengeManager의 private 필드에 접근하기 위해 리플렉션 사용
            var fieldInfo = typeof(TimeChallengeManager).GetField("rabbitObjDic", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (fieldInfo != null)
            {
                return fieldInfo.GetValue(manager) as System.Collections.Generic.Dictionary<System.Guid, HiddenObj>;
            }
            
            return null;
        }

        private async UniTaskVoid DeactivateCompassAsync(float delay)
        {
            await UniTask.Delay((int)(delay * 1000));
            IsCompassActive = false;
            Debug.Log("나침반 효과 해제");
        }

        private async UniTaskVoid DeactivateStopwatchAsync(float delay)
        {
            await UniTask.Delay((int)(delay * 1000));
            IsStopwatchActive = false;
            Debug.Log("초시계 효과 해제");
        }

        private async UniTaskVoid MagnifierEffectAsync()
        {
            var timeChallengeManager = TimeChallengeManager.Instance;
            if (timeChallengeManager == null)
            {
                Debug.LogError("TimeChallengeManager를 찾을 수 없습니다.");
                ResetMagnifierState();
                return;
            }

            // 무작위 오브젝트 선택
            var selectedObject = SelectRandomUnfoundObject(timeChallengeManager);
            if (selectedObject == null)
            {
                Debug.Log("찾을 수 있는 오브젝트가 없습니다.");
                ResetMagnifierState();
                return;
            }

            _currentMagnifierTarget = selectedObject.Value.rabbit;
            _magnifierTargetGuid = selectedObject.Value.guid;
            IsMagnifierEffectActive = true;

            Debug.Log($"돋보기 타겟 선택: {_currentMagnifierTarget.name}");

            // 오브젝트가 화면에 보이는지 확인
            if (!IsObjectVisibleOnScreen(_currentMagnifierTarget))
            {
                Debug.Log($"오브젝트가 화면 밖에 있음. 카메라 이동 시작: {_currentMagnifierTarget.name}");
                await MoveCameraToObjectAsync(_currentMagnifierTarget);
            }

            // 초기 UI 위치 설정
            UpdateMagnifierUIPosition();

            // 오브젝트가 찾아질 때까지 대기하면서 UI 위치 업데이트
            await WaitForTargetObjectFoundAsync();

            // 돋보기 효과 종료
            ResetMagnifierState();
            Debug.Log("돋보기 효과 종료");
        }

        private void ResetMagnifierState()
        {
            IsMagnifierEffectActive = false;
            IsHintActive = false;
            _currentMagnifierTarget = null;
            _magnifierTargetGuid = System.Guid.Empty;
            MagnifierUIPosition = Vector3.zero;
        }

        private (System.Guid guid, HiddenObj rabbit)? SelectRandomUnfoundObject(TimeChallengeManager manager)
        {
            var rabbitDict = GetRabbitDictionary(manager);
            if (rabbitDict == null) return null;

            var unfoundObjects = new System.Collections.Generic.List<(System.Guid, HiddenObj)>();
            
            foreach (var kvp in rabbitDict)
            {
                if (!kvp.Value.IsFound)
                {
                    unfoundObjects.Add((kvp.Key, kvp.Value));
                }
            }

            if (unfoundObjects.Count == 0) return null;

            int randomIndex = UnityEngine.Random.Range(0, unfoundObjects.Count);
            return unfoundObjects[randomIndex];
        }

        private bool IsObjectVisibleOnScreen(HiddenObj targetObject)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null) return false;

            Vector3 screenPoint = mainCamera.WorldToViewportPoint(targetObject.transform.position);
            
            // 뷰포트 좌표가 0-1 범위 내에 있으면 화면에 보임
            return screenPoint.x >= 0 && screenPoint.x <= 1 && 
                   screenPoint.y >= 0 && screenPoint.y <= 1 && 
                   screenPoint.z > 0;
        }

        private async UniTask MoveCameraToObjectAsync(HiddenObj targetObject)
        {
            var cameraView = Util.CameraSetting.CameraView2D.Instance;
            if (cameraView != null)
            {
                await cameraView.MoveCameraToPositionAsync(targetObject.transform.position, 2f);
            }
        }

        private async UniTask WaitForTargetObjectFoundAsync()
        {
            while (_currentMagnifierTarget != null && !_currentMagnifierTarget.IsFound && IsMagnifierEffectActive)
            {
                // UI 위치 업데이트 (실시간으로 오브젝트 따라가기)
                UpdateMagnifierUIPosition();
                
                await UniTask.Delay(100); // 0.1초마다 확인
            }

            if (_currentMagnifierTarget != null && _currentMagnifierTarget.IsFound)
            {
                Debug.Log($"돋보기 타겟 오브젝트가 발견됨: {_currentMagnifierTarget.name}");
            }
        }

        private void UpdateMagnifierUIPosition()
        {
            if (_currentMagnifierTarget == null) return;

            Camera mainCamera = Camera.main;
            if (mainCamera == null) return;

            // 월드 좌표를 스크린 좌표로 변환
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(_currentMagnifierTarget.transform.position);
            
            // UI 위치 업데이트
            MagnifierUIPosition = screenPosition;
        }

        private async UniTaskVoid DeactivateHintAsync(float delay)
        {
            await UniTask.Delay((int)(delay * 1000));
            IsHintActive = false;
            Debug.Log("힌트 효과 해제");
        }

        // 테스트용 아이템 추가 메서드들
        [Binding]
        public void AddCompass()
        {
            if (Global.ItemManager != null)
            {
                Global.ItemManager.AddItem(ItemType.Compass, 1);
                Debug.Log("나침반 1개 추가됨");
            }
        }

        [Binding]
        public void AddStopwatch()
        {
            if (Global.ItemManager != null)
            {
                Global.ItemManager.AddItem(ItemType.Stopwatch, 1);
                Debug.Log("초시계 1개 추가됨");
            }
        }

        [Binding]
        public void AddHint()
        {
            if (Global.ItemManager != null)
            {
                Global.ItemManager.AddItem(ItemType.Hint, 1);
                Debug.Log("힌트 1개 추가됨");
            }
        }
        
        // 아이템 사용 가능 여부 확인 메서드들
        [Binding]
        public bool CanUseCompass()
        {
            return Global.ItemManager != null && Global.ItemManager.GetItemCount(ItemType.Compass) > 0;
        }
        
        [Binding]
        public bool CanUseStopwatch()
        {
            return Global.ItemManager != null && Global.ItemManager.GetItemCount(ItemType.Stopwatch) > 0;
        }
        
        [Binding]
        public bool CanUseHint()
        {
            return Global.ItemManager != null && Global.ItemManager.GetItemCount(ItemType.Hint) > 0;
        }
    }
}
