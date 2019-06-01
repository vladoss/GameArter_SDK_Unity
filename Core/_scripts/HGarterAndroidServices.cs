#if (UNITY_ANDROID && !UNITY_EDITOR)
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using GameArter.Ads;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;
#endif
public class HGarterAndroidServices
{
#if (UNITY_ANDROID && !UNITY_EDITOR)
   /*
    public IEnumerator PropertyNotification(float startTime)
    {
        yield return new WaitForSeconds(startTime);
        yield break;
    }
    */
    public void PostToAchievements(string achievementId, float achievementValue){
         Social.ReportProgress(achievementId, achievementValue, (bool success) => {
             Debug.Log("[GameArter][Google Play Achievement]["+achievementId+"] | value: "+achievementValue+", success: "+success);
        });
    }
    
    public void PostToLeaderboard(string leaderboardId, decimal value)
    {
        Social.ReportScore((long)value, leaderboardId, (bool success) => {
             Debug.Log("[GameArter][Google Play Leaderboard]["+leaderboardId+"] | value: "+value+", success: "+success);
        });
    }

    public void InitGameSignature(string sdkVersion, char interpreter, uint gameId, bool multiplayer, byte projectVersion) { }

    public void GameInitialized(string state) { }

    public void SignInSrvc(Action<Garter._GoogleSignInAuth> cb)
    {
        if (!Social.localUser.authenticated)
        {
            Garter.I.SdkWindowOpened("login"); // mute game

            PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
                // enables saving game progress.
                //.EnableSavedGames()
                // registers a callback to handle game invitations received while the game is not running.
                //.WithInvitationDelegate(< callback method >)
                // registers a callback for turn based match notifications received while the game is not running.
                //.WithMatchDelegate(< callback method >)
                // requests the email address of the player be available. Will bring up a prompt for consent.
                .RequestEmail()
                
                //.AddOauthScope("email")
                // requests a server auth code be generated so it can be passed to an associated backend server application and exchanged for an OAuth token.
                .RequestServerAuthCode(true)
                // requests an ID token be generated. This OAuth token can be used to identify the player to other services such as Firebase.
                .RequestIdToken()
                .Build();

            PlayGamesPlatform.InitializeInstance(config);

            PlayGamesPlatform.DebugLogEnabled = true;

            PlayGamesPlatform.Activate();

            Social.localUser.Authenticate((bool success) =>
            {
                Garter._AccessTokens accessTokens = null;

                if (success)
                {
                    accessTokens = new Garter._AccessTokens
                    {
                        //ac = PlayGamesPlatform.Instance.GetServerAuthCode(),
                        at = ((PlayGamesLocalUser)Social.localUser).GetIdToken(),
                        e = ((PlayGamesLocalUser)Social.localUser).Email,
                        n = Social.localUser.userName
                    };
                }

                cb(new Garter._GoogleSignInAuth()
                {
                    success = success,
                    aTokens = accessTokens
                });
           });
        } else {
            cb(null);
        }
    }

    public void AdRequest(AdUnitData adData, Action<string> callback)
    {
        if(Garter.I.adMobAds != null){
            switch (adData.adType)
            {
                case AdsConfiguration.AdType.Fullscreen:
                    Garter.I.adMobAds.FullscreenAd(adData.channelId, callback);
                    break;
                case AdsConfiguration.AdType.Rewarded:
                    Garter.I.adMobAds.RewardedAds(adData.channelId, callback);
                    break;
                case AdsConfiguration.AdType.Banner:
                    Garter.I.adMobAds.BannerAd(adData, callback);
                    break;
                case AdsConfiguration.AdType.Native:
                    if(callback!=null) callback("unsupported"); // in progress
                    break;
            }
        } else {
            Debug.LogError("adMob instance does not exist");
        }
    }

    public void AnalyticsRequest(string mode, string scene, string category, string action, string label, int value) {
        // need to be connected with analytics
    }

    public void OpenLeaderboardUI(string leaderboardUI){
        if(!string.IsNullOrEmpty(leaderboardUI)){
            PlayGamesPlatform.Instance.ShowLeaderboardUI(leaderboardUI);
         } else {
            Social.ShowLeaderboardUI();
        }
        
    }

    public void OpenModule(string window) { 
        Debug.Log("[GameArter Android] → Display external panel : "+window);
        switch (window) {
                    case "PWA": break;
                    case "login": break;
				case "badge":
                        Social.ShowAchievementsUI();
                        //Application.OpenURL ("https://www.gamearter.com/modules/badges/"+Garter.I.GameId()+"/"+Garter.I._ProjectVersion());
					break;
				case "leaderboard":
                    Social.ShowLeaderboardUI();
					break;
				case "profile":
					Application.OpenURL ("https://www.gamearter.com/modules/profile/"+Garter.I.GameId());
					break;
				case "share":
					//Application.OpenURL ("https://www.gamearter.com/game_config/badges.php?g="+Garter.I.GameId()+"&u="+Garter.I.UserId());
					break;
				case "offline": // enables converstion between local and paco currency
					//Application.OpenURL ("https://www.gamearter.com/game_config/badges.php?g="+Garter.I.GameId()+"&u="+Garter.I.UserId());
					break;
				case "discussion":
					Application.OpenURL ("https://www.gamearter.com/modules/comments/"+Garter.I.GameId());
					break;
				case "report":
					
					break;
				case "development":
					
					break;
				case "video":
					Application.OpenURL ("https://www.gamearter.com/modules/video/"+Garter.I.GameId());
					
					break;
				case "gameinfo":
					Application.OpenURL ("https://www.gamearter.com/modules/controls/"+Garter.I.GameId());
					
					break;
				case "moregames":
					
					break;
				default:
					
					break;
				}
    }

    public void StoryAnimation(int num)
    {

    }

    public void UserBilanceCallback(string data) { }

    public void ActivityPing(int activityState) { }

    public void GameSettings(string data) { }

    public void GameRestart()
    {
         
    }

    public void GameBan(string data) { }

    public void Redirect(string url, bool _blank)
    {
        Application.OpenURL(url);
    }

    public void Fullscreen()
    {
        Debug.Log("[GameArter] Fullscreen is not supported on android.");
    }

    public void InstallAsPWA() { }

    public void GetAdConfiguration() { }

    public void Invite() { }

    public void SyncPlatformAchievements(){
        // stahni achievements data z google play, pokud zmena, aktualizuj
    }

    public void SyncPlatformLeaderboard(){
    }
#endif
}