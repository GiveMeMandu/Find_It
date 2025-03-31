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
        public event EventHandler<int> OnCashValueIncreasedParticle;
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

        public void AddCash(BigInteger amt, int spawnParticleCount = 0)
        {
            try
            {
                Cash += amt;
                Global.UserDataManager.SetCashData(Cash);
                Debug.Log($"캐시 추가됨: {amt}, 현재 캐시: {Cash}");
                if (spawnParticleCount > 0) OnCashValueIncreasedParticle?.Invoke(this, spawnParticleCount);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"캐시 추가 중 오류 발생: {e.Message}");
            }
        }
        public void AddCash(string amt)
        {
            try
            {
                BigInteger amtValue = GetCashUnitValue(amt);
                Cash += amtValue;
                Global.UserDataManager.SetCashData(Cash);
                Debug.Log($"캐시 추가됨(문자열): {amt}, 현재 캐시: {Cash}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"캐시 추가 중 오류 발생: {e.Message}");
            }
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
            return GetUnitText(targetValue, GetCashUnitValue(targetValue));
        }

        public string GetCashUnitText(BigInteger targetValue)
        {
            return GetUnitText(targetValue);
        }

        public BigInteger GetCashUnitValue(string cashUnitText)
        {
            return GetUnitValue(cashUnitText);
        }
    }
}