using UnityEngine;
using Sirenix.OdinInspector;
using Data;

#if UNITY_EDITOR
using UnityEditor;
#endif
[CreateAssetMenu(fileName = "PuzzleData", menuName = "Puzzle/PuzzleData")]
public class PuzzleData : ScriptableObject
{
#if UNITY_EDITOR
    [Button("GetDefaultImage")]
    public void GetDefaultImage()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/03.Resource/Sprite/Puzzle/Slice/Stage1" });
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            puzzleImage = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            
            // 프로젝트 창에서 에셋 하이라이트
            EditorGUIUtility.PingObject(texture);
        }
        else
        {
            Debug.LogWarning("지정된 경로에서 Texture를 찾을 수 없습니다.");
        }
    }
#endif
    public SceneName sceneName;
    public int stageIndex;
    public Sprite puzzleImage;    // 전체 퍼즐 이미지
    public int size = 4;             // 퍼즐 크기 (4x4, 3x3 등)
    public string puzzleName = "예시이름";    // 퍼즐 이름
    public float difficulty = 1.0f;     // 난이도
}