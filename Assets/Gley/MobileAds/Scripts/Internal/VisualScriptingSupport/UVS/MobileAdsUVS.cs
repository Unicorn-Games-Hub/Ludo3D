#if GLEY_UVS_SUPPORT
using Unity.VisualScripting;
using UnityEngine;

namespace Gley.MobileAds.Internal
{
    [IncludeInSettings(true)]
    public static class MobileAdsUVS
    {
        private static GameObject eventTarget;
        public static void InitializeAds()
        {
            Gley.MobileAds.API.Initialize();
        }

        public static void ShowBanner(BannerPosition position, BannerType type)
        {
            Gley.MobileAds.API.ShowBanner(position, type);
        }

        public static void HideBanner()
        {
            Gley.MobileAds.API.HideBanner();
        }

        public static bool IsInterstitialAvailable()
        {
            return Gley.MobileAds.API.IsInterstitialAvailable();
        }

        public static void ShowInterstitial()
        {
            Gley.MobileAds.API.ShowInterstitial();
        }

        public static void ShowInterstitial(GameObject _eventTarget)
        {
            eventTarget = _eventTarget;
            Gley.MobileAds.API.ShowInterstitial(InterstitialClosed);
        }

        private static void InterstitialClosed()
        {
            CustomEvent.Trigger(eventTarget, "InterstitialClosed");
        }


        public static bool IsAppOpenAvailable()
        {
            return Gley.MobileAds.API.IsAppOpenAvailable();
        }

        public static void ShowAppOpen()
        {
            Gley.MobileAds.API.ShowAppOpen();
        }

        public static bool IsRewardedVideoAvailable()
        {
            return Gley.MobileAds.API.IsRewardedVideoAvailable();
        }

        public static void ShowRewardedVideo(GameObject _eventTarget)
        {
            if (Gley.MobileAds.API.IsRewardedVideoAvailable())
            {
                eventTarget = _eventTarget;
                Gley.MobileAds.API.ShowRewardedVideo(VideoComplete);
            }
        }

        private static void VideoComplete(bool complete)
        {
            CustomEvent.Trigger(eventTarget, "VideoComplete", complete);
        }

        public static bool IsRewardedInterstitialAvailable()
        {
            return Gley.MobileAds.API.IsRewardedInterstitialAvailable();
        }

        public static void ShowRewardedInterstitial(GameObject _eventTarget)
        {
            if (Gley.MobileAds.API.IsRewardedInterstitialAvailable())
            {
                eventTarget = _eventTarget;
                Gley.MobileAds.API.ShowRewardedInterstitial(RewardedInterstitialComplete);
            }
        }

        private static void RewardedInterstitialComplete(bool complete)
        {
            CustomEvent.Trigger(eventTarget, "RewardedInterstitialComplete", complete);
        }


        public static void ShowBuiltInConsentPopup(GameObject _eventTarget)
        {
                eventTarget = _eventTarget;
                Gley.MobileAds.API.ShowBuiltInConsentPopup(ConsentPopupClosed);
        }

        private static void ConsentPopupClosed()
        {
            CustomEvent.Trigger(eventTarget, "ConsentPopupClosed");
        }

        public static void RemoveAds()
        {
            Gley.MobileAds.API.RemoveAds(true);
        }

        public static void SetGDPRConsent(bool value)
        {
            Gley.MobileAds.API.SetGDPRConsent(value);
        }

        public static void SetCCPAConsent(bool value)
        {
            Gley.MobileAds.API.SetCCPAConsent(value);
        }

        public static bool GDPRConsentWasSet()
        {
            return Gley.MobileAds.API.GDPRConsentWasSet();
        }

        public static bool CCPAConsentWasSet()
        {
            return Gley.MobileAds.API.CCPAConsentWasSet();
        }
    }
}
#endif