using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using Data;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Manager;
using UnityWeld;
using UI.Page;

public class PuzzleGameManager : MMSingleton<PuzzleGameManager>
{
    [SerializeField] private Transform gameTransform;
    [SerializeField] private Transform piecePrefab;
    [SerializeField] private PuzzleData[] puzzleDataList;
    [SerializeField] private float completionAnimationDuration = 0.5f;
    [SerializeField] private Ease completionEaseType = Ease.InOutQuad;
    [SerializeField] private GameObject completionParticleEffect;

    private List<Transform> pieces;
    private bool shuffling = false;
    private Transform draggedPiece = null;
    private Vector3 dragOffset;
    private Vector3 originalPosition;
    private PuzzleData currentPuzzleData;
    private bool isGamePaused = false;
    private bool isAnimating = false;
    private bool isGameCompleted = false;
    private InputManager inputManager;
    private DateTime startTime;

    public PuzzleData[] PuzzleDataList => puzzleDataList;
    public bool IsGamePaused => isGamePaused;
    public PuzzleData CurrentPuzzleData => currentPuzzleData;
    
    public event Action OnPuzzleCompleted;
    public event Action OnPuzzleStarted;
    public event Action OnPuzzlePaused;
    public event Action OnPuzzleResumed;

    void Start()
    {
        pieces = new List<Transform>();
        inputManager = FindAnyObjectByType<InputManager>();
        if (inputManager != null)
        {
            inputManager.OnTouchPressAction += HandleTouchStart;
            inputManager.OnTouchMoveAction += HandleTouchMove;
            inputManager.OnTouchPressEndAction += HandleTouchEnd;
        }
        
        // 파티클 시스템 초기 비활성화
        if (completionParticleEffect != null)
        {
            completionParticleEffect.SetActive(false);
        }
        
        // MainMenuSelectedManager에서 선택된 스테이지로 자동 시작
        AutoStartSelectedStage();
    }
    
    /// <summary>
    /// MainMenuSelectedManager에서 선택된 스테이지로 자동 시작합니다
    /// </summary>
    private void AutoStartSelectedStage()
    {
        if (Global.MainMenuSelectedManager != null && Global.MainMenuSelectedManager.HasSelectedStage())
        {
            SceneName selectedScene = Global.MainMenuSelectedManager.SelectedSceneName;
            SceneInfo selectedStageInfo = Global.MainMenuSelectedManager.SelectedStageInfo;
            
            if (selectedStageInfo == null)
            {
                Debug.LogWarning("[PuzzleGameManager] Selected stage info is null");
                return;
            }
            
            Debug.Log($"[PuzzleGameManager] Auto-starting selected stage - Scene: {selectedScene}, StageIndex: {selectedStageInfo.stageIndex}");
            
            // 선택된 씬과 스테이지 인덱스로 퍼즐 초기화
            InitializePuzzle(selectedScene, selectedStageInfo.stageIndex);
            
            // UI 처리는 퍼즐 초기화 성공 후에만 실행
            if (currentPuzzleData != null)
            {
                // 타이머 카운트 페이지 열기
                var timerPage = Global.UIManager.OpenPage<TimerCountPage>();
                PauseGame(); // 게임 일시정지

                // 3초 타이머 설정 및 완료 후 게임 재개
                timerPage.SetTimer(3, () => {
                    ResumeGame();
                    Global.UIManager.OpenPage<PuzzleInGamePage>();
                });
            }
        }
        else
        {
            Debug.LogWarning("[PuzzleGameManager] No stage selected in MainMenuSelectedManager");
        }
    }

    private void OnDestroy()
    {
        if (inputManager != null)
        {
            inputManager.OnTouchPressAction -= HandleTouchStart;
            inputManager.OnTouchMoveAction -= HandleTouchMove;
            inputManager.OnTouchPressEndAction -= HandleTouchEnd;
        }
    }

    public void InitializePuzzle(SceneName sceneName, int stageIndex)
    {
        isGameCompleted = false;
        // 파티클 이펙트 비활성화
        if (completionParticleEffect != null)
        {
            completionParticleEffect.SetActive(false);
        }
        
        // puzzleDataList null 체크
        if (puzzleDataList == null || puzzleDataList.Length == 0)
        {
            Debug.LogWarning("[PuzzleGameManager] puzzleDataList is null or empty");
            return;
        }
        
        // sceneName과 stageIndex에 맞는 퍼즐 찾기
        currentPuzzleData = puzzleDataList.FirstOrDefault(p => p.sceneName == sceneName && p.stageIndex == stageIndex);
        if (currentPuzzleData == null) 
        {
            Debug.LogWarning($"[PuzzleGameManager] No puzzle data found for Scene: {sceneName}, StageIndex: {stageIndex}");
            return;
        }

        // 기존 피스들 제거
        foreach (var piece in pieces)
        {
            if (piece != null) Destroy(piece.gameObject);
        }
        pieces.Clear();

        // 퍼즐 생성
        CreateGamePieces(currentPuzzleData, 0.01f);
        StartCoroutine(WaitShuffle(0.5f));
        
        // 게임 시작 시간 기록
        startTime = DateTime.Now;
        
        OnPuzzleStarted?.Invoke();
    }

