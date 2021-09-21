using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class LeaderboardHandler : MonoBehaviour
{
    public static LeaderboardHandler instance;
    public GameObject leaderboardUI;
    public GameObject continueBtn;
    public GameObject otherBtn;

    [System.Serializable]
    public class pInfo
    {
        public Text nameText;
        public Text scoreText;
        public Image iconImage;
    }
    public List<pInfo> lbInfoList=new List<pInfo>();

    [Header("Icons")]
    public Sprite[] playerIcons;
    public Sprite[] botIcons;

    [Header("Rate us")]
    public string appUrl="https://play.google.com/store/apps/details?id=com.UnicornGames.Ludo";
    public GameObject rateusUI;

    //time span
    private ulong gameStartTime;
    private bool rankShown=false;

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
        leaderboardUI.SetActive(false);
        rateusUI.SetActive(false);
        // if(GameDataHolder.instance!=null)
        // {
        //     GameDataHolder.instance.rateUsShownCounter++;
        // }

        //tracking total number of players
        int totalPlayers=0;
        int totalHumans=0;
        int totalBots=0;
        if(GameDataHolder.instance!=null)
        {
            GameDataHolder.instance.initialAdsShowCounter++;
            for(int i=0;i<GameDataHolder.instance.playerIndex.Length;i++)
            {
                if(GameDataHolder.instance.playerIndex[i]!=2)
                {
                    totalPlayers++;
                }
                if(GameDataHolder.instance.playerIndex[i]==0)
                {
                    totalHumans++;
                }
                if(GameDataHolder.instance.playerIndex[i]==1)
                {
                    totalBots++;
                }
            }

            if(totalHumans==totalPlayers)
            {
                PlayerPrefs.SetInt("total_player_vs_player",PlayerPrefs.GetInt("total_player_vs_player")+1);
            }
            else if(totalHumans==1)
            {
                PlayerPrefs.SetInt("total_player_vs_bots",PlayerPrefs.GetInt("total_player_vs_bots")+1);
            }
            else if(totalHumans>1&&totalPlayers>totalHumans)
            {
                PlayerPrefs.SetInt("total_player_vs_mixed",PlayerPrefs.GetInt("total_player_vs_mixed")+1);
            }

            if(!GameDataHolder.instance.isSessionStarted)
            {
                PlayerPrefs.SetInt("adsClickedThisSession",0);
                PlayerPrefs.SetInt("adsThisSession",0);
                GameDataHolder.instance.isSessionStarted=true;
            }
        }

        //increment total games played
        PlayerPrefs.SetInt("total_games_played",PlayerPrefs.GetInt("total_games_played")+1);
        int matchPlayedTillNow=PlayerPrefs.GetInt("total_games_played");

        if(Application.internetReachability==NetworkReachability.NotReachable)
        {
            PlayerPrefs.SetInt("total_offline_play",PlayerPrefs.GetInt("total_offline_play")+1);
            
        }
        else if(Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
        {
            PlayerPrefs.SetInt("total_online_play",PlayerPrefs.GetInt("total_online_play")+1);
        }

        if(!PlayerPrefs.HasKey("milestone_target"))
        {
            PlayerPrefs.SetInt("milestone_target",10);
        }

        //
        

        if(FirebaseHandler.instance!=null)
        {
            FirebaseHandler.instance.TrackNumberOfPlayers(totalPlayers);
            FirebaseHandler.instance.TrackNumberOfGamesPlayed(matchPlayedTillNow);
            FirebaseHandler.instance.TrackGamesPlayedOffline(PlayerPrefs.GetInt("total_offline_play"));
            FirebaseHandler.instance.TrackGamesPlayedOffline(PlayerPrefs.GetInt("total_online_play"));
            //
            if(matchPlayedTillNow>=PlayerPrefs.GetInt("milestone_target"))
            {
                FirebaseHandler.instance.TrackSessionMileStone();
                PlayerPrefs.SetInt("milestone_target",PlayerPrefs.GetInt("milestone_target")+10);
            }
        }

        if(AnalyticsTracker.instance!=null)
        {
            AnalyticsTracker.instance.TrackNumberOfGamesPlayed(matchPlayedTillNow);
            AnalyticsTracker.instance.TrackSessionAfterGameStart();
        }
        gameStartTime=(ulong)System.DateTime.Now.Ticks;
    }

    #region updating rank
    public void UpdateFirstRank(int id,string playerName,int botValue,float chance)
    {
        lbInfoList[0].nameText.text=playerName;
        lbInfoList[0].scoreText.text="Winner";
        if(botValue==0)
        {
            lbInfoList[0].iconImage.sprite=playerIcons[id];
            HandleGameEnd("First");
        }
        else
        {
            lbInfoList[0].iconImage.sprite=botIcons[id];
        }
    }

    public void UpdateSecondRank(int id,string playerName,int botValue,float chance)
    {
        lbInfoList[1].nameText.text=playerName;
        lbInfoList[1].scoreText.text=chance.ToString("F2")+"%";;
        if(botValue==0)
        {
            lbInfoList[1].iconImage.sprite=playerIcons[id];
            HandleGameEnd("Second");
        }
        else
        {
           lbInfoList[1].iconImage.sprite=botIcons[id];
        }
    }

    public void UpdateThirdRank(int id,string playerName,int botValue,float chance)
    {
        lbInfoList[2].nameText.text=playerName;
        lbInfoList[2].scoreText.text=chance.ToString("F2")+"%";;
        if(botValue==0)
        {
            lbInfoList[2].iconImage.sprite=playerIcons[id];
            HandleGameEnd("Third");
        }
        else
        {
            lbInfoList[2].iconImage.sprite=botIcons[id];
        }
    }

    public void UpdateFourthRank(int id,string playerName,int botValue,float chance)
    {
        lbInfoList[3].nameText.text=playerName;
        lbInfoList[3].scoreText.text=chance.ToString("F2")+"%";;
        if(botValue==0)
        {
            lbInfoList[3].iconImage.sprite=playerIcons[id];
            HandleGameEnd("Fourth");
        }
        else
        {
            lbInfoList[3].iconImage.sprite=botIcons[id];
        }
    }

    void HandleGameEnd(string gameRank)
    {
        if(!rankShown)
        {
            ulong newDiff=(ulong)System.DateTime.Now.Ticks-gameStartTime;
            ulong spentTimeSpan=newDiff/TimeSpan.TicksPerMillisecond;
            float timeSpent=(float)spentTimeSpan/1000f;
            float sessionTime=timeSpent/60f;
            
            if(FirebaseHandler.instance!=null)
            {
                FirebaseHandler.instance.TrackGameEnd(sessionTime.ToString("F2"),gameRank,"online");
            }
            rankShown=true;
        }
    }
    #endregion

    #region Leaderboard 
    public void ShowLeaderBoardUI(int lbValue)
    {
        leaderboardUI.SetActive(true);
        if(lbValue==0)
        {
            continueBtn.SetActive(true);
            otherBtn.SetActive(false);
        }
        else
        {
            continueBtn.SetActive(false);
            otherBtn.SetActive(true);
        }
        leaderboardUI.GetComponent<Animator>().SetBool("showLB",true);
    }
    #endregion

    #region button events
    public void Continue()
    {   
        leaderboardUI.GetComponent<Animator>().SetBool("showLB",false);
        StartCoroutine(WaitBeforeClosingLeaderboard());
    }

    IEnumerator WaitBeforeClosingLeaderboard()
    {
        yield return new WaitForSeconds(2f);
        leaderboardUI.SetActive(false);
    }
    
    public void Home()
    {
        TimeForRewardVideoAds();
        SceneManager.LoadScene("Home");
    }

    public void Restart()
    {
        TimeForRewardVideoAds();
        SceneManager.LoadScene("Ludo");
    }

    void TimeForRewardVideoAds()
    {
        ConsentManager.ConsentManagerDemo.Scripts.AppodealDemo demo=new ConsentManager.ConsentManagerDemo.Scripts.AppodealDemo();
        demo.TryToShowCachedAds(1);
    }

    public void Exit()
    {
        Application.Quit();
    }
    #endregion

    #region Rating
    public void ShowRateUsUI()
    {
        /*
        if(PlayerPrefs.GetInt("Ludo_rateus_value")==0)
        {
            if(GameDataHolder.instance!=null)
            {
                if(GameDataHolder.instance.rateUsShownCounter>=2&&!GameDataHolder.instance.rateusShown)
                {
                    rateusUI.SetActive(true);
                }
            }
        }
        */
    }

    public void RateUsClicked()
    {
        if(string.IsNullOrEmpty(appUrl))
        {
            if(Application.internetReachability != NetworkReachability.NotReachable)
            {
                Application.OpenURL(appUrl);
                PlayerPrefs.SetInt("Ludo_rateus_value",1);
            }
        }

        if(GameDataHolder.instance!=null)
        {
            GameDataHolder.instance.rateusShown=true;
        }
        rateusUI.SetActive(false);
    }

    public void RemindMeLaterClicked()
    {
        rateusUI.SetActive(false);
        if(GameDataHolder.instance!=null)
        {
            GameDataHolder.instance.rateusShown=true;
        }
    }

    #endregion
}
