using UnityEngine;
using Sirenix.OdinInspector;
using DeskCat.FindIt.Scripts.Core.Main.Utility.ClickedFunction;
using Effect;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Reflection;
using DG.Tweening;

public class ShakeVFXGroupSetting : MonoBehaviour
{
    [Title("흔들림 이펙트 설정")]
    [InfoBox("이 컴포넌트는 오브젝트에 흔들림 이펙트와 클릭 이벤트를 설정합니다.")]
    
    [FoldoutGroup("애니메이션 설정"), Range(1f, 30f)]
    [LabelText("최대 회전 각도"), Tooltip("Z축 최대 회전 각도 (도)")]
    public float maxRotationAngle = 2f;
    
    [FoldoutGroup("애니메이션 설정"), Range(1, 10)]
    [LabelText("흔들림 횟수"), Tooltip("흔들림 반복 횟수")]
    public int shakeCount = 1;
    
    [FoldoutGroup("애니메이션 설정"), Range(0.01f, 0.5f)]
    [LabelText("개별 흔들림 시간"), Tooltip("개별 흔들림 시간 (초)")]
    public float shakeDuration = 0.1f;
    
    [FoldoutGroup("애니메이션 설정"), Range(0.01f, 1f)]
    [LabelText("복원 시간"), Tooltip("원래 회전으로 복원하는 시간 (초)")]
    public float restoreDuration = 0.1f;
    
    [FoldoutGroup("애니메이션 설정"), Range(0f, 1f)]
    [LabelText("감소 계수"), Tooltip("흔들림 감소 계수 (높을수록 빠르게 감소)")]
    public float decayFactor = 0.3f;
    
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
            // 기존 컴포넌트 제거 후 새로 추가하는 방식으로 변경
            // 기존 ShakeVFX 제거
            ShakeVFX existingShakeVFX = targetObj.GetComponent<ShakeVFX>();
            if (existingShakeVFX != null)
            {
                DestroyImmediate(existingShakeVFX);
            }
            
            // 기존 ClickEvent 제거
            ClickEvent existingClickEvent = targetObj.GetComponent<ClickEvent>();
            if (existingClickEvent != null)
            {
                DestroyImmediate(existingClickEvent);
            }
            
            // ShakeVFX 컴포넌트 추가
            ShakeVFX shakeVFX = targetObj.AddComponent<ShakeVFX>();
            Debug.Log($"{targetObj.name}에 ShakeVFX 컴포넌트가 추가되었습니다.");
            
            // 이펙트 설정 적용
            ApplyEffectSettings(shakeVFX);
            
            // ClickEvent 컴포넌트 추가
            ClickEvent clickEvent = targetObj.AddComponent<ClickEvent>();
            Debug.Log($"{targetObj.name}에 ClickEvent 컴포넌트가 추가되었습니다.");
            
            // ClickEvent 활성화
            clickEvent.Enable = true;
            
            // 직접 이벤트 필드 초기화
            InitializeClickEventFields(clickEvent);
            
            // ShakeVFX 참조를 캡처하는 메서드 참조
            UnityAction playVFXAction = () => PlayVFXOnObject(targetObj);
            
