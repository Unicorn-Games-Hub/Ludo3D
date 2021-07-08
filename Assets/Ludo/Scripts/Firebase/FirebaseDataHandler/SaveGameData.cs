using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveGameData : MonoBehaviour
{
    public InputField messageInput;
    public GameObject[] radioBtns;
    public GameObject[] checkBoxes;
    private int radioBtnValue=0;
    private int checkBoxBtnValue=0;
    
    public int totalWins=0;

    void Start()
    {
        UpdateRadioBtn();
        UpdateCheckBox();
    }

    #region Radio btn
    void UpdateRadioBtn()
    {
        for(int i=0;i<radioBtns.Length;i++)
        {
            radioBtns[i].transform.GetChild(0).gameObject.SetActive(false);
        }
        radioBtns[radioBtnValue].transform.GetChild(0).gameObject.SetActive(true);
    }

    public void ChooseRadioBtnValue(int value)
    {
        radioBtnValue=value;
        UpdateRadioBtn();
    }
    #endregion

    #region CheckBox
    void UpdateCheckBox()
    {
        for(int i=0;i<checkBoxes.Length;i++)
        {
            checkBoxes[i].transform.GetChild(0).gameObject.SetActive(false);
        }
        checkBoxes[checkBoxBtnValue].transform.GetChild(0).gameObject.SetActive(true);
    }

    public void ChooseCheckBoxBtnValue(int value)
    {
        checkBoxBtnValue=value;
        UpdateCheckBox();
    }
    #endregion

    public void SubmitData()
    {
        string messageToSubmit=messageInput.text;
        if(string.IsNullOrEmpty(messageToSubmit))
        {
            messageToSubmit="Unicorn Games LLC.";
        }
        if(FirebaseDataHandler.instance!=null)
        {
            FirebaseDataHandler.instance.SubmitGameData(messageToSubmit,totalWins,radioBtnValue,checkBoxBtnValue);
        }
    }
}
