using UnityEngine;
using DeskCat.FindIt.Scripts.Core.Main.System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Linq;

/// <summary>
/// 구름(안개) 모드 - 맵의 일부 영역을 반투명한 구름/안개 UI로 가려 시야를 제한합니다.
/// 전체 찾을 물건에서 퍼센트로 fogAreas의 전체 갯수에 해당하는 갯수만큼 찾을 때마다 안개가 걷혀집니다.
/// </summary>
public class FogModeManager : ModeManager
{
    [Header("Fog Mode Settings")]
    public List<Transform> fogAreas = new List<Transform>();
    [Header("Fog Alpha Settings")]
    [Range(0.1f, 1.0f)]
    [Tooltip("첫 번째 인덱스 안개의 최소 알파값")]
    public float minFogAlpha = 0.3f;
    [Range(0.1f, 1.0f)]
    [Tooltip("마지막 인덱스 안개의 최대 알파값")]
    public float maxFogAlpha = 0.9f;
    [Header("Fog Reveal Settings")]
    [Range(0.1f, 1.0f)]
    [Tooltip("마지막 안개가 걷힐 때 필요한 진행률 (0.1 = 10%, 1.0 = 100%)")]
    public float lastFogRevealPercent = 0.8f; // 80%에서 마지막 안개 해제

    private List<CanvasGroup> fogCanvasGroups = new List<CanvasGroup>();
    private int totalHiddenObjects = 0;
    private int objectsFoundSinceLastReveal = 0;
    private int currentRevealedAreas = 0; // 시작 시 아무 영역도 해제되지 않음
    private int objectsPerAreaReveal = 0;

    public override void InitializeMode()
    {
        currentMode = GameMode.FOG;
        Debug.Log("[FogModeManager] 구름(안개) 모드 초기화 완료");

        if (levelManager != null)
        {
            levelManager.OnFoundObj += OnHiddenObjectFound;
            
            // 전체 숨겨진 오브젝트 수 계산
            totalHiddenObjects = levelManager.GetTotalHiddenObjCount();
            
            // 각 영역 공개에 필요한 오브젝트 수 계산 (퍼센트 기반)
            if (fogAreas.Count > 0)
            {
                fogAreas.First().parent.gameObject.SetActive(true);
                // 마지막 안개가 설정된 퍼센트에서 해제되도록 계산
                int objectsNeededForLastFog = Mathf.CeilToInt(totalHiddenObjects * lastFogRevealPercent);
                int fogAreaCount = fogAreas.Count; // 모든 영역에 안개 적용
                objectsPerAreaReveal = Mathf.CeilToInt((float)objectsNeededForLastFog / fogAreaCount);
                
                Debug.Log($"[FogModeManager] 안개 영역 수: {fogAreaCount}, 마지막 해제 필요 오브젝트: {objectsNeededForLastFog}");
            }
            
            Debug.Log($"[FogModeManager] 총 오브젝트: {totalHiddenObjects}, 마지막 안개 해제 퍼센트: {lastFogRevealPercent:P0}, 영역당 필요 오브젝트: {objectsPerAreaReveal}");
            
            // 필요하다면 HiddenObjUI에도 접근 가능
            var allUIs = levelManager.GetAllHiddenObjUIs();
            Debug.Log($"[FogModeManager] 사용 가능한 UI 수: {allUIs.Count}개");
        }

        SetupFogAreas();
    }

    /// <summary>
    /// 인덱스에 따른 안개 알파값 계산
    /// </summary>
    /// <param name="index">안개 영역 인덱스</param>
    /// <returns>계산된 알파값</returns>
    private float GetFogAlphaByIndex(int index)
    {
        if (fogAreas.Count <= 1)
            return maxFogAlpha;

        // 인덱스에 따른 퍼센트 계산 (0 = 0%, 마지막 = 100%)
        float indexPercent = (float)index / (fogAreas.Count - 1);
        
        // 최대값에서 최소값까지 선형 보간 (반대로)
        float alpha = Mathf.Lerp(maxFogAlpha, minFogAlpha, indexPercent);
        
        return alpha;
    }

