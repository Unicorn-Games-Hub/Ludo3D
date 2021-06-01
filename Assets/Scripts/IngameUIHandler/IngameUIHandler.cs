using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameUIHandler : MonoBehaviour
{
    public GameObject settingsUI;

    public void ShowSettingsUI()
    {
        settingsUI.SetActive(true);
    }

    public void CloseSettingsUI()
    {
        settingsUI.SetActive(false);
    }
}
