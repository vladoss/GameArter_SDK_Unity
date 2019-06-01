using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GHsInfoPanel : MonoBehaviour
{

    public Text textArea;

    // Update is called once per frame

    void Update()
    {
        if (Garter.I.IsInitialized()) {
            textArea.text = "Nick: " + Garter.I.UserNick() + "\nLang: " + Garter.I.UserLang() + "\nCountry: " + Garter.I.UserCountry() + "\nFunds: " + Garter.I.LocalCurrency() + " \nPlatform: " + Garter.I.targetPlatform;
        } else
        {
            textArea.text = "Iniztializing SDK...";
        }
    }
}
