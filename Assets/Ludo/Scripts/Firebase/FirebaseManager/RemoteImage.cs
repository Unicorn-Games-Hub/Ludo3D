using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RemoteImage : MonoBehaviour
{
    public string gameUrl;
    public Image promoImage;

    public void UpdateRemoteData(Sprite remoteImage,string url)
    {
        gameUrl=url;
        promoImage.sprite=remoteImage;
    }
    
    public void GoToUrl()
    {
        Application.OpenURL(gameUrl);
    }

    public void Close()
    {
        FirebaseManager.instance.CloseThis(this.gameObject);
    }
}
