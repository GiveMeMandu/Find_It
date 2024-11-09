using UnityEngine;

namespace DeskCat.FindIt.Scripts.Core.Main.Utility.Animation
{
    public class BGScaleLerp : FindItLerp
    {
        public bool UseCustomScale = false;
        public Vector3 FromScale;
        public Vector3 ToScale;

        // 초기 bounds 정보 저장
        public Bounds InitialBounds { get; private set; }

        private void Awake()
        {
            if (UseCustomScale == false)
            {
                FromScale = Vector3.zero;
                ToScale = transform.localScale;
            }
            
            // 초기 bounds 정보 저장
            if (TryGetComponent<SpriteRenderer>(out var spriteRenderer))
            {
                InitialBounds = spriteRenderer.bounds;
            }
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            transform.localScale = Vector3.zero;
        }

        private void Update()
        {
            var value = GetLerpValue(currentTime);
            transform.localScale = (FromScale * (1-value)) + (ToScale * value);
        }
    }
}