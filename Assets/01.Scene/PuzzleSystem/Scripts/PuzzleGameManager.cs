using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PuzzleGameManager : MonoBehaviour
{
    [SerializeField] private Transform gameTransform;
    [SerializeField] private Transform piecePrefab;
    [SerializeField] private PuzzleData[] puzzleDataList;

    private List<Transform> pieces;
    private bool shuffling = false;
    private Transform draggedPiece = null;
    private Vector3 dragOffset;
    private Vector3 originalPosition;
    private int currentPuzzleIndex = 0;

    void Start()
    {
        pieces = new List<Transform>();
        InitializePuzzle(currentPuzzleIndex);
    }

    public void InitializePuzzle(int puzzleIndex)
    {
        if (puzzleIndex < 0 || puzzleIndex >= puzzleDataList.Length) return;

        // 기존 피스들 제거
        foreach (var piece in pieces)
        {
            if (piece != null) Destroy(piece.gameObject);
        }
        pieces.Clear();

        // 새 퍼즐 설정
        PuzzleData puzzleData = puzzleDataList[puzzleIndex];
        currentPuzzleIndex = puzzleIndex;

        // 퍼즐 생성
        CreateGamePieces(puzzleData, 0.01f);
        StartCoroutine(WaitShuffle(0.5f));
    }

    void Update()
    {
        if (shuffling) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleDragStart();
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            HandleDragEnd();
        }
        else if (draggedPiece != null)
        {
            HandleDragging();
        }
    }

    private void HandleDragStart()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(mousePosition), Vector2.zero);

        if (hit && hit.transform != null)
        {
            draggedPiece = hit.transform;
            originalPosition = draggedPiece.localPosition;
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 0));
            dragOffset = draggedPiece.position - mouseWorld;
            draggedPiece.SetAsLastSibling(); // 드래그 중인 피스를 최상단에 표시
        }
    }

    private void HandleDragging()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 0));
        draggedPiece.position = mouseWorld + dragOffset;
    }

    private void HandleDragEnd()
    {
        if (draggedPiece == null) return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        RaycastHit2D[] hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(mousePosition), Vector2.zero);

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
            // 두 조각의 위치 교환
            Vector3 targetPosition = targetPiece.localPosition;
            targetPiece.localPosition = originalPosition;
            draggedPiece.localPosition = targetPosition;

            // pieces 리스트에서의 위치도 교환
            int draggedIndex = pieces.IndexOf(draggedPiece);
            int targetIndex = pieces.IndexOf(targetPiece);
            pieces[draggedIndex] = targetPiece;
            pieces[targetIndex] = draggedPiece;
        }
        else
        {
            // 교환할 대상이 없으면 원래 위치로 복귀
            draggedPiece.localPosition = originalPosition;
        }

        draggedPiece = null;
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

    private bool CheckCompletion()
    {
        for (int i = 0; i < pieces.Count; i++)
        {
            if (pieces[i].name != $"{i}") return false;
        }
        return true;
    }

    private IEnumerator WaitShuffle(float duration)
    {
        shuffling = true;
        yield return new WaitForSeconds(duration);
        Shuffle();
        shuffling = false;
        
        // 셔플이 완료된 후에 완성 체크를 시작하기 위해 새로운 코루틴 시작
        StartCoroutine(CheckCompletionRoutine());
    }

    private IEnumerator CheckCompletionRoutine()
    {
        while (true)
        {
            if (!shuffling && CheckCompletion())
            {
                Debug.Log("퍼즐 완성!");
                yield break; // 완성되면 코루틴 종료
            }
            yield return new WaitForSeconds(0.5f); // 0.5초마다 체크
        }
    }

    private void Shuffle()
    {
        shuffling = true;
        for (int i = 0; i < pieces.Count; i++)
        {
            int random = Random.Range(0, pieces.Count);
            
            // 위치 교환
            Vector3 tempPosition = pieces[i].localPosition;
            pieces[i].localPosition = pieces[random].localPosition;
            pieces[random].localPosition = tempPosition;

            // 리스트에서 교환
            (pieces[i], pieces[random]) = (pieces[random], pieces[i]);
        }
    }
}