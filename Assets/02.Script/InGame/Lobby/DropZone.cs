using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SnapPointInfo
{
    public Transform point;
    public DropObjectToZone targetObject; // 특정 오브젝트가 이 위치로 가야 한다면 할당
}

public class DropZone : MonoBehaviour
{
    [Header("설정")]
    public string zoneName; // 구역 이름 (필요 시)
    public List<SnapPointInfo> snapPoints = new List<SnapPointInfo>(); // 여러 스냅 포인트 설정

    public bool isPlayAnimationWhenDropped = false; // 드롭 시 애니메이션 실행 여부
    public List<string> animationNames; // 실행할 애니메이션 이름들

    private void Awake()
    {
        // 인스펙터에서 리스트가 비어있으면 자기 자신을 기본 포인트로 추가
        if (snapPoints == null || snapPoints.Count == 0)
        {
            snapPoints = new List<SnapPointInfo> { new SnapPointInfo { point = transform } };
        }
    }

    public Transform GetSnapPoint(DropObjectToZone dropObject)
    {
        // 1. 특정 오브젝트 전용으로 지정된 포인트가 있는지 확인
        foreach (var info in snapPoints)
        {
            if (info.targetObject == dropObject && info.point != null)
            {
                return info.point;
            }
        }

        // 2. 전용 포인트가 없다면, 특정 대상이 지정되지 않은(공용) 포인트 중 첫 번째 반환
        foreach (var info in snapPoints)
        {
            if (info.targetObject == null && info.point != null)
            {
                return info.point;
            }
        }

        // 3. 모두 실패 시 기본 위치 반환
        return transform;
    }

    public void DropSuccess(DropObjectToZone dropObject, Animator animator)
    {
        PlayDropAnimation(dropObject.gameObject, animator);
    }
    public void PlayDropAnimation(GameObject character, Animator animator)
    {
        if (!isPlayAnimationWhenDropped) return;

        if (animator == null || animationNames == null || animationNames.Count == 0) return;

        foreach (var animName in animationNames)
        {
            if (string.IsNullOrEmpty(animName)) continue;
            // Debug.Log($"드롭 애니메이션 시도: {animName}" + $" (Animator에 존재 여부: {animator.HasState(0, Animator.StringToHash(animName))})");
            if (animator.HasState(0, Animator.StringToHash(animName)))
            {
                animator.Play(animName);
                return;
            }
        }
    }
}