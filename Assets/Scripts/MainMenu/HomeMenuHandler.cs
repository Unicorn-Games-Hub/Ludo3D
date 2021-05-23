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

    public Button[] avaliablePlayers;

    [Header("Number of players")]
    public Sprite[] selectionIndicator;
    public GameObject[] playerSelectionUI;

    //
    [Header("Game start button")]
    public Button startButton;
    private int modeIndex=0;
    private int totalPlayers=0;
    private bool isGameStartValid=false;


    void Start()
    {
        //4 players at default
        UpdateSelectionUI(4);
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
        modeIndex=0;
        ShowGameModeUI();
    }

    public void PlayAgainstBot()
    {
        modeIndex=1;
        ShowGameModeUI();
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
    //set 4 players as default
    public void SelectPlayersNumber(int noOfPlayes)
    {
        UpdateSelectionUI(noOfPlayes);
    }

    void UpdateSelectionUI(int curPlayerNum)
    {
        totalPlayers=curPlayerNum;
        GameObject tempSelectionUI=null;
        for(int i=0;i<playerSelectionUI.Length;i++)
        {
            playerSelectionUI[i].GetComponent<Image>().sprite=selectionIndicator[0];
        }

        tempSelectionUI=playerSelectionUI[curPlayerNum-2];
        tempSelectionUI.GetComponent<Image>().sprite=selectionIndicator[1];

        for(int i=0;i<avaliablePlayers.Length;i++)
        {
            if(i<curPlayerNum)
            {
                if(modeIndex==0)
                {
                    GameDataHolder.instance.playerIndex[i]=0;
                }
                else
                {
                    if(avaliablePlayers[i].GetComponent<PlayerSelector>().playerID>0)
                    {
                        GameDataHolder.instance.playerIndex[i]=1;
                    }
                }
                avaliablePlayers[i].interactable=true;
            }
            else
            {
                avaliablePlayers[i].interactable=false;
                if(GameDataHolder.instance!=null)
                {
                    GameDataHolder.instance.playerIndex[i]=2;
                }
            }
        }
        //lets check game start is valid or not
        HandleStartValidation();
    }

    public void HandleStartValidation()
    {
        startButton.interactable=true;
        int botCount=0;
        for(int i=0;i<totalPlayers;i++)
        {
            if(avaliablePlayers[i].GetComponent<PlayerSelector>().playerID==1)
            {
                botCount++;
            }
        }

        if(botCount==totalPlayers)
        {
            startButton.interactable=false;
            // int tempHuman=Random.Range(0,totalPlayers);
            // GameModeSelectionHandler.instance.MakeItHuman(tempHuman);
        }
    }

    void ShowGameModeUI()
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
        SceneManager.LoadScene("Ludo");
    }
    #endregion
}
