using UnityEngine;
using UnityEditor;
using I2.Loc;

public class I2FontBatchUpdater : EditorWindow
{
    [MenuItem("Tools/폰트 일괄 업데이트")]
    public static void DeepUpdateAllFonts()
    {
        // 1. 프로젝트 내 모든 프리팹 검색
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        int updatedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            // 2. 프리팹 내부에 있는 모든 Localize 컴포넌트를 배열로 가져옴 (자식 오브젝트 포함)
            Localize[] localizers = prefab.GetComponentsInChildren<Localize>(true);

            if (localizers.Length > 0)
            {
                foreach (var loc in localizers)
                {
                    loc.mTermSecondary = "Font/Main"; // 아까 만든 폰트용 Term 이름
                    EditorUtility.SetDirty(loc);
                    updatedCount++;
                }
            }
        }

        // 3. 현재 열려 있는 씬의 오브젝트들도 처리
        Localize[] sceneLocalizers = Resources.FindObjectsOfTypeAll<Localize>();
        foreach (var loc in sceneLocalizers)
        {
            if (loc.gameObject.scene.name != null) 
            {
                loc.mTermSecondary = "Font/Main";
                EditorUtility.SetDirty(loc);
                updatedCount++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[I2 Localization] 총 {updatedCount}개의 UI 컴포넌트 설정을 갱신했습니다.");
    }
}