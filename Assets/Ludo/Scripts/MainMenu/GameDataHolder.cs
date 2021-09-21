using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDataHolder : MonoBehaviour
{
    public static GameDataHolder instance;

    public int[] playerIndex= new int[4];

    public int rateUsShownCounter=0;
    public bool rateusShown=false;

    public int initialAdsShowCounter=0;

    public bool isSessionStarted=false;

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
        DontDestroyOnLoad(this);
        if(FindObjectsOfType(GetType()).Length>1)
        {
            Destroy(this.gameObject);
        }
    }
}
