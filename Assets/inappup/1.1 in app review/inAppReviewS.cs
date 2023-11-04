using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if PLATFORM_ANDROID || UNITY_ANDROID || UNITY_ANDROID_API
using Google.Play.Review;
#endif

public class inAppReviewS : MonoBehaviour
{
#if PLATFORM_ANDROID || UNITY_ANDROID || UNITY_ANDROID_API

    private ReviewManager _reviewManager;
    PlayReviewInfo _playreviewinfo;
#endif

    public string iosRateusLink;
   public  void callForReview()
    {

        PlayerPrefs.SetString("reviewcall", "Done");


          StartCoroutine(review());
        


    }

    public void maybrLater()
    {
        transform.GetChild(0).gameObject.SetActive(false);

    }
    static int counter;
    private void Start()
    {


        if(PlayerPrefs.GetString("reviewcall")== "Done")
        {
        }
        else
        {
#if PLATFORM_ANDROID || UNITY_ANDROID || UNITY_ANDROID_API

            _reviewManager = new ReviewManager();
#endif
            counter += 1;
            if (counter==2)
            {
                counter = 0;
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                }
                else
                {
                  transform.GetChild(0).gameObject.SetActive(true);
                }
            }
        }
    }

    IEnumerator review()
    {
        yield return new WaitForSeconds(1f);
#if PLATFORM_ANDROID || UNITY_ANDROID || UNITY_ANDROID_API

        var requesflowOperation = _reviewManager.RequestReviewFlow();
        yield return requesflowOperation;
        if(requesflowOperation.Error != ReviewErrorCode.NoError)
        {
            yield break;
        }
        _playreviewinfo = requesflowOperation.GetResult();
        var launchFlowOperation = _reviewManager.LaunchReviewFlow(_playreviewinfo);
        yield return launchFlowOperation;
        _playreviewinfo = null;
        if(launchFlowOperation.Error!= ReviewErrorCode.NoError)
        {
            yield break;
        }
#else

        Application.OpenURL(iosRateusLink);


#endif

        yield return new WaitForSeconds(1);
        transform.GetChild(0).gameObject.SetActive(false);

    }
}
