using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnalyticsTracker : MonoBehaviour
{
    public static AnalyticsTracker instance;

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

    public void HandleLudoSettings(string settingsName)
    {
        switch(settingsName)
        {
            case "Ludo-CutScene":
            TrackCutSceneAnimationStatus(PlayerPrefs.GetInt("Ludo-CutScene"));
            break;
            case "ludo_board_artStyle":
            TrackLudoArtStyle(PlayerPrefs.GetInt("ludo_board_artStyle"));
            break;
            case "home_on_cut_only":
            break;
            case "punish_on_consecutive_roll":
            break;
            case "auto_bring_firstcoin":
            break;
            case "smooth_dice_movement":
            break;
            default:
            Debug.Log("settings name not found");
            break;
        }
    }

    public void TrackCrossPromotionShown()
    {
        if(GameAnalyticsHandler.instance!=null)
        {
            GameAnalyticsHandler.instance.TrackCorsspromoShown();
        }
    }

    public void TrackCrossPromoLinkClicked()
    {
        if(GameAnalyticsHandler.instance!=null)
        {
            GameAnalyticsHandler.instance.TrackCrossPromoClicked();
        }
    }

    public void TrackGameInstallThroughCrossPromo()
    {
        if(GameAnalyticsHandler.instance!=null)
        {
            
        }
    }

    public void TrackLudoPlayer(int lpType)
    {
        string playerUsed="Coin";
        if(lpType==0)
        {
            playerUsed="Character";
        }
        if(GameAnalyticsHandler.instance!=null)
        {
            GameAnalyticsHandler.instance.TrackPlayerType(playerUsed);
        }
    }

    public void TrackLudoCameraRotation()
    {
        if(GameAnalyticsHandler.instance!=null)
        {
            
        }
    }

    public void TrackLudoBoard(int lbtype)
    {
        string boardType="board_2D";
        if(lbtype==0)
        {
            boardType="board_3D";
        }
       
        if(GameAnalyticsHandler.instance!=null)
        {
            GameAnalyticsHandler.instance.TrackBoardType(boardType);
        }
    }

    public void TrackLudoArtStyle(int artStatus)
    {
        string artStyle="artstyle_metallic";
        if(artStatus==1)
        {
            artStyle="artstyle_default";
        }

        if(GameAnalyticsHandler.instance!=null)
        {
            GameAnalyticsHandler.instance.TrackBoardArtStyle(artStyle);
        }
    }

    public void TrackCutSceneAnimationStatus(int csValue)
    {
        string cutSceneValue="ludo_cutscene_disabled";
        if(csValue==0)
        {
            cutSceneValue="ludo_cutscene_enabled";
        }
        if(GameAnalyticsHandler.instance!=null)
        {
            GameAnalyticsHandler.instance.TrackCutSceneAnimation(cutSceneValue);
        }
    }

    public void TrackHomeEnterCondition(int homeValue)
    {
        string homeStatus="home_withoutCutting_disabled";
        if(homeValue==0)
        {
            homeStatus="home_withoutCutting_enabled";
        }

        if(GameAnalyticsHandler.instance!=null)
        {
            GameAnalyticsHandler.instance.TrackHomeEntering(homeStatus);
        }
    }

    public void TrackPunishOnConsecutiveRoll(int punishValue)
    {
        string punishStatus="consecutive_roll_punishment_enabled";
        if(punishValue==0)
        {
            punishStatus="consecutive_roll_punishment_disabled";
        }

        if(GameAnalyticsHandler.instance!=null)
        {
            GameAnalyticsHandler.instance.TrackPunishment(punishStatus);
        }
    }

    public void TrackFirstCoinAutoBring(int autoBringValue)
    {
        string autoBringStatus="autobring_first_coin_enabled";
        if(autoBringValue==0)
        {
            autoBringStatus="autobring_first_coin_disabled";
        }
        if(GameAnalyticsHandler.instance!=null)
        {
           GameAnalyticsHandler.instance.TrackAutoBringOfFirstCoin(autoBringStatus);
        }
    }

    public void TrackSmoothDiceMovement(int smoothDiceValue)
    {
        string diceMovementStatus="smooth_dice_movement_enabled";
        if(smoothDiceValue==0)
        {
            diceMovementStatus="smooth_dice_movement_disabled";
        }
        if(GameAnalyticsHandler.instance!=null)
        {
           GameAnalyticsHandler.instance.TrackSoothDiceOnTurnChange(diceMovementStatus);
        }
    }

    public void TrackShownInterstitialAds()
    {
        PlayerPrefs.SetInt("ludo-interstitial",PlayerPrefs.GetInt("ludo-interstitial")+1);
        if(GameAnalyticsHandler.instance!=null)
        {
            GameAnalyticsHandler.instance.TrackTotalInterstitialAds(PlayerPrefs.GetInt("ludo-interstitial"));
        }
    }

    public void TrackShownRewardVideoAds()
    {
        PlayerPrefs.SetInt("ludo-rewardVideo",PlayerPrefs.GetInt("ludo-rewardVideo")+1);
        if(GameAnalyticsHandler.instance!=null)
        {
            GameAnalyticsHandler.instance.TrackWatchedVideoAds(PlayerPrefs.GetInt("ludo-rewardVideo"));
        }
    }

    public void TrackNumberOfGamesPlayed(int totalGameNumber)
    {
        if(GameAnalyticsHandler.instance!=null)
        {
            GameAnalyticsHandler.instance.TrackTotalGamesPlayed(totalGameNumber);
        }
    }

    public void TrackSessionAfterGameStart()
    {
        if(GameAnalyticsHandler.instance!=null)
        {
            GameAnalyticsHandler.instance.TrackSessionStart();
        }
    }

    public void TrackSessionEndAfterGameExit()
    {
        if(GameAnalyticsHandler.instance!=null)
        {
            GameAnalyticsHandler.instance.TrackSessionEnd();
        }
    }



    public void TrackTimeSpentOnGame()
    {
        if(GameAnalyticsHandler.instance!=null)
        {
            
        }
    }


    public void TrackLudoAverageTimeSpent()
    {
        if(GameAnalyticsHandler.instance!=null)
        {
            
        }
    }
}
