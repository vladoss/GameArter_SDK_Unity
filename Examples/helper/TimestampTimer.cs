using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimestampTimer : MonoBehaviour
{
    public GameObject TextObj;

    private float timer = 0.0f;
    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        float seconds = timer % 60;
        TextObj.GetComponent<Text>().text = seconds.ToString("0");

    }
}
