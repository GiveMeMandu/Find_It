using System;
using System.Collections.Generic;
using Data;

namespace Manager
{
    public partial class UserDataManager
    {
        public UpgradeData GetUpgradeDataInSceneData(SceneData _sceneData, string _upgradeName)
        {
            foreach(var sceneData in userStorage.sceneData) {
                if(sceneData.sceneName == _sceneData.sceneName)
                {
                    foreach (var upgradeData in sceneData.upgradeData)
                    {
                        if (upgradeData.upgradeName == _upgradeName)
                            return upgradeData;
                    }
                }
            }
            //* 찾지 못했을 경우 씬 데이터 새로 생성
            return InitialUpgradeDataInSceneData(_sceneData, _upgradeName);;
        }
        public UpgradeData GetRecipeUpgradeData(string _upgradeName)
        {
            foreach (var upgradeData in userStorage.recipeUpgradeData)
            {
                if (upgradeData.upgradeName == _upgradeName)
                    return upgradeData;
            }
            //* 찾지 못했을 경우 씬 데이터 새로 생성
            return InitialRecipeData(_upgradeName);;
        }
        public void SetUpgradeData(SceneData _sceneData, UpgradeData _upgradeData)
        {
            _sceneData.upgradeData.Remove(GetUpgradeDataInSceneData(_sceneData, _upgradeData.upgradeName));
            _sceneData.upgradeData.Add(_upgradeData);

            Global.UserDataManager.SetSceneData(_sceneData);
        }
        public void SetRecipeUpgradeData(UpgradeData _upgradeData)
        {
            userStorage.recipeUpgradeData.Remove(GetRecipeUpgradeData(_upgradeData.upgradeName));
            userStorage.recipeUpgradeData.Add(_upgradeData);
            Save();
        }

        private UpgradeData InitialUpgradeDataInSceneData(SceneData _sceneData, string _upgradeName)
        {
            UpgradeData upgradeData = new UpgradeData();
            upgradeData.upgradeName = _upgradeName;
            upgradeData.level = 1;

            _sceneData.upgradeData.Add(upgradeData);
            Global.UserDataManager.SetSceneData(_sceneData);
            Save();
            
            return upgradeData;
        }
        private UpgradeData InitialRecipeData(string _upgradeName)
        {
            UpgradeData upgradeData = new UpgradeData();
            upgradeData.upgradeName = _upgradeName;
            upgradeData.level = 1;

            Global.UserDataManager.userStorage.recipeUpgradeData.Add(upgradeData);
            Save();
            
            return upgradeData;
        }
    }
}