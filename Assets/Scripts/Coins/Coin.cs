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

    public void SetClickable(bool clickValue)
    {
        isClickable=clickValue;
    }
}
