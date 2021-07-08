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

    #region Home Screen
    public void TrackVsHumanPlay()
    {
        Firebase.Analytics.FirebaseAnalytics.LogEvent("HumanVsHuman","vsHuman","vsHumanClicked");
    } 

    public void TrackVsBotPlay()
    {
       Firebase.Analytics.FirebaseAnalytics.LogEvent("HumanVsBot","vsBot","vsBotClicked");
    }

    public void TrackSettings()
    {
        Firebase.Analytics.FirebaseAnalytics.LogEvent("Settings","settingsClicked","");
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

    public void TrackGameSessions(int totalSessions)
    {
        Firebase.Analytics.FirebaseAnalytics.LogEvent("sessions_count","count",totalSessions);
    }
    public void TrackVicotry(int totalWin)
    {
        Firebase.Analytics.FirebaseAnalytics.LogEvent("Voctory","victoryCount",totalWin);
    }

    public void TrackDefeat(int totalDefeat)
    {
        Firebase.Analytics.FirebaseAnalytics.LogEvent("Defeat","defeatCount",totalDefeat);
    }

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
