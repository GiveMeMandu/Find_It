using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Util.Camera
{
    public class CameraResolution : MonoBehaviour
    {
        [LabelText("카메라 커지면 내려가는 정도")]
        [SerializeField] private float cameraPosDownRate = 0f;
        [LabelText("내려가는 정도 적용할 옵젝")]
        [SerializeField] private Transform targetObj;
        private readonly Vector2 targetAspectRatio = new(16, 9);
        private readonly Vector2 rectCenter = new(0.5f, 0.5f);
        private Vector2 lastResolution;

        private float camOrthographicSize;
        private Vector3 lastTargetPos;
        private void Start()
        {
            if(targetObj == null) targetObj = UnityEngine.Camera.main.transform;
            lastTargetPos = targetObj.position;


            camOrthographicSize = UnityEngine.Camera.main.orthographicSize;
        }


        public void LateUpdate()
        {
            var currentScreenResolution = new Vector2(Screen.width, Screen.height);

            // Don't run all the calculations if the screen resolution has not changed
            if (lastResolution != currentScreenResolution)
            {
                CalculateCameraRect(currentScreenResolution);
            }
        }

        private void CalculateCameraRect(Vector2 currentScreenResolution)
        {
            var normalizedAspectRatio = targetAspectRatio / currentScreenResolution;
            var size = normalizedAspectRatio / Mathf.Max(normalizedAspectRatio.x, normalizedAspectRatio.y);

            var cam = UnityEngine.Camera.main;
            if (cam != null)
            {
                // Camera.main.rect = new Rect(default, size) { center = rectCenter };
                // if (size.x >= 1)
                //     cam.orthographicSize = camOrthographicSize * (1 + (1 - size.y));
                // else
                if(size.x < 1)
                {
                    cam.orthographicSize = camOrthographicSize * size.x;
                    targetObj.position = new Vector3(lastTargetPos.x, lastTargetPos.y - cameraPosDownRate, lastTargetPos.z);
                }
                lastResolution = currentScreenResolution;
            }
            // var vcam = ActiveVcam;
            // if (vcam != null)
            // {
            //     // Camera.main.rect = new Rect(default, size) { center = rectCenter };
            //     if (size.y >= 1)
            //         vcam.m_Lens.OrthographicSize = camOrthographicSize * size.x;
            //     else
            //         vcam.m_Lens.OrthographicSize = camOrthographicSize * (1 + (1 - size.y));
            //     lastResolution = currentScreenResolution;
            // }
        }
    }
}