            // ClickEvent에 PlayVFX 메서드 연결
            if (clickEvent.OnClickEvent != null)
            {
                clickEvent.OnClickEvent.RemoveAllListeners();
                clickEvent.OnClickEvent.AddListener(playVFXAction);
                Debug.Log($"{targetObj.name}의 ClickEvent에 PlayVFX 메서드가 연결되었습니다. (리스너 수: {clickEvent.OnClickEvent.GetPersistentEventCount()})");
            }
            else
            {
                Debug.LogError($"{targetObj.name}의 OnClickEvent가 초기화 후에도 null입니다!");
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
        
        ShakeVFX shakeVFX = targetObj.GetComponent<ShakeVFX>();
        if (shakeVFX != null)
        {
            shakeVFX.PlayVFX();
            Debug.Log($"{targetObj.name}의 흔들림 이펙트가 실행되었습니다.");
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
                
                ClickEvent clickEvent = obj.GetComponent<ClickEvent>();
                
                if (clickEvent != null)
                {
                    // 모든 이벤트 필드 초기화
                    InitializeClickEventFields(clickEvent);
                    
                    // 이벤트에 리스너가 있는지 확인
                    if (clickEvent.OnClickEvent != null && clickEvent.OnClickEvent.GetPersistentEventCount() == 0)
                    {
                        // 리스너가 없으면 새로 추가
                        UnityAction playVFXAction = () => PlayVFXOnObject(obj);
                        clickEvent.OnClickEvent.AddListener(playVFXAction);
                        
                        Debug.Log($"{obj.name}의 OnClickEvent에 리스너를 추가했습니다.");
                    }
                    else if (clickEvent.OnClickEvent != null)
                    {
                        Debug.Log($"{obj.name}의 OnClickEvent에 {clickEvent.OnClickEvent.GetPersistentEventCount()}개의 리스너가 있습니다.");
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
                
                ShakeVFX shakeVFX = obj.GetComponent<ShakeVFX>();
                if (shakeVFX != null)
                {
                    ApplyEffectSettings(shakeVFX);
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
            
            Debug.Log("흔들림 이펙트 설정이 적용되었습니다.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"이펙트 설정 적용 중 오류 발생: {e.Message}\n{e.StackTrace}");
        }
    }
    
    public void ApplyEffectSettings(ShakeVFX shakeVFX)
    {
        if (shakeVFX == null)
        {
            Debug.LogError("ShakeVFX 컴포넌트가 null입니다.");
            return;
        }
        
        try
        {
            // 리플렉션을 사용하여 필드에 접근합니다 (private 필드에 직접 접근할 수 없으므로)
            var type = shakeVFX.GetType();
            if (type == null)
            {
                Debug.LogError("ShakeVFX 타입을 가져올 수 없습니다.");
                return;
            }
            
            // 리플렉션으로 필드에 접근
            var maxRotationAngleField = type.GetField("maxRotationAngle", BindingFlags.Instance | BindingFlags.NonPublic);
            var shakeCountField = type.GetField("shakeCount", BindingFlags.Instance | BindingFlags.NonPublic);
            var shakeDurationField = type.GetField("shakeDuration", BindingFlags.Instance | BindingFlags.NonPublic);
            var restoreDurationField = type.GetField("restoreDuration", BindingFlags.Instance | BindingFlags.NonPublic);
            var decayFactorField = type.GetField("decayFactor", BindingFlags.Instance | BindingFlags.NonPublic);
            var animationEaseField = type.GetField("animationEase", BindingFlags.Instance | BindingFlags.NonPublic);
            
            // 필드 값 설정
            if (maxRotationAngleField != null) maxRotationAngleField.SetValue(shakeVFX, maxRotationAngle);
            else Debug.LogWarning("maxRotationAngle 필드를 찾을 수 없습니다.");
            
            if (shakeCountField != null) shakeCountField.SetValue(shakeVFX, shakeCount);
            else Debug.LogWarning("shakeCount 필드를 찾을 수 없습니다.");
            
            if (shakeDurationField != null) shakeDurationField.SetValue(shakeVFX, shakeDuration);
            else Debug.LogWarning("shakeDuration 필드를 찾을 수 없습니다.");
            
            if (restoreDurationField != null) restoreDurationField.SetValue(shakeVFX, restoreDuration);
            else Debug.LogWarning("restoreDuration 필드를 찾을 수 없습니다.");
            
            if (decayFactorField != null) decayFactorField.SetValue(shakeVFX, decayFactor);
            else Debug.LogWarning("decayFactor 필드를 찾을 수 없습니다.");
            
            if (animationEaseField != null) animationEaseField.SetValue(shakeVFX, animationEase);
            else Debug.LogWarning("animationEase 필드를 찾을 수 없습니다.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"이펙트 설정 적용 중 리플렉션 오류 발생: {e.Message}\n{e.StackTrace}");
        }
    }
    
    [Button("흔들림 이펙트 실행", ButtonSizes.Medium)]
    public void PlayShakeEffect()
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
            
            Debug.Log("흔들림 이펙트가 실행되었습니다.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"이펙트 실행 중 오류 발생: {e.Message}\n{e.StackTrace}");
        }
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
                
                ShakeVFX shakeVFX = obj.GetComponent<ShakeVFX>();
                if (shakeVFX != null)
                {
                    DestroyImmediate(shakeVFX);
                }
                
                ClickEvent clickEvent = obj.GetComponent<ClickEvent>();
                if (clickEvent != null)
                {
                    DestroyImmediate(clickEvent);
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
