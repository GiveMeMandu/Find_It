using System;
using System.Collections.Generic;
using Data;

namespace Manager
{
    public partial class UserDataManager
    {
        public List<SceneData> GetSceneDatas()
        {
            return userStorage.sceneData;
        }
        public SceneData GetSceneData(SceneName _sceneName)
        {
            foreach(var data in userStorage.sceneData) {
                if (data.sceneName == _sceneName)
                {
                    return data;
                }
            }
            //* 찾지 못했을 경우 씬 데이터 새로 생성
            return InitialSceneData(_sceneName);;
        }

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
        public List<SceneName> GetAllScenesName()
        {
            int count = System.Enum.GetValues(typeof(SceneName)).Length;
            List<SceneName> SceneNameList = new List<SceneName>(); 

            for(int i = 0; i < count; i++) {
                SceneNameList.Add((SceneName)i);
            }
            return SceneNameList;
        }
    }
}