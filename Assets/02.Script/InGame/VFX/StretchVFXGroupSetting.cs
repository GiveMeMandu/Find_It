using UnityEngine;
using Sirenix.OdinInspector;
using DeskCat.FindIt.Scripts.Core.Main.Utility.ClickedFunction;
using Effect;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Reflection;
using DG.Tweening;
using Lean.Touch;

public class StretchVFXGroupSetting : MonoBehaviour
{
    [Title("스트레치 이펙트 설정")]
    [InfoBox("이 컴포넌트는 오브젝트에 스트레치 이펙트와 클릭 이벤트를 설정합니다.\n" +
             "※ 중요: 콜라이더가 겹치는 경우 하이어라키 순서상 위에 있는 객체만 클릭됩니다.")]
    
    [FoldoutGroup("애니메이션 설정"), Range(1f, 2f)]
    [LabelText("늘어남 배율"), Tooltip("X축 및 Y축 늘어남 배율")]
    public float stretchMultiplier = 1.1f;
    
    [FoldoutGroup("애니메이션 설정"), Range(0.01f, 1f)]
    [LabelText("X축 애니메이션 시간"), Tooltip("X축 늘어남/복원 애니메이션 시간 (초)")]
    public float xAnimDuration = 0.1f;
    
    [FoldoutGroup("애니메이션 설정"), Range(0.01f, 1f)]
    [LabelText("Y축 복원 시간"), Tooltip("Y축 복원 애니메이션 시간 (초)")]
    public float yRestoreDuration = 0.15f;
    
    [FoldoutGroup("애니메이션 설정")]
    [LabelText("애니메이션 이징"), Tooltip("모든 애니메이션에 적용할 이징 타입")]
    public Ease animationEase = Ease.OutSine;
    
    [FoldoutGroup("콜라이더 설정")]
    [LabelText("스프라이트 크기로 콜라이더 설정")]
    public bool useSpriteBounds = true;
    
    [FoldoutGroup("콜라이더 설정")]
    [LabelText("콜라이더 크기"), HideIf("useSpriteBounds")]
    public Vector2 colliderSize = new Vector2(1f, 1f);
    
    [FoldoutGroup("콜라이더 설정")]
    [LabelText("콜라이더 오프셋")]
    public Vector2 colliderOffset = Vector2.zero;
    
    [FoldoutGroup("고급 설정")]
    [LabelText("자식 오브젝트에도 적용")]
    public bool applyToChildren = false;
    
    [FoldoutGroup("사운드 설정")]
    [LabelText("클릭 사운드 추가")]
    public bool addClickSound = true;
    
    [FoldoutGroup("사운드 설정"), ShowIf("addClickSound")]
    [LabelText("사운드 타입")]
    public Data.SFXEnum clickSoundType = Data.SFXEnum.ClickStretch;
    
