#if ((UNITY_ANDROID || UNITY_IOS)  && !UNITY_EDITOR)
using System;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
#endif

namespace GameArter.Ads
{
    [System.Serializable]
    public class AdsConfiguration
    {
        public PlatformAppId platformAppId;
        public AdUnit[] adUnits;

        [System.Serializable]
        public class PlatformAppId
        {
            public string AndroidAppId;
            public string IosAppId;
        }

        [System.Serializable]
        public class AdUnit
        {
            public string id;
            public AdType adType;
            public string AdMobAndroidCh;
            public string AdMobIosCh;
            public uint MaxAdFrequencySeconds;
        }

        public enum AdType
        {
            Fullscreen,
            Banner,
            Rewarded,
            Native
        }
    }

    // adUnitData
    [System.Serializable]
    public class AdUnitData
    {
        public string unitId;
        public bool mutedGame;
        public bool editorMode;
        public string cursorLockState;
        public bool visibleCursor;
        public string channelId;
        public Garter.BannerAction action;
        public AdsConfiguration.AdType adType;
        public bool mPosition; // manual position
        public Garter.BannerPosition position;
        public int[] positionXY;
        public Garter.BannerSize size;
        public int[] sizeXY;
        public bool defined;
        public uint maxAdFrequency;
    }

#if ((UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR)
    public class AdMobAds/* : MonoBehaviour*/
    {
        [System.Serializable]
        private class ActiveBanner{
            public string id;
            public BannerView view;
        }

        private List<ActiveBanner> bannerView = new List<ActiveBanner>() { };

        private InterstitialAd interstitialAd;
        private RewardedAd rewardedAd;

        private AdsConfiguration.AdType activeAdType;
        private string appId = "";

        private Action<string> cachedlCallback = null;

        public void Initialize(string platformAppId)
        {
            appId = platformAppId;
            MobileAds.Initialize(appId);
            // create object and place the script inside
            //DontDestroyOnLoad(transform.gameObject);
        }
        /*
        void Start() // for next scenes? Requires to be tested
        {
            if (!string.IsNullOrEmpty(appId)) MobileAds.Initialize(appId);
        }
        */
        public void BannerAd(AdUnitData adData, Action<string> callback) // size, position, callback
        {
            Debug.Log("[GameArter][BannerUnit] "+adData.unitId+" | "+adData.action.ToString());

            // https://developers.google.com/admob/unity/banner
            activeAdType = AdsConfiguration.AdType.Banner;
            cachedlCallback = callback;

            if (adData.action == Garter.BannerAction.Display)
            {
                // Create a 320x50 banner at the top of the screen.
                if (adData.mPosition)
                {
                    AdSize adSize = new AdSize(adData.sizeXY[0], adData.sizeXY[1]);
                    bannerView.Add(new ActiveBanner()
                    {
                        id = adData.unitId,
                        view = new BannerView(adData.channelId, adSize, adData.positionXY[0], adData.positionXY[1])
                    });
                } else
                {
                    AdSize adSize = AdSize.Banner;
                    switch (adData.size)
                    {
                        case Garter.BannerSize.MediumRectangle: adSize = AdSize.MediumRectangle; break;
                        case Garter.BannerSize.IABBanner: adSize = AdSize.IABBanner; break;
                        case Garter.BannerSize.Leaderboard: adSize = AdSize.Leaderboard; break;
                        case Garter.BannerSize.SmartBanner: adSize = AdSize.SmartBanner; break;
                    }
                    AdPosition adPosition = AdPosition.Bottom;
                    switch (adData.position)
                    {
                        case Garter.BannerPosition.BottomLeft: adPosition = AdPosition.BottomLeft; break;
                        case Garter.BannerPosition.BottomRight: adPosition = AdPosition.BottomRight; break;
                        case Garter.BannerPosition.Center: adPosition = AdPosition.Center; break;
                        case Garter.BannerPosition.Top: adPosition = AdPosition.Top; break;
                        case Garter.BannerPosition.TopLeft: adPosition = AdPosition.TopLeft; break;
                        case Garter.BannerPosition.TopRight: adPosition = AdPosition.TopRight; break;
                    }
                    bannerView.Add(new ActiveBanner()
                    {
                        id = adData.unitId,
                        view = new BannerView(adData.channelId, adSize, adPosition)
                    });
                }
                // Called when an ad request has successfully loaded.
                bannerView[bannerView.Count - 1].view.OnAdLoaded += HandleOnAdLoaded;
                // Called when an ad request failed to load.
                bannerView[bannerView.Count - 1].view.OnAdFailedToLoad += HandleOnAdFailedToLoad;
                // Called when an ad is clicked.
                bannerView[bannerView.Count - 1].view.OnAdOpening += HandleOnAdOpened;
                // Called when the user returned from the app after an ad click.
                bannerView[bannerView.Count - 1].view.OnAdClosed += HandleOnAdClosed;
                // Called when the ad click caused the user to leave the application.
                bannerView[bannerView.Count - 1].view.OnAdLeavingApplication += HandleOnAdLeavingApplication;
                // Create an empty ad request.
                AdRequest request = new AdRequest.Builder().Build();
                bannerView[bannerView.Count - 1].view.LoadAd(request);
            } else {
                // find right banner
                for (int i = 0; i < bannerView.Count; i++)
                {
                    if (bannerView[i].id == adData.unitId) {
                        bannerView[i].view.Hide();
                        break;
                    }
                }
            }
        }

