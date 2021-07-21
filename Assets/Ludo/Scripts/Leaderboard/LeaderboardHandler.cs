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
        leaderboardUI.GetComponent<Animator>().SetBool("showLB",true);
    }
    #endregion

    #region button events
    public void Continue()
    {   
        leaderboardUI.GetComponent<Animator>().SetBool("showLB",false);
        StartCoroutine(WaitBeforeClosingLeaderboard());
    }

    IEnumerator WaitBeforeClosingLeaderboard()
    {
        yield return new WaitForSeconds(2f);
        leaderboardUI.SetActive(false);
    }

    public void Home()
    {
        SceneManager.LoadScene("Home");
    }

    public void Restart()
    {
        SceneManager.LoadScene("Ludo");
    }

    public void Exit()
    {
        Application.Quit();
    }
    #endregion
}
