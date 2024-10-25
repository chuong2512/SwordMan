using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SocialPlatforms;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

/*
 *
 * Document for Unity Ads : https://docs.unity.com/ads/ImplementingBasicAdsUnity.html
 */
/*
 *
 * Document for Google Admob : https://developers.google.com/admob/unity/quick-start
 */
namespace MoreMountains.CorgiEngine
{
    public class AdsControl : MonoBehaviour
    {
        private static AdsControl instance;

        //for Admob

        public string Android_AppID, IOS_AppID;

        public string Android_Banner_Key, IOS_Banner_Key;

        public string Android_Interestital_Key, IOS_Interestital_Key;

        public string Android_RW_Key, IOS_RW_Key;

        private bool isShowingAppOpenAd;

        [HideInInspector] public UnityEvent OnAdLoadedEvent;
        [HideInInspector] public UnityEvent OnAdFailedToLoadEvent;
        [HideInInspector] public UnityEvent OnAdOpeningEvent;
        [HideInInspector] public UnityEvent OnAdFailedToShowEvent;
        [HideInInspector] public UnityEvent OnUserEarnedRewardEvent;
        [HideInInspector] public UnityEvent OnAdClosedEvent;


        [HideInInspector] public int adCurrent;


        public static AdsControl Instance
        {
            get { return instance; }
        }

        void Awake()
        {
            if (FindObjectsOfType(typeof(AdsControl)).Length > 1)
            {
                Destroy(gameObject);
                return;
            }


            instance = this;
            DontDestroyOnLoad(gameObject);

            AdsControl.instance.ShowInterstitalRandom();
        }


        private void Start()
        {
        }


        #region BANNER ADS

        public void RequestBannerAd()
        {
            //PrintStatus("Requesting Banner ad.");

            // These ad units are configured to always serve test ads.
#if UNITY_EDITOR
            string adUnitId = "unused";

#elif UNITY_ANDROID
        string adUnitId = Android_Banner_Key;
#elif UNITY_IPHONE
        string adUnitId = IOS_Banner_Key;
#else
        string adUnitId = "unexpected_platform";
#endif
        }

        public void DestroyBannerAd()
        {
        }

        public void ShowBannerAd()
        {
        }

        public void HideBannerAd()
        {
        }

        #endregion

        #region INTERSTITIAL ADS

        public void RequestAndLoadInterstitialAd()
        {
        }

        public void ShowInterstitialAd()
        {
        }

        public void ShowInterstitalRandom()
        {
            StartCoroutine(ShowInterstitalRandomIE());
        }

        IEnumerator ShowInterstitalRandomIE()
        {
            yield return new WaitForSeconds(0.5f);

            if (adCurrent >= 1)
            {
                ShowInterstitialAd();
                adCurrent = 0;
            }
            else
                adCurrent++;
        }

        public void DestroyInterstitialAd()
        {
        }

        #endregion

        #region REWARDED ADS

        public void RequestAndLoadRewardedAd()
        {
        }

        public void ShowRewardedAd()
        {
        }

        public bool IsRWAvailable()
        {
            bool available = false;

            return available;
        }

        public void RequestAndLoadRewardedInterstitialAd()
        {
        }

        public void ShowRewardedInterstitialAd()
        {
        }

        #endregion

        public void ShowInterstitalMediation()
        {
            int numberShow = PlayerPrefs.GetInt("ShowAds");

            if (numberShow < 1)
            {
                numberShow++;
                PlayerPrefs.SetInt("ShowAds", numberShow);
                return;
            }
            else
            {
                numberShow = 0;
                PlayerPrefs.SetInt("ShowAds", numberShow);
            }
        }
    }
}