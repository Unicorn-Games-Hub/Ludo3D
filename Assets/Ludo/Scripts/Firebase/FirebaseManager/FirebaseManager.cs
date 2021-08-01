using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
// using Firebase.Extensions;
// using Firebase.Database;
using System;
using System.IO;
using UnityEngine.Networking;
using SimpleJSON;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager instance;

    //Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;
    protected bool isFirebaseInitialized = false;

    public string appBundleID;

    public class databaseData
    {
        public string appName;
        public string bundleID;
        public string appUrl;
        public int priority;
        public string message;
        public string image_icon;
        public string image_small;
        public string image_large;

        public databaseData(string name,string id,string url,int appPriority,string newMessage,string iconUrl,string smallImageUrl,string largeImageUrl)
        {
            appName=name;
            bundleID=id;
            appUrl=url;
            priority=appPriority;
            message=newMessage;
            image_icon=iconUrl;
            image_small=smallImageUrl;
            image_large=largeImageUrl;
        }
    }

    private List<databaseData> dataInfoList=new List<databaseData>();
    private List<int> dataPriorityList=new List<int>();
    public int downloadPriority=1;
    public GameObject iconImg;
    public GameObject smallImg;
    public GameObject largeImg;

    private bool isIconLoaded=false;
    private bool isSmallImageLoaded=false;
    private bool isLargeImageLoaded=false;

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

    #region Remote config setup
    protected virtual void Start() 
    {
        /*
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
        dependencyStatus = task.Result;
        if(dependencyStatus == Firebase.DependencyStatus.Available) 
        {
            InitializeFirebase();
        }
        else 
        {
            Debug.LogError( "Could not resolve all Firebase dependencies: " + dependencyStatus);
        }
      });
      */
    }

    void InitializeFirebase() 
    {
        /*
        System.Collections.Generic.Dictionary<string, object> defaults = new System.Collections.Generic.Dictionary<string, object>();
        defaults.Add("config_test_string", "default local string");
        defaults.Add("config_test_int", 1);
        defaults.Add("config_test_float", 1.0);
        defaults.Add("config_test_bool", false);
        Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults).ContinueWithOnMainThread(task => {
            Debug.Log("RemoteConfig configured and ready!");
                FetchDataAsync();
            isFirebaseInitialized = true;
        });   
        */     
    }

    /*
    public Task FetchDataAsync()
    {
        Debug.Log("Fetching data...");
        System.Threading.Tasks.Task fetchTask = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.Zero);
        return fetchTask.ContinueWithOnMainThread(FetchComplete);
    }

    void FetchComplete(Task fetchTask) 
    {
        if(fetchTask.IsCanceled) 
        {
            Debug.Log("Fetch canceled.");
        } 
        else if(fetchTask.IsFaulted)
        {
            Debug.Log("Fetch encountered an error.");
        } 
        else if (fetchTask.IsCompleted) {
            Debug.Log("Fetch completed successfully!");
        }

        var info = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.Info;
        switch(info.LastFetchStatus) 
        {
        case Firebase.RemoteConfig.LastFetchStatus.Success:
            Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.ActivateAsync().ContinueWithOnMainThread(task => {
            Debug.Log(String.Format("Remote data loaded and ready (last fetch time {0}).",info.FetchTime));

            string url=Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue("Ludo3D").StringValue.ToString();

            StartCoroutine(DownloadLudoData(url));
        });

        break;
        case Firebase.RemoteConfig.LastFetchStatus.Failure:
        switch(info.LastFetchFailureReason)
        {
        case Firebase.RemoteConfig.FetchFailureReason.Error:
            Debug.Log("Fetch failed for unknown reason");
            break;
        case Firebase.RemoteConfig.FetchFailureReason.Throttled:
            Debug.Log("Fetch throttled until " + info.ThrottledEndTime);
            break;
        }
        break;
        case Firebase.RemoteConfig.LastFetchStatus.Pending:
            Debug.Log("Latest Fetch call still pending.");
        break;
        }
       
    }
    */
    #endregion

    #region Downloading ludo data
    IEnumerator DownloadLudoData(string dataUrl)
    {
        if(string.IsNullOrEmpty(dataUrl))
        {
            Debug.Log("incorrect json url : "+dataUrl);
        }
        else
        {
            using(UnityWebRequest webRequest=UnityWebRequest.Get(dataUrl))
            {
                yield return webRequest.SendWebRequest();
                if(!string.IsNullOrEmpty(webRequest.error))
                {
                    Debug.Log("Error downloading game data : "+webRequest.error);
                }
                else
                {
                    //data found next step what to do with the downloaded data!!!!
                    JSONNode jsonData=SimpleJSON.JSON.Parse(webRequest.downloadHandler.text);
                    for(int i=0;i<jsonData.Count;i++)
                    {
                        string curBundleId=jsonData[i]["bundleID"];
                        if(appBundleID!=curBundleId)
                        {
                            string[] curGameName=curBundleId.Split(char.Parse("."));
                            dataInfoList.Add(new databaseData(curGameName[2],jsonData[i]["bundleID"],jsonData[i]["appUrl"],jsonData[i]["priority"],
                            jsonData[i]["message"],jsonData[i]["images"]["image_icon"],jsonData[i]["images"]["image_small"],
                            jsonData[i]["images"]["image_large"]));
                            dataPriorityList.Add(jsonData[i]["priority"]);
                        }
                    }
                    ComparePriorityAndDownloadAppData();
                }
            }
        }
    }
    #endregion

    #region priority and loading data
    void ComparePriorityAndDownloadAppData()
    {
        string tempMessage=null;
        foreach(databaseData d in dataInfoList)
        {
            if(d.priority==downloadPriority)
            {
                tempMessage=d.message;
                StartCoroutine(DownloadAndCacheIcons(d.image_icon,d.bundleID,d.appUrl));
                StartCoroutine(DownloadAndCacheSmallImages(d.image_small,d.bundleID,d.appUrl));
                StartCoroutine(DownloadAndCacheLargeImages(d.image_large,d.bundleID,d.appUrl));
            }
        }

        Debug.Log(tempMessage);
        //lets activate random crosspromo data
       System.Random random=new System.Random();
       int randomValue=random.Next(0,3);
       switch(randomValue)
       {
           case 0:
            StartCoroutine(LoadGameIcons());
           break;
           case 1:
           //StartCoroutine(LoadSmallImage());
           StartCoroutine(LoadLargeImage());
           break;
           case 2:
           StartCoroutine(LoadLargeImage());
           break;
       }

       //
       if(AnalyticsTracker.instance!=null)
       {
           AnalyticsTracker.instance.TrackCrossPromotionShown();
       }
    }

    IEnumerator LoadGameIcons()
    {
        while(!isIconLoaded)
        {
            yield return new WaitForSeconds(1f);
        }
        iconImg.SetActive(true);
    }

    IEnumerator LoadSmallImage()
    {
        while(!isSmallImageLoaded)
        {
            yield return new WaitForSeconds(1f);
        }
        smallImg.SetActive(true);
    }

    IEnumerator LoadLargeImage()
    {
        while(!isLargeImageLoaded)
        {
            yield return new WaitForSeconds(1f);
        }
        largeImg.SetActive(true);
    }
    #endregion

    #region Downloading images
    IEnumerator DownloadAndCacheIcons(string imageUrl,string bundleId,string gameUrl)
    {
        if(!string.IsNullOrEmpty(imageUrl))
        {
            string[] gameName=bundleId.Split(char.Parse("."));
            string fileName=gameName[2]+".png";
            using(UnityWebRequest webRequest=UnityWebRequestTexture.GetTexture(imageUrl))
            {
                yield return webRequest.SendWebRequest();
                if(!string.IsNullOrEmpty(webRequest.error))
                {
                    Debug.Log("Error : "+webRequest.error);
                }
                else
                {
                    if(webRequest.isDone)
                    {
                        Texture2D texture=DownloadHandlerTexture.GetContent(webRequest);
                        Sprite iconSprite=GetSprite(texture);
                        if(iconImg!=null)
                        {
                            iconImg.GetComponent<RemoteImage>().UpdateRemoteData(iconSprite,gameUrl);
                        }
                        //byte[] bytes=texture.EncodeToPNG();
                        //File.WirteAllBytes(fileName,bytes);
                        isIconLoaded=true;
                    }
                }
            }
        }
        else
        {
            Debug.Log("Invalid image url.");
        }
    }

    IEnumerator DownloadAndCacheSmallImages(string imageUrl,string bundleId,string gameUrl)
    {
        if(!string.IsNullOrEmpty(imageUrl))
        {
            string[] gameName=bundleId.Split(char.Parse("."));
            string fileName=gameName[2]+".png";
            using(UnityWebRequest webRequest=UnityWebRequestTexture.GetTexture(imageUrl))
            {
                yield return webRequest.SendWebRequest();
                if(!string.IsNullOrEmpty(webRequest.error))
                {
                    Debug.Log("Error : "+webRequest.error);
                }
                else
                {
                    if(webRequest.isDone)
                    {
                        Texture2D texture=DownloadHandlerTexture.GetContent(webRequest);
                        Sprite smallSprite=GetSprite(texture);
                        if(smallImg!=null)
                        {
                            smallImg.GetComponent<RemoteImage>().UpdateRemoteData(smallSprite,gameUrl);
                        }
                        //byte[] bytes=texture.EncodeToPNG();
                        //File.WirteAllBytes(fileName,bytes);
                        isSmallImageLoaded=true;
                    }
                }
            }
        }
        else
        {
            Debug.Log("Invalid image url.");
        }
    }

    IEnumerator DownloadAndCacheLargeImages(string imageUrl,string bundleId,string gameUrl)
    {
        if(!string.IsNullOrEmpty(imageUrl))
        {
            string[] gameName=bundleId.Split(char.Parse("."));
            string fileName=gameName[2]+".png";
            using(UnityWebRequest webRequest=UnityWebRequestTexture.GetTexture(imageUrl))
            {
                yield return webRequest.SendWebRequest();
                if(!string.IsNullOrEmpty(webRequest.error))
                {
                    Debug.Log("Error : "+webRequest.error);
                }
                else
                {
                    if(webRequest.isDone)
                    {
                        Texture2D texture=DownloadHandlerTexture.GetContent(webRequest);
                        Sprite largeSprite=GetSprite(texture);
                        if(largeImg!=null)
                        {
                            largeImg.GetComponent<RemoteImage>().UpdateRemoteData(largeSprite,gameUrl);
                        }
                        //byte[] bytes=texture.EncodeToPNG();
                        //File.WirteAllBytes(fileName,bytes);
                        isLargeImageLoaded=true;
                    }
                }
            }
        }
        else
        {
            Debug.Log("Invalid image url.");
        }
    }

    Sprite GetSprite(Texture2D tempTexture)
    {
        Sprite newSprite=Sprite.Create(tempTexture,new Rect(0,0,tempTexture.width,tempTexture.height),Vector2.zero);
        return newSprite;
    }
    #endregion

    #region close btn
    public void CloseThis(GameObject g)
    {
        g.SetActive(false);
    }
    #endregion

    #region Firebase Database
   // DatabaseReference reference;

    public class User 
    {
        public string username;
        public string email;

        public User() 
        {
            
        }

        public User(string username, string email) 
        {
            this.username = username;
            this.email = email;
        }
    }

    private void writeNewUser(string userId, string name, string email) 
    {
        Debug.Log("Time to push user data to database.");
        User user = new User(name, email);
        string json = JsonUtility.ToJson(user);
        //reference.Child("users").Child(userId).SetRawJsonValueAsync(json);
        GetUsers();
    }

    public void GetUsers()
    {
        /*
        FirebaseDatabase.DefaultInstance.GetReference("users").GetValueAsync().ContinueWith(task =>{
            if(task.IsFaulted)
            {
                Debug.Log("Error was:"+task.Exception.Message);
                Debug.LogError("Error was:"+task.Result.Children);
            }
            else if(task.IsCompleted) 
            {
                DataSnapshot snapshot = task.Result;
                foreach(DataSnapshot s in snapshot.Children)
                {
                    IDictionary dictUsers = (IDictionary)s.Value;   
                    Debug.Log(dictUsers["displayName"]);                    
                }   
            }
        });
        */
    }

    ArrayList leaderBoard = new ArrayList();
    protected void ConnectToFirebaseDatabase()
    {
        /*
       FirebaseDatabase.DefaultInstance.GetReference("Leaders").OrderByChild("score").ValueChanged += (object sender2, ValueChangedEventArgs e2) => {
            if (e2.DatabaseError != null) 
            {
                Debug.LogError(e2.DatabaseError.Message);
                return;
            }
            Debug.Log("Received values for Leaders.");
            string title = leaderBoard[0].ToString();
            leaderBoard.Clear();
            leaderBoard.Add(title);
            if (e2.Snapshot != null && e2.Snapshot.ChildrenCount > 0) 
            {
                foreach (var childSnapshot in e2.Snapshot.Children) 
                {
                    if (childSnapshot.Child("score") == null|| childSnapshot.Child("score").Value == null) 
                    {
                        Debug.LogError("Bad data in sample.");
                        break;
                    } 
                    else
                    {
                        Debug.Log("Leaders entry : " +
                        childSnapshot.Child("email").Value.ToString() + " - " +
                        childSnapshot.Child("score").Value.ToString());
                        leaderBoard.Insert(1, childSnapshot.Child("score").Value.ToString()
                        + "  " + childSnapshot.Child("email").Value.ToString());
                    }
                }
            }
        };
        writeNewUser("abc99", "mango", "mango@gmail.com");
        */
    }

    void Getdata()
    {
        //DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("Leaders");
    }
    #endregion
  
}