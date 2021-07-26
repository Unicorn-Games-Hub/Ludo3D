using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDataHolder : MonoBehaviour
{
    public static GameDataHolder instance;

    public int[] playerIndex= new int[4];

    public bool isInitialAdsShown=false;

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
