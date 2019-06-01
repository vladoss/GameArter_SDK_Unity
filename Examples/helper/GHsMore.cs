using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GHsMore : MonoBehaviour
{
    public void InstallAsApp()
    {
        if (Garter.I.GetStatePWA() == "enabled")
        {
            Garter.I.InstallAsPWA<string>((state) => {
                Debug.Log("PWA installation status: " + state);
            });
        }
        else
        {
            Debug.Log("PWA service is disabled");
        }
    }

    public void PWAState()
    {
        StaticHelpersGarterSDK.SdkDebugger("Garter.I.GetStatePWA()", Garter.I.GetStatePWA(), "-");
    }

    public void Redirect(bool toNewTab)
    {
        Garter.I.OpenWebPage("https://www.google.com", "individual", toNewTab);
        if (toNewTab)
        {
            StaticHelpersGarterSDK.SdkDebugger("Garter.I.OpenWebPage (\"https://www.google.com\")", "-", "-");
        }
        else
        {
            StaticHelpersGarterSDK.SdkDebugger("Garter.I.OpenWebPage (\"https://www.google.com\",\"individual\", false)", "-", "-");
        }
    }

    public void Fullscreen()
    {
        StaticHelpersGarterSDK.SdkDebugger("Garter.I.Fullcreen ()", "-", "Feature does not work in Unity editor");
        Garter.I.Fullcreen();
    }


    /***************************** Alerts ***************************/
    public void SettingsAlert(bool display)
    {
        Garter.I.SettingsAlert(display);
        StaticHelpersGarterSDK.SdkDebugger("Garter.I.SettingsAlert (" + display + ")", "-", "-");
    }
    public void ShopAlert(bool display)
    {
        Garter.I.ShopAlert(display);
        StaticHelpersGarterSDK.SdkDebugger("Garter.I.ShopAlert (" + display + ")", "-", "-");
    }

    public void RunStory()
    {
        StaticHelpersGarterSDK.SdkDebugger("Garter.I.RunStoryAnimation (0)", "-", "Run story number 0 (Feature does not work in editor)");
        Garter.I.RunStoryAnimation(0);
    }

    public Texture officeMap = null;

    int updateLimit = 1001;
    public void LoadAssetScreen()
    {
        StaticHelpersGarterSDK.SdkDebugger("Garter.I.CreateLoadingScreen (\"Loading map: Office\", officeMap, true)", "-", "-");
        Garter.I.CreateLoadingScreen("Loading map: Office", officeMap, true);
        updateLimit = 0;
    }

    void Update()
    {
        updateLimit++;
        if (updateLimit < 1000)
        {
            Garter.I.UpdateLoadingScreen(updateLimit / 10);
        }
        else if (updateLimit == 1000)
        {
            Garter.I.RemoveLoadingScreen();
        }

    }
}
