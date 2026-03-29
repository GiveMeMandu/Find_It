using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SO;
using Data;
using UnityWeld;
using UnityWeld.Binding;

namespace UI
{
    [Binding]
    public class PlacedStickerUI : ViewModel, IPointerDownHandler, IDragHandler
    {
        public string StickerId { get; private set; }
        public CollectionSO Collection { get; private set; }
        public CollectionDiaryViewModel DiaryViewModel { get; private set; }

        [Header("UI References")]
        public Image stickerImage;
        // Optional reference image you can assign in inspector —
        // its sprite size will be used to set the sticker size when requested
        public Image sizeReferenceImage;
        public GameObject selectionOutline;
        public Button closeButton;
        public UnityEngine.EventSystems.EventTrigger resizeRotateArea; // Or create a dedicated ResizeRotateHandle

        private RectTransform rectTransform;
        private Canvas canvas;
        private Vector2 originalLocalPointerPosition;
        private Vector3 originalPanelLocalPosition;

        private Vector2 _sizeDelta;

        [Binding]
        public Vector2 SizeDelta
        {
            get => _sizeDelta;
            set
            {
                _sizeDelta = value;
                OnPropertyChanged(nameof(SizeDelta));
            }
        }

        public void Init(CollectionData.PlacedStickerData data, CollectionSO collectionSO, CollectionDiaryViewModel diary)
        {
            rectTransform = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();
            DiaryViewModel = diary;
            Collection = collectionSO;
            StickerId = data.id;

            if (stickerImage != null)
                stickerImage.sprite = collectionSO.collectionImage;

            // Compute size according to reference container
            UpdateSizeFromReference();

            LoadData(data);

            if (closeButton != null)
                closeButton.onClick.AddListener(OnCloseClicked);
            
            // Set up resize handler if not using a custom script
            SetupResizeRotateHandle();
            
            SetSelected(false);
        }


        // Set SizeDelta using sizeReferenceImage as the bounding container
        public void UpdateSizeFromReference()
        {
            if (stickerImage != null && stickerImage.sprite != null && sizeReferenceImage != null)
            {
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(sizeReferenceImage.rectTransform);

                var spriteRect = stickerImage.sprite.rect;
                float spriteW = spriteRect.width;
                float spriteH = spriteRect.height;

                float containerW = sizeReferenceImage.rectTransform.rect.width;
                float containerH = sizeReferenceImage.rectTransform.rect.height;

                if (spriteW > 0 && spriteH > 0 && containerW > 0 && containerH > 0)
                {
                    float scale = Mathf.Min(containerW / spriteW, containerH / spriteH);
                    SizeDelta = new Vector2(spriteW * scale, spriteH * scale);
                }
            }
        }

        public void LoadData(CollectionData.PlacedStickerData data)
        {
            rectTransform.anchoredPosition3D = new Vector3(data.posX, data.posY, data.posZ);
            rectTransform.localEulerAngles = new Vector3(data.rotX, data.rotY, data.rotZ);
            rectTransform.localScale = new Vector3(data.scaleX, data.scaleY, data.scaleZ);
        }

