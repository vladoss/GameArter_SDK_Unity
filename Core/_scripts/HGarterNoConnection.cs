using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HGarterNoConnection : MonoBehaviour
{
    public void Restart()
    {
        MonoBehaviour.Destroy(GameObject.Find("Garter_NoConnection(Clone)"));

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Garter.I.RestartApp();
        } else
        {
            StartCoroutine(TryReconnect(5f));
        }
        
    }

    private IEnumerator TryReconnect(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            new HGarterGui().NoInternetConnection();
        }
        yield break;
    }
}
