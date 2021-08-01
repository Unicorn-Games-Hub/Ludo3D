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

    [Header("Icons")]
    public Sprite[] playerIcons;
    public Sprite[] botIcons;

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

    #region updating rank
    public void UpdateFirstRank(int id,string playerName,int botValue,float chance)
    {
        lbInfoList[0].nameText.text=playerName;
        lbInfoList[0].scoreText.text="Winner";
        if(botValue==0)
        {
            lbInfoList[0].iconImage.sprite=playerIcons[id];
        }
        else
        {
            lbInfoList[0].iconImage.sprite=botIcons[id];
        }
    }

    public void UpdateSecondRank(int id,string playerName,int botValue,float chance)
    {
        lbInfoList[1].nameText.text=playerName;
        lbInfoList[1].scoreText.text=chance.ToString("F2")+"%";;
        if(botValue==0)
        {
            lbInfoList[1].iconImage.sprite=playerIcons[id];
        }
        else
        {
           lbInfoList[1].iconImage.sprite=botIcons[id];
        }
    }

    public void UpdateThirdRank(int id,string playerName,int botValue,float chance)
    {
        lbInfoList[2].nameText.text=playerName;
        lbInfoList[2].scoreText.text=chance.ToString("F2")+"%";;
        if(botValue==0)
        {
            lbInfoList[2].iconImage.sprite=playerIcons[id];
        }
        else
        {
            lbInfoList[2].iconImage.sprite=botIcons[id];
        }
    }

    public void UpdateFourthRank(int id,string playerName,int botValue,float chance)
    {
        lbInfoList[3].nameText.text=playerName;
        lbInfoList[3].scoreText.text=chance.ToString("F2")+"%";;
        if(botValue==0)
        {
            lbInfoList[3].iconImage.sprite=playerIcons[id];
        }
        else
        {
            lbInfoList[3].iconImage.sprite=botIcons[id];
        }
    }
    #endregion

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
            ConsentManager.ConsentManagerDemo.Scripts.AppodealDemo demo=new ConsentManager.ConsentManagerDemo.Scripts.AppodealDemo();
            demo.showRewardedVideo();
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
