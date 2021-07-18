using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LeaderboardHandler : MonoBehaviour
{
    public static LeaderboardHandler instance;
    public GameObject leaderboardUI;
    public GameObject continueBtn;
    public GameObject otherBtn;

    [System.Serializable]
    public class pInfo
    {
        public Text nameText;
        public Text scoreText;
        public Image iconImage;
    }
    public List<pInfo> lbInfoList=new List<pInfo>();

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

    void Start()
    {
        leaderboardUI.SetActive(false);
    }

    #region Leaderboard 
    public void ShowLeaderBoardUI(int lbValue)
    {
        leaderboardUI.SetActive(true);
        if(lbValue==0)
        {
            continueBtn.SetActive(true);
            otherBtn.SetActive(false);
        }
        else
        {
            continueBtn.SetActive(false);
            otherBtn.SetActive(true); 
        }
    }
    #endregion

    #region button events
    public void Continue()
    {
        leaderboardUI.SetActive(false);
    }

    public void Home()
    {

    }

    public void Restart()
    {

    }

    public void Next()
    {

    }
    #endregion
}
