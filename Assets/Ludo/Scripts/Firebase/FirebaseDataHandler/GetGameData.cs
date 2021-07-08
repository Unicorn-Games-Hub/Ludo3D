using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GetGameData : MonoBehaviour
{
    public Text messageText;
    public Image promoPic;

    public GameObject[] radioBtnUI;
    public GameObject[] checkBoxUI;

    private string appURL;

    public void UpdateGameData(string msg,string appUrl,int radVal,int cVal)
    {
        messageText.text=msg;
        appURL=appUrl;
        for(int i=0;i<radioBtnUI.Length;i++)
        {
            radioBtnUI[i].SetActive(false);
        }
        radioBtnUI[radVal].SetActive(true);
        //
        for(int i=0;i<checkBoxUI.Length;i++)
        {
            checkBoxUI[i].SetActive(false);
        }   
        checkBoxUI[cVal].SetActive(true);
    }

    public void UpdatePicture(Sprite imageSprite)
    {
        promoPic.sprite=imageSprite;
    }


    public void HandleBtnClick()
    {
        if(!string.IsNullOrEmpty(appURL))
        {
            Application.OpenURL(appURL);
        }
    }
}
