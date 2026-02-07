using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Linq;
using DeskCat.FindIt.Scripts.Core.Main.System;
using UnityEngine.Events;
using NaughtyAttributes;
using UI.Page;
using Cysharp.Threading.Tasks;
using Manager;
using UI;
using @Button = Sirenix.OdinInspector.ButtonAttribute;

public class SetCompletionObject : MonoBehaviour
{
    [InfoBox("이 컴포넌트는 세트의 완성 상태를 관리합니다. 세트가 완성되면 `OnSetComplete` 이벤트가 호출됩니다.")]
    [Label("시작 시 숨기기")]
    public bool hideOnStart = false;

    [Label("세트 이름")]
    [InfoBox("완성 체크를 위해 사용하는 세트의 고유 이름을 입력하세요.")]
    public string SetName;

    [Header("Mission Alert Settings")]
    [Label("미션 알림 말풍선 (미수락 상태)")]
    public GameObject questionAlertObject;

    [Label("발견 알림 말풍선 (재료 찾음)")]
    public GameObject foundAlertObject;

    [Label("미션 완료 체크 아이콘")]
    public GameObject foundCheckIcon;
    
    [Label("화면 밖 알림 인디케이터")]
    public RectTransform OffScreenIndicator;

    [Label("화면 밖 알림 오프셋 (Screen Padding)")]
    public float IndicatorPadding = 50f;

    [Header("Completion Settings")]
    [Label("완성 뷰")]
    [InfoBox("세트 완성 시 표시할 GameObject를 연결하세요. 예: 세트 완성 아트 또는 이펙트.")]
    public GameObject CompletionView;  // 세트 완성 모습을 보여줄 GameObject
    public bool IsFound { get; private set; }
    
    // 미션을 수락했는지(클릭했는지) 추적
    public bool IsAccepted { get; private set; } = false;

    [Label("발견 시 사운드 재생")]
    public bool PlaySoundWhenFound = true;

    [Label("클릭 시 재생할 오디오")]
    public AudioClip AudioWhenClick;

    [Label("세트 완성 시 이벤트")]
    [InfoBox("세트가 완성되었을 때 실행할 이벤트를 연결하세요. 예: UI 알림, 애니메이션 재생 등.")]
    public UnityEvent OnSetComplete;
    
    [Header("Camera Effect Settings")]
    [Label("카메라 연출 활성화")]
    [InfoBox("세트 완성 시 카메라 연출 및 사진 촬영을 수행할지 여부를 설정합니다.")]
    public bool enableCameraEffect = false;
    
    [Label("카메라 확대 크기 (Orthographic Size)")]
    [InfoBox("카메라 연출 시 확대할 Orthographic Size 값입니다. 값이 작을수록 더 많이 확대됩니다. 기즈모로 확대 범위를 미리 확인할 수 있습니다.")]
    [ShowIf("enableCameraEffect")]
    [Range(0.5f, 10f)]
    public float defaultCameraZoomSize = 3f;
    
    private Camera _mainCamera;
    private Canvas _indicatorCanvas;
    
    // 자식으로 있는 미션 아이템 그룹 뷰모델 캐싱
    [SerializeField] private MissionElementViewModel _missionItemGroup;
    public Canvas MissionItemGroupCanvas;

    private void Awake()
    {
        if (CompletionView != null)
        {
            CompletionView.SetActive(false);
        }
        if (hideOnStart) gameObject.SetActive(false);  // 처음에는 숨김
        
        _mainCamera = Camera.main;
        
        // 인디케이터 초기화
        if (OffScreenIndicator != null)
        {
            OffScreenIndicator.gameObject.SetActive(false);
            _indicatorCanvas = OffScreenIndicator.GetComponentInParent<Canvas>();
        }
        
        if (questionAlertObject != null)
        {
            questionAlertObject.SetActive(true);
        }
        if (foundAlertObject != null)
        {
            foundAlertObject.SetActive(false);
        }
        if (foundCheckIcon != null)
        {
            foundCheckIcon.SetActive(false);
        }
        if(_missionItemGroup == null)
            _missionItemGroup = GetComponentInChildren<MissionElementViewModel>(true);
        if(MissionItemGroupCanvas == null)
            MissionItemGroupCanvas = _missionItemGroup.GetComponentInChildren<Canvas>(true);
    }

