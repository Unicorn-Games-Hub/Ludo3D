using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// using Firebase;
// using Firebase.Database;
// using Firebase.Extensions;
// using System.Threading.Tasks;
using System;
using UnityEngine.Networking;
using SimpleJSON;

public class FirebaseDataHandler : MonoBehaviour
{
    public static FirebaseDataHandler instance;
    
    //Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;
    //DatabaseReference reference;

    private const int MaxScores = 5;
    private string email = "xyz@gmail.com";
    private int score = 90;
    
    private string userID=null;


    [Header("UI")]
    public GameObject saveDataUI;
    public GameObject getDataUI;

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

   void Start()
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
    //    FirebaseApp app = FirebaseApp.DefaultInstance;
    //    StartCoroutine(GetLudoDataFromDatabase());
    //    GetPlayerData();
   }

    ArrayList leaderBoard = new ArrayList();

   void HandleGameData()
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
        */
   }

    public void AddScore(int score,string email) 
    {
        /*
      if (score == 0 || string.IsNullOrEmpty(email)) 
      {
        Debug.Log("invalid score or email.");
        return;
      }
      Debug.Log(String.Format("Attempting to add score {0} {1}", email, score.ToString()));

      DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("Leaders");

      Debug.Log("Running Transaction...");
      // Use a transaction to ensure that we do not encounter issues with
      // simultaneous updates that otherwise might create more than MaxScores top scores.
      reference.RunTransaction(AddScoreTransaction)
        .ContinueWithOnMainThread(task => {
          if (task.Exception != null) 
          {
            Debug.Log(task.Exception.ToString());
          } 
          else if (task.IsCompleted) 
          {
            Debug.Log("Transaction complete.");
          }
        });
        */
    }

    /*
    TransactionResult AddScoreTransaction(MutableData mutableData) 
    {
      List<object> leaders = mutableData.Value as List<object>;

      if (leaders == null) {
        leaders = new List<object>();
      } else if (mutableData.ChildrenCount >= MaxScores) {
        // If the current list of scores is greater or equal to our maximum allowed number,
        // we see if the new score should be added and remove the lowest existing score.
        long minScore = long.MaxValue;
        object minVal = null;
        foreach (var child in leaders) {
          if (!(child is Dictionary<string, object>))
            continue;
          long childScore = (long)((Dictionary<string, object>)child)["score"];
          if (childScore < minScore) {
            minScore = childScore;
            minVal = child;
          }
        }
        // If the new score is lower than the current minimum, we abort.
        if (minScore > score) {
          return TransactionResult.Abort();
        }
        // Otherwise, we remove the current lowest to be replaced with the new score.
        leaders.Remove(minVal);
      }

      // Now we add the new score as a new entry that contains the email address and score.
      Dictionary<string, object> newScoreMap = new Dictionary<string, object>();
      newScoreMap["score"] = score;
      newScoreMap["email"] = email;
      leaders.Add(newScoreMap);
      // You must set the Value to indicate data at that location has changed.
      mutableData.Value = leaders;
      return TransactionResult.Success(mutableData);
    }
    */
    #region Retriving Game Data From Database
    public class LudoUpdateData
    {
        public string message;
        public int radioValue;
        public int checkboxValue;
        public int totalWins;
        public int popupShown;

        public LudoUpdateData(string msg,int noOfWins,int radVal,int cboxVal)
        {
            this.message=msg;
            this.radioValue=radVal;
            this.totalWins=noOfWins;
            this.checkboxValue=cboxVal;
        }
    }

    public List<LudoUpdateData> updataDataList=new List<LudoUpdateData>();
    private string receivedMessage;
    private string receivedImageUrl;
    private string appUrl;
    private int r1,c1;

    private IEnumerator GetLudoDataFromDatabase()
    {   
       
        bool downloadCompleted=false;
        /*
        FirebaseDatabase.DefaultInstance.GetReference("LudoUpdate").GetValueAsync().ContinueWith(task => {
        if (task.IsFaulted) 
        {
            Debug.Log("Error Getting data from firebaseDatabase.");
        }
        else if (task.IsCompleted) 
        {
            DataSnapshot dataSnapshot = task.Result;
            if(dataSnapshot!=null&&dataSnapshot.ChildrenCount>0)
            {
                //updataDataList.Add(new LudoUpdateData(dataSnapshot.Child("message").Value,dataSnapshot.Child("message").Value));
                //Debug.Log(dataSnapshot.Child("message").Value.ToString());
                receivedMessage=dataSnapshot.Child("message").Value.ToString();
                receivedImageUrl=dataSnapshot.Child("imageUrl").Value.ToString();
                appUrl=dataSnapshot.Child("appUrl").Value.ToString();
                r1=Int32.Parse(dataSnapshot.Child("radioValue").Value.ToString());
                c1=Int32.Parse(dataSnapshot.Child("checkBoxValue").Value.ToString());
                downloadCompleted=true;
            }
        }});
        */

        while(!downloadCompleted)
        {
            yield return new WaitForSeconds(1f);
        }
        StartCoroutine(UpdateData());
        
    }

    IEnumerator UpdateData()
    {
        yield return new WaitForSeconds(1f);
        // if(getDataUI!=null)
        // {
        //     Debug.Log(receivedMessage);
        //     getDataUI.GetComponent<GetGameData>().UpdateGameData(receivedMessage,appUrl,r1,c1);
        // }
        // if(!string.IsNullOrEmpty(receivedImageUrl))
        // {
        //     StartCoroutine(DownloadImageFromDatabase(receivedImageUrl));
        // }
    }
    #endregion

    #region Download image
    IEnumerator DownloadImageFromDatabase(string tempUrl)
    {
        using(UnityWebRequest webRequest=UnityWebRequestTexture.GetTexture(tempUrl))
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
                    Sprite tempSprite=GetSprite(texture);
                    if(getDataUI!=null)
                    {
                        getDataUI.GetComponent<GetGameData>().UpdatePicture(tempSprite);
                    }
                    //byte[] bytes=texture.EncodeToPNG();
                    //File.WirteAllBytes(fileName,bytes);
                }
            }
        }
    }

    Sprite GetSprite(Texture2D tempTexture)
    {
        Sprite newSprite=Sprite.Create(tempTexture,new Rect(0,0,tempTexture.width,tempTexture.height),Vector2.zero);
        return newSprite;
    }
    #endregion

    public void SubmitGameData(string msg,int totalWins,int rvalue,int cvalue)
    {
        // reference = FirebaseDatabase.DefaultInstance.RootReference;
        // LudoUpdateData ludoData=new LudoUpdateData(msg,totalWins,rvalue,cvalue);
        // string json = JsonUtility.ToJson(ludoData);
        // string userId=SystemInfo.deviceUniqueIdentifier;
        // reference.Child("Users").Child(userId).SetRawJsonValueAsync(json).ContinueWith(task => {
        //     if(task.IsCanceled)
        //     {
        //         Debug.Log("data updated successfully");
        //     }
        //     else
        //     {
        //         Debug.Log("error updating data!");
        //     }
        // });
    }

    #region Retriving Player data from database
    public void GetPlayerData()
    {
    //     userID=SystemInfo.deviceUniqueIdentifier;
    //     int popUpValue=0;
    //    FirebaseDatabase.DefaultInstance.GetReference("Users").Child(userID).GetValueAsync().ContinueWith(task => {
    //     if (task.IsFaulted) 
    //     {
    //         Debug.Log("Error Getting data from firebaseDatabase.");
    //     }
    //     else if (task.IsCompleted) 
    //     {
    //         DataSnapshot dataSnapshot = task.Result;
    //         if(dataSnapshot!=null&&dataSnapshot.ChildrenCount>0)
    //         {
    //             popUpValue=Int32.Parse(dataSnapshot.Child("popupShown").Value.ToString());
    //             HandlePopUp(popUpValue);
    //         }
    //     }});
    }

    void HandlePopUp(int pValue)
    {
        if(pValue==0)
        {
            Debug.Log("popup has not been shown yet");
        }
        else
        {
            Debug.Log("popup has been shown already!");
        }
        UpdatePopUpValue();
    }

    public void UpdatePopUpValue()
    {
        // int newval=2;
        // FirebaseDatabase.DefaultInstance.GetReference("Users").Child(userID).Child("popupShown").SetValueAsync(newval).ContinueWith(task => {
        //     if(task.IsCompleted) 
        //     {
        //         Debug.Log("value updated successfully");
        //     }
        // });
    }
    #endregion
   
}
