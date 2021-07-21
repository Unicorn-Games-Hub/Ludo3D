using UnityEngine;
using UnityEngine.UI;

public class SettingsOption : MonoBehaviour
{
    public Image toggleBtnmage;
    public RectTransform toggleBtn;
    public Sprite[] optionBg;

    public Vector3 onPos=new Vector3(19.5f,0f,0f);
    public Vector3 offPos=new Vector3(-4f,0f,0f);

    public string optionName;

    void Start()
    {
        UpdateOptionSprite();
    }

    public void HandleOptionValue()
    {
        if(PlayerPrefs.GetInt(optionName)==0)
        {
            PlayerPrefs.SetInt(optionName,1);
        }
        else
        {
            PlayerPrefs.SetInt(optionName,0);
        }
        UpdateOptionSprite();

        if(AnalyticsTracker.instance!=null)
        {
            AnalyticsTracker.instance.HandleLudoSettings(optionName);
        }
    }

    void UpdateOptionSprite()
    {
        int toggleValue=PlayerPrefs.GetInt(optionName);
        Vector3 togglePos=Vector3.zero;
        toggleBtnmage.sprite=optionBg[toggleValue];

        if(toggleValue==0)
        {
            togglePos=offPos;
        }
        else
        {
            togglePos=onPos;
        }
        toggleBtn.anchoredPosition=togglePos;
    }




}
