using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HomeMenuHandler : MonoBehaviour
{
    [Header("HomeScreen UI")]
    public GameObject homeScreenUI;
    public GameObject homeUI;
    public GameObject settigsUI;
    public GameObject gameModeUI;

    private RectTransform uiToMove=null;
    private float moveSpeed=10f;

    void Start()
    {

    }

    void Update()
    {
        if(uiToMove!=null)
        {
            homeScreenUI.GetComponent<RectTransform>().anchoredPosition=Vector3.Lerp(homeScreenUI.GetComponent<RectTransform>().anchoredPosition,-uiToMove.anchoredPosition,Time.deltaTime*moveSpeed);
        }
    }

    #region Home button events
    public void PlayAgainstHuman()
    {
        ShowGameModeUI(0);
    }

    public void PlayAgainstBot()
    {
        ShowGameModeUI(1);
    }

    public void ShowSettingsUI()
    {
        settigsUI.SetActive(true);
        uiToMove=settigsUI.GetComponent<RectTransform>();
    }

    public void CloseTheGame()
    {
        Application.Quit();
    }
    #endregion

    #region Settings buttons events
    public void CloseSettingsUI()
    {
        uiToMove=homeUI.GetComponent<RectTransform>();
    }
    #endregion

    #region Game Mmode Selection
    void ShowGameModeUI(int modeIndex)
    {
        uiToMove=gameModeUI.GetComponent<RectTransform>();
        GameModeSelectionHandler.instance.UpdatePlayerSprite(modeIndex);
    }
    public void CloseGameModeUI()
    {
        uiToMove=homeUI.GetComponent<RectTransform>();
    }

    public void StartTheGame()
    {
        //lets not start the game if all players are bot
        int noOfBots=0;
        if(GameDataHolder.instance!=null)
        {
            for(int i=0;i<GameDataHolder.instance.playerIndex.Length;i++)
            {
                if(GameDataHolder.instance.playerIndex[i]==1)
                {
                    noOfBots++;
                }
            }
        }

        if(noOfBots!=GameDataHolder.instance.playerIndex.Length)
        {
            SceneManager.LoadScene("Ludo");
        }
        else
        {
            Debug.Log("Game cannot be started with all bots!");
        }
    }
    #endregion
}
