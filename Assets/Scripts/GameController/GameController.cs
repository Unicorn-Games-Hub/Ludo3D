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

    [Header("Start Rotation")]
    public Vector3[] startRotations;

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

    //bot behaviour selector
    public enum BotType
    {
        Normal,
        Easy,
        Aggressive
    }

    public BotType gameBotType;

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

    [Header("Ingame player indicator")]
    public Transform playerIndicators;

    [Header("Dice Position During Roll")]
    public Transform ludoDice;
    public Vector3[] diceInitialPos;
    private float diceMoveSpeed=15f;
    private Vector3 newDicePosition=Vector3.zero;
    private bool moveDice=false;
 

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
        if(GameDataHolder.instance!=null)
        {
            for(int i=0;i<GameDataHolder.instance.playerIndex.Length;i++)
            {
                if(GameDataHolder.instance.playerIndex[i]==0)
                {
                    players[i].player=playerType.Human;
                }
                else if(GameDataHolder.instance.playerIndex[i]==1)
                {
                    players[i].player=playerType.Bot;
                }
                else
                {
                    players[i].player=playerType.None;
                    playerIndicators.GetChild(i).gameObject.SetActive(false);
                }
            }
        }
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
                    //
                    newCoin.transform.localRotation=Quaternion.Euler(0f,startRotations[i].y,0f);
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
        //updating dice position inside board
       UpdateDicePositionOnBoard();
    }

    void UpdateTurn()
    {
        StartCoroutine(WaitBeforeUpdatingTurn());
    }

    IEnumerator WaitBeforeUpdatingTurn()
    {
        yield return new WaitForSeconds(1f);
        if(currentDiceValue!=rollChanceAt)
        {
            curTurn++;
            if(curTurn>gamePlayersList.Count-1)
            {
                curTurn=0;
            }
        }
        turnCounter=gamePlayersList[curTurn];
        // HandleDiceRoll(turnCounter);
        UpdateDicePositionOnBoard();
        yield return new WaitForSeconds(0.5f);
        HandleDiceRoll(turnCounter);
    }
    #endregion

    #region Handling obtained dice value
    public void HandleObtainedDiceValue(int diceValue)
    {
        currentDiceValue=diceValue;
        /* 
        if(diceValue==coinOutAt)
        {
            gameMove=Moves.TakeOut;
            if(players[turnCounter].player==playerType.Human)
            {
                for(int i=0;i<generatedCoinsHolder.GetChild(turnCounter).childCount;i++)
                {
                    generatedCoinsHolder.GetChild(turnCounter).GetChild(i).GetComponent<Coin>().SetClickable(true);
                }
            }
            else if(players[turnCounter].player==playerType.Bot)
            {
                StartCoroutine(CoinOutBotBehaviour());
            }
        }
        else if(diceValue==rollChanceAt)
        {
            if(players[turnCounter].outCoins.Count>0)
            {
                ChooseCoinForMovement();
            }
            else
            {
                gameMove=Moves.None;
                HandleDiceRoll(turnCounter);
            }
        }
        else
        {
            ChooseCoinForMovement();
        }*/

        if(diceValue==coinOutAt)
        {
            if(players[turnCounter].outCoins.Count>0)
            {
                if(players[turnCounter].player==playerType.Human)
                {
                    List<Coin> clickableCoinsList=new List<Coin>();
                    //lets check if there are some coins inside base or not if yes make base coin clickable
                    for(int i=0;i<generatedCoinsHolder.GetChild(turnCounter).childCount;i++)
                    {
                        Coin tempCoin=generatedCoinsHolder.GetChild(turnCounter).GetChild(i).GetComponent<Coin>();
                        if(tempCoin.atBase||!tempCoin.onWayToHome)
                        {
                            clickableCoinsList.Add(tempCoin);
                        }
                        else
                        {
                           if(tempCoin.onWayToHome)
                           {
                                if((tempCoin.stepCounter+currentDiceValue)<homePaths[turnCounter].childCount)
                                {
                                    clickableCoinsList.Add(tempCoin);
                                }
                           }
                        }
                    }

                    if(clickableCoinsList.Count>0)
                    {
                        foreach (Coin c in clickableCoinsList)
                        {
                            c.SetClickable(true);
                        }
                    }
                    else
                    {
                        UpdateTurn();
                    }
                }
                else if(players[turnCounter].player==playerType.Bot)
                {
                    ChooseCoinForMovement();
                }
            }
            else
            {
                if(players[turnCounter].player==playerType.Human)
                {
                    gameMove=Moves.TakeOut;
                    for(int i=0;i<generatedCoinsHolder.GetChild(turnCounter).childCount;i++)
                    {
                        generatedCoinsHolder.GetChild(turnCounter).GetChild(i).GetComponent<Coin>().SetClickable(true);
                    }
                }
                else if(players[turnCounter].player==playerType.Bot)
                {
                    StartCoroutine(CoinOutBotBehaviour());
                }
            }
        }
        else
        {
            ChooseCoinForMovement();
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
            StartCoroutine(HandleBotTurn());
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

        if(moveDice)
        {   
            ludoDice.transform.position=Vector3.MoveTowards(ludoDice.transform.position,newDicePosition,Time.deltaTime*diceMoveSpeed);
            if(Vector3.Distance(ludoDice.transform.position,newDicePosition)<0.1f)
            {
                moveDice=false;
            }
        }
    }

    IEnumerator UpdateCoinPosition(Coin coin)
    {
        if(coin.atBase)
        {
            int startingIndex=players[turnCounter].initialPosIndex;
            iTween.MoveTo(coin.gameObject, iTween.Hash("position", new Vector3(coinPathContainer.GetChild(startingIndex).position.x,-0.01f/*coin.transform.position.y*/,coinPathContainer.GetChild(startingIndex).position.z), 
            "speed", coinMoveSpeed, 
            "easetype", iTween.EaseType.linear,
            "oncomplete", "UpdateTurnAfterFirstMove", 
            "oncompletetarget", this.gameObject
            ));
            coin.isSafe=true;
            coin.atBase=false;
            charFromHome=coin.transform;
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
                                coin.onWayToHome=false;
                                coin.canGoHome=false;
                                coin.isReadyForHome=false;
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
                UpdateCharacterRotation(coin.transform);
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
                CheckForWinner(coin);
            }
        }
    } 

    Transform charFromHome=null;
    void UpdateTurnAfterFirstMove()
    {
        //for updating character rotation on the board
        UpdateCharacterRotation(charFromHome);
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
                    
                    if(newTempCount==newTempCount1&&!newTempCoin.isSafe&&!newTempCoin.onWayToHome&&!newTempCoin.atHome)
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
    private List<int> winnersList=new List<int>();

    void CheckForWinner(Coin coin)
    {
        int count=0;
        for(int i=0;i<generatedCoinsHolder.GetChild(turnCounter).childCount;i++)
        {
            if(generatedCoinsHolder.GetChild(turnCounter).GetChild(i).GetComponent<Coin>().atHome)
            {
                count++;
            }
        }

        if(count==noOfCoins)
        {
            Debug.Log("winner!");
            winnersList.Add(coin.id);
            //remove player from the list
            gamePlayersList.Remove(coin.id);
            //show win ui and give options for continue

            if(gamePlayersList.Count<2)
            {
                //show final win lose ui
                ShowPlayerRank();
            }
            else
            {
                UpdateTurn();
            }
        }
        else
        {
            HandleDiceRoll(turnCounter);
        }
    }

    void ShowPlayerRank()
    {
        Debug.Log("Congratulations ,"+players[winnersList[0]].colorName+" won the game");
    }
    #endregion

    #region BOT Behaviour
    IEnumerator HandleBotTurn()
    {
        yield return new WaitForSeconds(1f);
        StartCoroutine(Dice.instance.RolltheDice());
    }

    IEnumerator CoinOutBotBehaviour()
    {
        yield return new WaitForSeconds(1f);
        if(players[turnCounter].outCoins.Count<1)
        {
            HandleNewCoinOutBehaviour();
        }
        else
        {
            ChooseCoinForMovement();
        }
    }

    void HandleBotBehaviour()
    {
    }

    #region  Bringing coin out of the base 
    void HandleNewCoinOutBehaviour()
    {
        List<Transform> coinsAtBase=new List<Transform>();
        for(int i=0;i<generatedCoinsHolder.GetChild(turnCounter).childCount;i++)
        {
            Transform baseCoin=generatedCoinsHolder.GetChild(turnCounter).GetChild(i).transform;
            if(baseCoin.GetComponent<Coin>().atBase)
            {
                if(!players[turnCounter].outCoins.Contains(baseCoin))
                {
                    coinsAtBase.Add(baseCoin);
                }
            }
        }
        int newOutCoinIndex=Random.Range(0,coinsAtBase.Count);
        Transform coinToBringOut=coinsAtBase[newOutCoinIndex];
        StartCoroutine(UpdateCoinPosition(coinToBringOut.GetComponent<Coin>()));
        if(!players[turnCounter].outCoins.Contains(coinToBringOut))
        {
            players[turnCounter].outCoins.Add(coinToBringOut);
        }
    }
    #endregion

    #region Moving the coin which can cut the opponents coin
    void HandleCutCoinMovement()
    {
        Coin coinNeedToBeCut=avalibleCutCoinList[0].coinThatCanBeCut;
        Coin coinThatWillCut=avalibleCutCoinList[0].coinThatCuts;
        for(int i=0;i<avalibleCutCoinList.Count;i++)
        {
            if(avalibleCutCoinList[i].coinThatCanBeCut.stepCounter>coinNeedToBeCut.stepCounter)
            {
                coinNeedToBeCut=avalibleCutCoinList[i].coinThatCanBeCut;
                coinThatWillCut=avalibleCutCoinList[i].coinThatCuts;
            }
        }
        StartCoroutine(UpdateCoinPosition(coinThatWillCut));
    }
    #endregion
    
    void ChooseCoinForMovement()
    {
        List<Coin> tempPublicLaneCoinList=new List<Coin>();
        List<Coin> tempHomeLaneCoinList=new List<Coin>();

        if(players[turnCounter].outCoins.Count>0)
        {
            int coinCount=0;
            Coin outCoin=null;
            for(int i=0;i<players[turnCounter].outCoins.Count;i++)
            {
                outCoin=players[turnCounter].outCoins[i].GetComponent<Coin>();
                if(!outCoin.onWayToHome)
                {
                    tempPublicLaneCoinList.Add(outCoin);
                    coinCount++;
                }
                else
                {
                    if(outCoin.stepCounter<homePaths[turnCounter].childCount)
                    {
                        if((outCoin.stepCounter+currentDiceValue)<homePaths[turnCounter].childCount)
                        {
                            tempHomeLaneCoinList.Add(outCoin);
                            coinCount++;
                        }
                    }
                }
            }
            
            if(coinCount==0)
            {
                gameMove=Moves.None;
                UpdateTurn();
            }
            else if(coinCount==1)
            {
                //single coin then??
                if(players[turnCounter].player==playerType.Human)
                {
                    StartCoroutine(UpdateCoinPosition(outCoin));
                }
                else if(players[turnCounter].player==playerType.Bot)
                {
                    //check
                    HandleMovalbeCoinBehaviour(tempPublicLaneCoinList);
                }
            }
            else
            {
                //multiple coins avaliable for moving
                if(players[turnCounter].player==playerType.Human)
                {
                    gameMove=Moves.Move;
                    for(int i=0;i< players[turnCounter].outCoins.Count;i++)
                    {
                        players[turnCounter].outCoins[i].GetComponent<Coin>().isClickable=true;
                    }
                }
                else if(players[turnCounter].player==playerType.Bot)
                {
                    //from here we can change behaviour of bot according to its type
                    //lets check if we can cut any other coin or can move to the safe zone
                    if(IsCoinCutAvaliable()==true)
                    {
                        HandleCutCoinMovement();
                    }
                    else
                    {
                        if(tempHomeLaneCoinList.Count>0)
                        {
                            Coin movableHomeCoin=null;
                            for(int i=0;i<tempHomeLaneCoinList.Count;i++)
                            {
                                int homeCoinStepCount=tempHomeLaneCoinList[i].stepCounter+currentDiceValue;
                                if(homeCoinStepCount<homePaths[turnCounter].childCount)
                                {
                                   movableHomeCoin=tempHomeLaneCoinList[i];
                                }
                            }

                            if(movableHomeCoin!=null)
                            {
                                StartCoroutine(UpdateCoinPosition(movableHomeCoin));
                            }
                            else
                            {
                               HandleMovalbeCoinBehaviour(tempPublicLaneCoinList);
                            }
                        }
                        else 
                        {
                           HandleMovalbeCoinBehaviour(tempPublicLaneCoinList);
                        }
                    }
                }
            }
        }
        else
        {
            gameMove=Moves.None;
            UpdateTurn(); 
        }
    }

    public class CoinCutInfo
    {
        public Coin coinThatCuts;
        public Coin coinThatCanBeCut;
        //here 0 is for human and 1 for bot
        public int coinPlayID;
    }

    private List<CoinCutInfo> avalibleCutCoinList=new List<CoinCutInfo>();

    private bool IsCoinCutAvaliable()
    {
        if(avalibleCutCoinList.Count>0)
        {
            avalibleCutCoinList.Clear();
        }

        for(int i=0;i<gamePlayersList.Count;i++)
        {
            for(int j=0;j<players[gamePlayersList[i]].outCoins.Count;j++)
            {
                if(turnCounter!=players[gamePlayersList[i]].playerID)
                {
                    Coin opponentsCoin=players[gamePlayersList[i]].outCoins[j].GetComponent<Coin>();
                    Coin myCoin=players[turnCounter].outCoins[j].GetComponent<Coin>();
                    
                    int opponentCoinPosIndex=opponentsCoin.stepCounter+players[gamePlayersList[i]].initialPosIndex;
                    int myCoinNextPosIndex=myCoin.stepCounter+players[turnCounter].initialPosIndex+currentDiceValue;

                    if(opponentCoinPosIndex>coinPathContainer.childCount)
                    {
                        opponentCoinPosIndex=opponentCoinPosIndex-coinPathContainer.childCount;
                    }

                    if(myCoinNextPosIndex>coinPathContainer.childCount)
                    {
                        myCoinNextPosIndex=myCoinNextPosIndex-coinPathContainer.childCount;
                    }
                   
                    if(myCoinNextPosIndex==opponentCoinPosIndex&&!opponentsCoin.isSafe&&!opponentsCoin.onWayToHome&&!opponentsCoin.atHome)
                    {
                        CoinCutInfo coinInfo=new CoinCutInfo();
                        coinInfo.coinThatCuts=myCoin;
                        coinInfo.coinThatCanBeCut=opponentsCoin;
                        //so that we can choose to cut player coin or bot coin
                        if(players[gamePlayersList[i]].player==playerType.Human)
                        {
                            coinInfo.coinPlayID=0;
                        }
                        else if(players[gamePlayersList[i]].player==playerType.Bot)
                        {
                            coinInfo.coinPlayID=1;
                        }
                        avalibleCutCoinList.Add(coinInfo);
                    }
                }
            }
        }

       if(avalibleCutCoinList.Count>0)
       {
           return true;
       }
       else
       {
           return false;
       }
    }


    void HandleMovalbeCoinBehaviour(List<Coin> tempPublicLaneCoinList)
    {
        if(tempPublicLaneCoinList.Count>0)
        {
            Coin coinThatWillBeSafe=null;
            Coin coinThatMovedFarthest=tempPublicLaneCoinList[0];
            //lets check any of the coin could be moved to the safe position on adding current dice vale
            for(int i=0;i<tempPublicLaneCoinList.Count;i++)
            {
                int tempCoinStepValue=tempPublicLaneCoinList[i].stepCounter+currentDiceValue;
                if(IsCoinSafe(tempCoinStepValue))
                {
                    coinThatWillBeSafe=tempPublicLaneCoinList[i];
                }

                if(coinThatMovedFarthest.stepCounter>tempPublicLaneCoinList[i].stepCounter)
                {
                    coinThatMovedFarthest=tempPublicLaneCoinList[i];
                }
            }

            if(coinThatWillBeSafe!=null)
            {
                StartCoroutine(UpdateCoinPosition(coinThatWillBeSafe));
            }
            else
            {
                if(currentDiceValue==coinOutAt)
                {
                   //check if there are any coins inside base which can be taken out if yes take that out else move other coins
                    List<Transform> avaliableCoins=new List<Transform>();
                    for(int i=0;i<generatedCoinsHolder.GetChild(turnCounter).childCount;i++)
                    {
                        Coin tempBaseCoin=generatedCoinsHolder.GetChild(turnCounter).GetChild(i).GetComponent<Coin>();
                        if(tempBaseCoin.atBase)
                        {
                            if(!avaliableCoins.Contains(tempBaseCoin.transform))
                            {
                                avaliableCoins.Add(tempBaseCoin.transform);
                            }
                        }
                    }

                   if(avaliableCoins.Count>0)
                   {
                        HandleNewCoinOutBehaviour(); 
                   }
                   else
                   {
                        HandlingMaxMovedCoinBehaviour(coinThatMovedFarthest,tempPublicLaneCoinList);
                   }
                }
                else
                {
                    //check if there are any max moved coin
                    //if yes move the max moved coin else move random coin
                    HandlingMaxMovedCoinBehaviour(coinThatMovedFarthest,tempPublicLaneCoinList);
                }
            }
        }
    }

    void HandlingMaxMovedCoinBehaviour(Coin coinThatMovedFarthest,List<Coin> tempPublicLaneCoinList)
    {
        float coinpp=(float)((coinThatMovedFarthest.stepCounter*100)/coinPathContainer.childCount);
        float targetPP=0f;
        bool checkProgress=false;
        if(gameBotType==BotType.Normal)
        {
            checkProgress=true;
            targetPP=50f;
        }
        else if(gameBotType==BotType.Easy)
        {
            checkProgress=true;
            targetPP=75f;
        }

        if(coinpp>=targetPP&&checkProgress)
        {
            StartCoroutine(UpdateCoinPosition(coinThatMovedFarthest));
        }
        else
        {
            //choose random coin for movement
            int randomMovableCoinIndex=Random.Range(0,tempPublicLaneCoinList.Count);
            StartCoroutine(UpdateCoinPosition(tempPublicLaneCoinList[randomMovableCoinIndex]));
        }
    }
    #endregion


    #region Dice poisition
    void UpdateDicePositionOnBoard()
    {
        newDicePosition=diceInitialPos[turnCounter];
        moveDice=true;
    }
    #endregion

    #region Character Rotation
    void UpdateCharacterRotation(Transform ludoChar)
    {
        Coin charCoin=ludoChar.GetComponent<Coin>();
        
        if(!charCoin.onWayToHome)
        {
            int stepsToMove=charCoin.stepCounter+players[turnCounter].initialPosIndex+1;
            if(stepsToMove>coinPathContainer.childCount-1)
            {
                stepsToMove=stepsToMove-coinPathContainer.childCount;
            }
            ludoChar.rotation = Quaternion.LookRotation(ludoChar.position - coinPathContainer.GetChild(stepsToMove).transform.position);
        }
        else
        {
            int homeLastCount=homePaths[turnCounter].childCount-1;
            ludoChar.rotation = Quaternion.LookRotation(ludoChar.position - homePaths[turnCounter].GetChild(homeLastCount).transform.position);
        }
    }
    #endregion
}
