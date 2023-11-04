using UnityEngine;
using System.Collections;
//using GoogleMobileAds.Api;
//using GoogleMobileAds.Common;
using System;

public class ADScript : MonoBehaviour {
    public bool isShowBanner;
    public bool isOnTop;
	private static ADScript instance;
	
	private ADScript() {}
	public static ADScript Instance
	{
		get 
		{
            if (instance == null)
            {
                instance = new ADScript();
            }
           
      
			return instance;
		}
	}

    void Awake()
    {

        if (GameObject.FindObjectsOfType<ADScript>().Length > 1)
        {
            Destroy(this.gameObject);
        }
        else
        {

            DontDestroyOnLoad(gameObject);

           
                Gley.MobileAds.API.Initialize();
           
            

           // AppStateEventNotifier.AppStateChanged += OnAppStateChanged;
            //OpenConsentWindow();
        }
    }

    private void Start()
    {

        //if (isShowBanner)
        //{
        //    if (isOnTop)
        //    {
        //        Gley.MobileAds.API.ShowBanner(Gley.MobileAds.BannerPosition.Top, Gley.MobileAds.BannerType.Banner);
        //    }
        //    else
        //    {
        //        Gley.MobileAds.API.ShowBanner(Gley.MobileAds.BannerPosition.Bottom, Gley.MobileAds.BannerType.Banner);
        //    }
        //}
        // Gley.MobileAds.API.ShowAppOpen();
      
    }
    bool check;
    private void Update()
    {
        if (!check&&Gley.MobileAds.API.IsInitialized())
        {
            check = true;
           // StopCoroutine(startup());
            StartCoroutine(startup());

        }
    }

    IEnumerator startup()
    {
        OpenConsentWindow();
        yield return new WaitForSeconds(1);
        // Gley.MobileAds.API.ShowAppOpen();
        yield return new WaitForSeconds(1);
        ShowBanner();

    }
    public void ShowIntestitial()
    {
        Gley.MobileAds.API.ShowInterstitial();
    }
   
    public void ShowRewardedVideo()
    {
        Gley.MobileAds.API.ShowRewardedVideo(CloseMethod);
    }
    void CloseMethod(bool result)
    {
        if (result)
        {

        }
    }
    public void ShowBanner()
    {
        if (isShowBanner)
        {
            if (isOnTop)
            {
                Gley.MobileAds.API.ShowBanner(Gley.MobileAds.BannerPosition.Top, Gley.MobileAds.BannerType.Banner);
            }
            else
            {
                Gley.MobileAds.API.ShowBanner(Gley.MobileAds.BannerPosition.Bottom, Gley.MobileAds.BannerType.Banner);
            }
        }
    }
    public void RemoveBanner()
    {
        Gley.MobileAds.API.HideBanner();

    }
    public void OpenConsentWindow()
    {
        //if (!Gley.MobileAds.API.GDPRConsentWasSet())
        if(PlayerPrefs.GetString("gdrp")!="ok")
        Gley.MobileAds.API.ShowBuiltInConsentPopup(ConsentPopupClosed);
    }


    /// <summary>
    /// Callback called when consent popup is closed
    /// </summary>
    private void ConsentPopupClosed()
    {
        PlayerPrefs.SetString("gdrp", "ok");
       // Gley.MobileAds.API.GleyLogger.AddLog($"Consent Popup Closed");
    }


    //private void OnAppStateChanged(AppState state)
    //{
    //    Debug.Log("App State changed to : " + state);

    //    // if the app is Foregrounded and the ad is available, show it.
    //    if (state == AppState.Foreground)
    //    {
    //        if (Gley.MobileAds.API.IsAppOpenAvailable())
    //        {
    //           // Gley.MobileAds.API.ShowAppOpen();

    //        }
    //    }
    //}

    private void OnDestroy()
    {
        // Always unlisten to events when complete.
       // AppStateEventNotifier.AppStateChanged -= OnAppStateChanged;
    }
    ///////////////////////////////////////////////////////
    ///

    //private void OnApplicationFocus(bool focus)
    //{
    //    if (focus)
    //    {
    //      if(Gley.MobileAds.API.IsAppOpenAvailable())
    //        {
    //            print("appopen");
    //            Gley.MobileAds.API.ShowAppOpen();
    //        }
    //    }
    //}

}
