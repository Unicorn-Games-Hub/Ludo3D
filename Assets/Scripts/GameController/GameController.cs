using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    //
    public Transform coinContainer;
    public Transform[] coins;//put coins in array in order of green red blue and yellow

    [Range(2,4)]
    public int noOfCoins=4;

    [Header("Game Behaviour")]
    [SerializeField]private int coinOutAt=1;
    [SerializeField]private int rollChanceAt=6;

    void Awake()
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

    void Start()
    {
        GenerateCoins();
    }

    void GenerateCoins()
    {
        for(int i=0;i<coinContainer.childCount;i++)
        {
            for(int j=0;j<noOfCoins;j++)
            {
                Transform newCoin=Instantiate(coins[i],coinContainer.GetChild(i).GetChild(j).position,coins[i].transform.rotation);
            }
        }
    }
}
