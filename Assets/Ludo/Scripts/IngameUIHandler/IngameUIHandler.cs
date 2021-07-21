using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IngameUIHandler : MonoBehaviour
{
    public GameObject settingsUI;
    [SerializeField]private string sceneName;

    public void ShowSettingsUI()
    {
        settingsUI.SetActive(true);
    }

    public void CloseSettingsUI()
    {
        settingsUI.SetActive(false);
    }

    public void GoToHome()
    {
        if(AnalyticsTracker.instance!=null)
        {
            AnalyticsTracker.instance.TrackSessionEndAfterGameExit();
        }
        SceneManager.LoadScene(sceneName);
    }
}