    private void Update()
    {
        // 카메라 연출 중이면 MissionItemGroupCanvas 비활성화
        bool isCameraEffectPlaying = ItemSetManager.Instance != null && ItemSetManager.Instance.IsPlayingCameraEffect;
        
        if (isCameraEffectPlaying)
        {
            if (MissionItemGroupCanvas != null && MissionItemGroupCanvas.gameObject.activeSelf)
                MissionItemGroupCanvas.gameObject.SetActive(false);
        }
        else if (IsAccepted)
        {
            if (MissionItemGroupCanvas != null && !MissionItemGroupCanvas.gameObject.activeSelf)
                MissionItemGroupCanvas.gameObject.SetActive(true);
        }
        
        // 미션이 이미 수락되었거나, 완료되었거나, 인디케이터가 없으면 업데이트 안 함
        if (IsAccepted || IsFound || _mainCamera == null)
        {
            if (OffScreenIndicator != null && OffScreenIndicator.gameObject.activeSelf)
                OffScreenIndicator.gameObject.SetActive(false);
            if (questionAlertObject != null && questionAlertObject.activeSelf)
                questionAlertObject.SetActive(false);
            return;
        }

        UpdateIndicatorPosition();
    }

    private void UpdateIndicatorPosition()
    {
        Vector3 screenPos = _mainCamera.WorldToScreenPoint(transform.position);
        bool isOffScreen = screenPos.x <= IndicatorPadding || 
                           screenPos.x >= Screen.width - IndicatorPadding || 
                           screenPos.y <= IndicatorPadding || 
                           screenPos.y >= Screen.height - IndicatorPadding ||
                           screenPos.z < 0;

        if (isOffScreen)
        {
            // 화면 밖: 말풍선 끄고 인디케이터 켜기
            if (questionAlertObject != null) questionAlertObject.SetActive(false);
            if (foundAlertObject != null) foundAlertObject.SetActive(false);
            
            if (OffScreenIndicator != null)
            {
                if (!OffScreenIndicator.gameObject.activeSelf) OffScreenIndicator.gameObject.SetActive(true);

                // 인디케이터 위치 계산
                Vector3 center = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
                
                // 타겟이 카메라 뒤에 있다면 위치를 반전시킴
                if (screenPos.z < 0)
                {
                    screenPos *= -1;
                }

                Vector3 direction = (screenPos - center).normalized;
                
                // 화면 경계에 위치시키기 (Center 기준)
                Vector2 screenBounds = new Vector2(Screen.width * 0.5f - IndicatorPadding, Screen.height * 0.5f - IndicatorPadding);
                
                // 기울기(m) = dy / dx
                // y = m * x
                float m = direction.y / direction.x;
                
                Vector3 targetLocalPos = Vector3.zero;

                // 수직선 체크 (x가 0에 가까울 때)
                if (Mathf.Abs(direction.x) < 0.001f) // 거의 수직
                {
                    targetLocalPos.x = 0;
                    targetLocalPos.y = direction.y > 0 ? screenBounds.y : -screenBounds.y;
                }
                else
                {
                    // 화면의 좌/우 경계와 교차하는 지점 계산
                    float x = direction.x > 0 ? screenBounds.x : -screenBounds.x;
                    float y = m * x;

                    // 계산된 y가 화면 상/하 경계 내에 있는지 확인
                    if (y >= -screenBounds.y && y <= screenBounds.y)
                    {
                        targetLocalPos = new Vector3(x, y, 0);
                    }
                    else
                    {
                        // 상/하 경계와 교차하는 지점 계산
                        y = direction.y > 0 ? screenBounds.y : -screenBounds.y;
                        x = y / m;
                        targetLocalPos = new Vector3(x, y, 0);
                    }
                }

                // Canvas의 ScaleFactor 등을 고려하지 않고 Screen Space Overlay라면 position 직접 할당
                // Screen Space Camera라면 변환 필요. 여기서는 일반적인 Screen 좌표로 처리.
                OffScreenIndicator.position = center + targetLocalPos;

                // 회전 (화살표가 타겟을 가리키도록)
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                OffScreenIndicator.rotation = Quaternion.Euler(0, 0, angle - 90); // 화살표 스프라이트가 위쪽(90도)을 향한다고 가정 시 보정
            }
        }
        else
        {
            // 화면 안: 말풍선 켜기 (상태에 따라), 인디케이터 끄기
            // 아직 수락 안했고, 발견된 상태도 아니면 물음표 띄우기
            if (!IsAccepted && !IsFound)
            {
                // 재료를 찾아서 foundAlert가 떠있는 상태가 아닐 때만 물음표 표시
                bool isFoundAlertActive = foundAlertObject != null && foundAlertObject.activeSelf;
                if (questionAlertObject != null && !questionAlertObject.activeSelf && !isFoundAlertActive) 
                    questionAlertObject.SetActive(true);
            }
            
            if (OffScreenIndicator != null && OffScreenIndicator.gameObject.activeSelf) OffScreenIndicator.gameObject.SetActive(false);
        }
    }

