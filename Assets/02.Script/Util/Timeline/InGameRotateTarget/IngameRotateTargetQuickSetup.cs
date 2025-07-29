using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// InGameRotateTarget 2D 회전 빠른 설정 가이드
/// </summary>
public class IngameRotateTargetQuickSetup : MonoBehaviour
{
    [Header("빠른 설정")]
    [Tooltip("회전할 2D 오브젝트를 여기에 할당하세요")]
    public Transform targetToRotate;
    
    [Header("Timeline 설정")]
    public PlayableDirector playableDirector;
    
    [Header("설정 방법")]
    [TextArea(5, 15)]
    public string setupInstructions = 
        "=== 2D 회전 Timeline 설정 방법 ===\n" +
        "1. Timeline 에셋을 생성하세요\n" +
        "2. Timeline 창에서 'Add Track' → '회전트랙' 추가\n" +
        "3. 트랙에서 우클릭 → 'Add 회전클립'\n" +
        "4. 클립 선택 후 Inspector에서:\n" +
        "   - rotateTarget에 회전할 2D 오브젝트 할당\n" +
        "   - rotationMode 선택:\n" +
        "     • Instant: 즉시 회전\n" +
        "     • TimeBased: 시간 기반 회전\n" +
        "     • Flip: 반전 회전\n" +
        "5. PlayableDirector에 Timeline 에셋 할당\n" +
        "6. Play On Awake 체크 또는 코드에서 director.Play() 호출\n\n" +
        "=== 회전 모드 설명 ===\n" +
        "• Instant: 지정된 각도로 즉시 회전\n" +
        "• TimeBased: 설정된 시간 동안 목표 각도까지 회전\n" +
        "• Flip: 설정된 횟수만큼 180도씩 반전";
    
    void Start()
    {
        if (targetToRotate == null)
        {
            Debug.LogWarning("IngameRotateTargetQuickSetup: targetToRotate가 할당되지 않았습니다!");
        }
        
        if (playableDirector == null)
        {
            playableDirector = GetComponent<PlayableDirector>();
        }
        
        if (playableDirector != null && playableDirector.playableAsset != null)
        {
            Debug.Log("2D 회전 Timeline이 설정되어 있습니다. 재생을 시작합니다.");
            playableDirector.Play();
        }
        else
        {
            Debug.LogWarning("PlayableDirector 또는 TimelineAsset이 설정되지 않았습니다.");
        }
    }
    
    [ContextMenu("설정 상태 확인")]
    public void CheckSetup()
    {
        Debug.Log("=== InGameRotateTarget 2D 회전 설정 상태 ===");
        
        if (targetToRotate != null)
        {
            Debug.Log($"✅ 회전 대상: {targetToRotate.name}");
        }
        else
        {
            Debug.LogError("❌ 회전 대상이 할당되지 않음");
        }
        
        if (playableDirector != null)
        {
            Debug.Log("✅ PlayableDirector 할당됨");
            
            if (playableDirector.playableAsset != null)
            {
                Debug.Log("✅ TimelineAsset 할당됨");
            }
            else
            {
                Debug.LogError("❌ TimelineAsset이 할당되지 않음");
            }
        }
        else
        {
            Debug.LogError("❌ PlayableDirector가 할당되지 않음");
        }
        
        Debug.Log("=============================================");
    }
    
    void OnValidate()
    {
        // Inspector에서 값이 변경될 때 자동으로 확인
        if (Application.isPlaying)
        {
            CheckSetup();
        }
    }
} 