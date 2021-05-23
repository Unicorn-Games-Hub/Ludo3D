using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameModeSelectionHandler : MonoBehaviour
{
    public static GameModeSelectionHandler instance;
    
    public Sprite[] playerIndicator;
    public Sprite[] botIndicator;
    public GameObject[] playerUI;

    public HomeMenuHandler hm;

    void Start()
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

   public void UpdatePlayerSprite(int playIndex)
   {
       int randomPlayer=Random.Range(0,playerUI.Length);
       int tempPlayerIndex=0;
       for(int i=0;i<playerUI.Length;i++)
       {
           if(playIndex==0)
           {
               tempPlayerIndex=0;
               playerUI[i].GetComponent<Image>().sprite=playerIndicator[i];
           }
           else
           {
               if(i==randomPlayer)
               {
                    playerUI[randomPlayer].GetComponent<Image>().sprite=playerIndicator[randomPlayer];
                    tempPlayerIndex=0;
               }
               else
               {
                    tempPlayerIndex=1;
                    playerUI[i].GetComponent<Image>().sprite=botIndicator[i];
               }
           }
            GameDataHolder.instance.playerIndex[i]=tempPlayerIndex;
           playerUI[i].GetComponent<PlayerSelector>().playerID=tempPlayerIndex;
       }
   }

   public void ChangePlayerIcon(GameObject clickedBtn)
   {
       PlayerSelector ps=clickedBtn.GetComponent<PlayerSelector>();
       if(ps.playerID==0)
       {
           clickedBtn.GetComponent<Image>().sprite=playerIndicator[ps.colorIndex];
       }
       else if(ps.playerID==1)
       {
           clickedBtn.GetComponent<Image>().sprite=botIndicator[ps.colorIndex];
       }
        if(GameDataHolder.instance!=null)
        {
            GameDataHolder.instance.playerIndex[ps.colorIndex]=ps.playerID;
        }
        //here we will check if game can be started or not
        hm.HandleStartValidation();
   }

   public void MakeItHuman(int id)
   {
        PlayerSelector human=playerUI[id].GetComponent<PlayerSelector>();
        playerUI[id].GetComponent<Image>().sprite=playerIndicator[human.colorIndex];
        GameDataHolder.instance.playerIndex[human.colorIndex]=human.playerID;
   }
}
