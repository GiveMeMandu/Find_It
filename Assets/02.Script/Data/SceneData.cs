using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    public enum SceneName{
        MainMenu,
        InGame1,
        InGame2,
        InGame3,
        InGame4,
        InGame5,
        InGame6,
        InGame7,
        InGame8,
        InGame9,
        InGame10,
    }
    public class SceneData
    {
        public SceneData()
        {
            upgradeData = new List<UpgradeData>(); // 초기화 
        }
        public SceneName sceneName;
        public List<UpgradeData> upgradeData;
    }

    public class UpgradeData
    {
        public string upgradeName;
        public int level;
    }
}