using UnityEngine;

/// <summary>
/// Hidden 오브젝트에 부착하여 배경 애니메이션 동작을 제어하는 헬퍼입니다.
/// - 배경 애니메이션 사용 여부를 설정할 수 있습니다.
/// - 커스텀 프리팹을 지정하면 기본 프리팹 대신 사용됩니다.
/// </summary>
public class BGAnimationHelper : MonoBehaviour
{
    [Tooltip("배경 애니메이션 사용 여부 (체크 해제 시 배경 애니메이션을 생성하지 않음)")]
    public bool UseBgAnimation = true;

    [Tooltip("이 오브젝트에 사용할 커스텀 배경 애니메이션 프리팹")]
    public GameObject CustomBgAnimationPrefab;
}
