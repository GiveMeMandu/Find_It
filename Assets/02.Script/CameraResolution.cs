using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BunnyCafe.InGame
{
    public class CameraResolution : MonoBehaviour
    {
        private readonly Vector2 targetAspectRatio = new(16, 9);
        private readonly Vector2 rectCenter = new(0.5f, 0.5f);
        private Vector2 lastResolution;

        private float camOrthographicSize;

        private void Start()
        {
            camOrthographicSize = Camera.main.orthographicSize;
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

            var cam = Camera.main;
            if (cam != null)
            {
                // Camera.main.rect = new Rect(default, size) { center = rectCenter };
                // if (size.x >= 1)
                //     cam.orthographicSize = camOrthographicSize * (1 + (1 - size.y));
                // else
                if(size.x < 1)
                    cam.orthographicSize = camOrthographicSize * size.x;
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