        public CollectionData.PlacedStickerData SaveData()
        {
            return new CollectionData.PlacedStickerData
            {
                id = StickerId,
                collectionKey = Collection.name,
                posX = rectTransform.anchoredPosition3D.x,
                posY = rectTransform.anchoredPosition3D.y,
                posZ = rectTransform.anchoredPosition3D.z,
                rotX = rectTransform.localEulerAngles.x,
                rotY = rectTransform.localEulerAngles.y,
                rotZ = rectTransform.localEulerAngles.z,
                scaleX = rectTransform.localScale.x,
                scaleY = rectTransform.localScale.y,
                scaleZ = rectTransform.localScale.z
            };
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            rectTransform.SetAsLastSibling();
            DiaryViewModel.SelectSticker(this);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform.parent as RectTransform, eventData.position, eventData.pressEventCamera, out originalLocalPointerPosition);
            originalPanelLocalPosition = rectTransform.localPosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform.parent as RectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPointerPosition))
            {
                // Ensure this sticker becomes the selected one if dragged directly
                DiaryViewModel.SelectSticker(this);
                
                Vector3 offsetToOriginal = localPointerPosition - originalLocalPointerPosition;
                Vector3 newPos = originalPanelLocalPosition + offsetToOriginal;

                // Clamp to boundary if provided
                if (DiaryViewModel.boundaryRect != null)
                {
                    Vector3[] boundaryWorldCorners = new Vector3[4];
                    DiaryViewModel.boundaryRect.GetWorldCorners(boundaryWorldCorners);
                    
                    RectTransform parentRect = rectTransform.parent as RectTransform;
                    
                    // Convert boundary world corners to sticker parent's local space
                    Vector3 localBottomLeft = parentRect.InverseTransformPoint(boundaryWorldCorners[0]);
                    Vector3 localTopRight = parentRect.InverseTransformPoint(boundaryWorldCorners[2]);
                    
                    // 스티커의 중심을 기준으로 영역 제한 (minX, maxX, minY, maxY)
                    newPos.x = Mathf.Clamp(newPos.x, localBottomLeft.x, localTopRight.x);
                    newPos.y = Mathf.Clamp(newPos.y, localBottomLeft.y, localTopRight.y);
                }

                rectTransform.localPosition = newPos;
                DiaryViewModel.SaveStickerState(this);
            }
        }

        public void SetSelected(bool isSelected)
        {
            if (selectionOutline != null)
                selectionOutline.SetActive(isSelected);
            if (closeButton != null)
                closeButton.gameObject.SetActive(isSelected);
            if (resizeRotateArea != null)
                resizeRotateArea.gameObject.SetActive(isSelected);
        }

        private void OnCloseClicked()
        {
            DiaryViewModel.RemoveSticker(this);
        }

        private void SetupResizeRotateHandle()
        {
            if (resizeRotateArea == null) return;

            UnityEngine.EventSystems.EventTrigger.Entry dragEntry = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = EventTriggerType.Drag };
            dragEntry.callback.AddListener((data) => { OnResizeRotateDrag((PointerEventData)data); });
            resizeRotateArea.triggers.Add(dragEntry);

            UnityEngine.EventSystems.EventTrigger.Entry downEntry = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            downEntry.callback.AddListener((data) => { OnResizeRotateDown((PointerEventData)data); });
            resizeRotateArea.triggers.Add(downEntry);
        }

        private Vector2 _originalPointer;
        private Vector3 _originalScale;
        private float _originalAngle;
        private Vector2 _originalDir;

        private void OnResizeRotateDown(PointerEventData data)
        {
            DiaryViewModel.SelectSticker(this);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform.parent as RectTransform, data.position, data.pressEventCamera, out _originalPointer);
            _originalScale = rectTransform.localScale;
            
            // For rotation
            Vector2 rectLocalPos = rectTransform.localPosition;
            _originalDir = (_originalPointer - rectLocalPos).normalized;
            _originalAngle = rectTransform.localEulerAngles.z;
        }

        private void OnResizeRotateDrag(PointerEventData data)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform.parent as RectTransform, data.position, data.pressEventCamera, out Vector2 currentPointer))
            {
                Vector2 rectLocalPos = rectTransform.localPosition;
                Vector2 currentDir = (currentPointer - rectLocalPos).normalized;

                // Scale (distance based)
                float initialDist = Vector2.Distance(_originalPointer, rectLocalPos);
                float currentDist = Vector2.Distance(currentPointer, rectLocalPos);
                
                    if (initialDist > 0.01f)
                    {
                        float scaleFactor = currentDist / initialDist;
                        Vector3 newScale = _originalScale * scaleFactor;
                        
                        // Limit scale between 0.5x and 2.5x
                        newScale.x = Mathf.Clamp(newScale.x, 0.5f, 2.5f);
                        newScale.y = Mathf.Clamp(newScale.y, 0.5f, 2.5f);
                        newScale.z = Mathf.Clamp(newScale.z, 0.5f, 2.5f);
                        
                        rectTransform.localScale = newScale;
                    }

                    // Rotate
                float angleOffset = Vector2.SignedAngle(_originalDir, currentDir);
                rectTransform.localEulerAngles = new Vector3(0, 0, _originalAngle + angleOffset);

                DiaryViewModel.SaveStickerState(this);
            }
        }
    }
}