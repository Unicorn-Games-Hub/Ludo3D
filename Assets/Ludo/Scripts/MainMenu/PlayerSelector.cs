using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSelector : MonoBehaviour
{
    public int colorIndex=0;

    public int playerID=0;
    
    public void UpdatePlayerType()
    {
        if(playerID==0)
        {
            playerID=1;
        }
        else
        {
            playerID=0;
        }
        GameModeSelectionHandler.instance.ChangePlayerIcon(this.gameObject);
        if(HomeMenuHandler.instance!=null)
        {
            HomeMenuHandler.instance.PlayButtonClickSound();
        }
    }
}
