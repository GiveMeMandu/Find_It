using UnityEngine;
using DeskCat.FindIt.Scripts.Core.Main.System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Linq;
using Util.CameraSetting;

/// <summary>
/// 안개 영역 설정 데이터
/// </summary>
[System.Serializable]
public class FogAreaData
{
    [Tooltip("안개 영역 Transform")]
    public Transform fogArea;
    
    [Range(0.0f, 1.0f)]
    [Tooltip("이 안개가 걷힐 때 필요한 진행률 (0.1 = 10%, 1.0 = 100%)")]
    public float revealPercent = 0.5f;
}

/// <summary>
/// 구름(안개) 모드 - 맵의 일부 영역을 반투명한 구름/안개 UI로 가려 시야를 제한합니다.
/// 전체 찾을 물건에서 퍼센트로 fogAreas의 전체 갯수에 해당하는 갯수만큼 찾을 때마다 안개가 걷혀집니다.
/// </summary>
public class FogModeManager : ModeManager
{
    [Header("Fog Mode Settings")]
    [Tooltip("안개 영역 목록 (각 영역과 해제 진행률을 설정)")]
    public List<FogAreaData> fogAreaDataList = new List<FogAreaData>();
    [Header("Fog Alpha Settings")]
    [Range(0.1f, 1.0f)]
    [Tooltip("첫 번째 인덱스 안개의 최소 알파값")]
    public float minFogAlpha = 0.3f;
    [Range(0.1f, 1.0f)]
    [Tooltip("마지막 인덱스 안개의 최대 알파값")]
    public float maxFogAlpha = 0.9f;
    [Header("Camera Boundary Settings")]
    [Range(0.0f, 5.0f)]
    [Tooltip("안개 경계선에서의 여유 공간 (월드 단위)")]
    public float boundaryMargin = 0.5f;

    private List<SpriteRenderer> fogSpriteRenderers = new List<SpriteRenderer>();
    private List<List<SpriteRenderer>> fogAreaRenderers = new List<List<SpriteRenderer>>(); // 각 fogArea별 SpriteRenderer 그룹
    private int totalHiddenObjects = 0;
    private int currentRevealedAreas = 0; // 시작 시 아무 영역도 해제되지 않음
    private List<int> objectsRequiredPerArea = new List<int>(); // 각 영역별 필요한 오브젝트 수

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
            if (fogAreaDataList.Count > 0)
            {
                if (fogAreaDataList.First().fogArea != null)
                {
                    fogAreaDataList.First().fogArea.parent.gameObject.SetActive(true);
                }
                CalculateObjectsRequiredPerArea();
            }
            
            Debug.Log($"[FogModeManager] 총 오브젝트: {totalHiddenObjects}, 영역별 필요 오브젝트: [{string.Join(", ", objectsRequiredPerArea)}]");
            
