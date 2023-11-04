using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class instaref : MonoBehaviour
{
   public void instaPage()
    {

        Firebase.Analytics.FirebaseAnalytics.LogEvent("insta", "instaGo", "shown");

        Application.OpenURL("https://www.instagram.com/ugames_np/");


    }
}
