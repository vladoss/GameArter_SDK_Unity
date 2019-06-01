using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GHsGameMoney : MonoBehaviour
{
    public void UpdateCurrency()
    {
        System.Random r = new System.Random();
        decimal balanceChange = (decimal)r.Next(-10, 10);
        decimal c = Garter.I.LocalCurrency(balanceChange);
        StaticHelpersGarterSDK.SdkDebugger("Garter.I.LocalCurrency(" + balanceChange + ")", c.ToString("0.000"), "-");
        
        GameObject userFunds = GameObject.Find("UserFunds");
        if(userFunds != null)
        {
            Text userFundsTxt = userFunds.GetComponent<Text>();
            userFundsTxt.text = "User funds: " + c;
        }
    }

    public void GetCurrencyFunds()
    {
        StaticHelpersGarterSDK.SdkDebugger("Garter.I.LocalCurrency()", Garter.I.LocalCurrency().ToString("0.000"), "-");
    }

    public void OpenExchange()
    {
        Garter.I.OpenSdkWindow("exchange");
        StaticHelpersGarterSDK.SdkDebugger("Garter.I.OpenSdkWindow ('exchange');", "-", "Open module request");
    }
}