            // 필요하다면 HiddenObjUI에도 접근 가능
            var allUIs = levelManager.GetAllHiddenObjUIs();
            Debug.Log($"[FogModeManager] 사용 가능한 UI 수: {allUIs.Count}개");
        }

        SetupFogAreas();
        UpdateCameraBoundary();
        
        // 게임 시작 시 카메라를 왼쪽 경계로 이동
        MoveCameraToLeftBoundary().Forget();
    }
    
    /// <summary>
    /// 카메라를 왼쪽 경계 끝부분으로 이동시킵니다.
    /// </summary>
    private async UniTaskVoid MoveCameraToLeftBoundary()
    {
        if (CameraView2D.Instance == null) return;
        
        // 왼쪽 경계 계산
        Bounds backgroundBounds = CameraView2D.Instance.backgroundSprite.bounds;
        float leftBoundary = backgroundBounds.min.x + boundaryMargin;
        
        // 현재 카메라 Y 위치 유지, X만 왼쪽 끝으로
        Vector3 targetPosition = new Vector3(
            leftBoundary,
            UnityEngine.Camera.main.transform.position.y,
            UnityEngine.Camera.main.transform.position.z
        );
        
        Debug.Log($"[FogModeManager] 카메라를 왼쪽 경계로 이동: {targetPosition}");
        
        // 부드럽게 이동 (2초)
        await CameraView2D.Instance.MoveCameraToPositionAsync(targetPosition, 2f);
    }

    /// <summary>
    /// 각 안개 영역별로 필요한 오브젝트 수를 계산합니다.
    /// FogAreaData의 revealPercent를 사용합니다.
    /// </summary>
    private void CalculateObjectsRequiredPerArea()
    {
        objectsRequiredPerArea.Clear();
        
        if (fogAreaDataList.Count == 0)
            return;
        
        int cumulativeObjects = 0;
        
        for (int i = 0; i < fogAreaDataList.Count; i++)
        {
            float targetPercent = fogAreaDataList[i].revealPercent;
            
            // 퍼센트 범위 제한 (0.0 ~ 1.0)
            targetPercent = Mathf.Clamp(targetPercent, 0.0f, 1.0f);
            
            int targetObjects = Mathf.CeilToInt(totalHiddenObjects * targetPercent);
            int objectsForThisArea = targetObjects - cumulativeObjects;
            
            // 최소 1개는 필요하도록
            objectsForThisArea = Mathf.Max(objectsForThisArea, 1);
            
            objectsRequiredPerArea.Add(objectsForThisArea);
            cumulativeObjects = targetObjects;
            
            Debug.Log($"[FogModeManager] 영역 {i}: {targetPercent:P1}까지, 필요 오브젝트: {objectsForThisArea}개");
        }
    }
    
    /// <summary>
    /// 인덱스에 따른 안개 알파값 계산
    /// 첫 번째 인덱스 = 가장 진한 안개 (maxFogAlpha), 먼저 해제
    /// 마지막 인덱스 = 가장 연한 안개 (minFogAlpha), 나중에 해제
    /// </summary>
    /// <param name="index">안개 영역 인덱스</param>
    /// <returns>계산된 알파값</returns>
    private float GetFogAlphaByIndex(int index)
    {
        if (fogAreaDataList.Count <= 1)
            return maxFogAlpha;

        // 인덱스에 따른 퍼센트 계산 (0 = 0%, 마지막 = 100%)
        float indexPercent = (float)index / (fogAreaDataList.Count - 1);
        
        // 최대값(진함)에서 최소값(연함)까지 선형 보간
        float alpha = Mathf.Lerp(maxFogAlpha, minFogAlpha, indexPercent);
        
        return alpha;
    }

    private void SetupFogAreas()
    {
        // 모든 영역의 자식 SpriteRenderer를 수집하고 안개 상태로 설정
        for (int i = 0; i < fogAreaDataList.Count; i++)
        {
            if (fogAreaDataList[i].fogArea != null)
            {
                fogAreaDataList[i].fogArea.parent.gameObject.SetActive(true);
                
                // 자식 객체들에서 모든 SpriteRenderer 찾기
                SpriteRenderer[] childRenderers = fogAreaDataList[i].fogArea.GetComponentsInChildren<SpriteRenderer>();
                
                if (childRenderers.Length > 0)
                {
                    // 인덱스에 따른 안개 투명도 설정 (역순)
                    float alphaForThisIndex = GetFogAlphaByIndex(i);
                    
                    List<SpriteRenderer> areaRenderers = new List<SpriteRenderer>();
                    
                    // 이 영역의 모든 자식 SpriteRenderer에 동일한 알파값 적용
                    foreach (var spriteRenderer in childRenderers)
                    {
                        Color color = spriteRenderer.color;
                        color.a = alphaForThisIndex;
                        spriteRenderer.color = color;
                        fogSpriteRenderers.Add(spriteRenderer);
                        areaRenderers.Add(spriteRenderer);
                    }
                    
                    fogAreaRenderers.Add(areaRenderers);
                    
                    Debug.Log($"[FogModeManager] 안개 영역 {i} 설정: {fogAreaDataList[i].fogArea.name}, 자식 SpriteRenderer 수: {childRenderers.Length}, Alpha: {alphaForThisIndex:F2}, 해제 퍼센트: {fogAreaDataList[i].revealPercent:P1}");
                }
                else
                {
                    Debug.LogWarning($"[FogModeManager] fogAreaDataList[{i}]의 자식에 SpriteRenderer가 없습니다: {fogAreaDataList[i].fogArea.name}");
                    fogAreaRenderers.Add(new List<SpriteRenderer>()); // 빈 리스트 추가
                }
            }
        }
        
        Debug.Log($"[FogModeManager] {fogSpriteRenderers.Count}개의 안개 스프라이트 설정 완료, {fogAreaRenderers.Count}개의 영역 그룹");
    }
    
    /// <summary>
    /// 현재 해제된 안개 영역에 따라 카메라 경계를 업데이트합니다.
    /// 왼쪽 끝: backgroundSprite의 왼쪽 끝
    /// 오른쪽 끝: 활성화된(아직 안 걷힌) 첫 번째 안개 영역의 오른쪽 끝
    /// </summary>
    private void UpdateCameraBoundary()
    {
        if (CameraView2D.Instance == null || fogAreaDataList.Count == 0)
        {
            Debug.LogWarning("[FogModeManager] CameraView2D 또는 fogAreaDataList가 없어 경계 업데이트 불가");
            return;
        }

        if (CameraView2D.Instance.backgroundSprite == null)
        {
            Debug.LogWarning("[FogModeManager] backgroundSprite가 없어 경계 업데이트 불가");
            return;
        }

        // 왼쪽 끝: backgroundSprite의 왼쪽 경계
        Bounds backgroundBounds = CameraView2D.Instance.backgroundSprite.bounds;
        float leftBoundary = backgroundBounds.min.x;
        float bottomBoundary = backgroundBounds.min.y;
        float topBoundary = backgroundBounds.max.y;

        // 오른쪽 끝: 현재 활성화된(아직 안 걷힌) 첫 번째 안개 영역의 오른쪽 끝
        float rightBoundary = backgroundBounds.max.x; // 기본값은 배경의 오른쪽 끝
        
        // 현재 활성화된 첫 번째 안개 영역 찾기 (currentRevealedAreas 인덱스)
        if (currentRevealedAreas < fogAreaDataList.Count && fogAreaDataList[currentRevealedAreas].fogArea != null)
        {
            // 해당 영역의 자식 SpriteRenderer들에서 가장 오른쪽 끝 찾기
            SpriteRenderer[] childRenderers = fogAreaDataList[currentRevealedAreas].fogArea.GetComponentsInChildren<SpriteRenderer>();
            
            if (childRenderers.Length > 0)
            {
                float maxX = float.MinValue;
                
                foreach (var spriteRenderer in childRenderers)
                {
                    if (spriteRenderer != null)
                    {
                        float spriteRightEdge = spriteRenderer.bounds.max.x;
                        if (spriteRightEdge > maxX)
                        {
                            maxX = spriteRightEdge;
                        }
                    }
                }
                
                if (maxX > float.MinValue)
                {
                    rightBoundary = maxX;
                }
                
                Debug.Log($"[FogModeManager] 활성화된 안개 영역 {currentRevealedAreas}의 오른쪽 끝: {rightBoundary:F2}");
            }
        }
        else if (currentRevealedAreas >= fogAreaDataList.Count)
        {
            // 모든 안개가 해제되었으면 배경 전체를 경계로
            Debug.Log("[FogModeManager] 모든 안개 해제됨 - 배경 전체를 카메라 경계로 설정");
        }

        // 여유 공간 추가
        leftBoundary -= boundaryMargin;
        rightBoundary += boundaryMargin;
        bottomBoundary -= boundaryMargin;
        topBoundary += boundaryMargin;

        // CameraView2D의 경계 설정
        CameraView2D.Instance._panMinX = leftBoundary;
        CameraView2D.Instance._panMinY = bottomBoundary;
        CameraView2D.Instance._panMaxX = rightBoundary;
        CameraView2D.Instance._panMaxY = topBoundary;
        
        // 무한 팬 비활성화
        CameraView2D.Instance._infinitePan = false;
        CameraView2D.Instance._autoPanBoundary = false; // 수동으로 경계 설정
        
        Debug.Log($"[FogModeManager] 카메라 경계 업데이트: X({leftBoundary:F2} ~ {rightBoundary:F2}), Y({bottomBoundary:F2} ~ {topBoundary:F2})");
    }

    private void OnHiddenObjectFound(object sender, HiddenObj foundObj)
    {
        OnObjectFound(foundObj);
        
        // 퍼센트 기반으로 안개 해제 체크
        CheckForFogReveal();
    }
    
    private void CheckForFogReveal()
    {
        if (currentRevealedAreas >= fogAreaRenderers.Count)
            return;
        
        // 현재까지 찾은 오브젝트 수
        int totalFoundObjects = totalHiddenObjects - levelManager.GetLeftHiddenObjCount();
        
        // 현재 영역을 해제하는데 필요한 누적 오브젝트 수 계산
        int requiredObjectsForCurrentArea = 0;
        for (int i = 0; i <= currentRevealedAreas && i < objectsRequiredPerArea.Count; i++)
        {
            requiredObjectsForCurrentArea += objectsRequiredPerArea[i];
        }
        
        // 다음 영역을 공개할 시점인지 체크
        bool shouldRevealNextArea = totalFoundObjects >= requiredObjectsForCurrentArea;
        
        if (shouldRevealNextArea)
        {
            RevealNextArea();
        }
        
        Debug.Log($"[FogModeManager] CheckForFogReveal - 찾은 오브젝트: {totalFoundObjects}/{totalHiddenObjects}, 현재 영역 해제 필요: {requiredObjectsForCurrentArea}, 해제된 영역: {currentRevealedAreas}/{fogAreaRenderers.Count}");
    }

    private void RevealNextArea()
    {
        if (currentRevealedAreas >= fogAreaRenderers.Count) 
        {
            Debug.Log($"[FogModeManager] 모든 안개 영역이 이미 해제됨. 해제된 영역: {currentRevealedAreas}, 전체 안개 영역: {fogAreaRenderers.Count}");
            return;
        }

        int areaIndex = currentRevealedAreas; // 0부터 시작
        
        Debug.Log($"[FogModeManager] ===== 안개 영역 해제 시작 =====");
        Debug.Log($"[FogModeManager] 해제할 영역 인덱스: {areaIndex}");
        Debug.Log($"[FogModeManager] 현재까지 해제된 영역 수: {currentRevealedAreas}");
        
        // 먼저 currentRevealedAreas 증가 (다음 경계를 가리키도록)
        currentRevealedAreas++;
        
        // 카메라 경계를 먼저 업데이트 (페이드 전에)
        UpdateCameraBoundary();
        
        // 그 다음 페이드 아웃 시작
        if (areaIndex >= 0 && areaIndex < fogAreaRenderers.Count)
        {
            List<SpriteRenderer> areaRenderers = fogAreaRenderers[areaIndex];
            if (areaRenderers.Count > 0)
            {
                Debug.Log($"[FogModeManager] 안개 영역 {areaIndex} 페이드 아웃 시작 ({areaRenderers.Count}개의 스프라이트)");
                // 해당 영역의 모든 자식 SpriteRenderer에 페이드 효과 적용
                RemoveFogAreaAnimationAsync(areaRenderers).Forget();
            }
            else
            {
                Debug.LogWarning($"[FogModeManager] 안개 영역 {areaIndex}에 SpriteRenderer가 없습니다.");
            }
        }
        
        Debug.Log($"[FogModeManager] 영역 해제 완료! 총 해제된 영역: {currentRevealedAreas}/{fogAreaRenderers.Count}");
        Debug.Log($"[FogModeManager] ================================");
    }

    private async UniTaskVoid RemoveFogAreaAnimationAsync(List<SpriteRenderer> spriteRenderers)
    {
        if (spriteRenderers == null || spriteRenderers.Count == 0)
            return;

        float duration = 1.0f;
        float elapsedTime = 0;
        
        // 각 SpriteRenderer의 시작 색상과 알파값 저장
        Dictionary<SpriteRenderer, Color> startColors = new Dictionary<SpriteRenderer, Color>();
        Dictionary<SpriteRenderer, float> startAlphas = new Dictionary<SpriteRenderer, float>();
        
        foreach (var spriteRenderer in spriteRenderers)
        {
            if (spriteRenderer != null)
            {
                startColors[spriteRenderer] = spriteRenderer.color;
                startAlphas[spriteRenderer] = spriteRenderer.color.a;
            }
        }

        // 모든 SpriteRenderer를 동시에 페이드 아웃
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            foreach (var spriteRenderer in spriteRenderers)
            {
                if (spriteRenderer != null && startAlphas.ContainsKey(spriteRenderer))
                {
                    float alpha = Mathf.Lerp(startAlphas[spriteRenderer], 0, t);
                    Color newColor = startColors[spriteRenderer];
                    newColor.a = alpha;
                    spriteRenderer.color = newColor;
                }
            }
            
            await UniTask.Yield();
        }

        // 완전히 투명하게 설정 및 비활성화
        foreach (var spriteRenderer in spriteRenderers)
        {
            if (spriteRenderer != null)
            {
                Color finalColor = spriteRenderer.color;
                finalColor.a = 0;
                spriteRenderer.color = finalColor;
                spriteRenderer.gameObject.SetActive(false);
            }
        }
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
        
        // 다음 영역 해제까지 필요한 오브젝트 수 계산
        int requiredForNextArea = 0;
        for (int i = 0; i <= currentRevealedAreas && i < objectsRequiredPerArea.Count; i++)
        {
            requiredForNextArea += objectsRequiredPerArea[i];
        }
        int remainingForNextArea = requiredForNextArea - totalFoundObjects;
        
        Debug.Log($"[FogModeManager] 오브젝트 발견: {foundObj.name} (전체 진행률: {currentProgress:P1}, 다음 안개까지: {remainingForNextArea}개 필요)");
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