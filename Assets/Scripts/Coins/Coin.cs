using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public int[] scores=new int[8];
    public float[] weights=new float[8];
    public float priorityValue=0f;

    public float[] priorityValues=new float[8];

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
        priorityValue=((float)score/100f)*weight;
        priorityValues[priorityIndex]=priorityValue;
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
}
