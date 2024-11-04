using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Numerics;
using System;
using Unity.VisualScripting;
using Cysharp.Threading.Tasks;

namespace Manager
{
    public class GoldManager : MoneyManager
    {
        public void Initial()
        {
            EPSManager = new EPSManager();
            Gold = Global.UserDataManager.GetGoldDataBigInteger();
        }
        public event EventHandler<string> OnGoldValueChanged;
        public event EventHandler<BigInteger> OnGoldValueIncreased;
        public event EventHandler<BigInteger> OnGoldValueDecreased;
        //* 저장할 땐 string 형식으로 바꿔서 큰 값을 저장
        private BigInteger _gold;
        public BigInteger Gold
        {
            get{return _gold;}
            private set{
                if (value > _gold) OnGoldValueIncreased?.Invoke(this, value);
                else if (value < _gold) OnGoldValueDecreased?.Invoke(this, value);

                _gold = value;
                OnGoldValueChanged?.Invoke(this, GetGoldText());
                Global.UserDataManager.SetGoldData(Gold);
            }
        }
        private EPSManager EPSManager;
        public void AddGold(BigInteger amt, bool isEPSAffect = false)
        {
            Gold += amt;
            if(isEPSAffect) EPSManager.AddToEPS(amt);
        }
        public void AddGold(string amt, bool isEPSAffect = false)
        {
            var value = GetGoldUnitValue(amt);
            Gold += value;
            if(isEPSAffect) EPSManager.AddToEPS(value);
        }
        public void SubGold(BigInteger amt)
        {
            Gold -= amt;
        }
        public void SubGold(string amt)
        {
            Gold -= GetGoldUnitValue(amt);
        }
        public string GetGoldEPS(int seconds = 1) => GetGoldUnitText(EPSManager.GetEPS(seconds));
        public string GetGoldText()
        {
            return GetGoldUnitText(Gold);
        }
        public BigInteger GetGoldValue(){
            return Gold;
        }
        public bool CanPurchase(string cost)
        {
            return GetGoldValue() >= GetGoldUnitValue(cost);
        }
        
        public bool CanPurchase(BigInteger cost)
        {
            return GetGoldValue() >= cost;
        }
        
        public string GetGoldUnitText(string targetValue)
        {
            return GetGoldUnitText(GetGoldUnitValue(targetValue));
        }
        public string GetGoldUnitText(BigInteger targetValue)
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

            if(GetUnitText(numList.Count - 1) == "")
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
        public BigInteger GetGoldUnitValue(string goldUnitText)
        {
            if (string.IsNullOrEmpty(goldUnitText))
                return BigInteger.Zero;

            // 문자열에서 숫자와 소수점 부분 찾기
            int index = 0;
            while (index < goldUnitText.Length && (char.IsDigit(goldUnitText[index]) || goldUnitText[index] == '.'))
            {
                index++;
            }

            // 숫자 부분과 단위 부분을 분리
            string numberPart = goldUnitText.Substring(0, index);
            string unitPart = index < goldUnitText.Length ? goldUnitText.Substring(index) : "";

            // 숫자 부분을 float로 변환
            if (!float.TryParse(numberPart, out float number))
            {
                throw new FormatException("Invalid number format in goldUnitText.");
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