    [Button("컴포넌트 설정하기", ButtonSizes.Large)]
    public void SetupComponents()
    {
        try
        {
            if (applyToChildren)
            {
                SetupComponentsOnGameObject(gameObject);
                
                // 모든 자식 Transform 얻기
                Transform[] childTransforms = gameObject.GetComponentsInChildren<Transform>(true);
                
                foreach (Transform child in childTransforms)
                {
                    // 자기 자신이 아닌 경우에만 적용
                    if (child != null && child.gameObject != gameObject)
                    {
                        SetupComponentsOnGameObject(child.gameObject);
                    }
                }
            }
            else
            {
                SetupComponentsOnGameObject(gameObject);
            }
            
            Debug.Log("모든 컴포넌트 설정이 완료되었습니다!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"컴포넌트 설정 중 오류 발생: {e.Message}\n{e.StackTrace}");
        }
    }
    
    private void SetupComponentsOnGameObject(GameObject targetObj)
    {
        if (targetObj == null)
        {
            Debug.LogError("대상 게임오브젝트가 null입니다.");
            return;
        }
        
        try
        {
            // 기존 컴포넌트 제거 후 새로 추가하는 방식으로 변경 (중복 방지)
            // 기존 StretchVFX 제거
            StretchVFX existingStretchVFX = targetObj.GetComponent<StretchVFX>();
            if (existingStretchVFX != null)
            {
                DestroyImmediate(existingStretchVFX);
            }
            
            // 기존 ClickEvent, LeanClickEvent 모두 제거
            ClickEvent existingClickEvent = targetObj.GetComponent<ClickEvent>();
            if (existingClickEvent != null)
            {
                DestroyImmediate(existingClickEvent);
            }
            
            LeanClickEvent existingLeanClickEvent = targetObj.GetComponent<LeanClickEvent>();
            if (existingLeanClickEvent != null)
            {
                DestroyImmediate(existingLeanClickEvent);
            }
            
            // 기존 ClickTouchSound 제거 (중복 방지)
            ClickTouchSound existingClickTouchSound = targetObj.GetComponent<ClickTouchSound>();
            if (existingClickTouchSound != null)
            {
                DestroyImmediate(existingClickTouchSound);
            }
            
            // 기존 DragObj 제거 (드래그 방지)
            var existingDragObj = targetObj.GetComponent<DeskCat.FindIt.Scripts.Core.Main.Utility.DragObj.DragObj>();
            if (existingDragObj != null)
            {
                DestroyImmediate(existingDragObj);
                Debug.Log($"{targetObj.name}에서 DragObj 컴포넌트를 제거했습니다.");
            }
            
            // StretchVFX 컴포넌트 추가
            StretchVFX stretchVFX = targetObj.AddComponent<StretchVFX>();
            Debug.Log($"{targetObj.name}에 StretchVFX 컴포넌트가 추가되었습니다.");
            
            // 이펙트 설정 적용
            ApplyEffectSettings(stretchVFX);
            
            // LeanClickEvent 컴포넌트 추가 (기본적으로 LeanClickEvent 사용)
            LeanClickEvent leanClickEvent = targetObj.AddComponent<LeanClickEvent>();
            Debug.Log($"{targetObj.name}에 LeanClickEvent 컴포넌트가 추가되었습니다.");
            
            // ClickTouchSound 컴포넌트 추가 (사운드 설정이 활성화된 경우)
            ClickTouchSound clickTouchSound = null;
            if (addClickSound)
            {
                clickTouchSound = targetObj.AddComponent<ClickTouchSound>();
                clickTouchSound.SetSoundType(clickSoundType);
                Debug.Log($"{targetObj.name}에 ClickTouchSound 컴포넌트가 추가되었습니다.");
            }
            
            // LeanClickEvent 활성화
            leanClickEvent.Enable = true;
            
            // 직접 이벤트 필드 초기화
            InitializeLeanClickEventFields(leanClickEvent);
            
            // StretchVFX 참조를 캡처하는 메서드 참조
            UnityAction playVFXAction = () => PlayVFXOnObject(targetObj);
            
            // LeanClickEvent에 PlayVFX 메서드 연결 (오직 PlayVFX만 연결)
            if (leanClickEvent.OnClickEvent != null)
            {
                leanClickEvent.OnClickEvent.RemoveAllListeners();
                leanClickEvent.OnClickEvent.AddListener(playVFXAction);
                
                Debug.Log($"{targetObj.name}의 LeanClickEvent에 PlayVFX 메서드가 연결되었습니다. (리스너 수: {leanClickEvent.OnClickEvent.GetPersistentEventCount()})");
            }
            else
            {
                Debug.LogError($"{targetObj.name}의 OnClickEvent가 초기화 후에도 null입니다!");
            }
            
            // 사운드 이벤트는 StretchVFX의 OnEffectStart에 연결
            if (addClickSound && clickTouchSound != null)
            {
                ConnectSoundToVFXObject(stretchVFX, clickTouchSound);
            }
            
            // Collider2D 설정
            BoxCollider2D collider = targetObj.GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                collider = targetObj.AddComponent<BoxCollider2D>();
            }
            
            collider.isTrigger = true;
            
            // 스프라이트 크기로 콜라이더 설정
            if (useSpriteBounds)
            {
                SpriteRenderer spriteRenderer = targetObj.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null && spriteRenderer.sprite != null)
                {
                    // 스프라이트의 bounds를 기준으로 콜라이더 크기 설정
                    // 스프라이트 스케일 고려
                    Vector3 localScale = targetObj.transform.localScale;
                    Bounds bounds = spriteRenderer.sprite.bounds;
                    collider.size = new Vector2(bounds.size.x, bounds.size.y);
                    Debug.Log($"{targetObj.name}의 콜라이더 크기가 스프라이트 크기({collider.size})로 설정되었습니다.");
                }
                else
                {
                    // 스프라이트가 없는 경우 기본 크기 사용
                    collider.size = new Vector2(1f, 1f);
                    Debug.LogWarning($"{targetObj.name}에 SpriteRenderer 또는 Sprite가 없어 기본 콜라이더 크기(1,1)로 설정되었습니다.");
                }
            }
            else
            {
                // 지정된 크기 사용
                collider.size = colliderSize;
            }
            
            collider.offset = colliderOffset;
            Debug.Log($"{targetObj.name}에 BoxCollider2D가 설정되었습니다.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"{targetObj.name} 오브젝트에 컴포넌트 설정 중 오류 발생: {e.Message}\n{e.StackTrace}");
        }
    }
    
