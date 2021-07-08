using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Coin : MonoBehaviour
{
    public int id=0;
    public int stepCounter=0;
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
        for(int i=0;i<priorityValues.Length;i++)
        {
            priorityValues[i]=0f;
        }
    }

    #region number of player indicator
    public void UpdateIndicatorInfo(int playerCounter)
    {
        Debug.Log("Total coins at my positions is : "+playerCounter);
        //playerCounterText.text=playerCounter.ToString();
        //indicatorUI.SetActive(true);
    }
    #endregion
}