    // LeanClick 등을 통해 Inspector에서 연결할 Public 함수
    public void OnClickMission()
    {
        if (IsAccepted) return;

        IsAccepted = true;

        if (questionAlertObject != null) questionAlertObject.SetActive(false);
        if (foundAlertObject != null) foundAlertObject.SetActive(false);
        if (OffScreenIndicator != null) OffScreenIndicator.gameObject.SetActive(false);

        // 오디오 재생
        if (AudioWhenClick != null)
        {
            LevelManager.PlayItemFx(AudioWhenClick);
        }

        // 미션 아이템 그룹 뷰모델 활성화 및 초기화
        if (_missionItemGroup != null)
        {
            MissionItemGroupCanvas.gameObject.SetActive(true);
            _missionItemGroup.gameObject.SetActive(true);
            
            // 데이터 초기화
            if (ItemSetManager.Instance != null)
            {
                var itemSetDataList = ItemSetManager.Instance.GetItemSetDataList();
                var setData = itemSetDataList.Find(x => x.SetName == SetName);
                if (setData != null)
                {
                    _missionItemGroup.Initialize(setData);
                }
            }
        }
    }

    public void ShowSetCompletionNotification()
    {
        if (!IsFound)
        {
            IsFound = true;
            
            // 모든 알림 끄기
            if (questionAlertObject != null) questionAlertObject.SetActive(false);
            if (foundAlertObject != null) foundAlertObject.SetActive(false);
            if (AudioWhenClick != null && PlaySoundWhenFound)
            {
                LevelManager.PlayItemFx(AudioWhenClick);
            }
            // CompletionView.SetActive(true);
            
            // ItemSetManager에 클릭되었음을 알림
            if (ItemSetManager.Instance != null)
            {
                ItemSetManager.Instance.OnCompletionObjectClicked(SetName);
            }

            // 미션 완료 페이지 (재료 표시) - 완료 상태로 표시됨
            if (!IsAccepted)
            {
                OnClickMission();
            }
        }

        // 완료 체크 아이콘 활성화 (수락 여부 무관)
        if (foundCheckIcon != null)
        {
            foundCheckIcon.SetActive(true);
        }

        // gameObject.SetActive(true);  // 세트가 완성되면 알림 표시
        OnSetComplete?.Invoke();
        
        // Debug.Log($"SetCompletionObject {SetName} is now active and ready to be clicked!");
    }


    // 아이템을 찾았을 때 ItemSetManager에서 호출
    public void OnIngredientFound()
    {
        if (IsFound) return; // 이미 세트가 완성되었으면 무시

        // foundAlertObject 활성화
        if (foundAlertObject != null)
        {
            foundAlertObject.SetActive(true);
        }

        // 수락 안했었다면 questionAlertObject 비활성화
        if (questionAlertObject != null)
        {
            questionAlertObject.SetActive(false);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!enableCameraEffect) return;
        
        // 카메라 확대 영역을 와이어프레임 박스로 시각화
        float aspect = Camera.main != null ? Camera.main.aspect : 16f / 9f;
        float height = defaultCameraZoomSize * 2f;
        float width = height * aspect;
        
        // 초록색 와이어프레임: 확대 시 보이는 영역
        Gizmos.color = new Color(0f, 1f, 0f, 0.8f);
        Gizmos.DrawWireCube(transform.position, new Vector3(width, height, 0f));
        
        // 반투명 초록색 면: 확대 영역 채우기
        Gizmos.color = new Color(0f, 1f, 0f, 0.1f);
        Gizmos.DrawCube(transform.position, new Vector3(width, height, 0f));
        
        // 중앙 십자 표시
        Gizmos.color = Color.yellow;
        float crossSize = Mathf.Min(width, height) * 0.05f;
        Gizmos.DrawLine(transform.position - Vector3.right * crossSize, transform.position + Vector3.right * crossSize);
        Gizmos.DrawLine(transform.position - Vector3.up * crossSize, transform.position + Vector3.up * crossSize);
        
#if UNITY_EDITOR
        // 라벨 표시
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * (defaultCameraZoomSize + 0.3f),
            $"Zoom: {defaultCameraZoomSize:F1} (w:{width:F1} x h:{height:F1})",
            new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.green } }
        );
#endif
    }
#endif

    [@Button("테스트 : 이 세트의 모든 물건 찾기")]
    public void ForceCompleteMissionItems()
    {
        if (ItemSetManager.Instance == null || LevelManager.Instance == null) return;

        var itemSetDataList = ItemSetManager.Instance.GetItemSetDataList();
        var setData = itemSetDataList.Find(x => x.SetName == SetName);

        if (setData == null)
        {
            Debug.LogWarning($"[SetCompletionObject] SetName '{SetName}' not found in ItemSetManager.");
            return;
        }

        foreach (var groupName in setData.RequiredGroups)
        {
            var groupPair = LevelManager.Instance.TargetObjDic.FirstOrDefault(x => x.Value.BaseGroupName == groupName);

            if (groupPair.Value == null) continue;

            var guid = groupPair.Key;
            var group = groupPair.Value;

            var objectsToCheck = group.Objects.ToList();

            foreach (var obj in objectsToCheck)
            {
                if (!group.IsObjectFound(obj))
                {
                    group.LastClickedObject = obj;
                    LevelManager.Instance.FoundObjAction(guid);
                }
            }
        }
    }
}
