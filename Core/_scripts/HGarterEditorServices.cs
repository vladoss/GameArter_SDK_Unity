using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameArter.Ads;

public class HGarterEditorServices
{
    public void SignInSrvc(Action<Garter._GoogleSignInAuth> cb) { cb(null); }

    public void AdRequest(AdUnitData adData, Action<string> callback) {
        if (adData.defined)
        {
            string adText = "Ad " + adData.unitId + ". Adtype: " + adData.adType.ToString();
            Vector2 adSize = new Vector2(0, 0);
            Vector2 adPosition = new Vector2(0, 0); ;
            string adName = adData.unitId;

            if (adData.adType != AdsConfiguration.AdType.Banner)
            {
                adSize = new Vector2(Screen.width - 30, Screen.height - 120);
                adPosition = new Vector2(0, -50);
                /*
                switch (adData.adType)
                {
                    case AdsConfiguration.AdType.Fullscreen:
                        adSize = new Vector2(Screen.width - 50, Screen.height - 50);
                        adPosition = new Vector2(0, 0);
                        break;
                    case AdsConfiguration.AdType.Rewarded:
                        adSize = new Vector2(Screen.width - 50, Screen.height - 50);
                        adPosition = new Vector2(0, 0);
                        break;
                }
                */
                IllustrateFeature(adName, adText, adSize, adPosition, false, callback);
            } else
            {
                // on basis of action
                if (adData.action == Garter.BannerAction.Display)
                {
                    if (adData.mPosition)
                    {
                        adSize = new Vector2(adData.sizeXY[0], adData.sizeXY[1]);
                        float xDist = -(Screen.width / 2) + adData.positionXY[0];
                        float yDist = -(Screen.height / 2) + adData.positionXY[1];
                        adPosition = new Vector2(xDist, yDist);
                    }
                    else
                    {
                        adSize = new Vector2(320, 50);
                        // convert size
                        switch (adData.size)
                        {
                            case Garter.BannerSize.Banner: adSize = new Vector2(320, 50); break;
                            case Garter.BannerSize.IABBanner: adSize = new Vector2(468, 60); break;
                            case Garter.BannerSize.MediumRectangle: adSize = new Vector2(300, 250); break;
                            case Garter.BannerSize.Leaderboard: adSize = new Vector2(728, 90); break;
                            case Garter.BannerSize.SmartBanner: adSize = new Vector2(320, 50); break;
                        }

                        float xDist = 0;
                        float yDist = 0;
                        // convert position
                        switch (adData.position)
                        {
                            case Garter.BannerPosition.Bottom:
                                xDist = 0;
                                yDist = -(Screen.height / 2) + (adSize[1] / 2);
                                break;
                            case Garter.BannerPosition.BottomLeft:
                                xDist = -(Screen.width / 2);
                                yDist = -(Screen.height / 2) + (adSize[1] / 2);
                                break;
                            case Garter.BannerPosition.BottomRight:
                                xDist = (Screen.width / 2) - adSize[0];
                                yDist = -(Screen.height / 2) + (adSize[1] / 2);
                                break;
                            case Garter.BannerPosition.Center:
                                xDist = 0;
                                yDist = 0;
                                break;
                            case Garter.BannerPosition.Top:
                                xDist = 0;
                                yDist = (Screen.height / 2) - (adSize[1] / 2);
                                break;
                            case Garter.BannerPosition.TopLeft:
                                xDist = -(Screen.width / 2);
                                yDist = (Screen.height / 2) - (adSize[1] / 2);
                                break;
                            case Garter.BannerPosition.TopRight:
                                xDist = (Screen.width / 2) - adSize[0];
                                yDist = (Screen.height / 2) - (adSize[1] / 2);
                                break;
                        }
                        adPosition = new Vector2(xDist, yDist);
                        IllustrateFeature(adName, adText, adSize, adPosition, true, callback);
                    }
                }
                else
                {
                    RemoveSelectedFeatureBox(adName);
                }
            }
        }
    }

    [System.Serializable]
    private class FeatureWindow {
        public string featureName;
        public GameObject featureObject;
        public bool banner;
        public Action<string> cb;
    }

    private List<FeatureWindow> openedFeatureWindow = new List<FeatureWindow>();

