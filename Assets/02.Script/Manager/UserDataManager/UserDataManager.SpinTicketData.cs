using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

namespace Manager
{
    public partial class UserDataManager
    {
        public string GetSpinTicketDataString()
        {
            //* 돈 데이터 있으면 그냥 리턴하고
            if(userStorage.SpinTicketData != "") return userStorage.SpinTicketData;
            //* 아니면 새로 생성하고
            string startSpinTicket = "0";
            userStorage.SpinTicketData = startSpinTicket;
            Save();
            return startSpinTicket;
        }
        public BigInteger GetSpinTicketDataBigInteger()
        {
            return Global.SpinTicketManager.GetSpinTicketUnitValue(GetSpinTicketDataString());
        }

        public void SetSpinTicketData(string value)
        {
            userStorage.SpinTicketData = value;
            Save();
        }
        public void SetSpinTicketData(BigInteger value)
        {
            userStorage.SpinTicketData = value.ToString();
            Save();
        }
    }
}