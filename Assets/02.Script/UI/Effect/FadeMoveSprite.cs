using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace UI.Effect
{
    public class FadeMoveSprite: MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _sprite;
        private Vector2 originVec;
        private Transform r;
        void CheckInitial()
        {
            if(_sprite == null) _sprite = transform.GetComponentInChildren<SpriteRenderer>();
            r = _sprite.GetComponent<Transform>();
            originVec = r.localPosition;
        }
        void OnEnable()
        {
            if(r == null) CheckInitial();
            r.localPosition = originVec;
            _sprite.color = new Color(1,1,1,0);

            //* 이동 효과
            r.DOLocalMoveY(originVec.y + 0.5f, 1f).SetEase(Ease.Linear);

            //* 페이드 효과
            _sprite.DOFade(1, 0.25f).SetEase(Ease.InSine);
            _sprite.DOFade(0, 0.5f).SetEase(Ease.InSine).SetDelay(0.8f);
        }

    }
}