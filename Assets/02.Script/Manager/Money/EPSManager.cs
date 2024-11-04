using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Manager
{
    public class EPSManager
    {
        public EPSManager()
        {
            if(storeEPS == 0) ResetEPS();
            Global.Instance.OnApplicationPauseEvt += OnApplicationPauseEvt;
            CalculateEPS().Forget();
        }

        private void OnApplicationPauseEvt(object sender, EventArgs e)
        {
            SaveEPS();
        }

        private void SaveEPS()
        {
            if(storeEPS == 0) ResetEPS();
            else if (storeEPS != 0)
            {
                Global.UserDataManager.userStorage.EPS = storeEPS.ToString();
                Global.UserDataManager.Save();
            }
        }

        private float avgUnit = 10;
        private BigInteger EPS = 0;
        private BigInteger storeEPS = Global.GoldManager.GetGoldUnitValue(Global.UserDataManager.userStorage.EPS);

        // EPS 값을 설정하는 메서드
        public void AddToEPS(BigInteger amt)
        {
            EPS += amt;
        }
        // EPS 값을 가져오는 메서드
        public BigInteger GetEPS(int seconds = 1)
        {
            return storeEPS * seconds;
        }

        //* EPS 초기화 예) 스테이지 변경시
        public void ResetEPS()
        {
            // var recipeList = Global.GetRecipeStatsUpgradeList(Global.UserDataManager.userStorage.curScene);
            // var expensive = recipeList[0].GetUpgradeValueConvert();
            // foreach(var r in recipeList) {
            //     if(expensive < r.GetUpgradeValueConvert())
            //         expensive = r.GetUpgradeValueConvert();
            // }
            // storeEPS = (int)expensive;
            //* 임시
            storeEPS = 0;
            SaveEPS();
        }


        //* EPS : Earning Per Second
        private async UniTaskVoid CalculateEPS()
        {
            try
            {
                while (true)
                {
                    await UniTask.WaitUntil(() => EPS != 0);
                    if (EPS > storeEPS) storeEPS = EPS;
                    await UniTask.Delay(TimeSpan.FromSeconds(1)); // 1초 대기
                    // 매 1초마다 수익률 EPS 초기화
                    EPS = 0;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"An error occurred in CalculateEPS: {ex.Message}");
                // 예외 처리 후 필요한 로직 실행 (ex: 데이터 저장, 복구 시도 등)
            }
        }
    }

}