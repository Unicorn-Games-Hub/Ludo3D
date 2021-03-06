using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IngameUIHandler : MonoBehaviour
{
    public GameObject settingsUI;
    public GameObject permissionUI;

    public void ShowSettingsUI()
    {
        settingsUI.SetActive(true);
        settingsUI.GetComponent<Animator>().SetBool("settings",true);
        PlayClickSound();
    }

    public void CloseSettingsUI()
    {
        //settingsUI.SetActive(false);
        settingsUI.GetComponent<Animator>().SetBool("settings",false);
        PlayClickSound();
    }

    public void GoToHome()
    {
        permissionUI.SetActive(true);
        permissionUI.GetComponent<Animator>().SetBool("askExit",true);
        PlayClickSound();
    }

    public void CancleGameExit()
    {
        permissionUI.GetComponent<Animator>().SetBool("askExit",false);
        PlayClickSound();
        //permissionUI.SetActive(false);
    }

    public void Exit()
    {
        PlayClickSound();
        if(AnalyticsTracker.instance!=null)
        {
            AnalyticsTracker.instance.TrackSessionEndAfterGameExit();
        }
        SceneManager.LoadScene("Home");
    }

    void PlayClickSound()
    {
        if(GameAudioHandler.instance!=null)
        {
            GameAudioHandler.instance.PlayButtonClickedSound();
        }
    }
}
