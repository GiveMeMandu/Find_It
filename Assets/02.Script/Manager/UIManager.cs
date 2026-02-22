
using System.Collections.Generic;
using UI;
using UnityEngine;

namespace Manager
{
    public partial class UIManager : MonoBehaviour
    {
        public MouseUIController mouseUIController;
        
        [SerializeField] private Canvas canvas;
        
        private void OnEnable()
        {
            if (mouseUIController != null)
            {
                mouseUIController.Init();
            }
        }
        
        private void Start()
        {
            // 로딩 완료 후 초기화
            if (mouseUIController != null)
            {
                mouseUIController.Init();
            }
            
            // canvas.renderMode = RenderMode.ScreenSpaceCamera;
            // canvas.worldCamera = Camera.main;
            // canvas.sortingLayerID = SortingLayer.NameToID("UI");
            // canvas.sortingOrder = 0;
        }
    }
}