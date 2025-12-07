using System;
using System.Collections.Generic;
using Data;

namespace Manager
{
    public partial class UserDataManager
    {

        #region 스테이지 클리어 데이터 관리
        
        /// <summary>
        /// 특정 스테이지가 클리어되었는지 확인합니다.
        /// </summary>
        public bool IsStageClear(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
                return false;
            
            if (userStorage.clearedStages == null)
                userStorage.clearedStages = new HashSet<string>();
            
            return userStorage.clearedStages.Contains(sceneName);
        }
        
        /// <summary>
        /// 특정 스테이지를 클리어 상태로 설정합니다.
        /// </summary>
        public void SetStageClear(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
                return;
            
            if (userStorage.clearedStages == null)
                userStorage.clearedStages = new HashSet<string>();
            
            if (!userStorage.clearedStages.Contains(sceneName))
            {
                userStorage.clearedStages.Add(sceneName);
                Save();
            }
        }
        
        /// <summary>
        /// 특정 스테이지의 클리어 상태를 초기화합니다.
        /// </summary>
        public void ClearStageClear(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
                return;
            
            if (userStorage.clearedStages != null && userStorage.clearedStages.Contains(sceneName))
            {
                userStorage.clearedStages.Remove(sceneName);
                Save();
            }
        }
        
        /// <summary>
        /// 모든 스테이지 클리어 데이터를 초기화합니다.
        /// </summary>
        public void ClearAllStages()
        {
            if (userStorage.clearedStages != null)
            {
                userStorage.clearedStages.Clear();
                Save();
            }
        }
        
        #endregion
        private SceneData InitialSceneData(SceneName _sceneName)
        {
            SceneData sceneData = new SceneData();
            sceneData.sceneName = _sceneName;
            userStorage.sceneData.Add(sceneData);
            Save();
            return sceneData;
        }

        public void SetSceneData(SceneData sceneData)
        {
            SceneData targetSceneData = new SceneData();
            foreach (var data in userStorage.sceneData)
            {
                if (data.sceneName == sceneData.sceneName)
                {
                    targetSceneData = data;
                }
            }
            userStorage.sceneData.Remove(targetSceneData);
            userStorage.sceneData.Add(sceneData);
            Save();
        }
    }
}