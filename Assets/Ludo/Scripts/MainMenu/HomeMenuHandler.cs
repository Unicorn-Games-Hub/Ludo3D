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

   private List<int> playerIds=new List<int>();

   [Header("Onboarding UI")]
   public GameObject onboardingUI;

    void Start()
    {
        //4 players at default
        UpdateSelectionUI(4);
        if(FirebaseHandler.instance!=null)
        {
           FirebaseHandler.instance.TrackOpenFortheFirstTime();
        }
        onboardingUI.SetActive(false);
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
        if(FirebaseHandler.instance!=null)
        {
           FirebaseHandler.instance.TrackSettings();
        }
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
        playerIds.Clear();
        totalPlayers=curPlayerNum;
        GameObject tempSelectionUI=null;
        for(int i=0;i<playerSelectionUI.Length;i++)
        {
            playerSelectionUI[i].GetComponent<Image>().sprite=selectionIndicator[0];
        }

        tempSelectionUI=playerSelectionUI[curPlayerNum-2];
        tempSelectionUI.GetComponent<Image>().sprite=selectionIndicator[1];

        switch(curPlayerNum)
        {
            case 2:
            playerIds.Add(0);
            playerIds.Add(2);
            break;
            case 3:
            playerIds.Add(0);
            playerIds.Add(1);
            playerIds.Add(2);
            break;
            case 4:
            playerIds.Add(0);
            playerIds.Add(1);
            playerIds.Add(2);
            playerIds.Add(3);
            break;
            default:
            playerIds.Add(0);
            playerIds.Add(1);
            playerIds.Add(2);
            playerIds.Add(3);
            break;
        }

        for(int i=0;i<avaliablePlayers.Length;i++)
        {
            if(GameDataHolder.instance!=null)
            {
                GameDataHolder.instance.playerIndex[i]=2;
            }
            avaliablePlayers[i].interactable=false;
        }
            
            
            /*
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
            */


        for(int i=0;i<playerIds.Count;i++)
        {
            if(modeIndex==0)
            {
                GameDataHolder.instance.playerIndex[playerIds[i]]=0;
            }
            else
            {
                if(avaliablePlayers[playerIds[i]].GetComponent<PlayerSelector>().playerID>0)
                {
                    GameDataHolder.instance.playerIndex[playerIds[i]]=1;
                }
                else
                {
                    GameDataHolder.instance.playerIndex[playerIds[i]]=0;
                }
            }
            avaliablePlayers[playerIds[i]].interactable=true;
        }

        //lets check game start is valid or not
        HandleStartValidation();
    }

    public void HandleStartValidation()
    {
        startButton.interactable=true;
        int botCount=0;

        for(int i=0;i<GameDataHolder.instance.playerIndex.Length;i++)
        {
            if(GameDataHolder.instance.playerIndex[i]==1)
            {
                botCount++;
            }
        }
       
        if(botCount==totalPlayers)
        {
            startButton.interactable=false;
        }
    }

    void ShowGameModeUI()
    {
        uiToMove=gameModeUI.GetComponent<RectTransform>();
        GameModeSelectionHandler.instance.UpdatePlayerSprite(modeIndex);

        if(FirebaseHandler.instance!=null)
        {
            if(modeIndex==0)
            {
                FirebaseHandler.instance.TrackVsHumanPlay();
            }
            else
            {
                FirebaseHandler.instance.TrackVsBotPlay();
            }
        }
    }
    public void CloseGameModeUI()
    {
        uiToMove=homeUI.GetComponent<RectTransform>();
    }

    public void StartTheGame()
    {
        if(PlayerPrefs.GetInt("Ludo-Onboarding")==0)
        {
            gameModeUI.SetActive(false);
            onboardingUI.SetActive(true);
        }
        else
        {
           Play(); 
        }
    }

    public void CloseOnboardingUI()
    {
        gameModeUI.SetActive(true);
        onboardingUI.SetActive(false);
    }

    public void Play()
    {
        if(PlayerPrefs.GetInt("Ludo-Onboarding")==0)
        {
            PlayerPrefs.SetInt("Ludo-Onboarding",1);
        }
        
        SceneManager.LoadScene("Ludo");
    }
    #endregion
}
