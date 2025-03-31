using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

namespace Manager
{
    public enum MoneyType
    {
        [LabelText("골드")]
        Gold,
        [LabelText("다이아")]
        Cash,
        [LabelText("스핀티켓")]
        SpinTicket
    }

    public abstract class MoneyManager
    {
        protected string GetUnitText(string targetValue, BigInteger value)
        {
            return GetUnitText(GetUnitValue(targetValue));
        }

        protected string GetUnitText(BigInteger targetValue)
        {
            int placeN = 3;
            BigInteger value = targetValue;
            List<int> numList = new List<int>();

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

        protected string GetUnitText(int index)
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

        protected BigInteger GetUnitValue(string unitText)
        {
            if (string.IsNullOrEmpty(unitText))
                return BigInteger.Zero;

            int index = 0;
            while (index < unitText.Length && (char.IsDigit(unitText[index]) || unitText[index] == '.'))
            {
                index++;
            }

            string numberPart = unitText.Substring(0, index);
            string unitPart = index < unitText.Length ? unitText.Substring(index) : "";

            if (!decimal.TryParse(numberPart, out decimal number))
            {
                if (BigInteger.TryParse(numberPart, out BigInteger bigNumber))
                {
                    int calculatedUnitValue = CalculateUnitPlaceValue(unitPart);
                    return bigNumber * BigInteger.Pow(10, calculatedUnitValue);
                }
                throw new FormatException($"Invalid number format in {unitText}.");
            }

            int unitPlaceValue = CalculateUnitPlaceValue(unitPart);
            
            BigInteger result = new BigInteger(number * (decimal)Math.Pow(10, unitPlaceValue));
            return result;
        }

        private int CalculateUnitPlaceValue(string unitText)
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

            return value * 3;
        }
    }
}