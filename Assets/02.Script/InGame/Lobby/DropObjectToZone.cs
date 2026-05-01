using UnityEngine;

public class DropObjectToZone : MonoBehaviour
{
    private Transform character; // 드롭되는 캐릭터
    void OnEnable()
    {
        character = transform; // 드롭되는 오브젝트의 Transform을 캐릭터로 설정
    }
    private DropZone hoveredZone; // 현재 겹쳐 있는 드롭존
    public bool isDragging; // 드래그 중인지 여부 (외부에서 설정)
    public void SetDragging(bool dragging)
    {
        isDragging = dragging;
    }
    public Animator animator;
    // 1. 트리거에 들어왔을 때 (드래그 중)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<DropZone>(out var zone))
        {
            hoveredZone = zone;
            // 시각적 피드백: 드롭존 색깔을 바꾸거나 캐릭터를 살짝 투명하게 만들기 가능
            Debug.Log($"{zone.zoneName} 위로 진입함");
        }
    }

    // 2. 트리거에서 나갔을 때
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<DropZone>(out var zone))
        {
            if (hoveredZone == zone) hoveredZone = null;
            Debug.Log("드롭존 밖으로 나감");
        }
    }
    void Update()
    {
        if (!isDragging && hoveredZone != null)
            HandleDropSuccess();
    }

    // 캐릭터의 UnityEvent에 연결할 함수
    public void HandleDropSuccess()
    {
        if (hoveredZone == null) return; // 드롭존 위에 있지 않으면 무시
        
        // 1. 위치 이동 (DropZone에서 적절한 스냅 포인트를 가져옴)
        Transform targetPoint = hoveredZone.GetSnapPoint(this);
        character.transform.position = targetPoint.position;

        // 2. 물리 초기화
        if (character.TryGetComponent(out Rigidbody2D rb))
        {
            rb.Sleep();
        }
        hoveredZone.DropSuccess(this, animator); // 드롭존에 성공 알림 (애니메이션 등 처리)
        // Debug.Log($"{character.name}가 {hoveredZone.zoneName}의 {targetPoint.name} 위치에 안착했습니다.");
    }
}
