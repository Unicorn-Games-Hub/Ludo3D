using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusicHandler : MonoBehaviour
{
    public static BackgroundMusicHandler instance;
    private AudioSource ad;

    void Start()
    {
        if(instance!=null)
        {
            return;
        }
        else
        {
            instance=this;
        }

        ad=GetComponent<AudioSource>();
        HandleBgMusic();
    }

    public void UpdateBgMusicStatus()
    {
        HandleBgMusic();
    }

    void HandleBgMusic()
    {
        if(PlayerPrefs.GetInt("Ludo-Music")==0)
        {
            ad.mute=false;
        }
        else
        {
            ad.mute=true;
        }
    }

}
