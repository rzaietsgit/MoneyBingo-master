using System;
using System.Collections;
using System.Collections.Generic;
using ITSoft;
using UnityEngine;


namespace AdsITSoft {
    public class AdsManager : MonoBehaviour
    {
        private static System.Action OnCompleteRewardVideo;
        private static System.Action OnCompleteInterVideo;

        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }

        private void Start()
        {
            InitIronSDK();
            LoadInterstitial();
        }

        private void OnEnable()
        {
            IronSourceEvents.onRewardedVideoAdRewardedEvent += RewardedVideoAdRewardedEvent;
            IronSourceEvents.onRewardedVideoAdShowFailedEvent += ErrorShowingReward;
            IronSourceEvents.onInterstitialAdClosedEvent += LoadInterstitial;
            IronSourceEvents.onInterstitialAdClosedEvent += InterVideoAdRewardedEvent;
            IronSourceEvents.onInterstitialAdLoadFailedEvent += LoadInterstitial;
        }

        private void OnDisable()
        {
            IronSourceEvents.onRewardedVideoAdRewardedEvent -= RewardedVideoAdRewardedEvent;
            IronSourceEvents.onRewardedVideoAdShowFailedEvent -= ErrorShowingReward;
            IronSourceEvents.onInterstitialAdClosedEvent -= LoadInterstitial;
            IronSourceEvents.onInterstitialAdClosedEvent -= InterVideoAdRewardedEvent;
            IronSourceEvents.onInterstitialAdLoadFailedEvent -= LoadInterstitial;
        }

        private void InitIronSDK()
        {
#if UNITY_ANDROID
            // string appKey = "85460dcd";
            string appKey = "107c64b21";
#elif UNITY_IPHONE
        string appKey = "8545d445";
#else
        string appKey = "unexpected_platform";
#endif
            Debug.Log("unity-script: IronSource.Agent.validateIntegration");
            IronSource.Agent.validateIntegration();

            Debug.Log("unity-script: unity version" + IronSource.unityVersion());

            // SDK init
            Debug.Log($"unity-script: IronSource.Agent.init with app key - {appKey}");
            IronSource.Agent.init(appKey);
        }

        void RewardedVideoAdRewardedEvent(IronSourcePlacement ssp)
        {
            Debug.Log("unity-script: I got RewardedVideoAdRewardedEvent, amount = " + ssp.getRewardAmount() + " name = " + ssp.getRewardName());
            OnCompleteRewardVideo?.Invoke();
            OnCompleteRewardVideo = null;
        }

        void InterVideoAdRewardedEvent()
        {
            OnCompleteInterVideo?.Invoke();
            OnCompleteInterVideo = null;
        }

        public static void ShowRewarded(System.Action ViewComplete = null)
        {
            bool rewardIsReady = IronSource.Agent.isRewardedVideoAvailable();
            Debug.Log("Show reward video. Reward video is" + (rewardIsReady ? "" : " not") + " ready");
            if (rewardIsReady)
            {
                OnCompleteRewardVideo = ViewComplete;
                IronSource.Agent.showRewardedVideo();
            }
            else
            {
                Debug.Log("unity-script: IronSource.Agent.isRewardedVideoAvailable - False");
                #if UNITY_EDITOR
                ViewComplete?.Invoke();
                #endif
            }
        }

        private void ErrorShowingReward(IronSourceError error)
        {
            Debug.Log("Error to show reward! " + error.ToString());
        }
        
        public static void LoadInterstitial()
        {
            // if (BizzyBeeGames.IAPManager.Instance.IsProductPurchased("removeads"))
            //     return;
            Debug.Log("unity-script: IronSource.Agent.loadInterstitial - True");
            IronSource.Agent.loadInterstitial();
        }    
        
        public static void LoadInterstitial(IronSourceError error)
        {
            Debug.Log("unity-script: IronSource.Agent.loadInterstitial - " + error.getDescription());
            IronSource.Agent.loadInterstitial();
        }

        public static void ShowInterstitial(System.Action ViewComplete = null)
        {
            // if (BizzyBeeGames.IAPManager.Instance.IsProductPurchased("removeads"))
            //     return;
            bool adsIsReady = IronSource.Agent.isInterstitialReady();
            if (adsIsReady)
            {
                Debug.Log("unity-script: IronSource.Agent.isInterstitialReady - True");
                IronSource.Agent.showInterstitial();
                OnCompleteInterVideo = ViewComplete;
            }
            else
            {
                Debug.Log("unity-script: IronSource.Agent.isInterstitialReady - False");
                LoadInterstitial();
                ViewComplete?.Invoke();
            }
        }

        public static void ShowInterstitial(string placementName)
        {
            // if (BizzyBeeGames.IAPManager.Instance.IsProductPurchased("removeads"))
            //     return;

            ShowInterstitial();
            return;

            IronSource.Agent.showInterstitial(placementName);
        }

        public static void ShowBanner(IronSourceBannerPosition bannerPosition = IronSourceBannerPosition.BOTTOM)
        {
            // if (BizzyBeeGames.IAPManager.Instance.IsProductPurchased("removeads"))
            //     return;
            IronSource.Agent.loadBanner(IronSourceBannerSize.BANNER, IronSourceBannerPosition.BOTTOM);
        }

        public static void HideBanner()
        {
            IronSource.Agent.destroyBanner();
        }
    }
}
