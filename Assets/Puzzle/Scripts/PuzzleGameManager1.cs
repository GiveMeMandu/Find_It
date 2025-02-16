using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PuzzleGameManager1 : MonoBehaviour
{
    [SerializeField] private Transform gameTransform;
    [SerializeField] private Transform piecePrefab;

    private List<Transform> pieces;
    private int emptyLocation;
    private int size;
    private bool shuffling = false;

    void Start()
    {
        pieces = new List<Transform>();
        size = 3;
        CreateGamePieces(0.01f);
    }

    void Update()
    {
        if (!shuffling && CheckCompletion())
        {
            shuffling = true;
            StartCoroutine(WaitShuffle(0.5f));
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleMouseInput();
        }
    }

    private void HandleMouseInput()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(mousePosition), Vector2.zero);

        if (hit)
        {
            for (int i = 0; i < pieces.Count; i++)
            {
                if (pieces[i] == hit.transform)
                {
                    // Try all possible moves
                    if (SwapIfValid(i, -size, size)) break;
                    if (SwapIfValid(i, +size, size)) break;
                    if (SwapIfValid(i, -1, 0)) break;
                    if (SwapIfValid(i, +1, size - 1)) break;
                }
            }
        }
    }

    private void CreateGamePieces(float gapThickness)
    {
        float width = 1f / size;
        
        for (int row = 0; row < size; row++)
        {
            for (int col = 0; col < size; col++)
            {
                CreatePiece(row, col, width, gapThickness);
            }
        }
    }

    private void CreatePiece(int row, int col, float width, float gapThickness)
    {
        Transform piece = Instantiate(piecePrefab, gameTransform);
        pieces.Add(piece);

        piece.localPosition = new Vector3(
            -1 + (2 * width * col) + width,
            +1 - (2 * width * row) - width,
            0
        );
        piece.localScale = ((2 * width) - gapThickness) * Vector3.one;
        piece.name = $"{(row * size) + col}";

        if ((row == size - 1) && (col == size - 1))
        {
            emptyLocation = (size * size) - 1;
            piece.gameObject.SetActive(false);
        }
        else
        {
            SetupPieceUV(piece, row, col, width, gapThickness);
        }
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

    private bool SwapIfValid(int i, int offset, int colCheck)
    {
        if (((i % size) != colCheck) && ((i + offset) == emptyLocation))
        {
            (pieces[i], pieces[i + offset]) = (pieces[i + offset], pieces[i]);
            (pieces[i].localPosition, pieces[i + offset].localPosition) = 
                (pieces[i + offset].localPosition, pieces[i].localPosition);
            emptyLocation = i;
            return true;
        }
        return false;
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
        yield return new WaitForSeconds(duration);
        Shuffle();
        shuffling = false;
    }

    private void Shuffle()
    {
        int count = 0;
        int last = 0;
        
        while (count < (size * size * size))
        {
            int rnd = Random.Range(0, size * size);
            if (rnd == last) continue;
            
            last = emptyLocation;
            
            if (SwapIfValid(rnd, -size, size) ||
                SwapIfValid(rnd, +size, size) ||
                SwapIfValid(rnd, -1, 0) ||
                SwapIfValid(rnd, +1, size - 1))
            {
                count++;
            }
        }
    }
}