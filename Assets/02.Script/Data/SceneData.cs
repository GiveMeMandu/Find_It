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
        public const int TimeChallenge_STAGE1_1 = 13;
        public const int TimeChallenge_STAGE1_2 = 14;
        public const int TimeChallenge_STAGE1_3 = 15;
        public const int TimeChallenge_STAGE2_1 = 16;
        public const int TimeChallenge_STAGE2_2 = 17;
        public const int TimeChallenge_STAGE2_3 = 18;
        public const int TimeChallenge_STAGE3_1 = 19;
        public const int TimeChallenge_STAGE3_2 = 20;
        public const int TimeChallenge_STAGE3_3 = 21;
    
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
        TimeChallenge_STAGE1_1,
        TimeChallenge_STAGE1_2,
        TimeChallenge_STAGE1_3,
        TimeChallenge_STAGE2_1,
        TimeChallenge_STAGE2_2,
        TimeChallenge_STAGE2_3,
        TimeChallenge_STAGE3_1,
        TimeChallenge_STAGE3_2,
        TimeChallenge_STAGE3_3,
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
    
    /// <summary>
    /// 씬 관리를 위한 헬퍼 메서드들
    /// </summary>
    public static class SceneHelper
    {
        /// <summary>
        /// 씬 이름에서 월드 번호를 추출합니다 (1, 2, 3...)
        /// </summary>
        /// <param name="sceneName">씬 이름</param>
        /// <returns>월드 번호 (Stage1_1 -> 1, Stage2_2 -> 2), 해당없으면 0</returns>
        public static int GetWorldNumber(SceneName sceneName)
        {
            string name = sceneName.ToString();
            
            if (name.StartsWith("Stage1") || name.StartsWith("TimeChallenge_STAGE1"))
                return 1;
            if (name.StartsWith("Stage2") || name.StartsWith("TimeChallenge_STAGE2"))
                return 2;
            if (name.StartsWith("Stage3") || name.StartsWith("TimeChallenge_STAGE3"))
                return 3;
                
            return 0;
        }
        
        /// <summary>
        /// 씬 이름에서 스테이지 번호를 추출합니다 (1, 2, 3...)
        /// </summary>
        /// <param name="sceneName">씬 이름</param>
        /// <returns>스테이지 번호 (Stage1_2 -> 2), 추출 실패시 0</returns>
        public static int GetStageNumber(SceneName sceneName)
        {
            string name = sceneName.ToString();
            
            // Stage1_1, Stage2_2, TimeChallenge_STAGE1_1 등에서 마지막 숫자 추출
            if (name.Contains("_"))
            {
                string[] parts = name.Split('_');
                if (parts.Length > 0)
                {
                    string lastPart = parts[parts.Length - 1];
                    if (int.TryParse(lastPart, out int stageNum))
                    {
                        return stageNum;
                    }
                }
            }
            
            return 0;
        }
        
        /// <summary>
        /// 일반 스테이지인지 확인합니다 (타임챌린지가 아닌)
        /// </summary>
        /// <param name="sceneName">씬 이름</param>
        /// <returns>일반 스테이지면 true</returns>
        public static bool IsNormalStage(SceneName sceneName)
        {
            string name = sceneName.ToString();
            return name.StartsWith("Stage") && !name.Contains("TimeChallenge");
        }
        
        /// <summary>
        /// 타임챌린지 스테이지인지 확인합니다
        /// </summary>
        /// <param name="sceneName">씬 이름</param>
        /// <returns>타임챌린지면 true</returns>
        public static bool IsTimeChallengeStage(SceneName sceneName)
        {
            return sceneName.ToString().StartsWith("TimeChallenge");
        }
        
        /// <summary>
        /// 같은 월드의 씬들을 가져옵니다
        /// </summary>
        /// <param name="worldNumber">월드 번호 (1, 2, 3...)</param>
        /// <param name="includeTimeChallenge">타임챌린지 포함 여부</param>
        /// <returns>해당 월드의 씬 리스트</returns>
        public static List<SceneName> GetScenesInWorld(int worldNumber, bool includeTimeChallenge = false)
        {
            List<SceneName> scenes = new List<SceneName>();
            
            foreach (SceneName scene in System.Enum.GetValues(typeof(SceneName)))
            {
                if (GetWorldNumber(scene) == worldNumber)
                {
                    if (includeTimeChallenge || IsNormalStage(scene))
                    {
                        scenes.Add(scene);
                    }
                }
            }
            
            return scenes;
        }
        
        /// <summary>
        /// 씬 이름을 Build Settings 인덱스로 변환합니다
        /// </summary>
        /// <param name="sceneName">씬 이름</param>
        /// <returns>Build Settings 인덱스</returns>
        public static int SceneNameToIndex(SceneName sceneName)
        {
            switch (sceneName)
            {
                case SceneName.Bootstrap: return SceneNum.BOOTSTRAP;
                case SceneName.LoadingScene: return SceneNum.LOADING;
                case SceneName.Start: return SceneNum.START;
                case SceneName.Select: return SceneNum.SELECT;
                case SceneName.Stage1_1: return SceneNum.STAGE1_1;
                case SceneName.Stage1_2: return SceneNum.STAGE1_2;
                case SceneName.Stage1_3: return SceneNum.STAGE1_3;
                case SceneName.Stage2_1: return SceneNum.STAGE2_1;
                case SceneName.Stage2_2: return SceneNum.STAGE2_2;
                case SceneName.Stage2_3: return SceneNum.STAGE2_3;
                case SceneName.Puzzle: return SceneNum.PUZZLE;
                case SceneName.CloudRabbit: return SceneNum.CLOUD_RABBIT;
                case SceneName.Credit: return SceneNum.CREDIT;
                case SceneName.TimeChallenge_STAGE1_1: return SceneNum.TimeChallenge_STAGE1_1;
                case SceneName.TimeChallenge_STAGE1_2: return SceneNum.TimeChallenge_STAGE1_2;
                case SceneName.TimeChallenge_STAGE1_3: return SceneNum.TimeChallenge_STAGE1_3;
                case SceneName.TimeChallenge_STAGE2_1: return SceneNum.TimeChallenge_STAGE2_1;
                case SceneName.TimeChallenge_STAGE2_2: return SceneNum.TimeChallenge_STAGE2_2;
                case SceneName.TimeChallenge_STAGE2_3: return SceneNum.TimeChallenge_STAGE2_3;
                case SceneName.TimeChallenge_STAGE3_1: return SceneNum.TimeChallenge_STAGE3_1;
                case SceneName.TimeChallenge_STAGE3_2: return SceneNum.TimeChallenge_STAGE3_2;
                case SceneName.TimeChallenge_STAGE3_3: return SceneNum.TimeChallenge_STAGE3_3;
                default: return 0;
            }
        }
        
        /// <summary>
        /// 포맷된 스테이지 이름을 가져옵니다 (예: "World 1-2")
        /// </summary>
        /// <param name="sceneName">씬 이름</param>
        /// <returns>포맷된 이름</returns>
        public static string GetFormattedStageName(SceneName sceneName)
        {
            int world = GetWorldNumber(sceneName);
            int stage = GetStageNumber(sceneName);
            
            if (world > 0 && stage > 0)
            {
                string prefix = IsTimeChallengeStage(sceneName) ? "Time Challenge " : "";
                return $"{prefix}World {world}-{stage}";
            }
            
            return sceneName.ToString();
        }
    }
}