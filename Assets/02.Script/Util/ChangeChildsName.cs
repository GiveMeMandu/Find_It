using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class ChangeChildsName : MonoBehaviour
{
    [SerializeField] private string searchKeyword = "";  // 검색할 키워드
    [SerializeField] private string newName = "";       // 변경할 새 이름
    
    [Button("Change Children Names")]
    public void ChangeNames()
    {
        if (string.IsNullOrEmpty(searchKeyword) || string.IsNullOrEmpty(newName))
        {
            Debug.LogWarning("검색 키워드와 새 이름을 모두 입력해주세요.");
            return;
        }

        Transform[] allChildren = GetComponentsInChildren<Transform>();
        
        foreach (Transform child in allChildren)
        {
            if (child != transform && child.name.Contains(searchKeyword))
            {
                child.name = child.name.Replace(searchKeyword, newName);
            }
        }
    }

}
