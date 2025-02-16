using UnityEngine;

public class PuzzleSelectManager : MonoBehaviour
{
    [SerializeField] private PuzzleGameManager gameManager;
    [SerializeField] private PuzzleData[] availablePuzzles;

    public PuzzleData[] AvailablePuzzles => availablePuzzles;
    void OnEnable()
    {
        SelectPuzzle(0);
    }
    public void SelectPuzzle(int index)
    {
        gameManager.InitializePuzzle(index);
    }

    public Color GetDifficultyColor(float difficulty)
    {
        if (difficulty <= 0.3f) return Color.green;
        if (difficulty <= 0.6f) return Color.yellow;
        return Color.red;
    }
}
