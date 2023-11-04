using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTurnTimer : MonoBehaviour
{
    public int Id;
    public Text playerNameText;
    private Image timerIndicator;
    private Image playerImage;
    private float turnTimer=1f;
    private float turnSpeed=10f;
    public bool myturn=false;

    [Header("Player Bot Icons")]
    public Sprite[] playerIcon;
    private int pIndex;

    void Start()
    {
        timerIndicator=GetComponent<Image>();
        playerImage=transform.GetChild(0).GetComponent<Image>();
        if(GameDataHolder.instance!=null)
        {
            pIndex=GameDataHolder.instance.playerIndex[Id];
            playerImage.sprite=playerIcon[0];
            if(pIndex==1)
            {
                playerImage.sprite=playerIcon[1];
                playerNameText.text="Bot";
            }

            if (myMannager.Instance.isMultiPlayer)
            {
                playerImage.sprite = playerIcon[0];

                playerNameText.text=myMannager.Instance.names[Id];
                if (myMannager.Instance.flags[Id] != null)
                {
                    transform.GetChild(1).gameObject.SetActive(true);

                    transform.GetChild(1).GetComponent<Image>().sprite = myMannager.Instance.flags[Id];
                }
                else
                {
                   // myturn = true;
                }


            }
        }
    }

    void Update()
    {
        if(myturn)
        {
            if(turnTimer>0f)
            {
                turnTimer-=Time.deltaTime/turnSpeed;
                timerIndicator.fillAmount=turnTimer;
            }
            else
            {
                turnTimer=1f;
                myturn=false;
            }
        }




    }
}
