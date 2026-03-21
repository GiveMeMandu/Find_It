using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using SO;
using Manager;

public class CollectionImageAutoAssigner : EditorWindow
{
    [Header("타겟 설정")]
    public DefaultAsset targetFolder;
    public string collectionSOPath = "Assets/02.Script/Data/SO/Collection";

    private SerializedObject serializedObject;
    private SerializedProperty folderProperty;

    private Vector2 scrollPos;
    private List<CollectionSO> collections = new List<CollectionSO>();

    [MenuItem("Tools/Collection Image 자동 할당 도구")]
    public static void ShowWindow()
    {
        var window = GetWindow<CollectionImageAutoAssigner>("Collection Auto Assigner");
        window.Show();
    }

    private void OnEnable()
    {
        serializedObject = new SerializedObject(this);
        folderProperty = serializedObject.FindProperty("targetFolder");
        LoadCollections();
    }

    private void LoadCollections()
    {
        collections.Clear();
        string[] guids = AssetDatabase.FindAssets("t:CollectionSO", new[] { collectionSOPath });
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            CollectionSO so = AssetDatabase.LoadAssetAtPath<CollectionSO>(path);
            if (so != null)
            {
                collections.Add(so);
            }
        }
    }

    private void OnGUI()
    {
        serializedObject.Update();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("대상 폴더 설정", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(folderProperty, new GUIContent("탐색할 에셋 폴더 (예: Stage1)"));
        
        EditorGUILayout.Space();
        collectionSOPath = EditorGUILayout.TextField("Collection SO 폴더 경로", collectionSOPath);
        if (GUILayout.Button("Collection SO 새로고침"))
        {
            LoadCollections();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"로드된 Collection SO 개수: {collections.Count}");

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space(20);

        GUI.backgroundColor = new Color(0.6f, 1f, 0.6f);
        if (GUILayout.Button("자동 매핑 실행", GUILayout.Height(40)))
        {
            ExecuteMapping();
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndScrollView();
    }

    private void ExecuteMapping()
    {
        if (targetFolder == null)
        {
            EditorUtility.DisplayDialog("오류", "탐색할 폴더를 할당해주세요.", "확인");
            return;
        }

        string folderPath = AssetDatabase.GetAssetPath(targetFolder);
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            EditorUtility.DisplayDialog("오류", "유효한 폴더가 아닙니다.", "확인");
            return;
        }

        // Texture2D 에셋 검색
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new string[] { folderPath });
        
        int updatedCount = 0;
        int matchedSpritesCount = 0;

        foreach (var collection in collections)
        {
            if (collection == null) continue;

            List<string> searchKeywords = new List<string>();
            
            if (!string.IsNullOrEmpty(collection.mappingKeywords))
            {
                var keys = collection.mappingKeywords.Split(',');
                foreach (var k in keys)
                {
                    string trimmed = k.Trim().ToLower();
                    if (!string.IsNullOrEmpty(trimmed))
                    {
                        searchKeywords.Add(trimmed);
                    }
                }
            }
            else
            {
                // 기본으로 에셋 이름을 키워드로 사용
                searchKeywords.Add(collection.name.ToLower());
            }

            if (searchKeywords.Count == 0) continue;

            bool collectionUpdated = false;

            if (collection.inGameSprites == null)
            {
                collection.inGameSprites = new List<Sprite>();
            }

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(assetPath).Where(x => x is Sprite).ToArray();

                foreach (Sprite spr in sprites)
                {
                    string spriteName = spr.name.ToLower();
                    
                    bool match = false;
                    foreach (var keyword in searchKeywords)
                    {
                        if (spriteName.Contains(keyword))
                        {
                            match = true;
                            break;
                        }
                    }

                    if (match && !collection.inGameSprites.Contains(spr))
                    {
                        collection.inGameSprites.Add(spr);
                        collectionUpdated = true;
                        matchedSpritesCount++;
                    }
                }
            }

            if (collectionUpdated)
            {
                EditorUtility.SetDirty(collection);
                updatedCount++;
            }
        }

        if (updatedCount > 0)
        {
            AssetDatabase.SaveAssets();
        }

        EditorUtility.DisplayDialog("완료", $"자동 매핑이 완료되었습니다!\n\n업데이트된 Collection SO: {updatedCount}개\n매핑된 스프라이트 수: {matchedSpritesCount}개", "확인");
    }
}
