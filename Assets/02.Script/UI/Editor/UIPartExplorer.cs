using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UI.Editor
{
    public class UIPartExplorer : EditorWindow
    {
        struct PartData
        {
            public string path;
            public Object obj;
            public UIPartHelper uiPartHelper;

            public PartData(string path, Object obj, UIPartHelper uiPartHelper)
            {
                this.path = path;
                this.obj = obj;
                this.uiPartHelper = uiPartHelper;
            }
        }
        private readonly string ROOT_PATH = "Assets/5.Prefabs/UI/UIPart/";

        [MenuItem("UI/파츠 탐색기", false, 201)]
        public static void ShowWindow()
        {
            var window = GetWindow<UIPartExplorer>();
            window.titleContent = new GUIContent("UI 파츠 탐색기");
            window.RefreshPartData();
        }

        private GUIStyle _guiStyle_title = new GUIStyle();
        private Vector2 _scrollPosition;
        private List<string> _partPath = new List<string>();

        private Dictionary<string, PartData> _dicPartData = new Dictionary<string, PartData>();
        
        private void Awake()
        {
            RefreshPartData();
            _guiStyle_title.fontSize = 20;
            _guiStyle_title.normal.textColor = Color.white;
        }
        
        public void OnGUI()
        {
            if (GUILayout.Button("새로고침"))
            {
                RefreshPartData();
            }
            GUILayout.Space(6f);
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            using (new EditorGUILayout.VerticalScope())
            {
                DrawPartList();
            }
            GUILayout.EndScrollView();
        }
        
        private void DrawPartList()
        {
            foreach (var partData in _dicPartData)
            {
                DrawPart(partData.Key, partData.Value);
            }
        }
        
        private void DrawPart(string key, PartData partData)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    if (GUILayout.Button(AssetPreview.GetMiniThumbnail(partData.obj), GUILayout.Width(120), GUILayout.Height(60)))
                    {
                        EditorGUIUtility.PingObject(partData.obj);
                    }
        
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("열기", GUILayout.Width(57), GUILayout.Height(20)))
                        {
                            OpenPartPrefab(partData.path);
                        }
        
                        bool useAddFunc = !EditorApplication.isPlaying && PrefabStageUtility.GetCurrentPrefabStage() != null;
        
                        EditorGUI.BeginDisabledGroup(!useAddFunc);
                        if (GUILayout.Button("추가하기", GUILayout.Width(60), GUILayout.Height(20)))
                        {
                            LoadPartPrefab(partData.path);
                        }
                        EditorGUI.EndDisabledGroup();
                    }
                }
                    
        
                using (new EditorGUILayout.VerticalScope())
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Label(key, _guiStyle_title);
                    }
        
                    if (partData.uiPartHelper != null)
                    {
                        GUILayout.Label(partData.uiPartHelper.description);
                    }
                }
                GUILayout.FlexibleSpace();
            }
        
            GUILayout.Space(10);
        }
        
        
        private void OpenPartPrefab(string path)
        {
            AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)));
        }
        
        private void LoadPartPrefab(string path)
        {
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                var originPrefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
                GameObject prefab = PrefabUtility.InstantiatePrefab(originPrefab) as GameObject;
                StageUtility.PlaceGameObjectInCurrentStage(prefab);
        
                if (Selection.activeTransform != null)
                {
                    prefab.transform.SetParent(Selection.activeTransform);
                }
                prefab.transform.localPosition = Vector3.zero;
                
                Selection.activeObject = prefab;
                EditorSceneManager.MarkSceneDirty(prefabStage.scene);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "프리팹 편집 모드에서만 이용 가능합니다.", "확인");
            }
        }
        
        public void RefreshPartData()
        { 
            _partPath.Clear();
            _partPath = Directory.GetFiles(ROOT_PATH).ToList();
            _partPath.AddRange(Directory.GetFiles(ROOT_PATH + "/CustomUI").ToList());
            _partPath.AddRange(Directory.GetFiles(ROOT_PATH + "/InGameUI").ToList());
            _partPath = _partPath.FindAll(p => !(p.EndsWith(".meta")));
        
            _dicPartData.Clear();
        
            foreach (string path in _partPath)
            {
                Object obj = AssetDatabase.LoadMainAssetAtPath(path);
                if (obj == null)
                {
                    continue;
                }
        
                UIPartHelper uiPartHelper = ((GameObject)AssetDatabase.LoadMainAssetAtPath(path)).GetComponent<UIPartHelper>();
                if (uiPartHelper == null)
                {
                    continue;
                }
                string key = Path.GetFileNameWithoutExtension(path);
                _dicPartData.Add(key, new PartData(path, obj, uiPartHelper));
            }
        }
    }
}
