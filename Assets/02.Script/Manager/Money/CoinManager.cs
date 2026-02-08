using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Numerics;
using System;
using Unity.VisualScripting;
using Cysharp.Threading.Tasks;

namespace Manager
{
    public class CoinManager : MoneyManager
    {
        public void Initial()
        {
            EPSManager = new EPSManager();
            Coin = Global.UserDataManager.GetCoinDataBigInteger();
        }
        public event EventHandler<string> OnCoinValueChanged;
        public event EventHandler<BigInteger> OnCoinValueIncreased;
        public event EventHandler<BigInteger> OnCoinValueDecreased;

        public event EventHandler<int> OnCoinValueIncreasedParticle;

        public float customerProfitBonus = 1f;
        //* 저장할 땐 string 형식으로 바꿔서 큰 값을 저장
        private BigInteger _coin;
        public BigInteger Coin
        {
            get { return _coin; }
            private set
            {
                if (value > _coin) OnCoinValueIncreased?.Invoke(this, value);
                else if (value < _coin) OnCoinValueDecreased?.Invoke(this, value);

                _coin = value;
                OnCoinValueChanged?.Invoke(this, GetCoinText());
            }
        }
        private EPSManager EPSManager;

        /// <summary>
        /// 코인 데이터를 UserDataManager에 저장합니다.
        /// 게임 종료 시 한 번만 호출하여 저장합니다.
        /// </summary>
        public void SaveCoinData()
        {
            Global.UserDataManager.SetCoinData(Coin);
            Debug.Log($"[CoinManager] 코인 데이터 저장 완료: {GetCoinText()}");
        }
        public string GetCoinEPS(int seconds = 1) => GetCoinUnitText(EPSManager.GetEPS(seconds));
        public BigInteger GetCoinEPSBigInteger(int seconds = 1) => EPSManager.GetEPS(seconds);
        public void AddCoin(BigInteger amt, bool isEPSAffect = false, int spawnParticleCount = 0)
        {
            Coin += amt;
            if (isEPSAffect) EPSManager.AddToEPS(amt);
            if (spawnParticleCount > 0) OnCoinValueIncreasedParticle?.Invoke(this, spawnParticleCount);
        }

        public void AddCoin(string amt, bool isEPSAffect = false, int spawnParticleCount = 0)
        {
            var value = GetCoinUnitValue(amt);

            Coin += value;
            if (isEPSAffect) EPSManager.AddToEPS(value);
            if (spawnParticleCount > 0) OnCoinValueIncreasedParticle?.Invoke(this, spawnParticleCount);
        }

        public void SubCoin(BigInteger amt)
        {
            Coin -= amt;
        }
        public void SubCoin(string amt)
        {
            Coin -= GetCoinUnitValue(amt);
        }
        public string GetCoinText()
        {
            return GetCoinUnitText(Coin);
        }
        public BigInteger GetCoinValue()
        {
            return Coin;
        }
        public bool CanPurchase(string cost)
        {
            return GetCoinValue() >= GetCoinUnitValue(cost);
        }

        public bool CanPurchase(BigInteger cost)
        {
            return GetCoinValue() >= cost;
        }

        public string GetCoinUnitText(string targetValue)
        {
            return GetUnitText(GetUnitValue(targetValue));
        }
        public bool TryGetCoinUnitValue(string coinUnitText, out BigInteger result)
        {
            result = 0;
            try
            {
                result = GetCoinUnitValue(coinUnitText);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public string GetCoinUnitText(BigInteger targetValue)
        {
            return GetUnitText(targetValue);
        }
        public BigInteger GetCoinUnitValue(string coinUnitText)
        {
            return GetUnitValue(coinUnitText);
        }
    }
}
