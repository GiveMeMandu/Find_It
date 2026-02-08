using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

namespace Manager
{
    public partial class UserDataManager
    {
        public string GetCoinDataString()
        {
            //* 돈 데이터 있으면 그냥 리턴하고
            if(userStorage.CoinData != "") return userStorage.CoinData;
            //* 아니면 새로 생성하고
            string startGold = "0";
            userStorage.CoinData = startGold;
            Save();
            return startGold;
        }
        public BigInteger GetCoinDataBigInteger()
        {
            return Global.CoinManager.GetCoinUnitValue(GetCoinDataString());
        }

        public void SetCoinData(string value)
        {
            userStorage.CoinData = value;
            Save();
        }
        public void SetCoinData(BigInteger value)
        {
            userStorage.CoinData = value.ToString();
            Save();
        }
    }
}