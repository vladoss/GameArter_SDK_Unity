using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GHsEvents : MonoBehaviour
{
    public void AddKills(int increaseAbout)
    {
        decimal value = Garter.I.Event("kills", increaseAbout);
        StaticHelpersGarterSDK.SdkDebugger("Garter.I.Event (\"kills\", " + increaseAbout + ")", value.ToString(), "all data (for illustration): kills: " + value + " | deaths: " + Garter.I.Event("deaths"));
    }
    public void GetKills()
    {
        decimal value = Garter.I.Event("kills");
        StaticHelpersGarterSDK.SdkDebugger("Garter.I.Event (\"kills\")", value.ToString(), "all data (for illustration): kills: " + value + " | deaths: " + Garter.I.Event("deaths"));
    }

    public void AddDeaths(int increaseBy)
    {
        decimal value = Garter.I.Event("deaths", increaseBy);
        StaticHelpersGarterSDK.SdkDebugger("Garter.I.Event (\"kills\", " + increaseBy + ")", value.ToString(), "all data (for illustration): kills: " + value + " | deaths: " + Garter.I.Event("deaths"));
    }

    public void TakeTime()
    {
        decimal value = Garter.I.Event("bestTime", 0.32M);
        Debug.Log(value);
        StaticHelpersGarterSDK.SdkDebugger("Garter.I.Event (\"bestTime\", 0.32M)", value.ToString("0.000"));
    }
    public void GetTime()
    {
        decimal value = Garter.I.Event("bestTime");
        StaticHelpersGarterSDK.SdkDebugger("Garter.I.Event (\"bestTime\")", value.ToString("0.000"));
    }
}