using Sirenix.OdinInspector;
using UnityEngine;

public class KeepChildPosition : MonoBehaviour
{
    [Button("MoveParentToOrigin")]
    public void MoveParentToOrigin()
    {
        // 1. 모든 자식들을 임시로 저장
        Transform[] children = new Transform[transform.childCount];
        Vector3[] worldPositions = new Vector3[transform.childCount];
        Quaternion[] worldRotations = new Quaternion[transform.childCount];
        
        for (int i = 0; i < transform.childCount; i++)
        {
            children[i] = transform.GetChild(i);
            worldPositions[i] = children[i].position;
            worldRotations[i] = children[i].rotation;
        }

        // 2. 부모 오브젝트를 원점으로 이동
        transform.position = Vector3.zero;

        // 3. 각 자식의 원래 월드 포지션 유지하면서 다시 부모에 붙이기
        for (int i = 0; i < children.Length; i++)
        {
            children[i].SetParent(null); // 임시로 부모 분리
            children[i].position = worldPositions[i];
            children[i].rotation = worldRotations[i];
            children[i].SetParent(transform); // 다시 부모에 붙이기
        }
    }

    // 더 간단한 방법: SetParent 사용
    [Button("SimpleMoveParentToOrigin")]
    public void SimpleMoveParentToOrigin()
    {
        // 모든 자식들을 worldPositionStays=true로 설정하여 임시 분리
        Transform[] children = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            children[i] = transform.GetChild(i);
            children[i].SetParent(null, true);
        }

        // 부모 이동
        transform.position = Vector3.zero;

        // 자식들 다시 붙이기
        for (int i = 0; i < children.Length; i++)
        {
            children[i].SetParent(transform, true);
        }
    }
} 