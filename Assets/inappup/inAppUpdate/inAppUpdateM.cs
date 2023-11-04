using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if PLATFORM_ANDROID || UNITY_ANDROID || UNITY_ANDROID_API

using Google.Play.Common;
using Google.Play.AppUpdate;
#endif
public class inAppUpdateM : MonoBehaviour
{
    static bool check = true;

#if PLATFORM_ANDROID || UNITY_ANDROID || UNITY_ANDROID_API

    [SerializeField] private Text inAppStatus;

    AppUpdateManager appUpdateManager;// = new AppUpdateManager();


    private void Awake()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            this.appUpdateManager = new AppUpdateManager();
        }
    }


    // Start is called before the first frame update
    void Start()
    {

     if(check){
        check = false;
        CheckForUpdatebutonFunc();

        }
    }

    public  void CheckForUpdatebutonFunc()
    {
        StartCoroutine(CheckForUpdate());

    }

    IEnumerator CheckForUpdate()
    {
        PlayAsyncOperation<AppUpdateInfo, AppUpdateErrorCode> appUpdateInfoOperation = appUpdateManager.GetAppUpdateInfo(); //< --error here

        // Wait until the asynchronous operation completes.
        yield return appUpdateInfoOperation;

        if (appUpdateInfoOperation.IsSuccessful)
        {

            var appUpdateInfoResult = appUpdateInfoOperation.GetResult();
            // Check AppUpdateInfo's UpdateAvailability, UpdatePriority,
            // IsUpdateTypeAllowed(), etc. and decide whether to ask the user
            // to start an in-app update.

            //display if there us an update or not
            if (appUpdateInfoResult.UpdateAvailability == UpdateAvailability.UpdateAvailable)
            {
              //  inAppStatus.text = UpdateAvailability.UpdateAvailable.ToString();
            }
            else
            {
               // inAppStatus.text = "No Update Avaiable";
            }

            // Creates an AppUpdateOptions defining an immediate in-app
            // update flow and its parameters.

            var appUpdateOptions = AppUpdateOptions.ImmediateAppUpdateOptions();

            StartCoroutine(StartImmediateUpdate(appUpdateInfoResult, appUpdateOptions));
           // StartCoroutine(StartFlexibleUpdate(appUpdateInfoResult, appUpdateOptions));

        }

    }


    IEnumerator StartImmediateUpdate(AppUpdateInfo appUpdateInfoOp_i, AppUpdateOptions appUpdateOptions_i)
    {
        // Creates an AppUpdateRequest that can be used to monitor the
        // requested in-app update flow.
        var startUpdateRequest = appUpdateManager.StartUpdate(
        // The result returned by PlayAsyncOperation.GetResult().
        appUpdateInfoOp_i,
        // The AppUpdateOptions created defining the requested in-app update
        // and its parameters.
        appUpdateOptions_i);
        yield return startUpdateRequest;

        // If the update completes successfully, then the app restarts and this line
        // is never reached. If this line is reached, then handle the failure (for
        // example, by logging result.Error or by displaying a message to the user).
    }



     IEnumerator StartFlexibleUpdate(AppUpdateInfo appUpdateInfoOp_i, AppUpdateOptions appUpdateOptions_i)
    {
    // Creates an AppUpdateRequest that can be used to monitor the
    // requested in-app update flow.
    var startUpdateRequest = appUpdateManager.StartUpdate(
    // The result returned by PlayAsyncOperation.GetResult().
    appUpdateInfoOp_i,
    // The AppUpdateOptions created defining the requested in-app update
    // and its parameters.
    appUpdateOptions_i);

    while (!startUpdateRequest.IsDone)
    {
    // For flexible flow,the user can continue to use the app while
    // the update downloads in the background. You can implement a
    // progress bar showing the download status during this time.
    yield return null;
    }

    } 







#endif


}


