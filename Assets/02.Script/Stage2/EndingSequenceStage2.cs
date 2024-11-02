using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using InGame;
using UnityEngine;

public class EndingSequenceStage2 : AutoTaskControl
{
    [SerializeField] private AnimationObj drivingBunny;
    [SerializeField] private GameObject drivingBunnyShockFace;
    [SerializeField] private AnimationObj ghostBunny;
    [SerializeField] private AnimationObj sittingBunny;
    [SerializeField] private Transform NightBunnyGroup;
    [SerializeField] private Cart cart;

    private SpriteRenderer ghostBunnyRenderer;
    private SpriteRenderer ghostBunnyRendererFeet;
    private void Start()
    {
        ghostBunnyRenderer = ghostBunny.GetComponent<SpriteRenderer>();
        ghostBunnyRendererFeet = ghostBunny.transform.GetChild(0).GetComponent<SpriteRenderer>();
        ghostBunny.gameObject.SetActive(false);
    }
    public async UniTask StartSequence()
    {
        drivingBunny.ChangeAnimation("Angry");
        await UniTask.WaitForSeconds(2f);
        await drivingBunny.transform.DOScaleX(-0.34f, 0.15f).SetEase(Ease.Linear);
        SlideDown().Forget();
        
        drivingBunny.ChangeAnimation("Walk");

        ghostBunny.gameObject.SetActive(true);
        Ghost().Forget();
        await UniTask.WaitForSeconds(1.65f);
        drivingBunny.ChangeAnimation("Shock");
        drivingBunnyShockFace.SetActive(true);
        sittingBunny.ChangeAnimation("ShockDown");
        await UniTask.WaitForSeconds(1f);
        await drivingBunny.transform.DOScale(0.34f, 0.15f).SetEase(Ease.Linear);
        drivingBunny.ChangeAnimation("Walk");
        
        await UniTask.WhenAll(
            drivingBunny.transform.DOLocalMoveX(2.343f, 1.5f).SetEase(Ease.Linear).WithCancellation(destroyCancellationToken),
            sittingBunny.transform.DOScale(0.34f, 0.15f).SetEase(Ease.Linear).SetDelay(1f).WithCancellation(destroyCancellationToken)
        );
        sittingBunny.ChangeAnimation("SitCart");
        drivingBunny.ChangeAnimation("Sit");
        await drivingBunny.transform.DOLocalMoveY(-0.716f, 0.3f).SetEase(Ease.OutCubic);
        await UniTask.WaitForSeconds(1f);
        cart.StartWheel();
        await NightBunnyGroup.DOLocalMoveX(13.89f, 2f).SetEase(Ease.Linear);
    }

    private async UniTaskVoid SlideDown()
    {
        sittingBunny.ChangeAnimation("SlideDown");
        await UniTask.WaitForSeconds(1f);
        await sittingBunny.transform.DOScaleX(-0.34f, 0.15f).SetEase(Ease.Linear);
    }
    private async UniTaskVoid Ghost()
    {
        await UniTask.WhenAll(
            drivingBunny.transform.DOLocalMoveX(-3f, 2f).SetEase(Ease.Linear).WithCancellation(destroyCancellationToken),
            ghostBunnyRenderer.DOFade(1, 1f).SetEase(Ease.Linear).SetDelay(1f).WithCancellation(destroyCancellationToken),
            ghostBunnyRendererFeet.DOFade(1, 1).SetEase(Ease.Linear).SetDelay(1f).WithCancellation(destroyCancellationToken),
            ghostBunny.transform.DOLocalMove(new Vector3(-1.656f,4.122f), 3f).SetEase(Ease.OutCubic).SetDelay(1f).WithCancellation(destroyCancellationToken)
        );
    }

}
