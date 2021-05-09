using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    public Transform coinContainer;
    public Transform coinPathContainer;

    [Header("Transform to hold generated coins")]
    public Transform generatedCoinsHolder;

    [Header("Home Path")]
    public Transform[] homePaths;

    [Header("Start Position")]
    public Transform[] startPoints;

    [Range(2,4)]
    public int noOfCoins=4;

    [Header("Game Behaviour")]
    [SerializeField]private int coinOutAt=1;
    [SerializeField]private int rollChanceAt=6;

    [Header("Players")]
    public int turnCounter=0;
    private int curTurn=0;

    public enum playerType
    {
        Human,
        Bot,
        None
    }

    [System.Serializable]
    public class infoHolder
    {
        public string colorName;
        public int playerID;
        public Transform coinPrefab;
        public playerType player;
        public int initialPosIndex=0;
        public Color coinColor;
        public List<Transform> outCoins=new List<Transform>();
    }

    public List<infoHolder> players=new List<infoHolder>();
    public List<int> gamePlayersList=new List<int>();

    private int currentDiceValue;

    [Header("Game Rules")]
    public bool enterHomeWithOutCutting=true;

    //clickable layerMask
    public LayerMask coinLayer;
    private Vector3 newCoinPos;

    //Raycasting
    private Ray ray;
    private RaycastHit hit;

    public enum Moves
    {
        None,
        TakeOut,
        Move
    }

    public Moves gameMove;

    [Header("Coin Safe Pos Index")]
    public List<int> safePosIndexList=new List<int>();

    //Refrence to the dice material
    public Material diceMat;
 

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
        GenerateCoins();
    }

    void GenerateCoins()
    {
        for(int i=0;i<players.Count;i++)
        {
           for(int j=0;j<noOfCoins;j++)
           {
               if(players[i].player==playerType.Human||players[i].player==playerType.Bot)
               {
                    Transform newCoin=Instantiate(players[i].coinPrefab,coinContainer.GetChild(i).GetChild(j).position,players[i].coinPrefab.transform.rotation);
                    newCoin.SetParent(generatedCoinsHolder.GetChild(i).transform);
                    newCoin.GetComponent<Coin>().HandleCoinInfo(players[i].playerID,newCoin.transform.localPosition);
                    if(!gamePlayersList.Contains(players[i].playerID))
                    {
                        gamePlayersList.Add(players[i].playerID);
                    }
               }
           }
        }
        GetRandomTurn();
    }

    #region turn
    void GetRandomTurn()
    {
        int randomTurn=Random.Range(0,gamePlayersList.Count);
        curTurn=randomTurn;
        turnCounter=gamePlayersList[randomTurn];
        HandleDiceRoll(turnCounter);
    }

    void UpdateTurn()
    {
        if(currentDiceValue!=rollChanceAt)
        {
            curTurn++;
            if(curTurn>gamePlayersList.Count-1)
            {
                curTurn=0;
            }
        }
        turnCounter=gamePlayersList[curTurn];
        HandleDiceRoll(turnCounter);
    }
    #endregion

    #region Handling obtained dice value
    public void HandleObtainedDiceValue(int diceValue)
    {
        currentDiceValue=diceValue;
        if(diceValue==coinOutAt)
        {
            gameMove=Moves.TakeOut;
            for(int i=0;i<generatedCoinsHolder.GetChild(turnCounter).childCount;i++)
            {
                generatedCoinsHolder.GetChild(turnCounter).GetChild(i).GetComponent<Coin>().SetClickable(true);
            }
            Debug.Log("Hello "+players[turnCounter].colorName+" its time to take your coins from base.");
        }
        else if(diceValue==rollChanceAt)
        {
            Debug.Log("Congrats you got a chance to roll again..");
            if(players[turnCounter].outCoins.Count>0)
            {
                if(players[turnCounter].outCoins.Count==1)
                {
                    HanldeAutoMoveOfSingleCoin();
                }
                else
                {
                    gameMove=Moves.Move;
                    for(int i=0;i<players[turnCounter].outCoins.Count;i++)
                    {
                        players[turnCounter].outCoins[i].GetComponent<Coin>().isClickable=true;
                    }
                }
            }
            else
            {
                gameMove=Moves.None;
                HandleDiceRoll(turnCounter);
            }
        }
        else
        {
            if(players[turnCounter].outCoins.Count>0)
            {
                int count=0;
                for(int i=0;i<players[turnCounter].outCoins.Count;i++)
                {
                    Coin outCoin=players[turnCounter].outCoins[i].GetComponent<Coin>();
                    if(!outCoin.onWayToHome)
                    {
                        count++;
                        outCoin.SetClickable(true);
                    }
                    else
                    {
                        if(outCoin.stepCounter<homePaths[turnCounter].childCount)
                        {
                            if((outCoin.stepCounter+currentDiceValue)<homePaths[turnCounter].childCount)
                            {
                                count++;
                                outCoin.SetClickable(true);
                            }
                        }
                    }
                }

                if(count==1)
                {
                    players[turnCounter].outCoins[0].GetComponent<Coin>().SetClickable(false);
                    HanldeAutoMoveOfSingleCoin();
                }
                else if(count==0)
                {
                    gameMove=Moves.None;
                   UpdateTurn();
                }
                else
                {
                    gameMove=Moves.Move;
                    for(int i=0;i< players[turnCounter].outCoins.Count;i++)
                    {
                        players[turnCounter].outCoins[i].GetComponent<Coin>().isClickable=true;
                    }
                }
            }
            else
            {
                gameMove=Moves.None;
                UpdateTurn();  
            }
        }
    }

    //called only when single coin is out
    void HanldeAutoMoveOfSingleCoin()
    {
        Coin tempCoin=players[turnCounter].outCoins[0].GetComponent<Coin>();
        if(!tempCoin.onWayToHome)
        {
            StartCoroutine(UpdateCoinPosition(tempCoin)); 
        }
        else
        {
           if((tempCoin.stepCounter+currentDiceValue)<homePaths[turnCounter].childCount)
            {
                StartCoroutine(UpdateCoinPosition(tempCoin));
            }
            else
            {
                UpdateTurn();  
            }
        }
    }

    void HandleDiceRoll(int turnValue)
    {
        if(players[turnValue].player==playerType.Human)
        {
            Dice.instance.canRollDice=true;
        }
        else if(players[turnValue].player==playerType.Bot)
        {
            Dice.instance.canRollDice=false;
        }
        diceMat.color=players[turnCounter].coinColor;
    }
    #endregion

    #region Gameplay
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            ray=Camera.main.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray,out hit,coinLayer))
            {
                if(hit.collider!=null&&hit.collider.gameObject.GetComponent<Coin>()!=null)
                {
                    Transform clickedCoin=hit.collider.gameObject.transform;

                    if(clickedCoin.GetComponent<Coin>().id==turnCounter&&clickedCoin.GetComponent<Coin>().isClickable)
                    {
                        Coin newCoin=clickedCoin.GetComponent<Coin>();
                        if(gameMove==Moves.TakeOut)
                        {
                            if(clickedCoin.GetComponent<Coin>().atBase)
                            {
                                if(!players[turnCounter].outCoins.Contains(clickedCoin))
                                {
                                    players[turnCounter].outCoins.Add(clickedCoin);
                                }
                                StartCoroutine(UpdateCoinPosition(newCoin));
                            }
                            else
                            {
                                StartCoroutine(UpdateCoinPosition(newCoin));
                            }
                        }
                        else if(gameMove==Moves.Move)
                        {
                            StartCoroutine(UpdateCoinPosition(newCoin));
                        }

                        //Disable clicking for the every coin of current turn
                        for(int i=0;i<generatedCoinsHolder.GetChild(turnCounter).childCount;i++)
                        {
                            generatedCoinsHolder.GetChild(turnCounter).GetChild(i).GetComponent<Coin>().isClickable=false;
                        }
                    }
                }
            }
        }
    }

    IEnumerator UpdateCoinPosition(Coin coin)
    {
        if(coin.atBase)
        {
            int startingIndex=players[turnCounter].initialPosIndex;
            iTween.MoveTo(coin.gameObject, iTween.Hash("position", new Vector3(coinPathContainer.GetChild(startingIndex).position.x,coin.transform.position.y,coinPathContainer.GetChild(startingIndex).position.z), 
            "speed", coinMoveSpeed, 
            "easetype", iTween.EaseType.linear,
            "oncomplete", "UpdateTurnAfterFirstMove", 
            "oncompletetarget", this.gameObject
            ));
            coin.isSafe=true;
            coin.atBase=false;
        }
        else
        {
            int initialTargetCount=coin.stepCounter+players[turnCounter].initialPosIndex;
            int newTargetCount=initialTargetCount+currentDiceValue;
            int tempStepCount=0;

            while((coin.stepCounter+players[turnCounter].initialPosIndex)<newTargetCount)
            {
                if(coin.isReadyForHome)
                {
                    if(coin.stepCounter>=coinPathContainer.childCount-2)
                    {
                        coin.canGoHome=true;
                        tempStepCount=(newTargetCount-(coinPathContainer.childCount-2))-1;
                    }
                }
                
                if(coin.canGoHome)
                {
                    if(coin.stepCounter>=coinPathContainer.childCount-2)
                    {
                        coin.onWayToHome=true;
                        coin.stepCounter=0;
                        newTargetCount=tempStepCount;
                    }
                    else
                    {
                        coin.stepCounter++;
                    }

                    if(coin.onWayToHome)
                    {
                        if(coin.stepCounter<6)
                        {
                            newCoinPos=new Vector3(homePaths[turnCounter].GetChild(coin.stepCounter).position.x,coin.transform.position.y,homePaths[turnCounter].GetChild(coin.stepCounter).position.z);
                            if(coin.stepCounter==5)
                            {
                                coin.isClickable=false;
                                coin.atHome=true;
                                players[turnCounter].outCoins.Remove(coin.transform);
                                HandleDiceRoll(turnCounter);
                            }
                        }
                    }
                }
                else
                {
                    if(coin.stepCounter>=coinPathContainer.childCount-1)
                    {
                        newTargetCount=(newTargetCount-coinPathContainer.childCount-1);
                        coin.stepCounter=0;
                    }
                    else
                    {
                        coin.stepCounter++;
                    }
                }

                int curCoinPosIndex=(coin.stepCounter+players[turnCounter].initialPosIndex)%(coinPathContainer.childCount);
                if(!coin.onWayToHome)
                {
                    newCoinPos=new Vector3(coinPathContainer.GetChild(curCoinPosIndex).position.x,coin.transform.position.y,
                    coinPathContainer.GetChild(curCoinPosIndex).position.z);
                }

                iTween.MoveTo(coin.gameObject, iTween.Hash("position",newCoinPos, 
                "speed", coinMoveSpeed, 
                "easetype", iTween.EaseType.linear,
                "oncomplete", "TargetReached", 
                "oncompletetarget", this.gameObject
                ));

                yield return new WaitUntil(() => isStepCompleted);
                yield return new WaitForSeconds(0.1f);
                isStepCompleted = false;
            }

            //lets check if the current coin is safe or not
            coin.isSafe=IsCoinSafe(newTargetCount);

            //checking if coin is safe or can be cut
            if(!coin.onWayToHome)
            {
                if(CutTheCoin(newTargetCount))
                {
                    //give turn as a cut bonus
                    HandleDiceRoll(turnCounter);
                }
                else
                {
                    UpdateTurn();
                }
            }
            else if(!coin.atHome)
            {
                UpdateTurn();
            }
            else
            {
                //time to check the winner
                CheckForWinner();
            }
        }
    } 

    void UpdateTurnAfterFirstMove()
    {
        UpdateTurn();
    }

    private bool isStepCompleted=false;

    void TargetReached()
    {
        isStepCompleted=true;
    }

    private float curTimer=0f;
    private float waitTime=0.3f;

    private float coinMoveSpeed=3f;

    private bool move=false;
    private int targetIndexValue;
    #endregion

    #region Safe
    public bool IsCoinSafe(int curCoinPosValue)
    {
        bool safe=false;
        int tempPosValue=curCoinPosValue;
        if(tempPosValue>coinPathContainer.childCount)
        {
            tempPosValue=(tempPosValue-coinPathContainer.childCount);
        }
        if(safePosIndexList.Contains(tempPosValue))
        {
            safe=true;
        }
        return safe;
    }
    #endregion

    #region Cutting
    public bool CutTheCoin(int coinPositionIndex)
    {
        bool canCut=false;
        for(int i=0;i<gamePlayersList.Count;i++)
        {
            for(int j=0;j<players[gamePlayersList[i]].outCoins.Count;j++)
            {
                if(turnCounter!=players[gamePlayersList[i]].playerID)
                {
                    Coin newTempCoin=players[gamePlayersList[i]].outCoins[j].GetComponent<Coin>();

                    int newTempCount=newTempCoin.stepCounter+players[gamePlayersList[i]].initialPosIndex;
                    int newTempCount1=coinPositionIndex;

                    if(newTempCount>coinPathContainer.childCount)
                    {
                        newTempCount=(newTempCount-coinPathContainer.childCount);
                    }

                    if(newTempCount1>coinPathContainer.childCount)
                    {
                        newTempCount1=(newTempCount1-coinPathContainer.childCount);
                    }
                    
                    if(newTempCount==newTempCount1&&!newTempCoin.isSafe&&!newTempCoin.onWayToHome)
                    {
                        iTween.MoveTo(newTempCoin.gameObject, iTween.Hash("position", 
                        newTempCoin.initialPosInfo, 
                        "speed", coinMoveSpeed, 
                        "easetype", iTween.EaseType.linear,
                        "oncomplete", "CutCoinReset", 
                        "oncompletetarget", this.gameObject
                        ));
                        newTempCoin.stepCounter=0;
                        newTempCoin.atBase=true;
                        if(players[gamePlayersList[i]].outCoins.Contains(newTempCoin.gameObject.transform))
                        {
                            players[gamePlayersList[i]].outCoins.Remove(newTempCoin.gameObject.transform);
                        }
                        canCut=true;
                    }   
                }
            }
        }
        return canCut;
    }

    void CutCoinReset()
    {

    }
    #endregion
    
    #region Win lose
    void CheckForWinner()
    {
        Debug.Log("time to find out who is the winner!");
    }
    #endregion

    #region BOT Behaviour

    #endregion
}
