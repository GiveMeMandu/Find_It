using UnityEngine;

namespace DeskCat.FindIt.Scripts.Core.Main.Utility.ClickedFunction
{
    public class ToggleSprite : MonoBehaviour
    {
        public SpriteRenderer targetSprite;

        public bool LoopSprite;
        public Sprite[] SpriteList;

        private int currentIndex;

        private void Start()
        {
            if (targetSprite == null)
                targetSprite = GetComponent<SpriteRenderer>();
        }

        public void ToggleNextSprite()
        {
            if (targetSprite == null) return;
            if (SpriteList.Length <= 0) return;

            currentIndex++;
            if (currentIndex >= SpriteList.Length)
            {
                currentIndex = LoopSprite ? 0 : SpriteList.Length - 1;
            }

            targetSprite.sprite = SpriteList[currentIndex];
        }
    }
}