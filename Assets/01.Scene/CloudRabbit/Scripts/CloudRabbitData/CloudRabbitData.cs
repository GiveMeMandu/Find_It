using UnityEngine;
using Sirenix.OdinInspector;
using Data;

#if UNITY_EDITOR
using UnityEditor;
#endif
[CreateAssetMenu(fileName = "CloudRabbitData", menuName = "CloudRabbit/CloudRabbitData")]
public class CloudRabbitData : ScriptableObject
{
    public SceneName sceneName;
    public int stageIndex;
    public string stageName = "";    // 퍼즐 이름
    public float difficulty = 1.0f;     // 난이도
}