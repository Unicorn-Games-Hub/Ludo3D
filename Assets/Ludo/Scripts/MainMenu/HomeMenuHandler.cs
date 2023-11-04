using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;


public class HomeMenuHandler : MonoBehaviour
{
    public static HomeMenuHandler instance;

    [Header("HomeScreen UI")]
    public GameObject homeScreenUI;
    public GameObject homeUI;
    public GameObject settigsUI;
    public GameObject gameModeUI;

    public GameObject permissionUI;

    private RectTransform uiToMove=null;
    private float moveSpeed=10f;

    public Button[] avaliablePlayers;

    //
    public Animator homeAnim;

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

   [Header("Game Audio")]
   public AudioSource gameAudio;
   public AudioClip btnClickSound;

    public GameObject loadingscreen;

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
    static bool check;
    void Start()
    {


        if (!check)
        {
            check = true;
            loadingscreen.SetActive(true);
            loadingscreen.transform.GetChild(2).GetChild(0).GetComponent<Image>().DOFillAmount(1, 5).OnComplete(()=> {
                loadingscreen.SetActive(false);

            });
        }


        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        
        //4 players at default
        UpdateSelectionUI(4);
        if (FirebaseHandler.instance != null)
        {
            FirebaseHandler.instance.TrackOpenFortheFirstTime();
        }
        onboardingUI.SetActive(false);
        permissionUI.SetActive(false);


       
          //  ADScript.Instance.ShowBanner();
        
    }

    //void Update()
    //{
    //    if (uiToMove != null)
    //    {
    //        homeScreenUI.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(homeScreenUI.GetComponent<RectTransform>().anchoredPosition, -uiToMove.anchoredPosition, Time.deltaTime * moveSpeed);
    //    }
    //}

    #region Home button events
    public void PlayAgainstHuman()
    {
        PlayerPrefs.SetInt("total_against_human",PlayerPrefs.GetInt("total_against_human")+1);
        modeIndex=0;
        ShowGameModeUI();
        PlayButtonClickSound();
    }

    public void PlayAgainstBot()
    {
        
        //ADScript.Instance.ShowIntestitial();
        PlayerPrefs.SetInt("total_against_cpu",PlayerPrefs.GetInt("total_against_cpu")+1);
        modeIndex=1;
        ShowGameModeUI();
        PlayButtonClickSound();
    }

