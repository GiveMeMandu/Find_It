using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;
using System.Threading;
using NaughtyAttributes;
using TMPro;
using InGame;
using Manager;

public class IngameCoinLayer : AutoTaskControl
{
    [Header("애니메이션 오브젝트")]
    [SerializeField] private AnimationObj animationObj;
    [Label("아이콘 오브젝트")]
    public GameObject iconObj;

    [Header("애니메이션 상태명")]
    [SerializeField] private string showAnimation = "HUD_Resource_Show";
    [SerializeField] private string updateAnimation = "HUD_Resource_Update";
    [SerializeField] private string idleAnimation = "HUD_Resource_Idle";
    [SerializeField] private string hideAnimation = "HUD_Resource_Hide";
    public TextMeshProUGUI MoneyText;
    public TextMeshProUGUI AddMoneyText; // 재화 획득 알림용 텍스트

    [Header("UI 활성화 설정")]
    public bool EnableMoneyText = true; // 메인 텍스트 활성화 여부
    public Animation EnableAddMoneyTextAnimation; // 획득 알림 텍스트 애니메이션

    [Header("카운트 연출 설정")]
    [Label("추가 코인 획득 알림 표시 시간 (초)")]
    public float AddMoneyDisplayDuration = 2.0f; // 획득 알림 표시 시간 (초)
    [Label("텍스트 카운트 후 대기 시간 (초)")]
    public float CountAfterDelay = 2.0f; // 획득 알림 표시 시간 (초)
    [Label("텍스트 카운트 시작 지연시간 (초)")]
    public float CountStartDelay = 1.5f; // 텍스트 카운트 시작 지연시간 (초)
    [Label("MoneyText 카운트 올라가는 연출 시간 (초)")]
    public float CountDuration = 1.0f; // MoneyText 카운트 연출 시간 (초)

    [Header("이벤트")]
    public UnityEvent OnEnableAddMoneyText; // 획득 알림 텍스트 활성화 시
    public UnityEvent OnGainedMoneyDisplayComplete; // 2초 후 획득 알림 완료 시
    public UnityEvent OnMoneyCountComplete; // MoneyText 카운트 연출 완료 시

    private BigInteger _previousCoinValue = BigInteger.Zero; // 이전 코인 값
    private bool _isCountingUp = false; // 카운트업 중인지 여부
    private BigInteger _accumulatedGainedMoney = BigInteger.Zero; // 누적된 획득 코인
    private CancellationTokenSource _displayCancellation; // UI 표시용 토큰
    protected override void OnEnable()
    {
        Init();
    }
    public void Init()
    {
        // 초기값 설정
        MoneyText.text = FormatMoney(BigInteger.Zero);
        AddMoneyText.text = "";

        // 초기에는 획득 알림 텍스트 활성화 (비활성화하지 않음)
        if (EnableAddMoneyTextAnimation != null)
        {
            EnableAddMoneyTextAnimation.gameObject.SetActive(true);
        }

        // 이전 코인 값 초기화
        _previousCoinValue = Global.CoinManager.GetCoinValue();

        // Subscribe to coin value changes
        Global.CoinManager.OnCoinValueChanged += OnCoinValueChanged;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Global.CoinManager.OnCoinValueChanged -= OnCoinValueChanged;

        // UI 표시 토큰 정리
        if (_displayCancellation != null)
        {
            _displayCancellation.Cancel();
            _displayCancellation.Dispose();
        }
    }

    private void OnCoinValueChanged(object sender, string newCoinValue)
    {
        // 코인 재화가 변경되면 획득 알림 처리
        HandleMoneyAdd();
    }

    private void HandleMoneyAdd()
    {
        if (Global.CoinManager == null)
        {
            return;
        }

        BigInteger currentCoinValue = Global.CoinManager.GetCoinValue();
        BigInteger gainedAmount = currentCoinValue - _previousCoinValue;

        if (gainedAmount <= 0)
        {
            return;
        }

        _previousCoinValue = currentCoinValue;
        _accumulatedGainedMoney += gainedAmount;

        // 업데이트 애니메이션을 강제로 처음부터 재생
        animationObj.ChangeAnimation(updateAnimation, force: true);

        // 누적금액이 있다면 UI 활성화 및 애니메이션 재생
        if (_accumulatedGainedMoney > BigInteger.Zero)
        {
            // 획득 알림 텍스트 활성화 및 애니메이션 강제 재생
            if (EnableAddMoneyTextAnimation != null)
            {
                EnableAddMoneyTextAnimation.gameObject.SetActive(true);

                // 애니메이션 완전 정지 후 처음부터 강제 재생
                EnableAddMoneyTextAnimation.Stop();
                EnableAddMoneyTextAnimation.Rewind();
                EnableAddMoneyTextAnimation.Play();

            }
            OnEnableAddMoneyText?.Invoke();
            AddMoneyText.text = $"+{FormatMoney(_accumulatedGainedMoney)}";
        }

        // 기존 토큰들 취소 (새로운 코인이 추가되면 모든 대기시간 다시 시작)
        if (destroyCancellation != null)
        {
            destroyCancellation.Cancel();
            destroyCancellation.Dispose();
        }
        if (_displayCancellation != null)
        {
            _displayCancellation.Cancel();
            _displayCancellation.Dispose();
        }

        destroyCancellation = new CancellationTokenSource();
        _displayCancellation = new CancellationTokenSource();

        // 금액 표시 시간 동안 추가 금액 대기 및 애니메이션 재생 관리
        if (_accumulatedGainedMoney > BigInteger.Zero)
        {
            ManageAddMoneyDisplayAsync(_displayCancellation.Token).Forget();
        }

        // 새 UniTask들 시작 (모든 대기시간 다시 시작)
        ShowAddMoneyAsync(destroyCancellation.Token).Forget();
    }