    public void PauseGame()
    {
        if (!isGamePaused)
        {
            isGamePaused = true;
            inputManager?.DisableAllInput();
            OnPuzzlePaused?.Invoke();
        }
    }

    public void ResumeGame()
    {
        if (isGamePaused)
        {
            isGamePaused = false;
            inputManager?.EnableAllInput();
            OnPuzzleResumed?.Invoke();
        }
    }

    private void HandleTouchStart(object sender, InputManager.TouchData touchData)
    {
        if (shuffling || isGamePaused || isGameCompleted) return;

        RaycastHit2D hit = Physics2D.Raycast(touchData.WorldPosition, Vector2.zero);
        if (hit && hit.transform != null)
        {
            draggedPiece = hit.transform;
            originalPosition = draggedPiece.localPosition;
            dragOffset = draggedPiece.position - new Vector3(touchData.WorldPosition.x, touchData.WorldPosition.y, 0);
            draggedPiece.SetAsLastSibling();
        }
    }

    private void HandleTouchMove(object sender, InputManager.TouchData touchData)
    {
        if (draggedPiece == null || shuffling || isGamePaused || isGameCompleted) return;
        draggedPiece.position = new Vector3(touchData.WorldPosition.x, touchData.WorldPosition.y, 0) + dragOffset;
    }

    private void HandleTouchEnd(object sender, InputManager.TouchData touchData)
    {
        if (draggedPiece == null || isAnimating || shuffling || isGamePaused || isGameCompleted) return;

        RaycastHit2D[] hits = Physics2D.RaycastAll(touchData.WorldPosition, Vector2.zero);

        Transform targetPiece = null;
        foreach (var hit in hits)
        {
            if (hit.transform != draggedPiece)
            {
                targetPiece = hit.transform;
                break;
            }
        }

        if (targetPiece != null)
        {
            int draggedIndex = pieces.IndexOf(draggedPiece);
            int targetIndex = pieces.IndexOf(targetPiece);
            
            // 인덱스 유효성 검사
            if (draggedIndex >= 0 && draggedIndex < pieces.Count && 
                targetIndex >= 0 && targetIndex < pieces.Count)
            {
                Vector3 targetPosition = targetPiece.localPosition;
                targetPiece.localPosition = originalPosition;
                draggedPiece.localPosition = targetPosition;

                pieces[draggedIndex] = targetPiece;
                pieces[targetIndex] = draggedPiece;

                if (CheckCompletion())
                {
                    OnPuzzleCompleted?.Invoke();
                }
            }
            else
            {
                Debug.LogWarning($"Invalid piece indices: dragged={draggedIndex}, target={targetIndex}, pieces.Count={pieces.Count}");
                draggedPiece.localPosition = originalPosition;
            }
        }
        else
        {
            draggedPiece.localPosition = originalPosition;
        }

        draggedPiece = null;
    }

    private async UniTask PlayCompletionAnimation()
    {
        isAnimating = true;
        float width = 1f / currentPuzzleData.size;

        var tasks = new List<UniTask>();
        
        for (int row = 0; row < currentPuzzleData.size; row++)
        {
            for (int col = 0; col < currentPuzzleData.size; col++)
            {
                int index = (row * currentPuzzleData.size) + col;
                Transform piece = pieces[index];
                
                Vector3 targetPosition = new Vector3(
                    -1 + (2 * width * col) + width,
                    +1 - (2 * width * row) - width,
                    0
                );

                float delay = (row * currentPuzzleData.size + col) * 0.05f;
                
                // 위치 이동과 크기 조정을 동시에 실행
                tasks.Add(UniTask.Delay(TimeSpan.FromSeconds(delay))
                    .ContinueWith(async () => {
                        await UniTask.WhenAll(
                            piece.DOLocalMove(targetPosition, completionAnimationDuration)
                                .SetEase(completionEaseType)
                                .ToUniTask(),
                            piece.DOScale(Vector3.one * 2 * width, completionAnimationDuration)
                                .SetEase(completionEaseType)
                                .ToUniTask()
                        );
                    }));
            }
        }

        await UniTask.WhenAll(tasks);
        
        isAnimating = false;
        isGameCompleted = true;
        
        // 파티클 효과 활성화
        if (completionParticleEffect != null)
        {
            completionParticleEffect.SetActive(true);
        }
        
        // 퍼즐 완성 시 GameEndPage 열기
        ShowGameEndPage();
        
        OnPuzzleCompleted?.Invoke();
    }

