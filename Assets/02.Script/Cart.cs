using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Cart : MonoBehaviour
{
    public GameObject[] Wheels;

    [LabelText("시작시 돌릴것인가")]
    [SerializeField] private bool isRotateWheelOnStart = false;
    [LabelText("반대로 돌릴것인가")]
    [SerializeField] private bool isRotateInverse = false;
    [LabelText("가만히 있을때 자동으로 멈출 것인가")]
    [SerializeField] private bool isStopOnIdle = false;
    private bool isRotateWheel = false;
    private float wheelRotateSpd;
    private Vector2 pastPos;
    private Vector2 curPos;
    void Start()
    {
        pastPos = transform.position;
        if(isRotateWheelOnStart) StartWheel();
    }

    // Update is called once per frame
    void Update()
    {
        curPos = transform.position;
        if(isRotateWheel) {
            RotateWheel();
        }
    }

    private void RotateWheel() {
        if(isStopOnIdle)
        {
            if(pastPos == curPos) return;
            else pastPos = curPos;
        }
        foreach (var wheel in Wheels)
        {
            var dir = Vector3.forward;
            if(isRotateInverse) dir = Vector3.back;

            wheel.transform.Rotate(wheelRotateSpd * -500f * Time.deltaTime * dir);
        }
    }
    public void StartWheel(float speed = 1f) {
        isRotateWheel = true;
        wheelRotateSpd = speed;
    }

    public void StopWheel() {
        isRotateWheel = false;
    }
}
