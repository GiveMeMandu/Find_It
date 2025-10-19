using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using DeskCat.FindIt.Scripts.Core.Main.System;
using Cysharp.Threading.Tasks;

/// <summary>
/// 어두운 모드 - 손전등 UI를 드래그로 움직일 수 있도록 관리합니다.
/// UI 설정은 외부에서 처리하고, 여기서는 손전등 움직임만 담당합니다.
/// </summary>
public class DarkModeManager : ModeManager
{
    [Header("Dark Mode Settings")]
    public GameObject LirisShot;
    public RectTransform flashlightTransform; // 직접 할당받을 손전등 UI

    private Camera mainCamera;
    private Canvas mainCanvas;

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
        }
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

        // 손전등 효과 강화 (찾았을 때 잠깐 밝아지는 효과)
        FlashlightFoundEffectAsync().Forget();

        Debug.Log($"[DarkModeManager] 어둠 속 오브젝트 발견: {foundObj.name}");
    }

    private async UniTaskVoid FlashlightFoundEffectAsync()
    {
        if (flashlightTransform == null) return;

        Vector2 originalSize = flashlightTransform.sizeDelta;
        Vector2 enhancedSize = originalSize * 1.5f;
        float duration = 0.5f;
        float elapsedTime = 0f;

        // 손전등 크기 증가
        while (elapsedTime < duration * 0.5f)
        {
            elapsedTime += Time.deltaTime;
            Vector2 currentSize = Vector2.Lerp(originalSize, enhancedSize, elapsedTime / (duration * 0.5f));
            flashlightTransform.sizeDelta = currentSize;
            await UniTask.Yield();
        }

        // 손전등 크기 원상복구
        elapsedTime = 0f;
        while (elapsedTime < duration * 0.5f)
        {
            elapsedTime += Time.deltaTime;
            Vector2 currentSize = Vector2.Lerp(enhancedSize, originalSize, elapsedTime / (duration * 0.5f));
            flashlightTransform.sizeDelta = currentSize;
            await UniTask.Yield();
        }

        flashlightTransform.sizeDelta = originalSize;
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
        }
    }
}