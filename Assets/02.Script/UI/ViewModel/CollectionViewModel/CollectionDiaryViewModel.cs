using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using SO;
using Manager;
using Data;
using UI;
using UnityWeld.Binding;

[Binding]
public class CollectionDiaryViewModel : MonoBehaviour, IPointerDownHandler
{
    [Header("Sticker Settings")]
    public Transform stickerContainer; // Where stickers are spawned
    public GameObject stickerPrefab;   // Must have PlacedStickerUI component
    public RectTransform boundaryRect; // Limits the sticker's movement area

    private List<PlacedStickerUI> _spawnedStickers = new List<PlacedStickerUI>();
    private PlacedStickerUI _currentSelected;
    

    private void OnEnable()
    {
        LoadAllStickers();
    }

    private void OnDisable()
    {
        ClearStickerObjects();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // When the background itself is clicked (and not intercepted by a sticker)
        SelectSticker(null);
    }

    public void LoadAllStickers()
    {
        ClearStickerObjects();
        var stickers = Global.UserDataManager.GetAllPlacedStickers();
        if (stickers != null)
        {
            foreach (var data in stickers)
            {
                var collectionSO = Global.UserDataManager.FindCollectionByName(data.collectionKey);
                if (collectionSO != null)
                {
                    InstantiateSticker(data, collectionSO);
                }
                else
                {
                    // Fallback: If not a CollectionSO, might be a Photo from ES3
                    if (ES3.FileExists(data.collectionKey))
                    {
                        Texture2D tex = ES3.LoadImage(data.collectionKey);
                        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                        InstantiatePhotoSticker(data, sprite, data.collectionKey);
                    }
                }
            }
        }
    }
    [Binding]
    public void OnClickShareButton()
    {
        StartCoroutine(TakeScreenshotAndShareCoroutine());
    }

    private System.Collections.IEnumerator TakeScreenshotAndShareCoroutine()
    {
        // UI가 모두 렌더링 된 후 프레임이 끝날 때까지 대기
        yield return new WaitForEndOfFrame();

        // 1. boundaryRect의 화면 상 픽셀 영역 계산
        Vector3[] corners = new Vector3[4];
        boundaryRect.GetWorldCorners(corners);

        Canvas canvas = boundaryRect.GetComponentInParent<Canvas>();
        Camera cam = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;

        Vector2 min = RectTransformUtility.WorldToScreenPoint(cam, corners[0]); // 좌측 하단
        Vector2 max = RectTransformUtility.WorldToScreenPoint(cam, corners[2]); // 우측 상단

        int width = Mathf.RoundToInt(max.x - min.x);
        int height = Mathf.RoundToInt(max.y - min.y);
        int startX = Mathf.RoundToInt(min.x);
        int startY = Mathf.RoundToInt(min.y);

        width = Mathf.Clamp(width, 0, Screen.width - startX);
        height = Mathf.Clamp(height, 0, Screen.height - startY);

        if (width <= 0 || height <= 0)
        {
            Debug.LogWarning("캡처할 영역이 유효하지 않습니다.");
            yield break;
        }

        // 2. 화면에서 픽셀 읽어오기 (스크린샷)
        Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(startX, startY, width, height), 0, 0);
        screenshot.Apply();

        // 3. 파일로 저장
        string folderPath = Application.persistentDataPath;
        string filePath = System.IO.Path.Combine(folderPath, "StickerCollection_Share.png");
        
        byte[] bytes = screenshot.EncodeToPNG();
        System.IO.File.WriteAllBytes(filePath, bytes);
        
        Destroy(screenshot);

        Debug.Log($"[Share] 스크린샷 저장 완료: {filePath}");

        // 4. 모바일 기기의 클립보드 복사 및 공유 기능 확장
        // 유니티 기본 기능으로는 모바일 이미지 클립보드 복사가 지원되지 않으므로
        // 보통 'NativeShare (무료)' 에셋 등을 활용하여 아래와 같이 호출합니다.
        // ----------------------------------------------------
        // new NativeShare().AddFile(filePath)
        //     .SetSubject("내 컬렉션 보러와!")
        //     .SetText("내가 모은 스티커 다이어리야")
        //     .Share();
        // ----------------------------------------------------

