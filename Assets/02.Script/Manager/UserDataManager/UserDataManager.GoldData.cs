using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

namespace Manager
{
    public partial class UserDataManager
    {
        public string GetGoldDataString()
        {
            //* 돈 데이터 있으면 그냥 리턴하고
            if(userStorage.GoldData != "") return userStorage.GoldData;
            //* 아니면 새로 생성하고
            string startGold = "0";
            userStorage.GoldData = startGold;
            Save();
            return startGold;
        }
        public BigInteger GetGoldDataBigInteger()
        {
            return Global.GoldManager.GetGoldUnitValue(GetGoldDataString());
        }

        public void SetGoldData(string value)
        {
            userStorage.GoldData = value;
            Save();
        }
        public void SetGoldData(BigInteger value)
        {
            userStorage.GoldData = value.ToString();
            Save();
        }
    }
}