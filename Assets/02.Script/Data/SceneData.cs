using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    public static class SceneNum
    {
        public const int BOOTSTRAP = 0;
        public const int LOADING = 1;
        public const int START = 2;
        public const int SELECT = 3;
        public const int STAGE1 = 4;
        public const int STAGE2 = 5;
    
    }
    public enum SceneName{
        Bootstrap,
        LoadingScene,
        Start,
        Select,
        Stage1,
        Stage2,
        Puzzle,
        Credit
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