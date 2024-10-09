using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cart : MonoBehaviour
{
    public GameObject[] Wheels;

    private bool isRotateWheel = false;
    private float wheelRotateSpd;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(isRotateWheel) {
            RotateWheel();
        }
    }

    private void RotateWheel() {
        foreach (var wheel in Wheels)
        {
            wheel.transform.Rotate(wheelRotateSpd * -500f * Time.deltaTime * Vector3.forward);
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
