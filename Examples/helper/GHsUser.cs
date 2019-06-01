using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GHsUser : MonoBehaviour
{
    public void Nick()
    {
        StaticHelpersGarterSDK.SdkDebugger("Garter.I.UserNick()", Garter.I.UserNick(), "-");
    }
    public void Lang()
    {
        StaticHelpersGarterSDK.SdkDebugger("Garter.I.UserLang()", Garter.I.UserLang(), "User's prefered language");
    }
    public void Country()
    {
        StaticHelpersGarterSDK.SdkDebugger("Garter.I.UserCountry()", Garter.I.UserCountry(), "Country a user is connected from");
    }
    public void Picture()
    {
        StaticHelpersGarterSDK.SdkDebugger("Garter.I.UserImage()", "Texture2D image", "Returns user's profile image in Texture2D format");
    }
    public void Logged()
    {
        StaticHelpersGarterSDK.SdkDebugger("Garter.I.IsLoggedUser()", Garter.I.IsLoggedUser().ToString(), "-");
    }
    public void Rank()
    {
        StaticHelpersGarterSDK.SdkDebugger("Garter.I.UserRankName()", Garter.I.UserRankName(), "-");
    }
}
