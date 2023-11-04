using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnboardingUIHandler : MonoBehaviour
{
    //
    private Vector3 initialMousePos,finalMousePos;
    private bool isSwipeStarted=false;
    private bool shiftUI=false;
    private bool resetShift=false;
    private float shiftThreshold=0.2f;
    private float targetanchorPos=0f;
    //
    public float nextUiXpos=450f;
    public RectTransform onboardUIRect;
    public float swipeSpeed=20f;

    private int swipeIndex=0;

    float resetPos=0f;

    private bool isShifted=false;

    private Vector3 onboardUiPos;

    [Header("Boards")]
    public GameObject[] boardCheckMarks;
    public GameObject artStyleBtn;

    [Header("Players")]
    public GameObject[] playerCheckMarks;
    public Button cutSceneAnimBtn;
    public Sprite[] cutSceneSprite;

    [Header("Indicators")]
    public GameObject[] indicatiors;

    void Start()
    {
        onboardUiPos=onboardUIRect.anchoredPosition;
        UpdateBoardCheckMark();
        UpdatePlayerCheckMark();
        UpdateCutSceneSprite();
        UpdateArtStyleSprite();
    }
  
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            initialMousePos=Camera.main.ScreenToViewportPoint(Input.mousePosition);
            isSwipeStarted=true;
        }
       
        else if(Input.GetMouseButtonUp(0)&&isSwipeStarted)
        {
            finalMousePos=Camera.main.ScreenToViewportPoint(Input.mousePosition);
            Vector2 swipeDelta=(finalMousePos-initialMousePos);
            if(swipeDelta.magnitude>shiftThreshold)
            {
                if(swipeDelta.x>0f&&swipeIndex==1)
                {
                    targetanchorPos=0f;
                    shiftUI=true;
                    swipeIndex=0;
                }
                else if(swipeDelta.x<0f&&swipeIndex==0)
                {
                    targetanchorPos=-nextUiXpos;
                    shiftUI=true;
                    swipeIndex=1;
                }
            }
           
            UpdateIndicator(swipeIndex);
            isSwipeStarted=false;
        }

        // if(isSwipeStarted)
        // {
        //     finalMousePos=Camera.main.ScreenToViewportPoint(Input.mousePosition);
        //     Vector2 swipeDelta=(finalMousePos-initialMousePos);
        //     onboardUIRect.anchoredPosition=Vector3.Lerp(onboardUIRect.anchoredPosition,new Vector3(swipeDelta.x,0f,0f)*50f,Time.deltaTime*swipeSpeed);
        // }

        if(shiftUI)
        {
            onboardUIRect.anchoredPosition=Vector3.Lerp(onboardUIRect.anchoredPosition,new Vector3(targetanchorPos,0f,0f),Time.deltaTime*swipeSpeed);
            if(Vector3.Distance(onboardUIRect.anchoredPosition,new Vector3(targetanchorPos,0f,0f))<0.5f)
            {
                isShifted=true;
                shiftUI=false;
            }
        }

        if(resetShift)
        {
            onboardUIRect.anchoredPosition=Vector3.Lerp(onboardUIRect.anchoredPosition,new Vector3(onboardUiPos.x,0f,0f),Time.deltaTime*swipeSpeed); 
            if(Vector3.Distance(onboardUIRect.anchoredPosition,new Vector3(0f,0f,0f))<0.5f)
            {
                resetShift=false;
            }
        }
    }
   
    #region boardStyle
    public void ChooseLudoBoard(int boardId)
    {
        PlayerPrefs.SetInt("LudoBoard-Type",boardId);
        UpdateBoardCheckMark();
        //if(AnalyticsTracker.instance!=null)
        //{
        //    AnalyticsTracker.instance.TrackLudoBoard(PlayerPrefs.GetInt("LudoBoard-Type"));
        //}

        if(HomeMenuHandler.instance!=null)
        {
            HomeMenuHandler.instance.PlayButtonClickSound();
        }
    }

    void UpdateBoardCheckMark()
    {
        for(int i=0;i<boardCheckMarks.Length;i++)
        {
            boardCheckMarks[i].SetActive(false);
        }
        boardCheckMarks[PlayerPrefs.GetInt("LudoBoard-Type")].SetActive(true);
    }
    #endregion

    #region player Type
    public void ChooseLudoPlayer(int charId)
    {
        PlayerPrefs.SetInt("LudoPlayer-Type",charId);
        UpdatePlayerCheckMark();
        //if(AnalyticsTracker.instance!=null)
        //{
        //    AnalyticsTracker.instance.TrackLudoPlayer(PlayerPrefs.GetInt("LudoPlayer-Type"));
        //}

        if(HomeMenuHandler.instance!=null)
        {
            HomeMenuHandler.instance.PlayButtonClickSound();
        }
    }

    void UpdatePlayerCheckMark()
    {
        for(int i=0;i<playerCheckMarks.Length;i++)
        {
            playerCheckMarks[i].SetActive(false);
        }
        playerCheckMarks[PlayerPrefs.GetInt("LudoPlayer-Type")].SetActive(true);

        if(PlayerPrefs.GetInt("LudoPlayer-Type")==0)
        {
            cutSceneAnimBtn.interactable=true;
        }
        else
        {
            cutSceneAnimBtn.interactable=false;
        }
    }
    #endregion

    #region Cut Scene Btn
    public void HandleCutSceneAnimBtn()
    {
        if(PlayerPrefs.GetInt("Ludo-CutScene")==0)
        {
            PlayerPrefs.SetInt("Ludo-CutScene",1);
        }
        else 
        {
            PlayerPrefs.SetInt("Ludo-CutScene",0);
        }
        UpdateCutSceneSprite();

        //if(AnalyticsTracker.instance!=null)
        //{
        //    AnalyticsTracker.instance.TrackCutSceneAnimationStatus(PlayerPrefs.GetInt("Ludo-CutScene"));
        //}

        if(HomeMenuHandler.instance!=null)
        {
            HomeMenuHandler.instance.PlayButtonClickSound();
        }
    }
    void UpdateCutSceneSprite()
    {
        cutSceneAnimBtn.GetComponent<Image>().sprite=cutSceneSprite[PlayerPrefs.GetInt("Ludo-CutScene")];
    }
    #endregion

    #region Indicator
    void UpdateIndicator(int indicatorIndex)
    {
        for(int i=0;i<indicatiors.Length;i++)
        {
            indicatiors[i].SetActive(false);
        }
        indicatiors[indicatorIndex].SetActive(true);
    }
    #endregion

    #region ArtStyle selection
   
    public void UpdateBoardArtStyle()
    {
        if(PlayerPrefs.GetInt("ludo_board_artStyle")==0)
        {
            PlayerPrefs.SetInt("ludo_board_artStyle",1);
        }
        else
        {
            PlayerPrefs.SetInt("ludo_board_artStyle",0);
        }
        UpdateArtStyleSprite();

        //if(AnalyticsTracker.instance!=null)
        //{
        //    AnalyticsTracker.instance.TrackLudoArtStyle(PlayerPrefs.GetInt("ludo_board_artStyle"));
        //}
    }

    void UpdateArtStyleSprite()
    {
        if(!PlayerPrefs.HasKey("ludo_board_artStyle"))
        {
            PlayerPrefs.SetInt("ludo_board_artStyle",0);
        }
        artStyleBtn.GetComponent<Image>().sprite=cutSceneSprite[PlayerPrefs.GetInt("ludo_board_artStyle")];
    }
    #endregion
}
