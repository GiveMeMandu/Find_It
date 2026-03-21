
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SnowRabbit
{
    // 타입 정의
    public enum ImageType
    {
        Theme1,
        Theme2,
        Theme3,
    }

    [System.Serializable]
    public class ImageColorSet
    {
        public ImageType type;         // 타입 지정
        public List<Image> images;     // 해당 타입에 속하는 이미지들
        public Color targetColor;      // 해당 타입의 색상
    }

    public class ImageColorController : MonoBehaviour
    {
        [SerializeField] private List<ImageColorSet> imageColorSets = new List<ImageColorSet>();

        // 특정 타입의 이미지 색상을 적용
        public void ApplyColor(ImageType type)
        {
            foreach (var set in imageColorSets)
            {
                if (set.type == type)
                {
                    foreach (var img in set.images)
                    {
                        if (img != null)
                            img.color = set.targetColor;
                    }
                }
            }
        }

        // 특정 타입의 색상만 변경 (이미지들은 그대로 유지)
        public void SetColor(ImageType type, Color newColor)
        {
            foreach (var set in imageColorSets)
            {
                if (set.type == type)
                {
                    set.targetColor = newColor;
                }
            }
        }
    }

}
