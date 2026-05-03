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
        public string PhotoKey { get; private set; } // Added for Photo stickers
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

        public void InitPhoto(CollectionData.PlacedStickerData data, Sprite photoSprite, string photoName, CollectionDiaryViewModel diary)
        {
            rectTransform = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();
            DiaryViewModel = diary;
            Collection = null; // Null because it's a photo
            PhotoKey = photoName;
            StickerId = data.id;

            if (stickerImage != null)
                stickerImage.sprite = photoSprite;

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
                collectionKey = Collection != null ? Collection.name : PhotoKey,
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
            // 델타(delta)를 기반으로 이동하여 PageView_UI의 민감도 설정을 따릅니다.
            // ScreenPointToLocalPointInRectangle을 두 번 호출하여 현재와 이전 위치의 로컬 차이를 계산합니다.
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform.parent as RectTransform, eventData.position, eventData.pressEventCamera, out Vector2 currentPos) &&
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform.parent as RectTransform, eventData.position - eventData.delta, eventData.pressEventCamera, out Vector2 lastPos))
            {
                // Ensure this sticker becomes the selected one if dragged directly
                DiaryViewModel.SelectSticker(this);
                
                Vector2 localDelta = currentPos - lastPos;
                Vector3 newPos = rectTransform.localPosition + (Vector3)localDelta;

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

        private void OnResizeRotateDown(PointerEventData _)
        {
            DiaryViewModel.SelectSticker(this);
        }

        private void OnResizeRotateDrag(PointerEventData data)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform.parent as RectTransform, data.position, data.pressEventCamera, out Vector2 currentPos) &&
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform.parent as RectTransform, data.position - data.delta, data.pressEventCamera, out Vector2 lastPos))
            {
                Vector2 localDelta = currentPos - lastPos;
                Vector2 toPointer = currentPos - (Vector2)rectTransform.localPosition;
                float dist = toPointer.magnitude;

                if (dist > 0.01f)
                {
                    Vector2 radial = toPointer / dist;

                    // Scale: delta의 반경 방향 성분으로 incremental 스케일
                    float radialDelta = Vector2.Dot(localDelta, radial);
                    float scaleFactor = (dist + radialDelta) / dist;
                    Vector3 newScale = rectTransform.localScale * scaleFactor;
                    newScale.x = Mathf.Clamp(newScale.x, 0.5f, 2.5f);
                    newScale.y = Mathf.Clamp(newScale.y, 0.5f, 2.5f);
                    newScale.z = Mathf.Clamp(newScale.z, 0.5f, 2.5f);
                    rectTransform.localScale = newScale;

                    // Rotate: delta의 접선 방향 성분으로 incremental 회전
                    Vector2 tangent = new(-radial.y, radial.x);
                    float tangentialDelta = Vector2.Dot(localDelta, tangent);
                    float angleChange = tangentialDelta / dist * Mathf.Rad2Deg;
                    Vector3 euler = rectTransform.localEulerAngles;
                    rectTransform.localEulerAngles = new Vector3(euler.x, euler.y, euler.z + angleChange);
                }

                DiaryViewModel.SaveStickerState(this);
            }
        }
    }
}