    // LeanClickEvent의 UnityEvent 필드를 직접 초기화
    private void InitializeLeanClickEventFields(LeanClickEvent leanClickEvent)
    {
        if (leanClickEvent == null) return;
        
        try
        {
            // 모든 UnityEvent 필드 초기화
            if (leanClickEvent.OnMouseDownEvent == null)
                leanClickEvent.OnMouseDownEvent = new UnityEvent();
                
            if (leanClickEvent.OnMouseUpEvent == null)
                leanClickEvent.OnMouseUpEvent = new UnityEvent();
                
            if (leanClickEvent.OnClickEvent == null)
                leanClickEvent.OnClickEvent = new UnityEvent();
                
            // 계층구조 저장을 통해 이벤트가 유지되도록 보장
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(leanClickEvent);
#endif
            
            Debug.Log($"LeanClickEvent의 모든 이벤트 필드가 초기화되었습니다.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"LeanClickEvent 필드 초기화 중 오류: {e.Message}");
        }
    }

    // ClickEvent의 UnityEvent 필드를 직접 초기화
    private void InitializeClickEventFields(ClickEvent clickEvent)
    {
        if (clickEvent == null) return;
        
        try
        {
            // 모든 UnityEvent 필드 초기화
            if (clickEvent.OnMouseDownEvent == null)
                clickEvent.OnMouseDownEvent = new UnityEvent();
                
            if (clickEvent.OnMouseUpEvent == null)
                clickEvent.OnMouseUpEvent = new UnityEvent();
                
            if (clickEvent.OnClickEvent == null)
                clickEvent.OnClickEvent = new UnityEvent();
                
            // 계층구조 저장을 통해 이벤트가 유지되도록 보장
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(clickEvent);
#endif
            
            Debug.Log($"ClickEvent의 모든 이벤트 필드가 초기화되었습니다.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ClickEvent 필드 초기화 중 오류: {e.Message}");
        }
    }
    
    // 특정 게임오브젝트에서 VFX 재생
    private void PlayVFXOnObject(GameObject targetObj)
    {
        if (targetObj == null) return;
        
        StretchVFX stretchVFX = targetObj.GetComponent<StretchVFX>();
        if (stretchVFX != null)
        {
            stretchVFX.PlayVFX();
            Debug.Log($"{targetObj.name}의 스트레치 이펙트가 실행되었습니다.");
        }
    }
    
    // VFXObject의 OnEffectStart에 사운드 이벤트 연결
    private void ConnectSoundToVFXObject(StretchVFX stretchVFX, ClickTouchSound clickTouchSound)
    {
        if (stretchVFX == null || clickTouchSound == null) return;
        
        // StretchVFX의 OnEffectStart 이벤트에 사운드 연결
        if (stretchVFX.OnEffectStart != null)
        {
            // 기존 사운드 리스너 제거 (중복 방지)
            stretchVFX.OnEffectStart.RemoveListener(clickTouchSound.PlayClickSound);
            // 새 리스너 추가
            stretchVFX.OnEffectStart.AddListener(clickTouchSound.PlayClickSound);
            Debug.Log($"{stretchVFX.name}의 StretchVFX.OnEffectStart에 사운드가 연결되었습니다.");
        }
        else
        {
            Debug.LogWarning($"{stretchVFX.name}의 StretchVFX.OnEffectStart가 null입니다.");
        }
    }

    // LeanClickEvent에만 사운드 이벤트 연결 (PlayVFX와 분리)
    private void ConnectSoundToLeanClickEvent(GameObject targetObj, ClickTouchSound clickTouchSound)
    {
        if (targetObj == null || clickTouchSound == null) return;
        
        // LeanClickEvent 확인 및 연결
        LeanClickEvent leanClickEvent = targetObj.GetComponent<LeanClickEvent>();
        if (leanClickEvent != null && leanClickEvent.OnClickEvent != null)
        {
            // 기존 사운드 리스너 제거 (중복 방지)
            leanClickEvent.OnClickEvent.RemoveListener(clickTouchSound.PlayClickSound);
            // 새 리스너 추가
            leanClickEvent.OnClickEvent.AddListener(clickTouchSound.PlayClickSound);
            Debug.Log($"{targetObj.name}의 LeanClickEvent에 사운드가 연결되었습니다.");
        }
    }
    
    // ClickEvent 또는 LeanClickEvent에 사운드 이벤트 연결
    private void ConnectSoundToClickEvents(GameObject targetObj, ClickTouchSound clickTouchSound)
    {
        if (targetObj == null || clickTouchSound == null) return;
        
        // ClickEvent 확인 및 연결
        ClickEvent clickEvent = targetObj.GetComponent<ClickEvent>();
        if (clickEvent != null && clickEvent.OnClickEvent != null)
        {
            // 기존 사운드 리스너 제거 (중복 방지)
            clickEvent.OnClickEvent.RemoveListener(clickTouchSound.PlayClickSound);
            // 새 리스너 추가
            clickEvent.OnClickEvent.AddListener(clickTouchSound.PlayClickSound);
            Debug.Log($"{targetObj.name}의 ClickEvent에 사운드가 연결되었습니다.");
        }
        
        // LeanClickEvent 확인 및 연결
        LeanClickEvent leanClickEvent = targetObj.GetComponent<LeanClickEvent>();
        if (leanClickEvent != null)
        {
            // LeanClickEvent의 OnClickEvent 필드에 접근
            if (leanClickEvent.OnClickEvent != null)
            {
                // 기존 사운드 리스너 제거 (중복 방지)
                leanClickEvent.OnClickEvent.RemoveListener(clickTouchSound.PlayClickSound);
                // 새 리스너 추가
                leanClickEvent.OnClickEvent.AddListener(clickTouchSound.PlayClickSound);
                Debug.Log($"{targetObj.name}의 LeanClickEvent에 사운드가 연결되었습니다.");
            }
        }
    }
    
    // ClickEvent 또는 LeanClickEvent에서 사운드 이벤트 제거
    private void DisconnectSoundFromClickEvents(GameObject targetObj, ClickTouchSound clickTouchSound)
    {
        if (targetObj == null || clickTouchSound == null) return;
        
        // ClickEvent에서 제거
        ClickEvent clickEvent = targetObj.GetComponent<ClickEvent>();
        if (clickEvent != null && clickEvent.OnClickEvent != null)
        {
            clickEvent.OnClickEvent.RemoveListener(clickTouchSound.PlayClickSound);
            Debug.Log($"{targetObj.name}의 ClickEvent에서 사운드가 제거되었습니다.");
        }
        
        // LeanClickEvent에서 제거
        LeanClickEvent leanClickEvent = targetObj.GetComponent<LeanClickEvent>();
        if (leanClickEvent != null && leanClickEvent.OnClickEvent != null)
        {
            leanClickEvent.OnClickEvent.RemoveListener(clickTouchSound.PlayClickSound);
            Debug.Log($"{targetObj.name}의 LeanClickEvent에서 사운드가 제거되었습니다.");
        }
    }
    
    [Button("이벤트 연결 확인 및 수정", ButtonSizes.Medium)]
    public void CheckAndFixEvents()
    {
        try
        {
            List<GameObject> targetObjects = new List<GameObject>();
            targetObjects.Add(gameObject);
            
            if (applyToChildren)
            {
                Transform[] childTransforms = gameObject.GetComponentsInChildren<Transform>(true);
                foreach (Transform child in childTransforms)
                {
                    if (child != null && child.gameObject != gameObject)
                    {
                        targetObjects.Add(child.gameObject);
                    }
                }
            }
            
            foreach (GameObject obj in targetObjects)
            {
                if (obj == null) continue;
                
                LeanClickEvent leanClickEvent = obj.GetComponent<LeanClickEvent>();
                
                if (leanClickEvent != null)
                {
                    // 모든 이벤트 필드 초기화
                    InitializeLeanClickEventFields(leanClickEvent);
                    
                    // ClickTouchSound 컴포넌트 확인 및 추가
                    ClickTouchSound clickTouchSound = obj.GetComponent<ClickTouchSound>();
                    if (addClickSound && clickTouchSound == null)
                    {
                        clickTouchSound = obj.AddComponent<ClickTouchSound>();
                        clickTouchSound.SetSoundType(clickSoundType);
                        Debug.Log($"{obj.name}에 ClickTouchSound 컴포넌트를 추가했습니다.");
                    }
                    else if (!addClickSound && clickTouchSound != null)
                    {
                        DestroyImmediate(clickTouchSound);
                        Debug.Log($"{obj.name}에서 ClickTouchSound 컴포넌트를 제거했습니다.");
                        clickTouchSound = null;
                    }
                    
                    // 이벤트에 리스너가 있는지 확인
                    if (leanClickEvent.OnClickEvent != null && leanClickEvent.OnClickEvent.GetPersistentEventCount() == 0)
                    {
                        // 리스너가 없으면 PlayVFX만 새로 추가
                        UnityAction playVFXAction = () => PlayVFXOnObject(obj);
                        leanClickEvent.OnClickEvent.AddListener(playVFXAction);
                        
                        Debug.Log($"{obj.name}의 OnClickEvent에 리스너를 추가했습니다.");
                    }
                    else if (leanClickEvent.OnClickEvent != null)
                    {
                        Debug.Log($"{obj.name}의 OnClickEvent에 {leanClickEvent.OnClickEvent.GetPersistentEventCount()}개의 리스너가 있습니다.");
                    }
                    
                    // 사운드 이벤트는 StretchVFX의 OnEffectStart에 연결
                    StretchVFX stretchVFX = obj.GetComponent<StretchVFX>();
                    if (addClickSound && clickTouchSound != null && stretchVFX != null)
                    {
                        ConnectSoundToVFXObject(stretchVFX, clickTouchSound);
                    }
                }
            }
            
            // 계층구조 저장
#if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
#endif
            
            Debug.Log("모든 이벤트 연결을 확인하고 수정했습니다.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"이벤트 확인 중 오류 발생: {e.Message}");
        }
    }
    
    [Button("이펙트 설정 적용", ButtonSizes.Medium)]
    public void ApplyEffectSettingsButton()
    {
        try
        {
            List<GameObject> targetObjects = new List<GameObject>();
            targetObjects.Add(gameObject);
            
            if (applyToChildren)
            {
                // 모든 자식 Transform 얻기
                Transform[] childTransforms = gameObject.GetComponentsInChildren<Transform>(true);
                
                foreach (Transform child in childTransforms)
                {
                    // 자기 자신이 아닌 경우에만 적용
                    if (child != null && child.gameObject != gameObject)
                    {
                        targetObjects.Add(child.gameObject);
                    }
                }
            }
            
            // 모든 대상 오브젝트에 설정 적용
            foreach (GameObject obj in targetObjects)
            {
                if (obj == null) continue;
                
                StretchVFX stretchVFX = obj.GetComponent<StretchVFX>();
                if (stretchVFX != null)
                {
                    ApplyEffectSettings(stretchVFX);
                }
                
                // 콜라이더 설정도 업데이트
                BoxCollider2D collider = obj.GetComponent<BoxCollider2D>();
                if (collider != null)
                {
                    // 스프라이트 크기로 콜라이더 설정
                    if (useSpriteBounds)
                    {
                        SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
                        if (spriteRenderer != null && spriteRenderer.sprite != null)
                        {
                            Bounds bounds = spriteRenderer.sprite.bounds;
                            collider.size = new Vector2(bounds.size.x, bounds.size.y);
                        }
                    }
                    else
                    {
                        collider.size = colliderSize;
                    }
                    
                    collider.offset = colliderOffset;
                }
            }
            
            Debug.Log("스트레치 이펙트 설정이 적용되었습니다.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"이펙트 설정 적용 중 오류 발생: {e.Message}\n{e.StackTrace}");
        }
    }
    
    public void ApplyEffectSettings(StretchVFX stretchVFX)
    {
        if (stretchVFX == null)
        {
            Debug.LogError("StretchVFX 컴포넌트가 null입니다.");
            return;
        }
        
        try
        {
            // 리플렉션을 사용하여 필드에 접근합니다 (private 필드에 직접 접근할 수 없으므로)
            var type = stretchVFX.GetType();
            if (type == null)
            {
                Debug.LogError("StretchVFX 타입을 가져올 수 없습니다.");
                return;
            }
            
            // 리플렉션으로 필드에 접근
            var stretchMultiplierField = type.GetField("stretchMultiplier", BindingFlags.Instance | BindingFlags.NonPublic);
            var xAnimDurationField = type.GetField("xAnimDuration", BindingFlags.Instance | BindingFlags.NonPublic);
            var yRestoreDurationField = type.GetField("yRestoreDuration", BindingFlags.Instance | BindingFlags.NonPublic);
            var animationEaseField = type.GetField("animationEase", BindingFlags.Instance | BindingFlags.NonPublic);
            
            // 필드 값 설정
            if (stretchMultiplierField != null) stretchMultiplierField.SetValue(stretchVFX, stretchMultiplier);
            else Debug.LogWarning("stretchMultiplier 필드를 찾을 수 없습니다.");
            
            if (xAnimDurationField != null) xAnimDurationField.SetValue(stretchVFX, xAnimDuration);
            else Debug.LogWarning("xAnimDuration 필드를 찾을 수 없습니다.");
            
            if (yRestoreDurationField != null) yRestoreDurationField.SetValue(stretchVFX, yRestoreDuration);
            else Debug.LogWarning("yRestoreDuration 필드를 찾을 수 없습니다.");
            
            if (animationEaseField != null) animationEaseField.SetValue(stretchVFX, animationEase);
            else Debug.LogWarning("animationEase 필드를 찾을 수 없습니다.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"이펙트 설정 적용 중 리플렉션 오류 발생: {e.Message}\n{e.StackTrace}");
        }
    }
    
    [Button("스트레치 이펙트 실행", ButtonSizes.Medium)]
    public void PlayStretchEffect()
    {
        try
        {
            List<GameObject> targetObjects = new List<GameObject>();
            targetObjects.Add(gameObject);
            
            if (applyToChildren)
            {
                // 모든 자식 Transform 얻기
                Transform[] childTransforms = gameObject.GetComponentsInChildren<Transform>(true);
                
                foreach (Transform child in childTransforms)
                {
                    // 자기 자신이 아닌 경우에만 적용
                    if (child != null && child.gameObject != gameObject)
                    {
                        targetObjects.Add(child.gameObject);
                    }
                }
            }
            
            // 모든 대상 오브젝트에 이펙트 실행
            foreach (GameObject obj in targetObjects)
            {
                if (obj == null) continue;
                
                PlayVFXOnObject(obj);
            }
            
            Debug.Log("스트레치 이펙트가 실행되었습니다.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"이펙트 실행 중 오류 발생: {e.Message}\n{e.StackTrace}");
        }
    }
    
    [Button("사운드 설정 도구 열기", ButtonSizes.Medium)]
    [GUIColor(0.7f, 1f, 0.7f)]
    public void OpenSoundSetupTool()
    {
#if UNITY_EDITOR
        try
        {
            // 메뉴 아이템을 직접 실행 (가장 안전한 방법)
            UnityEditor.EditorApplication.ExecuteMenuItem("Tools/StretchVFX/사운드 자동 설정");
            Debug.Log("StretchVFX 사운드 설정 도구를 열었습니다.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"사운드 설정 도구를 열 수 없습니다: {e.Message}");
            Debug.Log("대신 'Tools > StretchVFX > 사운드 자동 설정' 메뉴를 직접 클릭해주세요.");
        }
#else
        Debug.LogWarning("이 기능은 에디터에서만 사용할 수 있습니다.");
#endif
    }
    
    [Button("전체 제거", ButtonSizes.Medium, ButtonStyle.FoldoutButton)]
    [GUIColor(1f, 0.5f, 0.5f)]
    public void RemoveAllComponents()
    {
        try
        {
            List<GameObject> targetObjects = new List<GameObject>();
            targetObjects.Add(gameObject);
            
            if (applyToChildren)
            {
                // 모든 자식 Transform 얻기
                Transform[] childTransforms = gameObject.GetComponentsInChildren<Transform>(true);
                
                foreach (Transform child in childTransforms)
                {
                    // 자기 자신이 아닌 경우에만 적용
                    if (child != null && child.gameObject != gameObject)
                    {
                        targetObjects.Add(child.gameObject);
                    }
                }
            }
            
            // 모든 대상 오브젝트에서 컴포넌트 제거
            foreach (GameObject obj in targetObjects)
            {
                if (obj == null) continue;
                
                StretchVFX stretchVFX = obj.GetComponent<StretchVFX>();
                if (stretchVFX != null)
                {
                    DestroyImmediate(stretchVFX);
                }
                
                ClickEvent clickEvent = obj.GetComponent<ClickEvent>();
                if (clickEvent != null)
                {
                    DestroyImmediate(clickEvent);
                }
                
                LeanClickEvent leanClickEvent = obj.GetComponent<LeanClickEvent>();
                if (leanClickEvent != null)
                {
                    DestroyImmediate(leanClickEvent);
                }
                
                // ClickTouchSound 컴포넌트 제거
                ClickTouchSound clickTouchSound = obj.GetComponent<ClickTouchSound>();
                if (clickTouchSound != null)
                {
                    DestroyImmediate(clickTouchSound);
                }
                
                // DragObj 컴포넌트도 제거
                var dragObj = obj.GetComponent<DeskCat.FindIt.Scripts.Core.Main.Utility.DragObj.DragObj>();
                if (dragObj != null)
                {
                    DestroyImmediate(dragObj);
                }
                
                BoxCollider2D collider = obj.GetComponent<BoxCollider2D>();
                if (collider != null)
                {
                    DestroyImmediate(collider);
                }
            }
            
            Debug.Log("모든 관련 컴포넌트가 제거되었습니다.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"컴포넌트 제거 중 오류 발생: {e.Message}\n{e.StackTrace}");
        }
    }
}
