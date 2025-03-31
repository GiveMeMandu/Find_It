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

        public event EventHandler<int> OnGoldValueIncreasedParticle;

        public float customerProfitBonus = 1f;
        //* 저장할 땐 string 형식으로 바꿔서 큰 값을 저장
        private BigInteger _gold;
        public BigInteger Gold
        {
            get { return _gold; }
            private set
            {
                if (value > _gold) OnGoldValueIncreased?.Invoke(this, value);
                else if (value < _gold) OnGoldValueDecreased?.Invoke(this, value);

                _gold = value;
                OnGoldValueChanged?.Invoke(this, GetGoldText());
                Global.UserDataManager.SetGoldData(Gold);
            }
        }
        private EPSManager EPSManager;
        public string GetGoldEPS(int seconds = 1) => GetGoldUnitText(EPSManager.GetEPS(seconds));
        public BigInteger GetGoldEPSBigInteger(int seconds = 1) => EPSManager.GetEPS(seconds);
        public void AddGold(BigInteger amt, bool isEPSAffect = false, int spawnParticleCount = 0)
        {
            Gold += amt;
            if (isEPSAffect) EPSManager.AddToEPS(amt);
            if (spawnParticleCount > 0) OnGoldValueIncreasedParticle?.Invoke(this, spawnParticleCount);
        }

        public void AddGold(string amt, bool isEPSAffect = false, int spawnParticleCount = 0)
        {
            var value = GetGoldUnitValue(amt);

            Gold += value;
            if (isEPSAffect) EPSManager.AddToEPS(value);
            if (spawnParticleCount > 0) OnGoldValueIncreasedParticle?.Invoke(this, spawnParticleCount);
        }

        public void SubGold(BigInteger amt)
        {
            Gold -= amt;
        }
        public void SubGold(string amt)
        {
            Gold -= GetGoldUnitValue(amt);
        }
        public string GetGoldText()
        {
            return GetGoldUnitText(Gold);
        }
        public BigInteger GetGoldValue()
        {
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
            return GetUnitText(GetUnitValue(targetValue));
        }
        public bool TryGetGoldUnitValue(string goldUnitText, out BigInteger result)
        {
            result = 0;
            try
            {
                result = GetGoldUnitValue(goldUnitText);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public string GetGoldUnitText(BigInteger targetValue)
        {
            return GetUnitText(targetValue);
        }
        public BigInteger GetGoldUnitValue(string goldUnitText)
        {
            return GetUnitValue(goldUnitText);
        }
    }
}