    private void SetupFogAreas()
    {
        // 모든 영역의 CanvasGroup을 수집하고 안개 상태로 설정
        for (int i = 0; i < fogAreas.Count; i++)
        {
            if (fogAreas[i] != null)
            {
                CanvasGroup canvasGroup = fogAreas[i].GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    // 인덱스에 따른 안개 투명도 설정
                    float alphaForThisIndex = GetFogAlphaByIndex(i);
                    canvasGroup.alpha = alphaForThisIndex;
                    fogCanvasGroups.Add(canvasGroup);
                    Debug.Log($"[FogModeManager] 안개 영역 {i} 설정: {fogAreas[i].name}, Alpha: {alphaForThisIndex:F2}");
                }
                else
                {
                    Debug.LogWarning($"[FogModeManager] fogAreas[{i}]에 CanvasGroup이 없습니다: {fogAreas[i].name}");
                }
            }
        }
        
        Debug.Log($"[FogModeManager] {fogCanvasGroups.Count}개의 안개 영역 설정 완료");
    }

    private void OnHiddenObjectFound(object sender, HiddenObj foundObj)
    {
        OnObjectFound(foundObj);
        
        objectsFoundSinceLastReveal++;
        
        // 퍼센트 기반으로 안개 해제 체크
        CheckForFogReveal();
    }
    
    private void CheckForFogReveal()
    {
        // 다음 영역을 공개할 시점인지 체크
        bool shouldRevealNextArea = objectsFoundSinceLastReveal >= objectsPerAreaReveal && 
                                  currentRevealedAreas < fogCanvasGroups.Count;
        
        if (shouldRevealNextArea)
        {
            RevealNextArea();
            objectsFoundSinceLastReveal = 0; // 카운터 리셋
        }
        
        Debug.Log($"[FogModeManager] CheckForFogReveal - 현재 해제된 영역: {currentRevealedAreas}, 전체 안개 영역: {fogCanvasGroups.Count}, 다음 해제 조건: {objectsFoundSinceLastReveal >= objectsPerAreaReveal}");
    }

    private void RevealNextArea()
    {
        if (currentRevealedAreas >= fogCanvasGroups.Count) 
        {
            Debug.Log($"[FogModeManager] 모든 안개 영역이 이미 해제됨. 해제된 영역: {currentRevealedAreas}, 전체 안개 영역: {fogCanvasGroups.Count}");
            return;
        }

        int fogCanvasGroupIndex = currentRevealedAreas; // 0부터 시작
        
        if (fogCanvasGroupIndex >= 0 && fogCanvasGroupIndex < fogCanvasGroups.Count)
        {
            Debug.Log($"[FogModeManager] 안개 영역 {fogCanvasGroupIndex} 해제 시작");
            // 안개 제거 애니메이션
            RemoveFogAnimationAsync(fogCanvasGroups[fogCanvasGroupIndex]).Forget();
        }

        currentRevealedAreas++;
        Debug.Log($"[FogModeManager] 영역 해제 완료! 해제된 영역: {currentRevealedAreas}/{fogCanvasGroups.Count}");
    }

    private async UniTaskVoid RemoveFogAnimationAsync(CanvasGroup canvasGroup)
    {
        float duration = 1.0f;
        float elapsedTime = 0;
        float startAlpha = canvasGroup.alpha; // 현재 알파값에서 시작

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 0, elapsedTime / duration);
            canvasGroup.alpha = alpha;
            await UniTask.Yield();
        }

        // 완전히 투명하게 설정
        canvasGroup.alpha = 0;
    }

    public override void OnGameStart()
    {
        base.OnGameStart();
        Debug.Log("[FogModeManager] 구름(안개) 모드 시작 - 오브젝트를 찾아 안개를 제거하세요!");
    }

    public override void OnObjectFound(HiddenObj foundObj)
    {
        base.OnObjectFound(foundObj);
        
        int totalFoundObjects = totalHiddenObjects - levelManager.GetLeftHiddenObjCount();
        float currentProgress = (float)totalFoundObjects / totalHiddenObjects;
        
        Debug.Log($"[FogModeManager] 오브젝트 발견: {foundObj.name} (전체 진행률: {currentProgress:P1}, 다음 안개까지: {objectsFoundSinceLastReveal}/{objectsPerAreaReveal})");
    }

    public override void OnGameEnd()
    {
        base.OnGameEnd();
        Debug.Log("[FogModeManager] 모든 영역이 해제되었습니다! 안개가 완전히 걷혔습니다.");
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