        public void FullscreenAd(string adUnitId, Action<string> callback)
        {
            // https://developers.google.com/admob/unity/interstitial
            activeAdType = AdsConfiguration.AdType.Fullscreen;
            cachedlCallback = callback;

            // Initialize an InterstitialAd.
            this.interstitialAd = new InterstitialAd(adUnitId);
            // Called when an ad request has successfully loaded.
            this.interstitialAd.OnAdLoaded += HandleOnAdLoaded;
            // Called when an ad request failed to load.
            this.interstitialAd.OnAdFailedToLoad += HandleOnAdFailedToLoad;
            // Called when an ad is shown.
            this.interstitialAd.OnAdOpening += HandleOnAdOpened;
            // Called when the ad is closed.
            this.interstitialAd.OnAdClosed += HandleOnAdClosed;
            // Called when the ad click caused the user to leave the application.
            this.interstitialAd.OnAdLeavingApplication += HandleOnAdLeavingApplication;
            // Create an empty ad request.
            AdRequest request = new AdRequest.Builder().Build();
            // Load the interstitial with the request.
            this.interstitialAd.LoadAd(request);
        }

        public void RewardedAds(string adUnitId, Action<string> callback)
        {
            // https://developers.google.com/admob/unity/rewarded-ads
            activeAdType = AdsConfiguration.AdType.Rewarded;
            cachedlCallback = callback;

            this.rewardedAd = new RewardedAd(adUnitId);
            // Called when an ad request has successfully loaded.
            this.rewardedAd.OnAdLoaded += HandleOnAdLoaded;
            // Called when an ad request failed to load.
            this.rewardedAd.OnAdFailedToLoad += HandleRewardedAdFailedToLoad;;
            // Called when an ad is shown.
            this.rewardedAd.OnAdOpening += HandleOnAdOpened;
            // Called when an ad request failed to show.
            this.rewardedAd.OnAdFailedToShow += HandleRewardedAdFailedToShow;
            // Called when the user should be rewarded for interacting with the ad.
            this.rewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
            // Called when the ad is closed.
            this.rewardedAd.OnAdClosed += HandleOnAdClosed;
            // Create an empty ad request.
            AdRequest request = new AdRequest.Builder().Build();
            // Load the rewarded ad with the request.
            this.rewardedAd.LoadAd(request);
        }

        private void DisplayAd()
        {
            switch (activeAdType)
            {
                case AdsConfiguration.AdType.Banner:
                    bannerView[bannerView.Count - 1].view.Show();
                    break;
                case AdsConfiguration.AdType.Fullscreen:
                    if (this.interstitialAd.IsLoaded())
                    {
                        this.interstitialAd.Show();
                    }
                    else
                    {
                        Debug.LogWarning("Fullscreen ad is not loaded");
                    }
                    break;
                case AdsConfiguration.AdType.Rewarded:
                    rewardedAd.Show();
                    break;
                case AdsConfiguration.AdType.Native:
                    Debug.Log("Native ad has not been implemented yet");
                    break;
            }
        }

        private void HandleOnAdLoaded(object sender, EventArgs args)
        {
            if (cachedlCallback != null)
            {
                cachedlCallback("loaded");
            }
            else
            {
                Debug.Log("[GameArter ADS] loaded");
            }
            DisplayAd();
        }

        private void HandleOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
        {
            if (cachedlCallback != null) cachedlCallback("failed");
            Debug.Log("[GameArter ADS] failed (err: "+ args.Message+")");
        }

        private void HandleRewardedAdFailedToLoad(object sender, AdErrorEventArgs args)
        {
            if (cachedlCallback != null) cachedlCallback("failed");
            MonoBehaviour.print(
                "HandleRewardedAdFailedToLoad event received with message: "
                                 + args.Message);
        }

        private void HandleRewardedAdFailedToShow(object sender, AdErrorEventArgs args)
        {
            if (cachedlCallback != null) cachedlCallback("failed");
            MonoBehaviour.print(
                "HandleRewardedAdFailedToShow event received with message: "
                                 + args.Message);
        }

        private void HandleUserEarnedReward(object sender, Reward args)
        {
            if (cachedlCallback != null) {
                cachedlCallback("completed");
            } else
            {
                Debug.Log("Callback listener must be attached for rewarded ads");
            }
            string type = args.Type;
            double amount = args.Amount;
            MonoBehaviour.print(
                "HandleRewardedAdRewarded event received for "
                            + amount.ToString() + " " + type);
        }
        private void HandleOnAdOpened(object sender, EventArgs args)
        {
            if (cachedlCallback != null)
            {
                cachedlCallback("opened");
            }
            else
            {
                Debug.Log("[GameArter ADS] opened");
            }
        }
        private void HandleOnAdClosed(object sender, EventArgs args)
        {

            if (cachedlCallback != null)
            {
                cachedlCallback("completed");
            }
            else
            {
                Debug.Log("[GameArter ADS] closed");
            }
        }
        private void HandleOnAdLeavingApplication(object sender, EventArgs args)
        {
            if (cachedlCallback != null)
            {
                cachedlCallback("leavingApplication");
            }
            else
            {
                Debug.Log("[GameArter ADS] leavingApplication");
            }
        }
    }
#endif
}
