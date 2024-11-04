using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Numerics;
using System;
using Unity.VisualScripting;

namespace Manager
{
    public class CashManager : MoneyManager
    {
        public void Initial()
        {
            Cash = Global.UserDataManager.GetCashDataBigInteger();
        }
        public event EventHandler<string> OnCashValueChanged;
        //* 저장할 땐 string 형식으로 바꿔서 큰 값을 저장
        private BigInteger _cash;
        public BigInteger Cash
        {
            get { return _cash; }
            private set
            {
                _cash = value;
                OnCashValueChanged?.Invoke(this, GetCashText());
            }
        }

        public void AddCash(BigInteger amt)
        {
            Cash += amt;
            Global.UserDataManager.SetCashData(Cash);
        }
        public void AddCash(string amt)
        {
            Cash += GetCashUnitValue(amt);
            Global.UserDataManager.SetCashData(Cash);
        }
        public void SubCash(BigInteger amt)
        {
            Cash -= amt;
            Global.UserDataManager.SetCashData(Cash);
        }
        public void SubCash(string amt)
        {
            Cash -= GetCashUnitValue(amt);
            Global.UserDataManager.SetCashData(Cash);
        }
        public string GetCashText()
        {
            return GetCashUnitText(Cash);
        }
        public BigInteger GetCashValue()
        {
            return Cash;
        }
        public bool CanPurchase(string cost)
        {
            return GetCashValue() >= GetCashUnitValue(cost);
        }

        public bool CanPurchase(BigInteger cost)
        {
            return GetCashValue() >= cost;
        }

        public string GetCashUnitText(string targetValue)
        {
            return GetCashUnitText(GetCashUnitValue(targetValue));
        }
        public string GetCashUnitText(BigInteger targetValue)
        {
            //* 자리수 자를 단위
            int placeN = 3;
            BigInteger value = targetValue;
            List<int> numList = new List<int>();

            //* 10^placeN 승 
            int p = (int)Mathf.Pow(10, placeN);

            do
            {
                numList.Add((int)(value % p));
                value /= p;
            }
            while (value >= 1);

            string retstr = "";
            int num = numList.Count < 2 ? numList[0] : numList[numList.Count - 1] * p + numList[numList.Count - 2];
            float f = (num / (float)p);

            if (GetUnitText(numList.Count - 1) == "")
                retstr = num.ToString() + GetUnitText(numList.Count - 1);
            else
                retstr = f.ToString("N2") + GetUnitText(numList.Count - 1);
            return retstr;
        }
        //* 자릿수 표기 얻기
        private string GetUnitText(int index)
        {
            int idx = index - 1;
            if (idx < 0) return "";
            int repeatCount = (idx / 26) + 1;
            string retStr = "";
            for (int i = 0; i < repeatCount; i++)
            {
                retStr += (char)(97 + idx % 26);
            }
            return retStr;
        }
        public BigInteger GetCashUnitValue(string cashUnitText)
        {
            if (string.IsNullOrEmpty(cashUnitText))
                return BigInteger.Zero;

            // 문자열에서 숫자와 소수점 부분 찾기
            int index = 0;
            while (index < cashUnitText.Length && (char.IsDigit(cashUnitText[index]) || cashUnitText[index] == '.'))
            {
                index++;
            }

            // 숫자 부분과 단위 부분을 분리
            string numberPart = cashUnitText.Substring(0, index);
            string unitPart = index < cashUnitText.Length ? cashUnitText.Substring(index) : "";

            // 숫자 부분을 float로 변환
            if (!float.TryParse(numberPart, out float number))
            {
                throw new FormatException("Invalid number format in cashUnitText.");
            }

            // 단위 부분을 자리수로 변환
            int unitValue = GetUnitValue(unitPart);

            // 최종 BigInteger 값 계산
            BigInteger result = new BigInteger(number * (float)Math.Pow(10, unitValue));
            return result;
        }

        private int GetUnitValue(string unitText)
        {
            if (string.IsNullOrEmpty(unitText))
                return 0;

            int value = 0;
            int length = unitText.Length;

            for (int i = 0; i < length; i++)
            {
                int charValue = unitText[i] - 'a' + 1;
                value += charValue * (int)Math.Pow(26, length - i - 1);
            }

            return value * 3; // 10^3의 배수이므로 3을 곱해줌
        }
    }
}