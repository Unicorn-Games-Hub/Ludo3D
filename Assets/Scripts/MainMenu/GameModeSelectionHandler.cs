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
       for(int i=0;i<playerUI.Length;i++)
       {
           if(playIndex==0)
           {
               playerUI[i].GetComponent<Image>().sprite=playerIndicator[i];
           }
           else
           {
               if(i==randomPlayer)
               {
                   playerUI[randomPlayer].GetComponent<Image>().sprite=playerIndicator[randomPlayer];
               }
               else
               {
                   playerUI[i].GetComponent<Image>().sprite=botIndicator[i];
               }
           }
           playerUI[i].GetComponent<PlayerSelector>().playerID=playIndex;
       }
   }

   public void ChangePlayerIcon()
   {
       for(int i=0;i<playerUI.Length;i++)
       {
           if(playerUI[i].GetComponent<PlayerSelector>().playerID==0)
           {
               playerUI[i].GetComponent<Image>().sprite=playerIndicator[i];
           }
           else
           {
               playerUI[i].GetComponent<Image>().sprite=botIndicator[i];
           }
           if(GameDataHolder.instance!=null)
           {
               GameDataHolder.instance.playerIndex[i]=playerUI[i].GetComponent<PlayerSelector>().playerID;
           }
       }
   }
}
