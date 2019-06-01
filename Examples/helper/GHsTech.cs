using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GHsTech : MonoBehaviour
{
    public void GetPlayer()
    {
        StaticHelpersGarterSDK.SdkDebugger("Garter.I.GetBrowserName()", Garter.I.GetBrowserName(), "Browser a game is running in");
    }
    public void IndividualGameMode()
    {
        byte limitSomething = Garter.I.IndividualGameMode();
        StaticHelpersGarterSDK.SdkDebugger("Garter.I.IndividualGameMode ()", limitSomething.ToString(), "-");
    }

    public void GetPhotonId()
    { //= AppId
        if (Garter.I.GetMultiplayerNetwork() != null)
        {
            string appId = Garter.I.GetMultiplayerNetwork()[0];
            StaticHelpersGarterSDK.SdkDebugger("Garter.I.GetMultiplayerNetwork()[0]", appId, "Returns photon ID");
        }
        else
        {
            StaticHelpersGarterSDK.SdkDebugger("-", "-", "Multiplayed is not set in init config");
        }
    }

    public void GetPhotonVer()
    { //=AppVersion
        if (Garter.I.GetMultiplayerNetwork() != null)
        {
            string AppVersion = Garter.I.GetMultiplayerNetwork()[1];
            StaticHelpersGarterSDK.SdkDebugger("Garter.I.GetMultiplayerNetwork()[1]", AppVersion, "Returns photon id ver");
        }
        else
        {
            StaticHelpersGarterSDK.SdkDebugger("-", "-", "Multiplayed is not set in init config");
        }
    }

    public void GetUserNetworkId()
    {
        if (Garter.I.GetMultiplayerNetwork() != null)
        {
            StaticHelpersGarterSDK.SdkDebugger("Garter.I.NetworkUniqueUserId()", Garter.I.NetworkUniqueUserId(), "Returns unique network user id");
        }
        else
        {
            StaticHelpersGarterSDK.SdkDebugger("-", "-", "Multiplayed is not set in init config");
        }
    }

    public void OpenModule(string moduleNameId)
    {
        Garter.I.OpenSdkWindow(moduleNameId);
        StaticHelpersGarterSDK.SdkDebugger("Garter.I.OpenSdkWindow (" + moduleNameId + ");", "-", "Open module request");
    }

    public void GetActivityState()
    {
        StaticHelpersGarterSDK.SdkDebugger("Garter.I.GetBrowserTabState ()", Garter.I.GetBrowserTabState(), "Browser tab state");
    }

    public void IsSdkInitialized()
    {
        StaticHelpersGarterSDK.SdkDebugger("Garter.I.IsInitialized()", Garter.I.IsInitialized().ToString(), "state of SDK initialiyation. True - initialized");
    }
}