    private async UniTask PlayCompletionAnimationSimultaneous()
    {
        isAnimating = true;
        float width = 1f / currentPuzzleData.size;

        var tasks = new List<UniTask>();
        
        for (int row = 0; row < currentPuzzleData.size; row++)
        {
            for (int col = 0; col < currentPuzzleData.size; col++)
            {
                int index = (row * currentPuzzleData.size) + col;
                Transform piece = pieces[index];
                
                Vector3 targetPosition = new Vector3(
                    -1 + (2 * width * col) + width,
                    +1 - (2 * width * row) - width,
                    0
                );

                tasks.Add(UniTask.WhenAll(
                    piece.DOLocalMove(targetPosition, completionAnimationDuration)
                        .SetEase(completionEaseType)
                        .ToUniTask(),
                    piece.DOScale(Vector3.one * 2 * width, completionAnimationDuration)
                        .SetEase(completionEaseType)
                        .ToUniTask()
                ));
            }
        }

        await UniTask.WhenAll(tasks);
        
        isAnimating = false;
        isGameCompleted = true;
        
        // 파티클 효과 활성화
        if (completionParticleEffect != null)
        {
            completionParticleEffect.SetActive(true);
        }
        
        OnPuzzleCompleted?.Invoke();
    }

    private bool CheckCompletion()
    {
        if (isAnimating) return false;

        for (int i = 0; i < pieces.Count; i++)
        {
            if (pieces[i].name != $"{(i)}") return false;
        }

        PlayCompletionAnimation().Forget();
        return true;
    }

    private IEnumerator WaitShuffle(float duration)
    {
        shuffling = true;
        yield return new WaitForSeconds(duration);
        Shuffle();
        shuffling = false;
    }

    private void Shuffle()
    {
        for (int i = 0; i < pieces.Count; i++)
        {
            int random = UnityEngine.Random.Range(0, pieces.Count);
            
            Vector3 tempPosition = pieces[i].localPosition;
            pieces[i].localPosition = pieces[random].localPosition;
            pieces[random].localPosition = tempPosition;

            (pieces[i], pieces[random]) = (pieces[random], pieces[i]);
        }
    }

    private void CreateGamePieces(PuzzleData data, float gapThickness)
    {
        float width = 1f / data.size;
        
        // 퍼즐 이미지로 머티리얼 생성
        Material pieceMaterial = new Material(Shader.Find("Unlit/Texture"));
        pieceMaterial.mainTexture = data.puzzleImage.texture;

        for (int row = 0; row < data.size; row++)
        {
            for (int col = 0; col < data.size; col++)
            {
                CreatePiece(row, col, width, gapThickness, pieceMaterial, data.size);
            }
        }
    }

    private void CreatePiece(int row, int col, float width, float gapThickness, Material material, int size)
    {
        Transform piece = Instantiate(piecePrefab, gameTransform);
        pieces.Add(piece);

        // 위치 및 크기 설정
        piece.localPosition = new Vector3(
            -1 + (2 * width * col) + width,
            +1 - (2 * width * row) - width,
            0
        );
        piece.localScale = ((2 * width) - gapThickness) * Vector3.one;
        piece.name = $"{(row * size) + col}";

        // 머티리얼 설정
        MeshRenderer renderer = piece.GetComponent<MeshRenderer>();
        renderer.material = new Material(material);

        // UV 설정
        SetupPieceUV(piece, row, col, width, gapThickness);
    }

    private void SetupPieceUV(Transform piece, int row, int col, float width, float gap)
    {
        Mesh mesh = piece.GetComponent<MeshFilter>().mesh;
        Vector2[] uv = new Vector2[4];
        
        uv[0] = new Vector2((width * col) + gap/2, 1 - ((width * (row + 1)) - gap/2));
        uv[1] = new Vector2((width * (col + 1)) - gap/2, 1 - ((width * (row + 1)) - gap/2));
        uv[2] = new Vector2((width * col) + gap/2, 1 - ((width * row) + gap/2));
        uv[3] = new Vector2((width * (col + 1)) - gap/2, 1 - ((width * row) + gap/2));
        
        mesh.uv = uv;
    }
    
    /// <summary>
    /// 게임 종료 페이지를 표시합니다
    /// </summary>
    private void ShowGameEndPage()
    {
        // 게임 플레이 시간 계산
        var endTime = DateTime.Now;
        var gameTime = endTime.Subtract(startTime);
        
        // 스테이지 이름 생성
        string stageName = "Puzzle";
        if (currentPuzzleData != null && !string.IsNullOrEmpty(currentPuzzleData.puzzleName))
        {
            stageName = currentPuzzleData.puzzleName;
        }
        else if (Global.MainMenuSelectedManager != null && Global.MainMenuSelectedManager.HasSelectedStage())
        {
            var selectedStageInfo = Global.MainMenuSelectedManager.SelectedStageInfo;
            stageName = $"Stage {selectedStageInfo.stageIndex + 1}";
        }
        
        // GameEndPage 열기
        var gameEndPage = Global.UIManager.OpenPage<GameEndPage>();
        if (gameEndPage != null)
        {
            gameEndPage.SetPuzzleGameResult(gameTime, stageName);
        }
        
        Debug.Log($"[PuzzleGameManager] Game completed in {gameTime:mm\\:ss} - {stageName}");
    }
}