using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GHsActivePanel : MonoBehaviour
{
    public GameObject ads;
    public GameObject events;
    public GameObject db;
    public GameObject money;
    public GameObject user;
    public GameObject analytics;
    public GameObject tech;
    public GameObject more;

    public void AdsPanel(bool active)
    {
        ads.SetActive(active);
    }
    public void EventsPanel(bool active)
    {
        events.SetActive(active);
    }
    public void DbPanel(bool active)
    {
        db.SetActive(active);
    }
    public void MoneyPanel(bool active)
    {
        money.SetActive(active);
    }
    public void UserPanel(bool active)
    {
        user.SetActive(active);
    }
    public void AnalyticsPanel(bool active)
    {
        analytics.SetActive(active);
    }
    public void TechPanel(bool active)
    {
        tech.SetActive(active);
    }
    public void MorePanel(bool active)
    {
        more.SetActive(active);
    }
}