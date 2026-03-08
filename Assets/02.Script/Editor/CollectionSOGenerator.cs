using UnityEngine;
using UnityEditor;
using SO;
using System.IO;

/// <summary>
/// carrot farm 폴더의 스프라이트들을 기반으로 CollectionSO 에셋을 자동 생성하는 에디터 도구입니다.
/// </summary>
public class CollectionSOGenerator : EditorWindow
{
    private string spriteFolderPath = "Assets/03.NormalResource/Sprite/Diary Res/Sticker/carrot farm";
    private string outputFolderPath = "Assets/02.Script/Data/SO/Collection";
    private int chapterIndex = 0;

    [MenuItem("Tools/Collection/Carrot Farm 컬렉션 SO 생성")]
    public static void ShowWindow()
    {
        GetWindow<CollectionSOGenerator>("Collection SO Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Collection SO 자동 생성기", EditorStyles.boldLabel);
        GUILayout.Space(10);

        spriteFolderPath = EditorGUILayout.TextField("스프라이트 폴더 경로", spriteFolderPath);
        outputFolderPath = EditorGUILayout.TextField("SO 출력 경로", outputFolderPath);
        chapterIndex = EditorGUILayout.IntField("챕터 인덱스", chapterIndex);

        GUILayout.Space(10);

        if (GUILayout.Button("CollectionSO 에셋 생성"))
        {
            GenerateCollectionAssets();
        }
    }

    private void GenerateCollectionAssets()
    {
        if (!Directory.Exists(spriteFolderPath))
        {
            Debug.LogError($"스프라이트 폴더를 찾을 수 없습니다: {spriteFolderPath}");
            return;
        }

        if (!Directory.Exists(outputFolderPath))
        {
            Directory.CreateDirectory(outputFolderPath);
        }

        string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { spriteFolderPath });
        int createdCount = 0;
        int skippedCount = 0;

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);

            if (sprite == null) continue;

            string spriteName = Path.GetFileNameWithoutExtension(assetPath);
            string soPath = $"{outputFolderPath}/{spriteName}.asset";

            // 이미 존재하는 에셋은 건너뜀
            if (AssetDatabase.LoadAssetAtPath<CollectionSO>(soPath) != null)
            {
                skippedCount++;
                continue;
            }

            CollectionSO collectionSO = ScriptableObject.CreateInstance<CollectionSO>();
            collectionSO.collectionImage = sprite;
            collectionSO.collectionName = "Collection/Name/" + spriteName;
            collectionSO.chapterIndex = chapterIndex;

            AssetDatabase.CreateAsset(collectionSO, soPath);
            createdCount++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"CollectionSO 생성 완료! 생성: {createdCount}개, 건너뜀(이미 존재): {skippedCount}개");
    }
}
