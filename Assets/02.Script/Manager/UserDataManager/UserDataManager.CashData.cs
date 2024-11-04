using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

namespace Manager
{
    public partial class UserDataManager
    {
        public string GetCashDataString()
        {
            //* 돈 데이터 있으면 그냥 리턴하고
            if(userStorage.CashData != "") return userStorage.CashData;
            //* 아니면 새로 생성하고
            string startCash = "0";
            userStorage.CashData = startCash;
            Save();
            return startCash;
        }
        public BigInteger GetCashDataBigInteger()
        {
            return Global.CashManager.GetCashUnitValue(GetCashDataString());
        }

        public void SetCashData(string value)
        {
            userStorage.CashData = value;
            Save();
        }
        public void SetCashData(BigInteger value)
        {
            userStorage.CashData = value.ToString();
            Save();
        }
    }
}