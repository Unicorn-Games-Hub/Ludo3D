using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAnalyticsSDK;

public class GameAnalyticsHandler : MonoBehaviour
{
    public static GameAnalyticsHandler instance;

    void Start()
    {
        if(instance!=null)
        {
            return;
        }
        else
        {
            instance=this;
        }
        GameAnalytics.Initialize();
    }

    #region Events to track

    //board art style
    public void TrackBoardArtStyle(string artName)
    {
        GameAnalytics.NewDesignEvent ("board_art_style:"+artName);
    }

    //board type 2d 3d
    public void TrackBoardType(string bType)
    {
        GameAnalytics.NewDesignEvent ("board_type:"+bType);
    }

    //player type coin,character
    public void TrackPlayerType(string pType)
    {
        GameAnalytics.NewDesignEvent ("PlayerType:"+pType);
    }

    //home entering on cuttting
    public void TrackHomeEntering(string enteringStatus)
    {
        GameAnalytics.NewDesignEvent ("Home_entering:"+enteringStatus);
    }

    //punishment on 3 consecutive roll
    public void TrackPunishment(string pStatus)
    {
        GameAnalytics.NewDesignEvent ("Punishment:"+pStatus);
    }

    //auto bring first coin out
    public void TrackAutoBringOfFirstCoin(string autoBringStatus)
    {
        GameAnalytics.NewDesignEvent ("FirstCoin_auto_Out:"+autoBringStatus);
    }

    //smooth dice movement
    public void TrackSoothDiceOnTurnChange(string diceMovementStatus)
    {
        GameAnalytics.NewDesignEvent ("Dice_movement"+diceMovementStatus);
    }  

    //cut scene animation
    public void TrackCutSceneAnimation(string cutSceneValue)
    {
       GameAnalytics.NewDesignEvent("cutscene_animation:"+cutSceneValue);
    }

    //interstitial ads
    public void TrackTotalInterstitialAds(int totalInterstitialAds)
    {
        GameAnalytics.NewDesignEvent("interstitial_ads", (float)totalInterstitialAds);
    }

    //reward video ads
    public void TrackWatchedVideoAds(int totalAdsWatched)
    {
       GameAnalytics.NewDesignEvent("rewarded_video_ads", (float)totalAdsWatched);
    }

    //total games played
    public void TrackTotalGamesPlayed(int noOfGames)
    {
        GameAnalytics.NewDesignEvent("total_games_played", (float)noOfGames);
    }

    public void TrackSessionStart()
    {
       GameAnalytics.StartSession();
    }

    public void TrackSessionEnd()
    {
        GameAnalytics.EndSession();
    }

    //average time spent on game
    public void TrackAverageTimeSpent()
    {
        GameAnalytics.NewDesignEvent("average_time_spent", 200f);
    }


    public void TrackMaxGamePlayed(int totalGamesPlayed)
    {
        if(totalGamesPlayed>=10)
        {
            
        }
    }

    public void TrackMaxTimeSpent(float timeSpent)
    {
        if(timeSpent>=10f)
        {

        }
    }

    #endregion

    #region  crosspromotion tracking
    public void TrackCorsspromoShown()
    {
        GameAnalytics.NewDesignEvent("Crosspromo_shown:true");
    }

    public void TrackCrossPromoClicked()
    {
        GameAnalytics.NewDesignEvent("Crosspromo_clicked:true");
    }
    #endregion
}
