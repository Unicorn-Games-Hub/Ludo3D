using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Analytics;

public class FirebaseHandler : MonoBehaviour
{
    public static FirebaseHandler instance;

    void Awake()
    {
        if(instance!=null)
        {
            return;
        }
        else
        {
            instance=this;
        }
    }

    void Start()
    {
        FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
        FirebaseAnalytics.SetUserProperty(FirebaseAnalytics.UserPropertySignUpMethod,"google");
        FirebaseAnalytics.SetUserId("Unicorn_Test_User");
    }

    public void TrackOpenFortheFirstTime()
    {
        Firebase.Analytics.FirebaseAnalytics.LogEvent("app_open");
    }
    public void TrackNumOfGamePlay(int numOfGames)
    {
        Firebase.Analytics.FirebaseAnalytics.LogEvent("Game_Start","number", "numOfGames");
    }

    #region Home Screen
    public void TrackVsHumanPlay()
    {
        Firebase.Analytics.FirebaseAnalytics.LogEvent("button_local","HumanvsHuman","vsHumanClicked");
    } 

    public void TrackVsBotPlay()
    {
       Firebase.Analytics.FirebaseAnalytics.LogEvent("button_cpu","HumanvsCpu","vsBotClicked");
    }
    public void TrackVsmultiPlay()
    {
        Firebase.Analytics.FirebaseAnalytics.LogEvent("button_multiplayer", "HumanvsMultiplayer", "vsMultiplayerClicked");
    }

    public void TrackNumberOfPlayers(int noOfPlayers)
    {
        Firebase.Analytics.FirebaseAnalytics.LogEvent("number_of_player","number","noOfPlayers");
    }


    public void TrackNumberOfGamesPlayed(int gameNumber)
    {
        Firebase.Analytics.FirebaseAnalytics.LogEvent("games_Played","total",gameNumber);
    }

    public void TrackGamesPlayedOffline(int gameNumber)
    {
        Firebase.Analytics.FirebaseAnalytics.LogEvent("offlie_play","total",gameNumber);
    }

    public void TrackGamesPlayedOnline(int gameNumber)
    {
        Firebase.Analytics.FirebaseAnalytics.LogEvent("online_Play","total",gameNumber);
    }

    public void TrackGameEnd(string timeSpent,string rank,string onlineStatus)
    {
        Firebase.Analytics.FirebaseAnalytics.LogEvent("game_end",new Parameter[]{
            new Firebase.Analytics.Parameter("gameplay_time",timeSpent),
            new Firebase.Analytics.Parameter("player_rank",rank),
            new Firebase.Analytics.Parameter("online_status",onlineStatus)
            
        });
        ADScript.Instance.ShowIntestitial();
    }

    public void TrackAdLoad(string adType,string adPos)
    {
        Firebase.Analytics.FirebaseAnalytics.LogEvent("ads_load",new Parameter[]{
            new Firebase.Analytics.Parameter("ads_type",adType),
            new Firebase.Analytics.Parameter("clicked",PlayerPrefs.GetInt("adsClickedThisSession")),
            new Firebase.Analytics.Parameter("position",adPos),
            new Firebase.Analytics.Parameter("ads_per_session",PlayerPrefs.GetInt("adsThisSession")),
            new Firebase.Analytics.Parameter("ads_till_date",PlayerPrefs.GetInt("total_ads_shown"))
        });
    }

    public void TrackAdMileStone()
    {
        Firebase.Analytics.FirebaseAnalytics.LogEvent("ads_milestone",new Parameter[]{
            new Firebase.Analytics.Parameter("total_ads",PlayerPrefs.GetInt("total_ads_shown")),
            new Firebase.Analytics.Parameter("reward_ads_count",PlayerPrefs.GetInt("reward_video_shown")),
            new Firebase.Analytics.Parameter("interstitial_ads_count",PlayerPrefs.GetInt("interstitial_shown")),
            new Firebase.Analytics.Parameter("reward_ads_clicked",PlayerPrefs.GetInt("reward_clicked")),
            new Firebase.Analytics.Parameter("interstitial_ads_clicked",PlayerPrefs.GetInt("interstitial_clicked"))
        });
    }

