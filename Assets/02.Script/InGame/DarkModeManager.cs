using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using DeskCat.FindIt.Scripts.Core.Main.System;
using Cysharp.Threading.Tasks;
using Manager;
using UI.Page;

/// <summary>
/// 어두운 모드 - 손전등 UI를 드래그로 움직일 수 있도록 관리합니다.
/// UI 설정은 외부에서 처리하고, 여기서는 손전등 움직임만 담당합니다.
/// </summary>
public class DarkModeManager : ModeManager
{
    [Header("Dark Mode Settings")]
    public GameObject LirisShot;
    public RectTransform flashlightTransform; // 직접 할당받을 손전등 UI
    public float expandDuration = 2f; // 손전등이 완전히 커지는데 걸리는 시간
    public float maxScale = 50f; // 손전등의 최대 크기 배율
    public float sizeIncreasePerFind = 1.1f; // 오브젝트를 찾을 때마다 증가하는 크기 배율

    private Camera mainCamera;
    private Canvas mainCanvas;
    private bool isFlashlightEffectPlaying = false; // 이펙트 재생 중인지 확인
    private Vector2 baseFlashlightSize; // 손전등의 기본 크기 (게임 중 점점 커짐)

    protected override void Start()
    {
        base.Start();
        LirisShot.SetActive(false);
    }

    public override void InitializeMode()
    {
        currentMode = GameMode.DARK;
        Debug.Log("[DarkModeManager] 다크 모드 초기화 완료");

        mainCamera = Camera.main;
        mainCanvas = Object.FindFirstObjectByType<Canvas>();

        if (levelManager != null)
        {
            levelManager.OnFoundObj += OnHiddenObjectFound;
            LirisShot.gameObject.SetActive(true);
            
            // 게임 종료 시 손전등 확대 효과를 실행하도록 등록
            levelManager.OnEndEvent.Add(ExpandFlashlightToFullAsync);
        }

        // 손전등 초기 크기 저장
        if (flashlightTransform != null)
        {
            baseFlashlightSize = flashlightTransform.sizeDelta;
        }

        Global.UIManager.GetPages<InGameMainPage>().ForEach(page =>
        {
            page.MissionGroupEnabled = false;
        });
    }

    protected override void Update()
    {
        base.Update();
    }

    private void OnHiddenObjectFound(object sender, HiddenObj foundObj)
    {
        OnObjectFound(foundObj);
    }

    public override void OnGameStart()
    {
        base.OnGameStart();
        Debug.Log("[DarkModeManager] 다크 모드 시작 - 손전등으로 탐색하세요!");
    }

    public override void OnObjectFound(HiddenObj foundObj)
    {
        base.OnObjectFound(foundObj);

        // 손전등 효과 (찾았을 때 잠깐 밝아지는 효과 + 크기 증가)
        FlashlightFoundEffectAsync().Forget();
        IncreaseFlashlightSizeAsync().Forget();

        Debug.Log($"[DarkModeManager] 어둠 속 오브젝트 발견: {foundObj.name}");
    }

    /// <summary>
    /// 오브젝트를 찾았을 때 손전등이 잠깐 밝아지는 효과 (깜빡임)
    /// 이펙트 재생 중에는 중복 재생되지 않도록 플래그로 관리
    /// </summary>
    private async UniTaskVoid FlashlightFoundEffectAsync()
    {
        if (flashlightTransform == null || isFlashlightEffectPlaying) return;

        isFlashlightEffectPlaying = true;

        Vector2 currentSize = flashlightTransform.sizeDelta;
        Vector2 enhancedSize = currentSize * 1.5f;
        float duration = 0.5f;
        float elapsedTime = 0f;

        // 손전등 크기 증가
        while (elapsedTime < duration * 0.5f)
        {
            elapsedTime += Time.deltaTime;
            Vector2 tempSize = Vector2.Lerp(currentSize, enhancedSize, elapsedTime / (duration * 0.5f));
            flashlightTransform.sizeDelta = tempSize;
            await UniTask.Yield();
        }

        // 손전등 크기 원상복구 (현재 베이스 크기로)
        elapsedTime = 0f;
        while (elapsedTime < duration * 0.5f)
        {
            elapsedTime += Time.deltaTime;
            Vector2 tempSize = Vector2.Lerp(enhancedSize, baseFlashlightSize, elapsedTime / (duration * 0.5f));
            flashlightTransform.sizeDelta = tempSize;
            await UniTask.Yield();
        }

        flashlightTransform.sizeDelta = baseFlashlightSize;
        isFlashlightEffectPlaying = false;
    }

    /// <summary>
    /// 오브젝트를 찾을 때마다 손전등의 기본 크기를 조금씩 증가시킵니다.
    /// </summary>
    private async UniTaskVoid IncreaseFlashlightSizeAsync()
    {
        if (flashlightTransform == null) return;

        Vector2 targetSize = baseFlashlightSize * sizeIncreasePerFind;
        float duration = 0.3f;
        float elapsedTime = 0f;
        Vector2 startSize = baseFlashlightSize;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            baseFlashlightSize = Vector2.Lerp(startSize, targetSize, t);
            
            // 이펙트가 재생 중이 아닐 때만 크기 적용
            if (!isFlashlightEffectPlaying)
            {
                flashlightTransform.sizeDelta = baseFlashlightSize;
            }
            
            await UniTask.Yield();
        }

        baseFlashlightSize = targetSize;
        
        Debug.Log($"[DarkModeManager] 손전등 크기 증가: {baseFlashlightSize}");
    }

    /// <summary>
    /// 게임 종료 시 손전등을 화면 전체로 확대합니다.
    /// LevelManager의 OnEndEvent에 등록되어 게임 종료 전에 실행됩니다.
    /// </summary>
    private async UniTask ExpandFlashlightToFullAsync()
    {
        if (flashlightTransform == null)
        {
            Debug.LogWarning("[DarkModeManager] flashlightTransform이 null입니다. 손전등 확대를 건너뜁니다.");
            return;
        }

        Debug.Log("[DarkModeManager] 손전등 확대 시작!");

        Vector2 originalSize = flashlightTransform.sizeDelta;
        // 화면 크기를 기준으로 충분히 큰 크기 계산
        Vector2 targetSize = originalSize * maxScale;
        
        float elapsedTime = 0f;

        // 손전등을 점점 크게 확대
        while (elapsedTime < expandDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / expandDuration;
            // EaseOut 효과를 위한 커브 적용
            float smoothT = 1f - Mathf.Pow(1f - t, 3f);
            
            Vector2 currentSize = Vector2.Lerp(originalSize, targetSize, smoothT);
            flashlightTransform.sizeDelta = currentSize;
            
            await UniTask.Yield();
        }

        flashlightTransform.sizeDelta = targetSize;
        Debug.Log("[DarkModeManager] 손전등 확대 완료!");
    }

    public override void OnGameEnd()
    {
        base.OnGameEnd();
        Debug.Log("[DarkModeManager] 어둠 속 모든 오브젝트 발견 완료! 빛이 진실을 드러냅니다!");
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        if (levelManager != null)
        {
            levelManager.OnFoundObj -= OnHiddenObjectFound;
            levelManager.OnEndEvent.Remove(ExpandFlashlightToFullAsync);
        }
    }
}