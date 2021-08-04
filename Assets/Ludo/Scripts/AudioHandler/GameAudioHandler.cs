using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAudioHandler : MonoBehaviour
{
    public static GameAudioHandler instance;

    public AudioSource uiAd;
    public AudioSource ad;
    public AudioSource respawnAd;

    [Header("UI")]
    public AudioClip buttonSound;
    
    [Header("Dice")]
    public AudioClip diceRollSound;
    
    [Header("Character Sound")]
    public AudioClip outOfBaseSound;
    public AudioClip walkSound;
    public AudioClip attackSound;
    public AudioClip defendSound;
    public AudioClip partFallingSound;
    public AudioClip respawnSound;

    [Header("Coin Sound")]  
    public AudioClip coinMoveSound;

    [Header("Home Sounds")]
    public AudioClip homeSound;
    public AudioClip victorySound;
    public AudioClip defeatSound;

    [Header("Safe Zone")]
    public AudioClip safeZoneSound;

    [Header("Confetti")]
    public AudioClip confettiExplosion;

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

    #region UI
    public void PlayButtonClickedSound()
    {
        if(PlayerPrefs.GetInt("Ludo-Sound")==0)
        {
            uiAd.clip=buttonSound;
            uiAd.Play();
        }
    }
    #endregion

    #region Dice
    public void PlayDiceRollSound()
    {
        if(PlayerPrefs.GetInt("Ludo-Sound")==0)
        {
            ad.clip=diceRollSound;
            ad.Play();
        }
    }
    #endregion 

    #region Character
    public void PlayOutOfBaseSound()
    {
        if(PlayerPrefs.GetInt("Ludo-Sound")==0)
        {
            ad.clip=outOfBaseSound;
            ad.Play();
        }
    }

    public void PlayCharacterClickSound()
    {
        if(PlayerPrefs.GetInt("Ludo-Sound")==0)
        {
            uiAd.clip=null;
            uiAd.Play();
        }
    }

    public void PlayCharacterWalkSound()
    {
        if(PlayerPrefs.GetInt("Ludo-Sound")==0)
        {
            ad.clip=walkSound;
            ad.Play();
        }
    }

    public void PlayCharacterAttackSound()
    {
        if(PlayerPrefs.GetInt("Ludo-Sound")==0)
        {
            ad.clip=attackSound;
            ad.Play();
        }
    }

    public void PlayCharacterDefendSound()
    {
        if(PlayerPrefs.GetInt("Ludo-Sound")==0)
        {
            ad.clip=defendSound;
            ad.Play();
        }
    }   

    public void PlayPartShatterSound()
    {
        if(PlayerPrefs.GetInt("Ludo-Sound")==0)
        {
            ad.clip=partFallingSound;
            ad.Play();
        }
    }

    public void PlaySafeZoneReachedSound()
    {
        if(PlayerPrefs.GetInt("Ludo-Sound")==0)
        {
            ad.clip=safeZoneSound;
            ad.Play();
        }
    }

    public void PlayHomeReachedSound()
    {
        if(PlayerPrefs.GetInt("Ludo-Sound")==0)
        {
            ad.clip=null;
            ad.Play();
        }
    }

    public void PlayVictorySound()
    {
        if(PlayerPrefs.GetInt("Ludo-Sound")==0)
        {
            ad.clip=victorySound;
            ad.Play();
        }
    }

    public void PlayDefeatSound()
    {
        if(PlayerPrefs.GetInt("Ludo-Sound")==0)
        {
            ad.clip=defeatSound;
            ad.Play();
        }
    }

    public void PlayReSpawnSound()
    {
        if(PlayerPrefs.GetInt("Ludo-Sound")==0)
        {
            respawnAd.clip=respawnSound;
            respawnAd.Play();
        }
    }
    #endregion

    #region Coin
    public void PlayCoinMoveSound()
    {
        if(PlayerPrefs.GetInt("Ludo-Sound")==0)
        {
            ad.clip=coinMoveSound;
            ad.Play();
        }
    }

    public void PlayCoinCutSound()
    {
        if(PlayerPrefs.GetInt("Ludo-Sound")==0)
        {
            ad.clip=null;
            ad.Play();
        }
    }
    #endregion

    #region Confetti 
    public void PlayConfettiExplosionSound()
    {
        if(PlayerPrefs.GetInt("Ludo-Sound")==0)
        {
            ad.clip=confettiExplosion;
            ad.Play();
        }
    }
    #endregion
}