        // 임시로 에디터 편의상 파일 경로를 클립보드 텍스트에 복사
        GUIUtility.systemCopyBuffer = filePath;
    }

    private void ClearStickerObjects()
    {
        foreach (var st in _spawnedStickers)
        {
            if (st != null)
                Destroy(st.gameObject);
        }
        _spawnedStickers.Clear();
    }

    // Called when a user selects a sticker to place from the ScrollView
    public void PlaceNewSticker(CollectionSO collection)
    {
        if (collection == null) return;

        // Check if we have enough
        int currentAmount = Global.CollectionManager.GetCollectionCount(collection);
        if (currentAmount > 0)
        {
            // Reduce amount
            Global.UserDataManager.RemoveCollection(collection, 1);

            // Create Data
            var data = new CollectionData.PlacedStickerData
            {
                id = System.Guid.NewGuid().ToString(),
                collectionKey = collection.name,
                posX = 0, posY = 0, posZ = 0,
                rotX = 0, rotY = 0, rotZ = 0,
                scaleX = 1, scaleY = 1, scaleZ = 1
            };

            // Save new data
            Global.UserDataManager.SavePlacedSticker(data);

            // Instantiate only if active (it will be loaded on OnEnable otherwise)
            if (this.gameObject.activeInHierarchy)
            {
                var ui = InstantiateSticker(data, collection);
                SelectSticker(ui);
            }
        }
        else
        {
            Debug.Log($"Not enough {collection.collectionName} stickers!");
        }
    }

    private PlacedStickerUI InstantiateSticker(CollectionData.PlacedStickerData data, CollectionSO collectionSO)
    {
        GameObject go = Instantiate(stickerPrefab, stickerContainer);
        var stickerUI = go.GetComponent<PlacedStickerUI>();
        if (stickerUI != null)
        {
            stickerUI.Init(data, collectionSO, this);
            _spawnedStickers.Add(stickerUI);
        }
        go.SetActive(true);
        return stickerUI;
    }

    public void PlaceNewPhotoSticker(string photoName, Sprite photoSprite)
    {
        if (string.IsNullOrEmpty(photoName) || photoSprite == null) return;

        // Check if already placed
        var stickers = Global.UserDataManager.GetAllPlacedStickers();
        if (stickers != null && stickers.Exists(x => x.collectionKey == photoName))
        {
            Debug.Log($"{photoName} is already placed!");
            return;
        }

        // Create Data
        var data = new CollectionData.PlacedStickerData
        {
            id = System.Guid.NewGuid().ToString(),
            collectionKey = photoName,
            posX = 0, posY = 0, posZ = 0,
            rotX = 0, rotY = 0, rotZ = 0,
            scaleX = 1, scaleY = 1, scaleZ = 1
        };

        // Save new data
        Global.UserDataManager.SavePlacedSticker(data);

        // Instantiate only if active
        if (this.gameObject.activeInHierarchy)
        {
            var ui = InstantiatePhotoSticker(data, photoSprite, photoName);
            SelectSticker(ui);
        }
    }

    private PlacedStickerUI InstantiatePhotoSticker(CollectionData.PlacedStickerData data, Sprite photoSprite, string photoName)
    {
        GameObject go = Instantiate(stickerPrefab, stickerContainer);
        var stickerUI = go.GetComponent<PlacedStickerUI>();
        if (stickerUI != null)
        {
            stickerUI.InitPhoto(data, photoSprite, photoName, this);
            _spawnedStickers.Add(stickerUI);
        }
        go.SetActive(true);
        return stickerUI;
    }

    public void SelectSticker(PlacedStickerUI sticker)
    {
        if (_currentSelected != null && _currentSelected != sticker)
        {
            _currentSelected.SetSelected(false);
        }
        _currentSelected = sticker;
        if (_currentSelected != null)
        {
            _currentSelected.SetSelected(true);
        }
    }

    public void SaveStickerState(PlacedStickerUI sticker)
    {
        if (sticker == null) return;
        Global.UserDataManager.SavePlacedSticker(sticker.SaveData());
    }

    public void RemoveSticker(PlacedStickerUI sticker)
    {
        if (sticker == null) return;

        Global.UserDataManager.RemovePlacedSticker(sticker.StickerId);
        _spawnedStickers.Remove(sticker);
        Destroy(sticker.gameObject);
    }

    [Binding]
    public void OnClickClearAllStickers()
    {
        Global.UserDataManager.ClearAllPlacedStickers();
        ClearStickerObjects();
    }
}
