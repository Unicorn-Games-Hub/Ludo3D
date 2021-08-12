using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Coin : MonoBehaviour
{
    public int id=0;
    public int stepCounter=0;
    public int stepsMovedTillNow=0;
    public bool isClickable=false;
    public bool atBase=true;
    public bool isReadyForHome=true;
    public bool canGoHome=false;
    public bool onWayToHome=false;
    public bool atHome=false;
    public bool isSafe=false;

    public Vector3 initialPosInfo;
    
    //score for handling scores and weight
    private float priorityWeight=0f;
    public float[] priorityValues=new float[8];
    public float maxValueAmongAll=0;

    //player number indicator
    // public GameObject indicatorUI;
    // public Text playerCounterText;
    // private int playerCounter=1;
    public GameObject indicator;

    private Vector3 initialScale;
    private float s1,s2;

    void Start()
    {
        initialScale=transform.localScale;
        s1=initialScale.x;
        s2=s1+0.4f;
    }


    public void HandleCoinInfo(int Id,Vector3 coinPos)
    {
        id=Id;
        initialPosInfo=coinPos;
        atBase=true;
    }

    public void SetClickable(bool clickValue)
    {
        isClickable=clickValue;
    }

    public void ComputeWeightage(int score,float weight,int priorityIndex)
    {
        priorityWeight=((float)score/100f)*weight;
        priorityValues[priorityIndex]=priorityWeight;
    }
   
    public float GetMaxWeightPercentage()
    {
        float maxWeight=priorityValues[0];
        for(int i=0;i<priorityValues.Length;i++)
        {
            if(priorityValues[i]>maxWeight)
            {
                maxWeight=priorityValues[i];
            }
        }
        return maxWeight;
    }
  
    public void ResetPriorityValues()
    {
        Array.Clear(priorityValues, 0, priorityValues.Length);
    }

    #region number of player indicator
    public void HideIndicator()
    {
        isClickable=false;
        indicator.SetActive(false);
        StopScaleAnimation();
    }

    public void UpdateIndicatorInfo(int playerCounter)
    {
        //playerCounterText.text=playerCounter.ToString();
        //indicatorUI.SetActive(true);
    }
    #endregion

    #region animating scale
    public void StartScaleAnimation()
    {
        iTween.ValueTo(gameObject, iTween.Hash("name", "objScale","from", s1, "to", s2,"onupdate", 
        "UpdateScale","loopType", iTween.LoopType.pingPong, "easetype", iTween.EaseType.linear, "time", .4f, "delay", 0.05f));
    }

    public void StopScaleAnimation()
    {
        iTween.StopByName(gameObject, "objScale");
        transform.localScale=initialScale;
    }

    void UpdateScale(float scaleValue)
    {
        transform.localScale=new Vector3(scaleValue,scaleValue,scaleValue);
    }
    #endregion
}