    private void IllustrateFeature(string featureName, string text, Vector2 dimension, Vector2 position, bool isBanner, Action<string> callback)
    {
        if (GameObject.Find(featureName + "_unitObject") == null)
        {
            GameObject featureUnitCanvas = new GameObject(featureName + "_unitObject");
            Canvas canvas = featureUnitCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            featureUnitCanvas.AddComponent<CanvasScaler>();
            featureUnitCanvas.AddComponent<GraphicRaycaster>();
            // create feature window
            GameObject featureWindow = new GameObject(featureName + "_window");
            featureWindow.transform.SetParent(featureUnitCanvas.transform);
            RectTransform featureWindowPosition = featureWindow.AddComponent<RectTransform>();
            featureWindowPosition.sizeDelta = dimension; // width
            featureWindowPosition.anchoredPosition = position; // possition
            Image panel = featureWindow.AddComponent<Image>();
            panel.color = Color.black;
            // fill feature window with text
            GameObject featureTextBox = new GameObject(featureName + "_text");
            featureTextBox.transform.SetParent(featureWindow.transform);
            Text featureText = featureTextBox.AddComponent<Text>();
            featureText.text = text;
            featureText.color = Color.white;
            featureText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            featureText.alignment = TextAnchor.MiddleCenter;
            RectTransform featureBoxRT = featureTextBox.GetComponent<RectTransform>();
            featureBoxRT.anchoredPosition = new Vector2(0, 5);
            featureBoxRT.sizeDelta = new Vector2(461, 88);
            featureBoxRT.localScale = new Vector2(1, 1);
            // add close button with listeners (cache listeners externally)
            GameObject closeFeatureWindow = new GameObject(featureName + "_close");
            closeFeatureWindow.transform.SetParent(featureWindow.transform);
            Button featureCloseBtn = closeFeatureWindow.AddComponent<Button>();
            Text featureCloseBtnTxt = closeFeatureWindow.AddComponent<Text>();
            featureCloseBtnTxt.text = "Close";
            featureCloseBtnTxt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            featureCloseBtnTxt.alignment = TextAnchor.MiddleCenter;
            featureCloseBtn.targetGraphic = featureCloseBtnTxt;
            RectTransform featureCloseBtnRT = closeFeatureWindow.GetComponent<RectTransform>();
            featureCloseBtnRT.anchoredPosition = new Vector2(-3f, -22f);
            featureCloseBtnRT.sizeDelta = new Vector2(160, 30);
            featureCloseBtnRT.transform.localScale = new Vector2(1, 1);
            featureCloseBtnRT.localScale = new Vector2(1, 1);
            featureCloseBtn.transition = Selectable.Transition.ColorTint;
            ColorBlock btnc = featureCloseBtn.colors;
            btnc.highlightedColor = new Color32(249, 201, 0, 255);
            featureCloseBtn.colors = btnc;
            featureCloseBtn.onClick.AddListener(RemoveLastFeatureBox);

            if (callback != null) {
                callback("loaded");
            } else if (!isBanner)
            {
                Garter.I.SdkWindowOpened("ad"); // mute and pause
            }
            openedFeatureWindow.Add(new FeatureWindow { featureName = featureName, featureObject = featureUnitCanvas, banner = isBanner, cb = callback });
        }
        else
        {
            Debug.LogWarning("Previous feature window must be closed first");
        }
    }

    public void OpenLeaderboardUI(string eventId)
    {}

    private void RemoveLastFeatureBox()
    {
        MonoBehaviour.Destroy(openedFeatureWindow[openedFeatureWindow.Count-1].featureObject);
        if (openedFeatureWindow[openedFeatureWindow.Count - 1].cb != null) {
            openedFeatureWindow[openedFeatureWindow.Count - 1].cb("completed");
        } else if(!openedFeatureWindow[openedFeatureWindow.Count - 1].banner)
        {
            Garter.I.SdkWindowClosed("ad");
        }
        openedFeatureWindow.RemoveAt(openedFeatureWindow.Count-1);
    }
    
    private void RemoveSelectedFeatureBox(string featureName)
    {
        for (int i = 0; i < openedFeatureWindow.Count; i++)
        {
            if (openedFeatureWindow[i].featureName == featureName)
            {
                MonoBehaviour.Destroy(openedFeatureWindow[i].featureObject);
                if (openedFeatureWindow[openedFeatureWindow.Count - 1].cb != null) {
                    openedFeatureWindow[openedFeatureWindow.Count - 1].cb("completed");
                } else if (!openedFeatureWindow[openedFeatureWindow.Count - 1].banner)
                {
                    Garter.I.SdkWindowClosed("ad");
                }
                openedFeatureWindow.RemoveAt(i);
                break;
            }
        }
    }

    public void AnalyticsRequest(string mode, string scene, string category, string action, string label, int value) {
        Debug.Log("[GameArter][Analytics] Posting " + mode + " / " + scene + " / " +category +"/"+ action+"/"+label+"/"+value + " to analytics. | NOTE: Feature works in GameArter gameplayer only.");
    }

    public void OpenModule(string data) {

    }

    public void StoryAnimation(int num) {
        IllustrateFeature("cutscene", "Running story " + num + ". (feature available for Web only)", new Vector2(Screen.width - 50, Screen.height - 50), new Vector2(0, 0), false, null);
    }

    public void UserBilanceCallback(string data) { }

    public void ActivityPing(int activityState) { }

    public void GameSettings(string data) { }

    public void GameRestart() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    public void GameBan(string data) { }

    public void Redirect(string url, bool _blank)
    {
        //_blank = false;
        Application.OpenURL(url);
    }
    public void Fullscreen() { Debug.Log("[GameArter] Fullscreen is supported on web only"); }
    public void InstallAsPWA() { Debug.Log("[GameArter] Progressive Web App is  supported on web only."); }
    public void GetAdConfiguration() { }
    public void InitGameSignature(string sdkVersion, char interpreter, uint gameId, bool multiplayer, byte projectVersion) { }
    public void GameInitialized(string state) { }
    public void Invite() { }
}
