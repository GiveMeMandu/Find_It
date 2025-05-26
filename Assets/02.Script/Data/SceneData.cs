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
        public const int STAGE1_1 = 4;
        public const int STAGE1_2 = 5;
        public const int STAGE1_3 = 6;
        public const int STAGE2_1 = 7;
        public const int STAGE2_2 = 8;
        public const int STAGE2_3 = 9;
        public const int PUZZLE = 10;
        public const int CLOUD_RABBIT = 11;
        public const int CREDIT = 12;
        public const int CLOUD_RABBIT_STAGE1_1 = 13;
        public const int CLOUD_RABBIT_STAGE1_2 = 14;
        public const int CLOUD_RABBIT_STAGE1_3 = 15;
        public const int CLOUD_RABBIT_STAGE2_1 = 16;
        public const int CLOUD_RABBIT_STAGE2_2 = 17;
        public const int CLOUD_RABBIT_STAGE2_3 = 18;
        public const int CLOUD_RABBIT_STAGE3_1 = 19;
        public const int CLOUD_RABBIT_STAGE3_2 = 20;
        public const int CLOUD_RABBIT_STAGE3_3 = 21;
    
    }
    public enum SceneName{
        Bootstrap,
        LoadingScene,
        Start,
        Select,
        Stage1_1,
        Stage1_2,
        Stage1_3,
        Stage2_1,
        Stage2_2,
        Stage2_3,
        Puzzle,
        CloudRabbit,
        Credit,
        CloudRabbitStage1_1,
        CloudRabbitStage1_2,
        CloudRabbitStage1_3,
        CloudRabbitStage2_1,
        CloudRabbitStage2_2,
        CloudRabbitStage2_3,
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