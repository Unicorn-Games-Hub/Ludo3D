using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUIHandler : MonoBehaviour
{
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
    }
    #endregion
}