    private async UniTaskVoid ShowAddMoneyAsync(CancellationToken cancellationToken)
    {
        // 텍스트 카운트 시작 지연시간만큼 대기
        await UniTask.WaitForSeconds(CountStartDelay, cancellationToken: cancellationToken);

        // 대기 완료 후 카운트업 시작
        _isCountingUp = true;
        BigInteger targetCoinValue = Global.CoinManager.GetCoinValue();

        // 카운트 연출
        await CountUpMoneyAsync(targetCoinValue, cancellationToken);

        // 상태 초기화 (UI는 숨기지 않음)
        _accumulatedGainedMoney = BigInteger.Zero;
        _isCountingUp = false;

        await UniTask.WaitForSeconds(CountAfterDelay, cancellationToken: cancellationToken);
        // 대기 시간 후 이벤트 호출
        OnGainedMoneyDisplayComplete?.Invoke();

        // UI는 계속 표시되도록 유지 (비활성화하지 않음)
    }

    /// <summary>
    /// 금액 표시 시간 동안 추가 금액을 관리하고, 시간이 끝나면 UI 숨기지 않고 계속 표시합니다.
    /// 새로운 금액이 추가되면 애니메이션을 다시 재생합니다.
    /// </summary>
    /// <param name="cancellationToken">취소 토큰</param>
    private async UniTaskVoid ManageAddMoneyDisplayAsync(CancellationToken cancellationToken)
    {
        try
        {

            // AddMoneyDisplayDuration 동안 대기
            await UniTask.WaitForSeconds(AddMoneyDisplayDuration, cancellationToken: cancellationToken);
            // 시간이 끝났지만 UI는 숨기지 않음 (애니메이션만 관리)
            // 추가 금액이 있다면 애니메이션을 다시 재생할 준비
        }
        catch (OperationCanceledException)
        {
            // Cancelled due to new money; animation will restart via caller
        }
    }

    private async UniTask CountUpMoneyAsync(BigInteger initialTargetMoney, CancellationToken cancellationToken)
    {
        if (CountDuration <= 0)
        {
            // 카운트 시간이 0 이하면 즉시 목표값으로 설정
            BigInteger currentTargetMoney = Global.CoinManager.GetCoinValue();
            MoneyText.text = FormatMoney(currentTargetMoney);
            OnMoneyCountComplete?.Invoke();
            return;
        }

        float elapsedTime = 0f;
        BigInteger startMoney = initialTargetMoney - _accumulatedGainedMoney; // 획득 전 코인에서 시작
        BigInteger lastTargetMoney = initialTargetMoney;

        while (elapsedTime < CountDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / CountDuration);

            // 현재 실제 코인 값으로 목표값 업데이트 (카운트 중에 새로운 돈이 추가될 수 있음)
            BigInteger currentTargetMoney = Global.CoinManager.GetCoinValue();

            // 목표값이 변경되었으면 갱신
            if (currentTargetMoney != lastTargetMoney)
            {
                lastTargetMoney = currentTargetMoney;
            }

            // BigInteger Lerp: start + (target - start) * progress
            BigInteger range = currentTargetMoney - startMoney;
            int progressScaled = (int)(progress * 10000f);
            BigInteger currentMoney = startMoney + range * progressScaled / 10000;
            MoneyText.text = FormatMoney(currentMoney);

            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
        }

        // 최종값 설정 (최신 코인 값으로 설정)
        BigInteger finalTargetMoney = Global.CoinManager.GetCoinValue();
        MoneyText.text = FormatMoney(finalTargetMoney);
        OnMoneyCountComplete?.Invoke();
    }

    /// <summary>
    /// BigInteger 금액을 CoinManager의 단위 텍스트로 포맷팅합니다.
    /// overflow 위험 없이 string으로 변환합니다.
    /// </summary>
    /// <param name="amount">포맷팅할 금액 (BigInteger)</param>
    /// <returns>CoinManager가 포맷팅한 단위 문자열</returns>
    private string FormatMoney(BigInteger amount)
    {
        return Global.CoinManager.GetCoinUnitText(amount);
    }
}
