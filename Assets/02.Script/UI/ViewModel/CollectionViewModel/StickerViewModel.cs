using UnityEngine;
using UnityEngine.UI;
using UnityWeld.Binding;
using SO;
using I2.Loc;

namespace UI
{
    /// <summary>
    /// GameEndViewModel의 GroupView에서 사용되는 개별 스티커(컬렉션) 아이템 ViewModel입니다.
    /// </summary>
    [Binding]
    public class StickerViewModel : UnityWeld.ViewModel
    {
        public Image IconImage;

        private Sprite _image;
        [Binding]
        public Sprite Image
        {
            get => _image;
            set
            {
                _image = value;
                OnPropertyChanged(nameof(Image));
            }
        }

        private string _stickerName;
        [Binding]
        public string StickerName
        {
            get => _stickerName;
            set
            {
                _stickerName = value;
                OnPropertyChanged(nameof(StickerName));
            }
        }

        /// <summary>
        /// CollectionSO 데이터로 스티커 UI를 초기화합니다.
        /// </summary>
        public void Init(CollectionSO collection)
        {
            if (collection == null) return;

            Image = collection.collectionImage;
            StickerName = LocalizationManager.GetTranslation(collection.collectionName);

            if (IconImage != null && Image != null)
            {
                IconImage.sprite = Image;
            }
        }
    }
}
