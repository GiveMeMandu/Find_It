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
            }
        }
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
