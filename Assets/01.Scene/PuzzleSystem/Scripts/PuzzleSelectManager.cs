using UnityEngine;

public class PuzzleSelectManager : MonoBehaviour
{
    [SerializeField] private PuzzleGameManager puzzleGameManager;
    [SerializeField] private PuzzleData[] availablePuzzles;

    public PuzzleData[] AvailablePuzzles => availablePuzzles;

    private void Awake()
    {
        if (puzzleGameManager == null)
        {
            Debug.LogError("PuzzleGameManager reference is missing!");
            return;
        }
    }

    public void SelectPuzzle(int index)
    {
        if (puzzleGameManager != null && index >= 0 && index < availablePuzzles.Length)
        {
            PuzzleData selectedPuzzle = availablePuzzles[index];
            puzzleGameManager.InitializePuzzle(selectedPuzzle.sceneName, selectedPuzzle.stageIndex);
        }
    }

    public Color GetDifficultyColor(float difficulty)
    {
        if (difficulty <= 0.3f) return Color.green;
        if (difficulty <= 0.6f) return Color.yellow;
        return Color.red;
    }
}
