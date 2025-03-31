using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Numerics;
using System;
using Unity.VisualScripting;

namespace Manager
{
    public class SpinTicketManager : MoneyManager
    {
        public void Initial()
        {
            SpinTicket = Global.UserDataManager.GetSpinTicketDataBigInteger();
        }
        public event EventHandler<string> OnSpinTicketValueChanged;
        public event EventHandler<int> OnSpinTicketValueIncreasedParticle;
        private BigInteger _spinTicket;
        public BigInteger SpinTicket

        {
            get { return _spinTicket; }
            private set
            {
                _spinTicket = value;
                OnSpinTicketValueChanged?.Invoke(this, GetSpinTicketText());
            }
        }

        public void AddSpinTicket(BigInteger amt, int spawnParticleCount = 0)
        {
            SpinTicket += amt;
            Global.UserDataManager.SetSpinTicketData(SpinTicket);
            if (spawnParticleCount > 0) OnSpinTicketValueIncreasedParticle?.Invoke(this, spawnParticleCount);
        }
        
        public void AddSpinTicket(string amt)
        {
            SpinTicket += GetSpinTicketUnitValue(amt);
            Global.UserDataManager.SetSpinTicketData(SpinTicket);
        }
        
        public void SubSpinTicket(BigInteger amt)
        {
            SpinTicket -= amt;
            Global.UserDataManager.SetSpinTicketData(SpinTicket);
        }
        
        public void SubSpinTicket(string amt)
        {
            SpinTicket -= GetSpinTicketUnitValue(amt);
            Global.UserDataManager.SetSpinTicketData(SpinTicket);
        }
        
        public string GetSpinTicketText()
        {
            return GetSpinTicketUnitText(SpinTicket);
        }
        
        public BigInteger GetSpinTicketValue()
        {
            return SpinTicket;
        }
        
        public bool CanPurchase(string cost)
        {
            return GetSpinTicketValue() >= GetSpinTicketUnitValue(cost);
        }

        public bool CanPurchase(BigInteger cost)
        {
            return GetSpinTicketValue() >= cost;
        }

        public string GetSpinTicketUnitText(string targetValue)
        {
            return GetUnitText(targetValue, GetSpinTicketUnitValue(targetValue));
        }
        
        public string GetSpinTicketUnitText(BigInteger targetValue)
        {
            return GetUnitText(targetValue);
        }
        
        public BigInteger GetSpinTicketUnitValue(string spinTicketUnitText)
        {
            return GetUnitValue(spinTicketUnitText);
        }
    }
}