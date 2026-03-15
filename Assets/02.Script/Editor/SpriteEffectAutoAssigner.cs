using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class SpriteEffectAutoAssigner : EditorWindow
{
    [System.Serializable]
    public class ParticleTypeMapping
    {
        [Tooltip("파티클 타입")]
        public SpriteEffectMapSO.ParticleType particleType;
        
        [Tooltip("매칭할 스프라이트 이름의 키워드들 (쉼표로 구분. 예: apple, berry, leaf)")]
        public string keywords;
    }

    [Header("타겟 설정")]
    public SpriteEffectMapSO targetSO;
    public DefaultAsset targetFolder;

    [Header("매핑 규칙 (Particle Type 기준)")]
    public List<ParticleTypeMapping> mappingRules = new List<ParticleTypeMapping>();

    private SerializedObject serializedObject;
    private SerializedProperty soProperty;
    private SerializedProperty folderProperty;
    private SerializedProperty rulesProperty;

    private Vector2 scrollPos;

    [MenuItem("Tools/Sprite Effect 자동 할당 도구")]
    public static void ShowWindow()
    {
        var window = GetWindow<SpriteEffectAutoAssigner>("Sprite Effect Assigner");
        window.Show();
    }

    private void OnEnable()
    {
        serializedObject = new SerializedObject(this);
        soProperty = serializedObject.FindProperty("targetSO");
        folderProperty = serializedObject.FindProperty("targetFolder");
        rulesProperty = serializedObject.FindProperty("mappingRules");

        LoadSettings();
    }

    private void OnDisable()
    {
        SaveSettings();
    }

    private void LoadSettings()
    {
        string data = EditorPrefs.GetString("SpriteEffectAutoAssigner_Rules_PerType", "");
        if (!string.IsNullOrEmpty(data))
        {
            EditorJsonUtility.FromJsonOverwrite(data, this);
        }
        
        if (mappingRules == null || mappingRules.Count == 0 || !mappingRules.Any(r => r.particleType == SpriteEffectMapSO.ParticleType.FruitJuicy))
        {
            InitializeDefaultRules();
        }
    }

    private void InitializeDefaultRules()
    {
        mappingRules = new List<ParticleTypeMapping>();
        
        // 모든 Enum 값에 대해 기본 항목 생성
        foreach (SpriteEffectMapSO.ParticleType pType in System.Enum.GetValues(typeof(SpriteEffectMapSO.ParticleType)))
        {
            if (pType == SpriteEffectMapSO.ParticleType.None) continue;
            
            var rule = new ParticleTypeMapping { particleType = pType, keywords = "" };
            
            // 기존에 제공된 키워드들을 각 타입에 맞춰 기본값으로 할당
            switch (pType)
            {
                case SpriteEffectMapSO.ParticleType.FruitJuicy:
                    rule.keywords = "apple, cherry, strawberry, strawberries, berry, berries, avocado, fruit, food, lemon, jam, honey, radish";
                    break;
                case SpriteEffectMapSO.ParticleType.Carrot:
                    rule.keywords = "carrot";
                    break;
                case SpriteEffectMapSO.ParticleType.Leaf:
                    rule.keywords = "tree, leaf, grass, flower, reed, dandelion, sunflower, clover, branch, liquid";
                    break;
                case SpriteEffectMapSO.ParticleType.WoodyChop:
                    rule.keywords = "wood, table, chair, ladder, house, swing, cart, log, desk, shelf, door";
                    break;
                case SpriteEffectMapSO.ParticleType.StoneCrunch:
                    rule.keywords = "rock, stone, chest, gear, screw, scissors";
                    break;
                case SpriteEffectMapSO.ParticleType.WaterSplash:
                    rule.keywords = "sea, water, surface";
                    break;
                case SpriteEffectMapSO.ParticleType.DustPuff:
                    rule.keywords = "sky, cloud, dust, puff";
                    break;
                case SpriteEffectMapSO.ParticleType.MagicalGlow:
                    rule.keywords = "candle, lamp, fire, light, magic, crystal, star, moon";
                    break;
                case SpriteEffectMapSO.ParticleType.EarthDust:
                    rule.keywords = "book, paper, parchment, pad, ground, earth";
                    break;
                case SpriteEffectMapSO.ParticleType.FabricSoft:
                    rule.keywords = "carpet, drapery, canvas, balloon, hat, feather";
                    break;
            }
            
            mappingRules.Add(rule);
        }
        
        // None 처리를 위한 기타 항목
        mappingRules.Add(new ParticleTypeMapping { 
            particleType = SpriteEffectMapSO.ParticleType.None, 
            keywords = "cup, bowl, fork, knife, spoon, pot, mirror, necklace, heart, nest, coin, clock, key, bee, butterfly, caterpillar, rabbit, otter" 
        });
    }

    private void SaveSettings()
    {
        string data = EditorJsonUtility.ToJson(this);
        EditorPrefs.SetString("SpriteEffectAutoAssigner_Rules_PerType", data);
    }

    private void OnGUI()
    {
        serializedObject.Update();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("대상 파일 및 폴더 설정", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(soProperty, new GUIContent("타겟 SO (SpriteEffectMapSO)"));
        EditorGUILayout.PropertyField(folderProperty, new GUIContent("탐색할 에셋 폴더 (예: Stage1)"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("파티클 타입별 키워드 설정", EditorStyles.boldLabel);
        
        // 커스텀 리스트 그리기
        EditorGUI.indentLevel++;
        for (int i = 0; i < mappingRules.Count; i++)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(mappingRules[i].particleType.ToString(), EditorStyles.boldLabel, GUILayout.Width(150));
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                mappingRules.RemoveAt(i);
                break;
            }
            EditorGUILayout.EndHorizontal();
            
            SerializedProperty ruleProp = rulesProperty.GetArrayElementAtIndex(i);
            SerializedProperty keywordsProp = ruleProp.FindPropertyRelative("keywords");
            
            EditorGUILayout.PropertyField(keywordsProp, new GUIContent("키워드 (쉼표 구분)"));
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }
        EditorGUI.indentLevel--;
        
        if (GUILayout.Button("+ 새 규칙 추가"))
        {
            mappingRules.Add(new ParticleTypeMapping { particleType = SpriteEffectMapSO.ParticleType.None, keywords = "" });
        }

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space(20);

        GUI.backgroundColor = new Color(0.6f, 1f, 0.6f);
        if (GUILayout.Button("자동 매핑 실행", GUILayout.Height(40)))
        {
            ExecuteMapping();
            SaveSettings(); // 실행 시 확실하게 저장
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.Space(10);
        if (GUILayout.Button("설정 초기화 (기본 키워드로 복구)"))
        {
            InitializeDefaultRules();
        }

        EditorGUILayout.EndScrollView();
    }

    private void ExecuteMapping()
    {
        if (targetSO == null)
        {
            EditorUtility.DisplayDialog("오류", "타겟 SpriteEffectMapSO를 할당해주세요.", "확인");
            return;
        }

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

        // Texture2D 에셋만 검색 (PSD, PNG 등 모두 포함됨)
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new string[] { folderPath });
        
        int addedCount = 0;
        int updatedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            
            // 파일 경로 안에 있는 모든 서브 에셋 로드 (PSD 레이어 스프라이트 파싱)
            Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (Object asset in allAssets)
            {
                // 스프라이트 타입인 경우만 처리
                if (asset is Sprite sprite)
                {
                    string spriteName = sprite.name.ToLower();
                    bool isMatch = false;

                    // 등록된 규칙 전부 순회
                    foreach (var rule in mappingRules)
                    {
                        if (string.IsNullOrEmpty(rule.keywords)) continue;

                        string[] keywordArray = rule.keywords.Split(',');

                        foreach (string kw in keywordArray)
                        {
                            string trimKw = kw.Trim().ToLower();
                            if (string.IsNullOrEmpty(trimKw)) continue;

                            // 완전 일치 또는 포함되는지 확인
                            if (spriteName.Contains(trimKw))
                            {
                                isMatch = true;
                                break;
                            }
                        }

                        if (isMatch)
                        {
                            var existingEntry = targetSO.entries.FirstOrDefault(e => e.sprite == sprite);
                            
                            // 이미 SO에 등록된 스프라이트라면 enum 갱신
                            if (existingEntry != null)
                            {
                                if (existingEntry.particleType != rule.particleType)
                                {
                                    existingEntry.particleType = rule.particleType;
                                    updatedCount++;
                                }
                            }
                            // 없는 경우 새로 추가
                            else
                            {
                                var newEntry = new SpriteEffectMapSO.SpriteEffectEntry
                                {
                                    sprite = sprite,
                                    particleType = rule.particleType
                                };
                                targetSO.entries.Add(newEntry);
                                addedCount++;
                            }

                            break; // 첫 번째로 일치하는 규칙을 적용하고 다음 스프라이트로 넘어감
                        }
                    }
                }
            }
        }

        // SO의 변경사항 저장 요청
        EditorUtility.SetDirty(targetSO);
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("완료", $"자동 매핑이 완료되었습니다!\n\n새로 추가됨: {addedCount}개\n업데이트됨: {updatedCount}개", "확인");
    }
}