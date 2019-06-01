using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdRequestsExampleScenes : MonoBehaviour
{
    public void Docs()
    {

    }

    public void RequestFullscreenAd()
    {
        Garter.I.RequestAd("fullscreen");
        StaticHelpersGarterSDK.SdkDebugger("Garter.I.RequestAd('fullscreen')", "-", "If no ad displayed, there was not enough time from prev ad");
    }

    public void RequestFullscreenAdCb()
    {
        StaticHelpersGarterSDK.SdkDebugger("Garter.I.RequestAd('fullscreen', callback)", "-", "-");
        Garter.I.RequestAd("fullscreen", (state) => {
            StaticHelpersGarterSDK.SdkDebugger("Garter.I.RequestAd('fullscreen', callback)", "callback: " + state, "-");
            if (state == "loaded")
            {
                // MUTE GAME
                try
                {
                    AudioListener.volume = 0f;
                    AudioListener.pause = true;
                }
                catch
                {
                    Debug.LogWarning("Problem in Audio Listener setting");
                }

                Time.timeScale = 0f;

            }
            else if (state == "completed")
            {
                try
                {
                    AudioListener.volume = 1f;
                    AudioListener.pause = false;
                }
                catch
                {
                    Debug.LogWarning("Problem in Audio Listener setting");
                }
                Time.timeScale = 1f;
            }
            else
            {
                // ...
            }
        });
    }

    public void RequestFullscreenDocs()
    {

    }

    public void RequestBanner(string position)
    {
        if (position == "top")
        {
            StaticHelpersGarterSDK.SdkDebugger("Garter.I.RequestAd('banner1', Garter.BannerAction.Display, Garter.BannerPosition.Top, Garter.BannerSize.Banner, callback)", "-", "-");
            Garter.I.RequestAd("banner1", Garter.BannerAction.Display, Garter.BannerPosition.Top, Garter.BannerSize.Banner, (state) =>
            {
                StaticHelpersGarterSDK.SdkDebugger("Garter.I.RequestAd('banner1', Garter.BannerAction.Display, Garter.BannerPosition.Top, Garter.BannerSize.Banner, callback)", "callback: " + state, "-");
            });
        } else // bottom
        {
            StaticHelpersGarterSDK.SdkDebugger("Garter.I.RequestAd('banner2', Garter.BannerAction.Display, Garter.BannerPosition.Bottom, Garter.BannerSize.Banner, callback)", "-", "-");
            Garter.I.RequestAd("banner2", Garter.BannerAction.Display, Garter.BannerPosition.Bottom, Garter.BannerSize.Banner, (state) =>
            {
                StaticHelpersGarterSDK.SdkDebugger("Garter.I.RequestAd('banner2', Garter.BannerAction.Display, Garter.BannerPosition.Bottom, Garter.BannerSize.Banner, callback)", "callback: " + state, "-");
            });
        }
    }

    public void CloseBanner(string position)
    {
        if (position == "top")
        {
            StaticHelpersGarterSDK.SdkDebugger("Garter.I.RequestAd('banner1', Garter.BannerAction.Hide, Garter.BannerPosition.Top, Garter.BannerSize.Banner, callback)", "-", "-");
            Garter.I.RequestAd("banner1", Garter.BannerAction.Hide, Garter.BannerPosition.Top, Garter.BannerSize.Banner, (state) =>
            {
                StaticHelpersGarterSDK.SdkDebugger("Garter.I.RequestAd('banner1', Garter.BannerAction.Hide, Garter.BannerPosition.Top, Garter.BannerSize.Banner, callback)", "callback: " + state, "-");
            });
        }
        else // bottom
        {
            StaticHelpersGarterSDK.SdkDebugger("Garter.I.RequestAd('banner2', Garter.BannerAction.Hide, Garter.BannerPosition.Bottom, Garter.BannerSize.Banner, callback)", "-", "-");
            Garter.I.RequestAd("banner2", Garter.BannerAction.Hide, Garter.BannerPosition.Bottom, Garter.BannerSize.Banner, (state) =>
            {
                StaticHelpersGarterSDK.SdkDebugger("Garter.I.RequestAd('banner2', Garter.BannerAction.Hide, Garter.BannerPosition.Bottom, Garter.BannerSize.Banner, callback)", "callback: " + state, "-");
            });
        }
    }

    public void BannerDocs() { }

    public void RequestRewardAd()
    {
        StaticHelpersGarterSDK.SdkDebugger("Garter.I.RewardedAd('rewarded',callback)", "-", "-");
        Garter.I.RequestAd("rewarded", (state) =>
        {
            StaticHelpersGarterSDK.SdkDebugger("Garter.I.RequestAd('rewarded', callback)", "callback: " + state, "-");
        });
    }

    public void RewardedAdDocs()
    {

    }

    public void GetAdConf()
    {
        Garter.I.GetAdConf((conf) => {
            StaticHelpersGarterSDK.SdkDebugger("Garter.I.GetAdConf ((conf) => {})", ("Next ad in " + conf.nextAdM.ToString() + "s. Minimum time between ads " + conf.meantimeM.ToString() + "s."), "returns ads configuration.");
        });
    }

    public void OpenDocs(string adType)
    {
        Application.OpenURL("https://developers.gamearter.com/docs");
    }
}
