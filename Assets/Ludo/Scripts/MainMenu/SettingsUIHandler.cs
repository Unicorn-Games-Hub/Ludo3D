using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUIHandler : MonoBehaviour
{
    [Header("Settings UI")]
    public GameObject[] settingsUI;

    [Header("Ludo Settings")]
    public Transform soundToggleBtn;
    public Transform musicToggleBtn;
    public Transform vibrationToggleBtn;

    [Header("Toggle Text")]
    public Text soundText;
    public Text musicText;
    public Text vibrationText;

    //
    private Vector3 onPos=new Vector3(19.5f,0f,0f);
    private Vector3 offPos=new Vector3(-4f,0f,0f);

    private string OnMessage="ON";
    private string OffMessage="OFF";

    public Sprite[] toggleBgSprite;
    

    void Start()
    {
        UpdateSoundToggle();
        UpdateMusicToggle();
        UpdateVibrationToggle();

        UpdatePlayerSelection();
        UpdateBoardSelection();
        HandleSettingsUI(0);
    }   

    #region Updating toggle and on off value of text
    void UpdateSoundToggle()
    {
        int soundValue=PlayerPrefs.GetInt("Ludo-Sound");
        if(soundValue==0)
        {
            soundToggleBtn.GetChild(0).GetComponent<RectTransform>().anchoredPosition=onPos;
            soundText.text=OnMessage;
        }
        else
        {
            soundText.text=OffMessage;
            soundToggleBtn.GetChild(0).GetComponent<RectTransform>().anchoredPosition=offPos;
        }
        soundToggleBtn.GetComponent<Image>().sprite=toggleBgSprite[soundValue];
    }

    void UpdateMusicToggle()
    {
        int musicValue=PlayerPrefs.GetInt("Ludo-Music");
        if(musicValue==0)
        {
            musicToggleBtn.GetChild(0).GetComponent<RectTransform>().anchoredPosition=onPos;
            musicText.text=OnMessage;
        }
        else
        {
            musicText.text=OffMessage;
            musicToggleBtn.GetChild(0).GetComponent<RectTransform>().anchoredPosition=offPos;
        }
        musicToggleBtn.GetComponent<Image>().sprite=toggleBgSprite[musicValue];

        if(BackgroundMusicHandler.instance!=null)
        {
            BackgroundMusicHandler.instance.UpdateBgMusicStatus();
        }
    }

    void UpdateVibrationToggle()
    {
        int vibrationValue=PlayerPrefs.GetInt("Ludo-Vibration");
        if(vibrationValue==0)
        {
            vibrationToggleBtn.GetChild(0).GetComponent<RectTransform>().anchoredPosition=onPos;
            vibrationText.text=OnMessage;
        }
        else
        {
            vibrationText.text=OffMessage;
            vibrationToggleBtn.GetChild(0).GetComponent<RectTransform>().anchoredPosition=offPos;
        }
        vibrationToggleBtn.GetComponent<Image>().sprite=toggleBgSprite[vibrationValue];

    }
    #endregion

    #region toggle button events
    public void ChangeGameSoundSettings()
    {
        if(PlayerPrefs.GetInt("Ludo-Sound")==0)
        {
            PlayerPrefs.SetInt("Ludo-Sound",1);
        }
        else if(PlayerPrefs.GetInt("Ludo-Sound")==1)
        {
            PlayerPrefs.SetInt("Ludo-Sound",0);
        }
        UpdateSoundToggle();

        if(FirebaseHandler.instance!=null)
        {
            FirebaseHandler.instance.TrackGameSfx(PlayerPrefs.GetInt("Ludo-Sound"));
        }

        if(HomeMenuHandler.instance!=null)
        {
            HomeMenuHandler.instance.PlayButtonClickSound();
        }
    }

    public void ChangeGameMusicSettings()
    {
        if(PlayerPrefs.GetInt("Ludo-Music")==0)
        {
            PlayerPrefs.SetInt("Ludo-Music",1);
        }
        else if(PlayerPrefs.GetInt("Ludo-Music")==1)
        {
            PlayerPrefs.SetInt("Ludo-Music",0);
        }
        UpdateMusicToggle();

        if(FirebaseHandler.instance!=null)
        {
            FirebaseHandler.instance.TrackGameMusic(PlayerPrefs.GetInt("Ludo-Music"));
        }
        if(HomeMenuHandler.instance!=null)
        {
            HomeMenuHandler.instance.PlayButtonClickSound();
        }
    }

    public void ChangeGameVibrationSettings()
    {
        if(PlayerPrefs.GetInt("Ludo-Vibration")==0)
        {
            PlayerPrefs.SetInt("Ludo-Vibration",1);
        }
        else if(PlayerPrefs.GetInt("Ludo-Vibration")==1)
        {
            PlayerPrefs.SetInt("Ludo-Vibration",0);
        }
        UpdateVibrationToggle();

        if(FirebaseHandler.instance!=null)
        {
            FirebaseHandler.instance.TrackGameVibration(PlayerPrefs.GetInt("Ludo-Vibration"));
        }
        if(HomeMenuHandler.instance!=null)
        {
            HomeMenuHandler.instance.PlayButtonClickSound();
        }
    }
    #endregion

    #region Player options
    private string[] playerType={"Character","Coin"};
    private int playerIndexCounter=0;
    public Text playerText;

    public void ChoosePrevChar()
    {
        if(playerIndexCounter==0)
        {
            playerIndexCounter=playerType.Length-1;
        }
        else
        {
            playerIndexCounter--;
        }
        HandlePlayerTypleIndexValue(playerIndexCounter);
    }

    public void ChooseNextChar()
    {
        if(playerIndexCounter<playerType.Length-1)
        {
            playerIndexCounter++;
        }
        else
        {
            playerIndexCounter=0;
        }
        HandlePlayerTypleIndexValue(playerIndexCounter);
    }

    void HandlePlayerTypleIndexValue(int pId)
    {
        PlayerPrefs.SetInt("LudoPlayer-Type",pId);
        UpdatePlayerSelection();
        if(AnalyticsTracker.instance!=null)
        {
            AnalyticsTracker.instance.TrackLudoBoard(PlayerPrefs.GetInt("LudoPlayer-Type"));
        }

        if(HomeMenuHandler.instance!=null)
        {
            HomeMenuHandler.instance.PlayButtonClickSound();
        }
    }

    void UpdatePlayerSelection()
    {
        playerText.text=playerType[PlayerPrefs.GetInt("LudoPlayer-Type")];
    }
    #endregion

    #region Board options
    public Sprite[] boardSprites;
    private int boardIndexCounter=0;
    public Image optionSettingsBoardImage;
    public void ChoosePrevBoard()
    {
       if(boardIndexCounter==0)
       {
           boardIndexCounter=boardSprites.Length-1;
       }
       else
       {
           boardIndexCounter--;
       }
        HandleBoardChangeCount(boardIndexCounter);
    }

    public void ChooseNextBoard()
    {
        if(boardIndexCounter<boardSprites.Length-1)
        {
            boardIndexCounter++;
        }
        else
        {
            boardIndexCounter=0;
        }
        HandleBoardChangeCount(boardIndexCounter);
    }

    void HandleBoardChangeCount(int bId)
    {
        PlayerPrefs.SetInt("LudoBoard-Type",bId);
        UpdateBoardSelection();
        //
        if(AnalyticsTracker.instance!=null)
        {
            AnalyticsTracker.instance.TrackLudoBoard(PlayerPrefs.GetInt("LudoBoard-Type"));
        }

        if(HomeMenuHandler.instance!=null)
        {
            HomeMenuHandler.instance.PlayButtonClickSound();
        }
    }

    void UpdateBoardSelection()
    {
        optionSettingsBoardImage.sprite=boardSprites[PlayerPrefs.GetInt("LudoBoard-Type")];
    }
    #endregion

    #region Settings ui
    public void ShowSettingsUI(int settingsIndex)
    {
        HandleSettingsUI(settingsIndex);
        
        if(HomeMenuHandler.instance!=null)
        {
            HomeMenuHandler.instance.PlayButtonClickSound();
        }
    }

    void HandleSettingsUI(int sId)
    {
        for(int i=0;i<settingsUI.Length;i++)
        {
            settingsUI[i].SetActive(false);
        }
        settingsUI[sId].SetActive(true);
    }
    #endregion
}
