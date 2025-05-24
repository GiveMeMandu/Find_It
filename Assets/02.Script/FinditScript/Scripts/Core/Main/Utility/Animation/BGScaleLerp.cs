using UnityEngine;
using DeskCat.FindIt.Scripts.Core.Main.System;

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
            // 부모에 HiddenObj가 있고, Collider가 있는지 확인하여 위치 설정
            if (transform.parent != null && transform.parent.TryGetComponent<HiddenObj>(out var parentHiddenObj))
            {
                // Collider 체크 및 위치 설정
                if (parentHiddenObj.TryGetComponent<Collider>(out var collider))
                {
                    transform.position = collider.bounds.center;
                    InitialBounds = collider.bounds;
                }
                else if (parentHiddenObj.TryGetComponent<Collider2D>(out var collider2D))
                {
                    transform.position = collider2D.bounds.center;
                    InitialBounds = collider2D.bounds;
                }
            }

            if (UseCustomScale == false && transform.localScale == Vector3.zero)
            {
                FromScale = Vector3.zero;
                
                // HiddenObj의 spriteRenderer 사용
                if (transform.parent != null && transform.parent.TryGetComponent<HiddenObj>(out var hiddenObj) && hiddenObj.spriteRenderer != null)
                {
                    var bgSprite = GetComponent<SpriteRenderer>();
                    if (bgSprite != null)
                    {
                        // 스프라이트 실제 크기 사용
                        float targetWidth = hiddenObj.spriteRenderer.sprite.bounds.size.x;
                        float targetHeight = hiddenObj.spriteRenderer.sprite.bounds.size.y;
                        float bgWidth = bgSprite.sprite.bounds.size.x;
                        float bgHeight = bgSprite.sprite.bounds.size.y;
                        
                        float scaleX = targetWidth / bgWidth;
                        float scaleY = targetHeight / bgHeight;
                        float scale = Mathf.Max(scaleX, scaleY) * 1.5f; // 약간 더 크게 설정
                        
                        ToScale = new Vector3(scale, scale, 1f);
                        
                        // sorting order 설정
                        bgSprite.sortingOrder = hiddenObj.spriteRenderer.sortingOrder - 1;
                    }
                    else
                    {
                        ToScale = Vector3.one;
                    }
                }
                else
                {
                    ToScale = Vector3.one;
                }
            }else
            {
                ToScale = transform.localScale;
            }
            
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            transform.localScale = FromScale;
        }

        private void Update()
        {
            var value = GetLerpValue(currentTime);
            transform.localScale = Vector3.Lerp(FromScale, ToScale, value);
        }
    }
}