    public GameObject conectionLost, Connecting;
    public void PlayAgainstMultiplayer()
    {

        //ADScript.Instance.ShowIntestitial();
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            conectionLost.SetActive(true);
            conectionLost.GetComponent<DOTweenAnimation>().DORestart();
            Invoke("offConectionlosttxt", 2f);

        }
        else
        {
            Connecting.SetActive(true);
            //Connecting.GetComponent<DOTweenAnimation>().DORestart();
            Invoke("offConectingtxt", Random.Range(5,10));

        }


       
    }
    void offConectionlosttxt()
    {
        conectionLost.SetActive(false);

    }
    void offConectingtxt()
    {

        Connecting.SetActive(false);

        PlayerPrefs.SetInt("total_against_cpu", PlayerPrefs.GetInt("total_against_cpu") + 1);
        modeIndex = 2;
        ShowGameModeUI();
        PlayButtonClickSound();

    }
    public void ShowSettingsUI()
    {
        PlayerPrefs.SetInt("total_settings_clicked",PlayerPrefs.GetInt("total_settings_clicked")+1);
        settigsUI.GetComponent<Animator>().SetBool("settings_show",true);
        //settigsUI.SetActive(true);
        uiToMove=settigsUI.GetComponent<RectTransform>();
        if (FirebaseHandler.instance != null)
        {
            FirebaseHandler.instance.TrackSettings();
        }
        PlayButtonClickSound();
    }

    #endregion

    #region Settings buttons events
    public void CloseSettingsUI()
    {
        settigsUI.GetComponent<Animator>().SetBool("settings_show",false);
        //uiToMove=homeUI.GetComponent<RectTransform>();
        PlayButtonClickSound();
    }
    #endregion

    #region Game Mmode Selection
    //set 4 players as default
    public void SelectPlayersNumber(int noOfPlayes)
    {
        UpdateSelectionUI(noOfPlayes);
        PlayButtonClickSound();
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
           // avaliablePlayers[i].interactable=false;
            avaliablePlayers[i].gameObject.SetActive(false);
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
            //avaliablePlayers[playerIds[i]].interactable=true;
            avaliablePlayers[playerIds[i]].gameObject.SetActive(true);

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
        homeAnim.SetBool("showSelection",true);
        //uiToMove=gameModeUI.GetComponent<RectTransform>();
        GameModeSelectionHandler.instance.UpdatePlayerSprite(modeIndex);

        if (FirebaseHandler.instance != null)
        {
            if (modeIndex == 0)
            {
                FirebaseHandler.instance.TrackVsHumanPlay();
            }
            else
            {
                if (modeIndex == 2)
                {
                    FirebaseHandler.instance.TrackVsmultiPlay();

                }
                else
                {
                FirebaseHandler.instance.TrackVsBotPlay();

                }
                //
            }
        }
    }
    public void CloseGameModeUI()
    {
        homeAnim.SetBool("showSelection",false);
        uiToMove=homeUI.GetComponent<RectTransform>();
        PlayButtonClickSound();
    }
    static int count;
    public void StartTheGame()
    {
        // if(PlayerPrefs.GetInt("Ludo-Onboarding")==0)
        // {
        //     gameModeUI.SetActive(false);
        //     onboardingUI.SetActive(true);
        // }
        // else
        // {

        if (modeIndex == 2)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                conectionLost.SetActive(true);
                conectionLost.GetComponent<DOTweenAnimation>().DORestart();
                Invoke("offConectionlosttxt", 2f);

            }
            else
            {
                // Connecting.SetActive(true);
                //Connecting.GetComponent<DOTweenAnimation>().DORestart();
                //  Invoke("offConectingtxt", 1f);
                count += 1;
                if (count > 1)
                {
                    if (Gley.MobileAds.API.IsRewardedVideoAvailable())
                    {
                        ADScript.Instance.ShowRewardedVideo();
                    }
                    else
                    {
                        ADScript.Instance.ShowIntestitial();

                    }
                }
                FirebaseHandler.instance.TrackNumberOfGamesPlayed(count);
                Play();
                // }
                PlayButtonClickSound();
            }
        }
        else
        {
            count += 1;
            if (count > 1)
            {
                ADScript.Instance.ShowIntestitial();
            }
            FirebaseHandler.instance.TrackNumberOfGamesPlayed(count);
            Play();
            // }
            PlayButtonClickSound();
        }
    }

    public GameObject rewardedIcon;

    private void Update()
    {
        if (modeIndex == 2&& count > 0)
        {
            if (Gley.MobileAds.API.IsRewardedVideoAvailable())
            {
                if(!rewardedIcon.activeInHierarchy)
                rewardedIcon.SetActive(true);
            }
            else
            {
                if(rewardedIcon.activeInHierarchy)
                rewardedIcon.SetActive(false);

            }
        }
        else
        {
            if (rewardedIcon.activeInHierarchy)
                rewardedIcon.SetActive(false);
        }
    }
    public void CloseOnboardingUI()
    {
        gameModeUI.SetActive(true);
        onboardingUI.SetActive(false);
        PlayButtonClickSound();
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

    #region permission
    public void CloseTheGame()
    {
        HandlePermissionAnimation(true);
        PlayButtonClickSound();
    }

    public void ContinuePlaying()
    {
        HandlePermissionAnimation(false);
        PlayButtonClickSound();
    }

    void HandlePermissionAnimation(bool permissionValue)
    {
        permissionUI.GetComponent<Animator>().SetBool("askPermission",permissionValue);
    }

    public void QuitTheGame()
    {
        ADScript.Instance.ShowIntestitial();

        Application.Quit();
    }
    #endregion

    #region Game Audio
    public void PlayButtonClickSound()
    {
        if(PlayerPrefs.GetInt("Ludo-Sound")==0)
        {
            gameAudio.clip=btnClickSound;
            gameAudio.Play();
        }
    }
    #endregion
}
