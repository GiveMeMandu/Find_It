using System;
using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using UnityEngine;

namespace Manager
{
    public class GoogleMobileAdsManager : MonoBehaviour
    {
        public event EventHandler OnAdShowed;
        public event EventHandler OnAdClosed;
        public event EventHandler OnAdEnd; //* 광고의 마지막 처리
        private RewardedAd _rewardedAd;

        //* 광고 만료 시간
        private DateTime _expireTime;
        private readonly TimeSpan TIMEOUT = TimeSpan.FromHours(4);

        // 광고 제거 관련 상수
        private const string ADS_REMOVED_KEY = "AdsRemoved";

        private bool _isLoadingAd = false;
        private bool _isShowingAd = false;
        private Action _onRewardedCallback = null;

        public void Start()
        {
            // Initialize the Google Mobile Ads SDK.
            MobileAds.Initialize((InitializationStatus initStatus) =>
            {
                // 광고가 제거되지 않은 경우에만 광고 로드
                if (!IsAdRemoved())
                {
                    LoadRewardedAd();
                }
                else
                {
                    Debug.Log("광고가 제거되어 광고를 로드하지 않습니다.");
                }
            });
        }

        #region 광고 제거 기능
        /// <summary>
        /// 광고 제거 상태를 확인합니다.
        /// </summary>
        /// <returns>광고가 제거되었으면 true, 아니면 false</returns>
        public bool IsAdRemoved()
        {
            // UserDataManager에서 광고 제거 상태 확인
            if (Global.UserDataManager != null && Global.UserDataManager.userStorage != null)
            {
                return Global.UserDataManager.userStorage.AdsRemoved;
            }
            return false;
        }

        /// <summary>
        /// 광고를 제거합니다. IAP 구매 성공 시 호출됩니다.
        /// </summary>
        public void RemoveAds()
        {
            // UserDataManager에 광고 제거 상태 저장
            if (Global.UserDataManager != null && Global.UserDataManager.userStorage != null)
            {
                Global.UserDataManager.userStorage.AdsRemoved = true;
                Global.UserDataManager.Save();
                
                // 현재 로드된 광고 제거
                if (_rewardedAd != null)
                {
                    _rewardedAd.Destroy();
                    _rewardedAd = null;
                }
                
                Debug.Log("광고가 영구적으로 제거되었습니다.");
            }
            else
            {
                Debug.LogError("UserDataManager가 초기화되지 않아 광고 제거 상태를 저장할 수 없습니다.");
            }
        }

        /// <summary>
        /// 광고 제거 상태를 복원합니다. 앱 재설치 후 구매 복원 시 호출됩니다.
        /// </summary>
        public void RestoreAdRemoval()
        {
            // 이미 광고가 제거된 상태라면 아무 작업도 하지 않음
            if (IsAdRemoved()) return;
            
            // 여기서는 IAP 복원 로직이 별도로 처리된다고 가정하고,
            // 복원 성공 시 이 메서드를 호출하여 상태만 업데이트
            RemoveAds();
        }
        #endregion

        #region 보상형 광고
        public void ShowRewardedAd(Action onRewarded = null)
        {
            // 광고가 제거된 경우 바로 보상 지급
            if (IsAdRemoved())
            {
                Debug.Log("광고가 제거되어 보상을 바로 지급합니다.");
                onRewarded?.Invoke();
                return;
            }

            if (_isShowingAd)
            {
                Debug.Log("광고가 이미 재생 중입니다.");
                return;
            }

            if (_isLoadingAd)
            {
                Debug.Log("광고를 로딩 중입니다. 잠시만 기다려주세요.");
                return;
            }

            _onRewardedCallback = onRewarded;

            if (_rewardedAd != null && _rewardedAd.CanShowAd() && DateTime.Now < _expireTime)
            {
                _isShowingAd = true;
                _rewardedAd.Show((Reward reward) =>
                {
                    Debug.Log($"광고 보상 지급: {reward.Type}, {reward.Amount}");
                    _onRewardedCallback?.Invoke();
                    _onRewardedCallback = null;
                    _isShowingAd = false;
                });
            }
            else
            {
                Debug.Log("새로운 광고를 로딩합니다.");
                LoadRewardedAd();
            }
        }

        // These ad units are configured to always serve test ads.
#if UNITY_ANDROID
        private string _adUnitId = "ca-app-pub-3940256099942544/5224354917";
#elif UNITY_IPHONE
  private string _adUnitId = "ca-app-pub-3940256099942544/1712485313";
#else
  private string _adUnitId = "unused";
#endif

        /// <summary>
        /// Loads the rewarded ad.
        /// </summary>
        public void LoadRewardedAd()
        {
            if (_isLoadingAd) return;
            
            _isLoadingAd = true;
            if (_rewardedAd != null)
            {
                _rewardedAd.Destroy();
                _rewardedAd = null;
            }

            Debug.Log("광고 로딩 시작");
            var adRequest = new AdRequest();

            RewardedAd.Load(_adUnitId, adRequest,
                (RewardedAd ad, LoadAdError error) =>
                {
                    _isLoadingAd = false;
                    
                    if (error != null || ad == null)
                    {
                        Debug.LogError($"광고 로딩 실패: {error}");
                        return;
                    }

                    Debug.Log($"광고 로딩 성공: {ad.GetResponseInfo()}");
                    _expireTime = DateTime.Now + TIMEOUT;
                    _rewardedAd = ad;
                    RegisterEventHandlers(_rewardedAd);
                });
        }

        private void RegisterEventHandlers(RewardedAd ad)
        {
            // Raised when the ad is estimated to have earned money.
            ad.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(System.String.Format("Rewarded ad paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
            };
            // Raised when an impression is recorded for an ad.
            ad.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Rewarded ad recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            ad.OnAdClicked += () =>
            {
                Debug.Log("Rewarded ad was clicked.");
            };
            // Raised when an ad opened full screen content.
            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("광고 화면이 열렸습니다.");
                _isShowingAd = true;
                OnAdShowed?.Invoke(this, EventArgs.Empty);
            };
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("광고 화면이 닫혔습니다.");
                _isShowingAd = false;
                LoadRewardedAd(); // 다음 광고 미리 로딩
                OnAdClosed?.Invoke(this, EventArgs.Empty);
            };
            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError($"광고 표시 실패: {error}");
                _isShowingAd = false;
                LoadRewardedAd(); // 실패시 다시 로딩
            };
        }
        #endregion

        #region 전면 광고
        #endregion

        #region 배너 광고
        #endregion
        
        private void ServerSSVCheck()
        {
            // create our request used to load the ad.
            var adRequest = new AdRequest();
            // send the request to load the ad.
            RewardedAd.Load(_adUnitId, adRequest, (RewardedAd ad, LoadAdError error) =>
            {
                // If the operation failed, an error is returned.
                if (error != null || ad == null)
                {
                    Debug.LogError("Rewarded ad failed to load an ad with error : " + error);
                    return;
                }

                // If the operation completed successfully, no error is returned.
                Debug.Log("Rewarded ad loaded with response : " + ad.GetResponseInfo());

                // Create and pass the SSV options to the rewarded ad.
                var options = new ServerSideVerificationOptions();
                options.UserId = "SAMPLE_CUSTOM_DATA_STRING";
                options.CustomData = "SAMPLE_CUSTOM_DATA_STRING";
            
                ad.SetServerSideVerificationOptions(options);

            });
        }
    }
}