using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GHsDbData : MonoBehaviour
{
    private DataManagerExample garterLite = null;

    void Awake()
    {
        try
        {
            garterLite = GameObject.Find("ExampleObjectForSdkInteraction").GetComponent<DataManagerExample>();
        }
        catch
        {
        }
    }

    public void PostDataDirectlyLiteSDK()
    {
        StaticHelpersGarterSDK.SdkDebugger("garterLite.PostData ();", "-", "Check PostData () in GarterLite script for more info.");
        garterLite.PostData();
    }

    public void PostDataViaMiddlemanLiteSDK()
    {
        StaticHelpersGarterSDK.SdkDebugger("garterLite.PostData (false);", "-", "Check PostData () in GarterLite script for more info.");
        garterLite.PostDataInMeantime();
    }

    public void GetDataLiteSDK()
    {
        StaticHelpersGarterSDK.SdkDebugger("garterLite.GetData ()", (Garter.I.GetData<string>("my-key")), "Check GetData () in GarterLite script for more info.");
        garterLite.GetData();
    }

    public void GameProgress(bool getProgress)
    {
        if (getProgress)
        {
            StaticHelpersGarterSDK.SdkDebugger("Garter.I.UserProgress ();", Garter.I.UserProgress().ToString(), "-");
        }
        else
        { // set
          // generate random float number
            System.Random random = new System.Random();
            float randomProgressVal = (float)random.Next(0, 100);
            StaticHelpersGarterSDK.SdkDebugger("Garter.I.UserProgress (" + randomProgressVal + ");", Garter.I.UserProgress(randomProgressVal).ToString(), "Depends on game progress counting option");
        }
    }

    public void ClearUserData(bool directly)
    {
        if (directly)
        {
            Garter.I.ClearDataUserConfirm();
            StaticHelpersGarterSDK.SdkDebugger("Garter.I.ClearDataUserConfirm ()", "-", "Clear saved data request");
        }
        else
        {
            Garter.I.ClearDataUserReq();
            StaticHelpersGarterSDK.SdkDebugger("Garter.I.ClearDataUserReq ()", "-", "Clear saved data request");
        }
    }
}
