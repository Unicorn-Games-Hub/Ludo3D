using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayCamHandler : MonoBehaviour
{
    private Camera gameCam;

    public enum screenMode
    {
        mode3D,
        mode2D
    }
    public screenMode displayMode;

    [Header("Mode 3D")]
    public Vector3 mode3dPos;

    [Header("Mode 2D")]
    public Vector3 mode2dPos;

    [Header("Player Indicator UI")]
    public Transform indicatorContainer;
    private int modeIndex=0;

    [System.Serializable]
    public class indicatorInfo
    {
        public Vector3[] indicatorPos;
    }
    public List<indicatorInfo> infoList=new List<indicatorInfo>();

    public Text modeText;

    [Header("Direction Light Shadow")]
    public Light dirLight;

    void Start()
    {
        gameCam=GetComponent<Camera>();

        //default will be 3D
        if(PlayerPrefs.GetInt("LudoBoard-Type")==0)
        {
            displayMode=screenMode.mode3D;
        }
        else
        {
            displayMode=screenMode.mode2D;
        }
        UpdateDisplayMode();
    }

    public void ChangeDisplayMode()
    {
        if(displayMode==screenMode.mode3D)
        {
            displayMode=screenMode.mode2D;
        }
        else if(displayMode==screenMode.mode2D)
        {
            displayMode=screenMode.mode3D;
        }
        UpdateDisplayMode();

        if(GameAudioHandler.instance!=null)
        {
            GameAudioHandler.instance.PlayButtonClickedSound();
        }
    }

    void UpdateDisplayMode()
    {
        if(displayMode==screenMode.mode3D)
        {
            gameCam.fieldOfView=60;
            gameCam.transform.position=mode3dPos;
            gameCam.transform.rotation=Quaternion.Euler(45f,0f,0f);
            Camera.main.orthographic = false;
            modeIndex=0;
        }
        else if(displayMode==screenMode.mode2D)
        {
            gameCam.transform.position=mode2dPos;
            gameCam.transform.rotation=Quaternion.Euler(90f,0f,0f);
            Camera.main.orthographic = true;
            gameCam.orthographicSize=6f;
            modeIndex=1;
        }
        UpdateIndicatorPositions(modeIndex);
    }

    void UpdateIndicatorPositions(int id)
    {
        UpdateModeText();
        for(int i=0;i<indicatorContainer.childCount;i++)
        {
            indicatorContainer.GetChild(i).GetComponent<RectTransform>().anchoredPosition=infoList[id].indicatorPos[i];
        }
    }

    void UpdateModeText()
    {
        if(displayMode==screenMode.mode3D)
        {
            modeText.text="3D";
            dirLight.shadows = LightShadows.Soft;
        }
        else if(displayMode==screenMode.mode2D)
        {
            modeText.text="2D";
            dirLight.shadows = LightShadows.None;
        }
    }
}
