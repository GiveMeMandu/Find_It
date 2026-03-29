using UnityEngine;
using UnityWeld;
using UnityWeld.Binding;
using UI.Page;

namespace UI
{
    public enum CollectionTabType
    {
        Sticker,
        Camera,
        StickerBook
    }

    [Binding]
    public class CollectionPageTabMenuViewModel : ViewModel
    {
        [SerializeField] private CollectionPage _collectionPage;
        [SerializeField] private CollectionTabType _tabType;

        private bool _isActive;

        [Binding]
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive == value) return;
                _isActive = value;
                OnPropertyChanged(nameof(IsActive));
            }
        }

        public CollectionTabType TabType => _tabType;

        [Binding]
        public void OnClickTab()
        {
            if (_collectionPage == null) return;

            switch (_tabType)
            {
                case CollectionTabType.Sticker:
                    _collectionPage.OnClickStickerViewButton();
                    break;
                case CollectionTabType.Camera:
                    _collectionPage.OnClickAlbumViewButton();
                    break;
                case CollectionTabType.StickerBook:
                    _collectionPage.OnClickStickerBookViewButton();
                    break;
            }
        }
    }
}