    public void TrackSessionMileStone()
    {
        Firebase.Analytics.FirebaseAnalytics.LogEvent("session_milestone",new Parameter[]{
            new Firebase.Analytics.Parameter("win_total",PlayerPrefs.GetInt("total_victory")),
            new Firebase.Analytics.Parameter("loss_total",PlayerPrefs.GetInt("total_defeat")),
            new Firebase.Analytics.Parameter("ads_total",PlayerPrefs.GetInt("total_ads_shown")),
            new Firebase.Analytics.Parameter("ads_clicked_total",PlayerPrefs.GetInt("total_ads_clicked")),
            new Firebase.Analytics.Parameter("settings",PlayerPrefs.GetInt("total_settings_clicked")),
            new Firebase.Analytics.Parameter("with_net",PlayerPrefs.GetInt("total_online_play")),
            new Firebase.Analytics.Parameter("without_net",PlayerPrefs.GetInt("total_offline_play")),
            new Firebase.Analytics.Parameter("pvp_total",PlayerPrefs.GetInt("total_player_vs_player")),
            new Firebase.Analytics.Parameter("pvb_total",PlayerPrefs.GetInt("total_player_vs_bots")),
            new Firebase.Analytics.Parameter("pvm_total",PlayerPrefs.GetInt("total_player_vs_mixed")),
            new Firebase.Analytics.Parameter("local_total",PlayerPrefs.GetInt("total_against_human")),
            new Firebase.Analytics.Parameter("cpu_total",PlayerPrefs.GetInt("total_against_cpu"))
        });
    }

    public void TrackRewardVideoAds()
    {
        Firebase.Analytics.FirebaseAnalytics.LogEvent("ads_rewarded","rewarded","shown");
    }

    public void TrackInerstitialAds()
    {
        Firebase.Analytics.FirebaseAnalytics.LogEvent("ads_inter","interstitial","shown");
    }

    public void TrackSettings()
    {
        Firebase.Analytics.FirebaseAnalytics.LogEvent("settings_changed","settings","settingsClicked");
    }

    public void TrackGameMusic(int musicValue)
    {
        string musicStatus="off";
        if(musicValue==0)
        {
            musicStatus="on";
        }
        Firebase.Analytics.FirebaseAnalytics.LogEvent("GameMusic","status",musicStatus);
    }

    public void TrackGameSfx(int sfxValue)
    {
        string sfxStatus="off";
        if(sfxValue==0)
        {
            sfxStatus="on";
        }
        Firebase.Analytics.FirebaseAnalytics.LogEvent("GameSfx","status",sfxStatus);
    }

    public void TrackGameVibration(int vibrationValue)
    {
        string vibrationStatus="off";
        if(vibrationValue==0)
        {
            vibrationStatus="on";
        }
        Firebase.Analytics.FirebaseAnalytics.LogEvent("GameVibration","status",vibrationStatus);
    }
    #endregion

    // public void TrackVicotry(int totalWin)
    // {
    //     Firebase.Analytics.FirebaseAnalytics.LogEvent("Voctory","victoryCount",totalWin);
    // }

    // public void TrackDefeat(int totalDefeat)
    // {
    //     Firebase.Analytics.FirebaseAnalytics.LogEvent("Defeat","defeatCount",totalDefeat);
    // }

    public void TrackGameStatistics()
    {
        Firebase.Analytics.FirebaseAnalytics.LogEvent("Game Statistics",new Parameter[]{
            new Firebase.Analytics.Parameter("OpponentCut",2),
            new Firebase.Analytics.Parameter("Total Bonus Turn",1),
            new Firebase.Analytics.Parameter("OpponentsDefeated",3),
            new Firebase.Analytics.Parameter("High Score",30)
        });
    }
}
