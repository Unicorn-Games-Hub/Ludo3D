using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AppodealAds.Unity.Api;
using AppodealAds.Unity.Common;
using ConsentManager.Api;
using ConsentManager.Common;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace ConsentManager.ConsentManagerDemo.Scripts
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "ParameterHidesMember")]
    public class AppodealDemo : MonoBehaviour, IConsentFormListener, IConsentInfoUpdateListener,
        IBannerAdListener, IMrecAdListener, IRewardedVideoAdListener, IInterstitialAdListener,
        IPermissionGrantedListener
    {
        #if UNITY_EDITOR && !UNITY_ANDROID && !UNITY_IPHONE
            public static string appKey = "";
        #elif UNITY_ANDROID
            public static string appKey ="2265263c3f13e00e7c7338b6ab64a3a056b88b485f198684"; //"a4d74cb47efe8df4ca24afae6af650aa655d7a5108bd3de2";
        #elif UNITY_IPHONE
            public static string appKey = "466de0d625e01e8811c588588a42a55970bc7c132649eede";
        #else
            public static string appKey = "";
        #endif

        private ConsentForm consentForm;
        private Api.ConsentManager consentManager;
        private bool isShouldSaveConsentForm;
        public Consent currentConsent;

        public bool enableTestAds=false;

        [Header("Banner Ads Settings")]
        public bool showBannerAds=false;


        private void Awake()
        {
            Appodeal.requestAndroidMPermissions(this);
        }

        private void Start()
        {
            consentManager = Api.ConsentManager.getInstance();
            //initialize appodeal
            initialize();
        }

        #region conset handling
        
        private void OnDestroy()
        {
            Appodeal.destroy(Appodeal.BANNER); 
        }

        public void requestConsentInfoUpdate()
        {
            consentManager.requestConsentInfoUpdate(appKey, this);
        }

        public void setCustomVendor()
        {
            var customVendor = new Vendor.Builder(
                    "Appodeal Test",
                    "com.appodeal.test",
                    "https://customvendor.com")
                .setPurposeIds(new List<int> {100, 200, 300})
                .setFeatureId(new List<int> {400, 500, 600})
                .setLegitimateInterestPurposeIds(new List<int> {700, 800, 900})
                .build();

            consentManager.setCustomVendor(customVendor);

            var vendor = consentManager.getCustomVendor("com.appodeal.test");
            if (vendor == null) return;
            Debug.Log("Vendor getName: " + vendor.getName());
            Debug.Log("Vendor getBundle: " + vendor.getBundle());
            Debug.Log("Vendor getPolicyUrl: " + vendor.getPolicyUrl());
            foreach (var purposeId in vendor.getPurposeIds())
            {
                Debug.Log("Vendor getPurposeIds: " + purposeId);
            }

            foreach (var featureId in vendor.getFeatureIds())
            {
                Debug.Log("Vendor getFeatureIds: " + featureId);
            }

            foreach (var legitimateInterestPurposeId in vendor.getLegitimateInterestPurposeIds())
            {
                Debug.Log("Vendor getLegitimateInterestPurposeIds: " + legitimateInterestPurposeId);
            }
        }

        public void shouldShowForm()
        {
            Debug.Log("shouldShowConsentDialog: " + consentManager.shouldShowConsentDialog());
        }

        public void getConsentZone()
        {
            Debug.Log("getConsentZone: " + consentManager.getConsentZone());
        }

        public void getConsentStatus()
        {
            Debug.Log("getConsentStatus: " + consentManager.getConsentStatus());
        }

        public void loadConsentForm()
        {
            consentForm = new ConsentForm.Builder().withListener(this).build();
            consentForm?.load();
        }

        public void isLoadedConsentForm()
        {
            if (consentForm != null)
            {
                Debug.Log("isLoadedConsentForm:  " + consentForm.isLoaded());
            }
        }

        public void showFormAsActivity()
        {
            if (consentForm != null)
            {
                consentForm.showAsActivity();
            }
            else
            {
                Debug.Log("showForm - false");
            }
        }

        public void showFormAsDialog()
        {
            if (consentForm != null)
            {
                consentForm.showAsDialog();
            }
            else
            {
                Debug.Log("showForm - false");
            }
        }

        public void printIABString()
        {
            Debug.Log("Consent IAB String is: " + consentManager.getConsent().getIabConsentString());
        }

        public void printCurrentConsent()
        {
            if (consentManager.getConsent() == null) return;
            Debug.Log(
                "consent.getIabConsentString() - " + consentManager.getConsent().getIabConsentString());
            Debug.Log(
                "consent.hasConsentForVendor() - " +
                consentManager.getConsent().hasConsentForVendor("com.appodeal.test"));
            Debug.Log("consent.getStatus() - " + consentManager.getConsent().getStatus());
            Debug.Log("consent.getZone() - " + consentManager.getConsent().getZone());
        }
        #endregion

        public void initialize()
        {
            if (currentConsent != null)
            {
                initWithConsent(true);
            }
            else
            {
                initWithConsent(false);
            }
        }

        public void initWithConsent(bool isConsent)
        {
            Appodeal.setTesting(enableTestAds);
           // Appodeal.setLogLevel(tgLogging.isOn ? Appodeal.LogLevel.Verbose : Appodeal.LogLevel.None);
            Appodeal.setUserId("1");
            Appodeal.setUserAge(1);
            Appodeal.setUserGender(UserSettings.Gender.OTHER);
            Appodeal.disableLocationPermissionCheck();
            Appodeal.disableWriteExternalStoragePermissionCheck();
            Appodeal.setTriggerOnLoadedOnPrecache(Appodeal.INTERSTITIAL, true);
            Appodeal.setSmartBanners(true);
            Appodeal.setBannerAnimation(false);
            Appodeal.setTabletBanners(false);
            Appodeal.setBannerBackground(false);
            Appodeal.setChildDirectedTreatment(false);
            Appodeal.muteVideosIfCallsMuted(true);
            Appodeal.setSharedAdsInstanceAcrossActivities(true);
            Appodeal.setAutoCache(Appodeal.INTERSTITIAL, true);
            Appodeal.setAutoCache(Appodeal.REWARDED_VIDEO, true);
            Appodeal.setExtraData(ExtraData.APPSFLYER_ID, "1527256526604-2129416");
            if (isConsent)
            {
                Appodeal.initialize(appKey,
                    Appodeal.INTERSTITIAL | Appodeal.BANNER | Appodeal.REWARDED_VIDEO | Appodeal.MREC,
                    currentConsent);
            }
            else
            {
                Appodeal.initialize(appKey,
                    Appodeal.INTERSTITIAL | Appodeal.BANNER | Appodeal.REWARDED_VIDEO | Appodeal.MREC,
                    true);
            }

            Appodeal.setBannerCallbacks(this);
            Appodeal.setInterstitialCallbacks(this);
            Appodeal.setRewardedVideoCallbacks(this);
            Appodeal.setMrecCallbacks(this);

            Appodeal.setSegmentFilter("newBoolean", true);
            Appodeal.setSegmentFilter("newInt", 1234567890);
            Appodeal.setSegmentFilter("newDouble", 123.123456789);
            Appodeal.setSegmentFilter("newString", "newStringFromSDK");

            if(showBannerAds)
            {
                showBanner();
            }
            
            //caching ads
            TryAdsCaching();
        }

        #region caching ads
        void TryAdsCaching()
        {
            Appodeal.cache(Appodeal.REWARDED_VIDEO);
            Appodeal.cache(Appodeal.INTERSTITIAL);
            TryToShowCachedAds(0);
        }

        string adcallPosition="";
        string adName="";
        private bool eligibleForAds=false;
        public void TryToShowCachedAds(int posId)
        {
            switch(posId)
            {
                case 0:
                eligibleForAds=false;
                adcallPosition="start";
                break;
                case 1:
                eligibleForAds=true;
                adcallPosition="end";
                break;
                default:
                adcallPosition="";
                break;
            }
           
            if(SceneManager.GetActiveScene()==SceneManager.GetSceneByName("Ludo"))
            {
                if(GameDataHolder.instance!=null&&posId==0)
                {
                    if(GameDataHolder.instance.initialAdsShowCounter>=2)
                    {
                        eligibleForAds=true;
                    }
                }
                if(eligibleForAds)
                {
                    if(Appodeal.canShow(Appodeal.REWARDED_VIDEO))
                    {
                        adName="reward_video";
                        Appodeal.show(Appodeal.REWARDED_VIDEO);
                    }
                    else if(Appodeal.canShow(Appodeal.INTERSTITIAL))
                    {
                        adName="interstitial";
                        Appodeal.show(Appodeal.INTERSTITIAL);
                    }
                }
            } 

            if(FirebaseHandler.instance!=null)
            {
                FirebaseHandler.instance.TrackAdLoad(adName,adcallPosition);
            }
        }
        #endregion

        #region show ads
        public void showInterstitial()
        {
            if (Appodeal.canShow(Appodeal.INTERSTITIAL))
            {
                Appodeal.show(Appodeal.INTERSTITIAL);
            }
        }

        public void showRewardedVideo()
        {
            if(Appodeal.canShow(Appodeal.REWARDED_VIDEO))
            {
               Appodeal.show(Appodeal.REWARDED_VIDEO);
            }
        }

        public void showBanner()
        {
            Appodeal.show(Appodeal.BANNER_BOTTOM, "default");
        }
        #endregion

        public void hideBanner()
        {
            Appodeal.hide(Appodeal.BANNER);
        }

        public void showBannerView()
        {
            Appodeal.showBannerView(Screen.currentResolution.height - Screen.currentResolution.height / 10,
                Appodeal.BANNER_HORIZONTAL_CENTER, "default");
        }

        public void hideBannerView()
        {
            Appodeal.hideBannerView();
        }

        public void showMrecView()
        {
            Appodeal.showMrecView(Screen.currentResolution.height - Screen.currentResolution.height / 10,
                Appodeal.BANNER_HORIZONTAL_CENTER, "default");
        }

        public void hideMrecView()
        {
            Appodeal.hideMrecView();
        }

        public void showBannerLeft()
        {
            Appodeal.show(Appodeal.BANNER_LEFT);
        }

        public void showBannerRight()
        {
            Appodeal.show(Appodeal.BANNER_RIGHT);
        }


        #region ConsentFormListener

        public void onConsentFormLoaded()
        {
            Debug.Log("ConsentFormListener - onConsentFormLoaded");
        }

        public void onConsentFormError(ConsentManagerException exception)
        {
            Debug.Log($"ConsentFormListener - onConsentFormError, reason - {exception.getReason()}");
        }

        public void onConsentFormOpened()
        {
            Debug.Log("ConsentFormListener - onConsentFormOpened");
        }

        public void onConsentFormClosed(Consent consent)
        {
            currentConsent = consent;
            Debug.Log($"ConsentFormListener - onConsentFormClosed, consentStatus - {consent.getStatus()}");
        }

        #endregion

        #region ConsentInfoUpdateListener

        public void onConsentInfoUpdated(Consent consent)
        {
            currentConsent = consent;
            Debug.Log("onConsentInfoUpdated");
        }

        public void onFailedToUpdateConsentInfo(ConsentManagerException error)
        {
            Debug.Log($"onFailedToUpdateConsentInfo");

            if (error == null) return;
            Debug.Log($"onFailedToUpdateConsentInfo Reason: {error.getReason()}");

            switch (error.getCode())
            {
                case 0:
                    Debug.Log("onFailedToUpdateConsentInfo - UNKNOWN");
                    break;
                case 1:
                    Debug.Log(
                        "onFailedToUpdateConsentInfo - INTERNAL - Error on SDK side. Includes JS-bridge or encoding/decoding errors");
                    break;
                case 2:
                    Debug.Log("onFailedToUpdateConsentInfo - NETWORKING - HTTP errors, parse request/response ");
                    break;
                case 3:
                    Debug.Log("onFailedToUpdateConsentInfo - INCONSISTENT - Incorrect SDK API usage");
                    break;
            }
        }

        #endregion

        #region Banner callback handlers

        public void onBannerLoaded(int height, bool precache)
        {
            Debug.Log("onBannerLoaded");
            Debug.Log($"Banner height - {height}");
            Debug.Log($"Banner precache - {precache}");
        }

        public void onBannerFailedToLoad()
        {
            Debug.Log("onBannerFailedToLoad");
        }

        public void onBannerShown()
        {
            print("onBannerShown");
        }

        public void onBannerClicked()
        {
            print("onBannerClicked");
        }

        public void onBannerExpired()
        {
            print("onBannerExpired");
        }

        #endregion

        #region Interstitial callback handlers

        public void onInterstitialLoaded(bool isPrecache)
        {
            if (!isPrecache)
            {

            }
            else
            {
                Debug.Log("Appodeal. Interstitial loaded. isPrecache - true");
            }
        }

        public void onInterstitialFailedToLoad()
        {
        }

        public void onInterstitialShowFailed()
        {
            Debug.Log("onInterstitialShowFailed");
        }

        public void onInterstitialShown()
        {
            if(FirebaseHandler.instance!=null)
            {
                FirebaseHandler.instance.TrackInerstitialAds();
            }

            CountTotalAdsShown(0);
        }

        public void onInterstitialClosed()
        {
            TrackAdsClosed();
        }

        public void onInterstitialClicked()
        {
            Debug.Log("onInterstitialClicked");
            CountTotalAdsClicked(0);
        }

        public void onInterstitialExpired()
        {
            Debug.Log("onInterstitialExpired");
        }

        #endregion

        #region Rewarded Video callback handlers

        public void onRewardedVideoLoaded(bool isPrecache)
        {
        }

        public void onRewardedVideoFailedToLoad()
        {
        }

        public void onRewardedVideoShowFailed()
        {
            print("onRewardedVideoShowFailed");
        }

        public void onRewardedVideoShown()
        {
            if(FirebaseHandler.instance!=null)
            {
                FirebaseHandler.instance.TrackRewardVideoAds();
            }
            CountTotalAdsShown(1);
        }

        public void onRewardedVideoClosed(bool finished)
        {
            TrackAdsClosed();
        }

        public void onRewardedVideoFinished(double amount, string name)
        {
            
            print("onRewardedVideoFinished. Reward: " + amount + " " + name);
        }

        public void onRewardedVideoExpired()
        {
            print("onRewardedVideoExpired");
        }

        public void onRewardedVideoClicked()
        {
            print("onRewardedVideoClicked");
            CountTotalAdsClicked(1);
        }

        #endregion

        #region Mrec callback handlers

        public void onMrecLoaded(bool precache)
        {
            print($"onMrecLoaded. Precache - {precache}");
        }

        public void onMrecFailedToLoad()
        {
            print("onMrecFailedToLoad");
        }

        public void onMrecShown()
        {
            print("onMrecShown");
        }

        public void onMrecClicked()
        {
            print("onMrecClicked");
        }

        public void onMrecExpired()
        {
            print("onMrecExpired");
        }

        #endregion

        #region PermissionGrantedListener

        public void writeExternalStorageResponse(int result)
        {
            if (result == 0)
            {
                Debug.Log("WRITE_EXTERNAL_STORAGE permission granted");
            }
            else
            {
                Debug.Log("WRITE_EXTERNAL_STORAGE permission grant refused");
            }
        }

        public void accessCoarseLocationResponse(int result)
        {
            if (result == 0)
            {
                Debug.Log("ACCESS_COARSE_LOCATION permission granted");
            }
            else
            {
                Debug.Log("ACCESS_COARSE_LOCATION permission grant refused");
            }
        }

        #endregion PermissionGrantedListener

        #region counting total ads shown
        void CountTotalAdsShown(int adIndex)
        {
            PlayerPrefs.SetInt("total_ads_shown",PlayerPrefs.GetInt("total_ads_shown")+1);
            if(adIndex==0)
            {
                PlayerPrefs.SetInt("interstitial_shown",PlayerPrefs.GetInt("interstitial_shown")+1);
            }
            else if(adIndex==1)
            {
                PlayerPrefs.SetInt("reward_video_shown",PlayerPrefs.GetInt("reward_video_shown")+1);
            }

            if(!PlayerPrefs.HasKey("ads_mileStone"))
            {
                PlayerPrefs.SetInt("ads_mileStone",30);
            }
            if(PlayerPrefs.GetInt("total_ads_shown")>=PlayerPrefs.GetInt("ads_mileStone"))
            {
                if(FirebaseHandler.instance!=null)
                {
                    FirebaseHandler.instance.TrackAdMileStone();
                }
                PlayerPrefs.SetInt("ads_mileStone",PlayerPrefs.GetInt("ads_mileStone")+30);
            }
            PlayerPrefs.SetInt("adsThisSession",PlayerPrefs.GetInt("adsThisSession")+1);
        }
        #endregion

        #region  counting total ads clicked
        void CountTotalAdsClicked(int adIndex)
        {
            PlayerPrefs.SetInt("total_ads_clicked",PlayerPrefs.GetInt("total_ads_clicked")+1);
            if(adIndex==0)
            {
                PlayerPrefs.SetInt("interstitial_clicked",PlayerPrefs.GetInt("interstitial_clicked")+1);
            }
            else if(adIndex==1)
            {
                PlayerPrefs.SetInt("reward_clicked",PlayerPrefs.GetInt("reward_clicked")+1);
            }
            PlayerPrefs.SetInt("adsClickedThisSession",PlayerPrefs.GetInt("adsClickedThisSession")+1);
        }
        #endregion

        #region Tracking ads closed
        void TrackAdsClosed()
        {
            if(FirebaseHandler.instance!=null)
            {
                FirebaseHandler.instance.TrackAdLoad(adName,adcallPosition);
            }
        }
        #endregion
    }
}