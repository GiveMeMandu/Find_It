using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class SetPositionHelper : MonoBehaviour
{
    public Transform copyPositionTargetTrans;
    public Vector3 targetPosition;
    [LabelText("위치 설정 후 호출할 이벤트")]
    public UnityEvent onPositionSet;
    public void SetTransformPosition()
    {
        if (copyPositionTargetTrans != null)
        {
            transform.position = copyPositionTargetTrans.position;
        }
        else
        {
            transform.position = targetPosition;
            onPositionSet.Invoke();
        }
    }
    public void SetTranfsormPositionAndRotation()
    {
        if (copyPositionTargetTrans != null)        {
            transform.position = copyPositionTargetTrans.position;
            transform.rotation = copyPositionTargetTrans.rotation;
        }
        else
        {
            transform.position = targetPosition;
            onPositionSet.Invoke();
        }
    }
    public void SetTransformPositionTo(Vector3 newPosition)
    {
        transform.position = newPosition;
        onPositionSet.Invoke();
    }
    public void SetTransformPositionToTarget(Transform targetTransform)
    {
        if (targetTransform != null)
        {
            transform.position = targetTransform.position;
            onPositionSet.Invoke();
        }
    }
    public void SetTransformPositionAndRotationToTarget(Transform targetTransform)
    {
        if (targetTransform != null)
        {
            transform.position = targetTransform.position;
            transform.rotation = targetTransform.rotation;
            onPositionSet.Invoke();
        }
    }
}
