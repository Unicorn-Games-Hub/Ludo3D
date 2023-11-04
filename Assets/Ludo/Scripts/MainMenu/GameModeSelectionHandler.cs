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
    string[] nameArray = { "James", "Emma", "Amelia", "Ryan", "Mason", "Jacob", "Owen", "Nathan", "Benjamin", "Liam", "Riley", "Alexander",
        "Cameron","Abigail","Tyler","Thomas",
        "Aaradhya","Alisha","Amoli","Anaisha","Drishti","Ela","Geetika","Reddy","Jaspreet" ,"Yadav","Puja","Binita","Chaaya","Saanvi"};


    public Sprite[] allFlags;
   public void UpdatePlayerSprite(int playIndex)
   {

        myMannager.Instance.flags.Clear();
        myMannager.Instance.isMultiPlayer = false;
        myMannager.Instance.myplayerIndex = 0;
        myMannager.Instance.names.Clear();

       int randomPlayer=Random.Range(0,playerUI.Length);
       int tempPlayerIndex=0;
       for(int i=0;i<playerUI.Length;i++)
       {
           if(playIndex==0)
           {
               tempPlayerIndex=0;
               playerUI[i].GetComponent<Image>().sprite=playerIndicator[i];
                playerUI[i].GetComponent<Button>().interactable = true;
               playerUI[i].GetComponent<PlayerSelector>().nameText.text="Human";
                playerUI[i].transform.GetChild(1).gameObject.SetActive(false);
                myMannager.Instance.isMultiPlayer = false;

            }
            else
           {

                if (playIndex == 2)
                {
                   randomPlayer = 0;
                    if (i == 0)
                    {
                        playerUI[i].GetComponent<Image>().sprite = playerIndicator[0];
                        playerUI[i].GetComponent<Button>().interactable = false;
                        tempPlayerIndex = 0;
                        playerUI[i].GetComponent<PlayerSelector>().nameText.text = "You";
                        playerUI[i].transform.GetChild(1).gameObject.SetActive(false);

                        myMannager.Instance.myplayerIndex = i;
                        myMannager.Instance.names.Add("You");
                        myMannager.Instance.flags.Add(null);

                    }
                    else
                    {
                        tempPlayerIndex = 1;
                        playerUI[i].GetComponent<Image>().sprite = playerIndicator[i];
                        playerUI[i].GetComponent<Button>().interactable = false;
                        int namNum = Random.Range(0, nameArray.Length);
                        playerUI[i].GetComponent<PlayerSelector>().nameText.text = nameArray[namNum];

                        playerUI[i].transform.GetChild(1).gameObject.SetActive(true);
                        int flagnum = Random.Range(0, allFlags.Length);
                        playerUI[i].transform.GetChild(1).GetComponent<Image>().sprite=allFlags[flagnum];
                        myMannager.Instance.names.Add(nameArray[namNum]);
                        myMannager.Instance.flags.Add(allFlags[flagnum]);
                    }
                    myMannager.Instance.isMultiPlayer = true;
                   


                }
                else
                {
                    myMannager.Instance.isMultiPlayer = false;

                    if (i == randomPlayer)
                    {
                        playerUI[randomPlayer].GetComponent<Image>().sprite = playerIndicator[randomPlayer];
                        playerUI[randomPlayer].GetComponent<Button>().interactable = true;

                        tempPlayerIndex = 0;
                        playerUI[i].GetComponent<PlayerSelector>().nameText.text = "Human";
                        playerUI[i].transform.GetChild(1).gameObject.SetActive(false);

                    }
                    else
                    {
                        tempPlayerIndex = 1;
                        playerUI[i].GetComponent<Image>().sprite = botIndicator[i];
                        playerUI[i].GetComponent<Button>().interactable = true;

                        playerUI[i].GetComponent<PlayerSelector>().nameText.text = "Bot";
                        playerUI[i].transform.GetChild(1).gameObject.SetActive(false);

                    }
                }
           }
            GameDataHolder.instance.playerIndex[i]=tempPlayerIndex;
           playerUI[i].GetComponent<PlayerSelector>().playerID=tempPlayerIndex;
       }
        if (playIndex != 2)
        {

           // flagsOff();
        }
   }
    void flagsOff()
    {
        for (int i = 0; i < playerUI.Length; i++)
        {

        playerUI[i].transform.GetChild(1).gameObject.SetActive(false);
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
