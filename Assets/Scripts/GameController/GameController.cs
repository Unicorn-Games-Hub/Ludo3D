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
    public bool showCutSceneAnimation=true;
    public bool punishPlayerOnConsecutiveRoll=false;

    //clickable layerMask
    public LayerMask coinLayer;
    private Vector3 newCoinPos=Vector3.zero;

    //Raycasting
    private Ray ray;
    private RaycastHit hit;

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

    [Header("Highlight Animation")]
    public HighLightAnimation[] highLights;
 

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
                    newCoin.transform.localRotation=Quaternion.Euler(0f,startRotations[i].y,0f);
                    if(!gamePlayersList.Contains(players[i].playerID))
                    {
                        gamePlayersList.Add(players[i].playerID);
                    }

                    if(enterHomeWithOutCutting)
                    {
                        newCoin.GetComponent<Coin>().isReadyForHome=true;
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
        highLights[turnCounter].StopAnimation();
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
        UpdateDicePositionOnBoard();
        yield return new WaitForSeconds(0.5f);
        HandleDiceRoll(turnCounter);
    }
    #endregion

    #region Handling obtained dice value
    public void HandleObtainedDiceValue(int diceValue)
    {
        currentDiceValue=diceValue;
        if(!IsItThreeConsecutiveRoll(turnCounter))
        {
            if(diceValue==coinOutAt)
            {  
                if(players[turnCounter].player==playerType.Human)
                {
                    List<Coin> clickableCoinsList=new List<Coin>();
                    for(int i=0;i<generatedCoinsHolder.GetChild(turnCounter).childCount;i++)
                    {
                        Coin curTempCoin=generatedCoinsHolder.GetChild(turnCounter).GetChild(i).GetComponent<Coin>();
                        if(!curTempCoin.onWayToHome)
                        {
                            clickableCoinsList.Add(curTempCoin);
                        }
                        else
                        {
                            if(curTempCoin.stepCounter<homePaths[turnCounter].childCount)
                            {
                                if((curTempCoin.stepCounter+currentDiceValue)<homePaths[turnCounter].childCount)
                                {
                                    clickableCoinsList.Add(curTempCoin);
                                }
                            }
                        }
                    }

                    if(clickableCoinsList.Count>0)
                    {
                        foreach(Coin c in clickableCoinsList)
                        {
                            c.SetClickable(true);
                        }
                    }
                    else
                    {
                        UpdateTurn();
                    }
                }
            }
            else
            {
                if(players[turnCounter].outCoins.Count==1&&players[turnCounter].player==playerType.Human)
                {
                    Coin singleOutCoin=players[turnCounter].outCoins[0].GetComponent<Coin>();
                    if(!singleOutCoin.onWayToHome)
                    {
                        StartCoroutine(UpdateCoinPosition(singleOutCoin));
                    }
                    else
                    {
                        if(singleOutCoin.stepCounter<homePaths[turnCounter].childCount)
                        {
                            if((singleOutCoin.stepCounter+currentDiceValue)<homePaths[turnCounter].childCount)
                            {
                                StartCoroutine(UpdateCoinPosition(singleOutCoin));
                            }
                            else
                            {
                                UpdateTurn();
                            }
                        }
                    }
                }
                else if(players[turnCounter].outCoins.Count>1&&players[turnCounter].player==playerType.Human)
                {
                    int tempCoinCounter=0;
                    Coin coinThatisMovable=null;
                   
                    for(int i=0;i<players[turnCounter].outCoins.Count;i++)
                    {
                        Coin tempOutCoin=players[turnCounter].outCoins[i].GetComponent<Coin>();
                        if(!tempOutCoin.onWayToHome)
                        {
                            coinThatisMovable=tempOutCoin;
                            tempOutCoin.SetClickable(true);
                            tempCoinCounter++;
                        }
                        else
                        {
                            if(tempOutCoin.stepCounter<homePaths[turnCounter].childCount)
                            {
                                if((tempOutCoin.stepCounter+currentDiceValue)<homePaths[turnCounter].childCount)
                                {
                                    coinThatisMovable=tempOutCoin;
                                    tempOutCoin.SetClickable(true);
                                    tempCoinCounter++;
                                }
                            }
                        }
                    }

                    if(tempCoinCounter==1)
                    {
                        StartCoroutine(UpdateCoinPosition(coinThatisMovable));
                    }
                    else if(tempCoinCounter==0)
                    {
                        UpdateTurn();
                    }
                }
                else if(players[turnCounter].player==playerType.Human)
                {
                    UpdateTurn();
                }
            }   

            if(players[turnCounter].player==playerType.Bot)
            {
                StartCoroutine(HandleBotBehaviour());
            }
        }
        else
        {
            currentDiceValue=0;
            UpdateTurn();
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
        highLights[turnCounter].HighLight();
    }

    public void StopBlinkingAnimation()
    {
        highLights[turnCounter].StopAnimation();
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
            "speed",coinMoveSpeed, 
            "easetype", iTween.EaseType.linear,
            "oncomplete", "UpdateTurnAfterFirstMove", 
            "oncompletetarget", this.gameObject
            ));

            //lets play walk animation
            coin.isSafe=true;
            coin.atBase=false;
            charFromHome=coin.transform;
            Walk(charFromHome);
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
                                coin.atHome=true;
                                //remove out coins
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

                //adjusting character rotation
                UpdateCharacterRotation(coin.transform);

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

                //play walk animation
                Walk(coin.transform);

                yield return new WaitUntil(() => isStepCompleted);
                yield return new WaitForSeconds(0.1f);
                isStepCompleted = false;
            }

            //lets check if the current coin is safe or not
            coin.isSafe=IsCoinSafe(coin);

            if(coin.isSafe)
            {
                Defensive(coin.transform);
            }
            else
            {
                Idle(coin.transform);
            }

            //checking if coin is safe or can be cut
            if(!coin.onWayToHome)
            {
                if(CutTheCoin(coin))
                {
                    //lets make game wait until the cut scene finishes
                    //give turn as a cut bonus
                    // HandleDiceRoll(turnCounter);
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
        Defensive(charFromHome);
    }

    private bool isStepCompleted=false;

    void TargetReached()
    {
        isStepCompleted=true;
    }

    private float coinMoveSpeed=1f;
    #endregion

    #region Safe
    public bool IsCoinSafe(Coin checkCoin)
    {
        int tempPosValue=checkCoin.stepCounter;
        if(tempPosValue>coinPathContainer.childCount-1)
        {
            tempPosValue=(tempPosValue-coinPathContainer.childCount);
        }
        if(safePosIndexList.Contains(tempPosValue))
        {
           return true;
        }
        return false;
    }
    #endregion

    #region Cutting
    private Coin lastCuttedCoin=null;
    public bool CutTheCoin(Coin myChar)
    {
        List<Coin> cuttableCoins=new List<Coin>();
        for(int i=0;i<gamePlayersList.Count;i++)
        {
            for(int j=0;j<players[gamePlayersList[i]].outCoins.Count;j++)
            {
                if(turnCounter!=players[gamePlayersList[i]].playerID)
                {
                    Coin newTempCoin=players[gamePlayersList[i]].outCoins[j].GetComponent<Coin>();
                    if(!newTempCoin.isSafe&&!newTempCoin.onWayToHome&&!newTempCoin.atHome)
                    {
                        if(!cuttableCoins.Contains(newTempCoin))
                        {
                            cuttableCoins.Add(newTempCoin); 
                        }
                    }
                }
            }
        }

        if(cuttableCoins.Count>0)
        {
            for(int i=0;i<cuttableCoins.Count;i++)
            {
                int tempCoinPosIndex=cuttableCoins[i].stepCounter+players[cuttableCoins[i].id].initialPosIndex;
                int tempMyCharPosIndex=myChar.stepCounter+players[myChar.id].initialPosIndex;

                if(tempCoinPosIndex>coinPathContainer.childCount-1)
                {
                    tempCoinPosIndex=tempCoinPosIndex-coinPathContainer.childCount;
                }

                if(tempMyCharPosIndex>coinPathContainer.childCount-1)
                {
                    tempMyCharPosIndex=tempMyCharPosIndex-coinPathContainer.childCount;
                }
                
                if(tempCoinPosIndex==tempMyCharPosIndex)
                {
                    lastCuttedCoin=cuttableCoins[i];

                    if(showCutSceneAnimation)
                    {
                        if(CutSceneAnimationHandler.instance!=null)
                        {
                            CutSceneAnimationHandler.instance.StartCutSceneAnimation(lastCuttedCoin.id,myChar.id);
                        }
                    }
                    else
                    {
                        iTween.MoveTo(cuttableCoins[i].gameObject, iTween.Hash("position", 
                        cuttableCoins[i].initialPosInfo, 
                        "speed", coinMoveSpeed, 
                        "easetype", iTween.EaseType.linear,
                        "oncomplete", "ResetCutCoin", 
                        "oncompletetarget", this.gameObject
                        ));
                    }
                    return true;
                }
            }
        }
        return false;
    }

    public void ResetCutCoin()
    {
        ResetThisCoin(lastCuttedCoin);
        if(!enterHomeWithOutCutting)
        {
            for(int j=0;j<generatedCoinsHolder.GetChild(turnCounter).childCount;j++)
            {
                generatedCoinsHolder.GetChild(turnCounter).GetChild(j).GetComponent<Coin>().isReadyForHome=true;
            }
        }
        StartCoroutine(UpdateTurnAfterCutting());
    }

    void ResetThisCoin(Coin coinToReset)
    {
        coinToReset.stepCounter=0;
        coinToReset.atBase=true;
        coinToReset.canGoHome=false;
        coinToReset.onWayToHome=false;
        coinToReset.atHome=false;
        coinToReset.isSafe=false;
        coinToReset.transform.position=coinToReset.initialPosInfo;
        coinToReset.transform.localRotation=Quaternion.Euler(0f,startRotations[coinToReset.id].y,0f);

        Transform tempCoinTransform=coinToReset.gameObject.transform;
        if(players[coinToReset.id].outCoins.Contains(tempCoinTransform))
        {
            players[coinToReset.id].outCoins.Remove(tempCoinTransform);
        }
        //
        UpdateDicePositionOnBoard();
    }

    IEnumerator UpdateTurnAfterCutting()
    {
        yield return new WaitForSeconds(0.5f);
        HandleDiceRoll(turnCounter);
    }
    #endregion

    #region Win lose
    private List<int> winnersList=new List<int>();

    void CheckForWinner(Coin coin)
    {
        int count=0;
        for(int i=0;i<generatedCoinsHolder.GetChild(turnCounter).childCount;i++)
        {
            Coin temphomeCoin=generatedCoinsHolder.GetChild(turnCounter).GetChild(i).GetComponent<Coin>();
            if(temphomeCoin.atHome)
            {
                temphomeCoin.stepCounter=0;
                temphomeCoin.atBase=false;
                temphomeCoin.isClickable=false;
                temphomeCoin.isReadyForHome=false;
                temphomeCoin.canGoHome=false;
                temphomeCoin.onWayToHome=false;
                count++;
            }
        }

        if(count==noOfCoins)
        {
            Debug.Log("winner!");
            winnersList.Add(coin.id);
            //remove player from the list
            gamePlayersList.Remove(coin.id);

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

    //from here we will control the behaviour of the bot
    private enum BotPriority {cut=0,takeOut=1,safezone=2,home=3,move=4,nothing=5};
    List<Transform> coinsAtBase=new List<Transform>();
    List<Coin> safeCoinsList=new List<Coin>();
    List<Coin> homeCoinsList=new List<Coin>();
    List<Coin> movableCoinsList=new List<Coin>();

    IEnumerator HandleBotBehaviour()
    {
        yield return new WaitForEndOfFrame();
        BotPriority priority=ResolveBotPriority();
        Coin coinToMove=null;
        switch(priority)
        {
            case BotPriority.cut:
            coinToMove=GetCutCoin();
            break;
            case BotPriority.takeOut:
            int newOutCoinIndex=Random.Range(0,coinsAtBase.Count);
            Transform coinToBringOut=coinsAtBase[newOutCoinIndex];
            if(!players[turnCounter].outCoins.Contains(coinToBringOut))
            {
                players[turnCounter].outCoins.Add(coinToBringOut);
            }
            coinToMove=coinToBringOut.GetComponent<Coin>();
            break;
            case BotPriority.safezone:
            Coin tempsafeCoin=safeCoinsList[0];
            for(int i=0;i<safeCoinsList.Count;i++)
            {
                if(tempsafeCoin.stepCounter<safeCoinsList[i].stepCounter)
                {
                    tempsafeCoin=safeCoinsList[i];
                }
            }
            coinToMove=tempsafeCoin;
            break;
            case BotPriority.home:
            int newHomeCoinIndex=Random.Range(0,homeCoinsList.Count);
            coinToMove=homeCoinsList[newHomeCoinIndex];
            break;
            case BotPriority.move:
            coinToMove=GetCoinToMove();
            break;
            case BotPriority.nothing:
            UpdateTurn();
            break;
        }

        if(priority!=BotPriority.nothing)
        {
            StartCoroutine(UpdateCoinPosition(coinToMove));
        }
    }

    private BotPriority ResolveBotPriority()
    {
        if(IsCoinCutAvaliable())
        {
            return BotPriority.cut;
        }
        
        if(IsTakeOutAvaliable())
        {
            return BotPriority.takeOut;
        }

        if(IsSafeCoinAvaliable())
        {
            return BotPriority.safezone;
        }

        if(IsHomeCoinAvaliable())
        {
            return BotPriority.home;
        }

        if(IsMovableCoinAvaliable())
        {
            return BotPriority.move; 
        }

        return BotPriority.nothing;
    }

    #region Coin that cuts opponent
    private Coin GetCutCoin()
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
        return coinThatWillCut;
    }
    #endregion

    #region Coin Avaliable for taking out of the base
    private bool IsTakeOutAvaliable()
    {
        coinsAtBase.Clear();
        if(currentDiceValue==coinOutAt)
        {
            for(int i=0;i<generatedCoinsHolder.GetChild(turnCounter).childCount;i++)
            {
                Transform baseCoin=generatedCoinsHolder.GetChild(turnCounter).GetChild(i).transform;
                if(baseCoin.GetComponent<Coin>().atBase)
                {
                    if(!coinsAtBase.Contains(baseCoin))
                    {
                        coinsAtBase.Add(baseCoin);
                    }
                }
            }

            if(coinsAtBase.Count>0)
            {
                return true;
            }
        }
       return false;
    }
    #endregion

    #region Checking if coin avaliable for safe zone
    private bool IsSafeCoinAvaliable()
    {
        safeCoinsList.Clear();
        for(int i=0;i<players[turnCounter].outCoins.Count;i++)
        {
            Coin tempOutchar=players[turnCounter].outCoins[i].GetComponent<Coin>();
            if(!tempOutchar.onWayToHome)
            {
                if(IsCoinSafe(tempOutchar))
                {
                    safeCoinsList.Add(tempOutchar);
                }
            }
        }
        if(safeCoinsList.Count>0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    #endregion

    #region Checking if movalble coin avaliable 
    private bool IsMovableCoinAvaliable()
    {
       movableCoinsList.Clear();
        for(int i=0;i<players[turnCounter].outCoins.Count;i++)
        {
            Coin tempOutsideCoin=players[turnCounter].outCoins[i].GetComponent<Coin>();
            if(tempOutsideCoin.onWayToHome)
            {
               if(tempOutsideCoin.stepCounter<homePaths[turnCounter].childCount)
                {
                    if((tempOutsideCoin.stepCounter+currentDiceValue)<homePaths[turnCounter].childCount)
                    {
                       movableCoinsList.Add(tempOutsideCoin);
                    }
                }
            }
            else
            {
                movableCoinsList.Add(tempOutsideCoin);
            }
        }

        if(movableCoinsList.Count>0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    #endregion
    
    #region Checking if home coin avaliable
    private bool IsHomeCoinAvaliable()
    {
        homeCoinsList.Clear();
        for(int i=0;i<players[turnCounter].outCoins.Count;i++)
        {
            Coin newHomeCoin=players[turnCounter].outCoins[i].GetComponent<Coin>();
            if(newHomeCoin.onWayToHome)
            {
                if((newHomeCoin.stepCounter+currentDiceValue)==homePaths[turnCounter].childCount-1)
                {
                    homeCoinsList.Add(newHomeCoin);
                }
            }
        }
       
       if(homeCoinsList.Count>0)
       {
           return true;
       }
       else
       {
           return false;
       }
    }
    #endregion

    #region Coin Avaliable for moving
    private Coin GetCoinToMove()
    {
        Coin tempMaxMovedCoin=movableCoinsList[0];
        for(int i=0;i<movableCoinsList.Count;i++)
        {
            if(tempMaxMovedCoin.stepCounter<movableCoinsList[i].stepCounter)
            {
                tempMaxMovedCoin=movableCoinsList[i];
            }
            if(movableCoinsList[i].onWayToHome)
            {
                return movableCoinsList[i];
            }
        }

        float coinpp=(float)((tempMaxMovedCoin.stepCounter*100)/coinPathContainer.childCount);
        if(gameBotType==BotType.Normal)
        {
            if(coinpp>50f)
            {
                return tempMaxMovedCoin;
            }
            else
            {
                return GetRandomMovableCoin();
            }
        }
        else if(gameBotType==BotType.Easy)
        {
            if(coinpp>50f)
            {
                return tempMaxMovedCoin;
            }
            else
            {
                return GetRandomMovableCoin();
            }
        }
        else
        {
            return GetRandomMovableCoin();
        }
    }

    Coin GetRandomMovableCoin()
    {
        int tempCoinIndex=Random.Range(0,movableCoinsList.Count);
        return movableCoinsList[tempCoinIndex];
    }
    #endregion

    [System.Serializable]
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

        List<Coin> coinsOutsideBase=new List<Coin>();
    
        for(int i=0;i<gamePlayersList.Count;i++)
        {
            for(int j=0;j<players[gamePlayersList[i]].outCoins.Count;j++)
            {
                if(turnCounter!=players[gamePlayersList[i]].playerID)
                {
                    Coin opponentCoin=players[gamePlayersList[i]].outCoins[j].GetComponent<Coin>();
                    if(!opponentCoin.isSafe&&!opponentCoin.onWayToHome)
                    {
                        coinsOutsideBase.Add(opponentCoin);
                    }
                }
            }
        }

        //now lets check wether the out coins can be cut or not
        for(int i=0;i<players[turnCounter].outCoins.Count;i++)
        {
            for(int j=0;j<coinsOutsideBase.Count;j++)
            {
                Coin myCoin=players[turnCounter].outCoins[i].GetComponent<Coin>(); 

                int opponentCoinPosIndex=coinsOutsideBase[j].stepCounter+players[coinsOutsideBase[j].id].initialPosIndex;
                int myCoinNextPosIndex=myCoin.stepCounter+players[myCoin.id].initialPosIndex+currentDiceValue;

                if(opponentCoinPosIndex>coinPathContainer.childCount-1)
                {
                    opponentCoinPosIndex=opponentCoinPosIndex-coinPathContainer.childCount;
                }

                if(myCoinNextPosIndex>coinPathContainer.childCount-1)
                {
                    myCoinNextPosIndex=myCoinNextPosIndex-coinPathContainer.childCount;
                }

                if(myCoinNextPosIndex==opponentCoinPosIndex)
                {
                    Debug.Log("Coin avaliable for cutting..");
                    CoinCutInfo coinInfo=new CoinCutInfo();
                    coinInfo.coinThatCuts=myCoin;
                    coinInfo.coinThatCanBeCut=coinsOutsideBase[j];

                    // //so that we can choose to cut player coin or bot coin
                    if(players[coinsOutsideBase[j].id].player==playerType.Human)
                    {
                        coinInfo.coinPlayID=0;
                    }
                    else if(players[coinsOutsideBase[j].id].player==playerType.Bot)
                    {
                        coinInfo.coinPlayID=1;
                    }
                    avalibleCutCoinList.Add(coinInfo);
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
    #endregion

    #region Consecutive roll of 6
    private List<int> consecutiveRollList=new List<int>();

    bool IsItThreeConsecutiveRoll(int curPlayerID)
    {
        if(currentDiceValue!=6)
        {
            consecutiveRollList.Clear();
            return false;
        }

        if(consecutiveRollList.Count==0)
        {
            consecutiveRollList.Add(curPlayerID); 
        }
        else
        {
            if(consecutiveRollList[consecutiveRollList.Count-1]==curPlayerID)
            {
                consecutiveRollList.Add(curPlayerID);
                if(consecutiveRollList.Count==3)
                {
                    if(players[curPlayerID].outCoins.Count==0)
                    {
                        return false;
                    }
                    else
                    {
                        Coin maxTravelledCoin=players[curPlayerID].outCoins[0].GetComponent<Coin>();
                        for(int i=0;i<players[curPlayerID].outCoins.Count;i++)
                        {
                            Coin tempTravelledCoin=players[curPlayerID].outCoins[i].GetComponent<Coin>();
                            if(tempTravelledCoin.stepCounter>maxTravelledCoin.stepCounter)
                            {
                                maxTravelledCoin=tempTravelledCoin;
                            }
                        }
                        //reset the coin
                        if(punishPlayerOnConsecutiveRoll)
                        {
                            ResetThisCoin(maxTravelledCoin);
                        }
                        consecutiveRollList.Clear();
                        return true;
                    }
                }
            }
            else
            {
                consecutiveRollList.Clear();
                consecutiveRollList.Add(curPlayerID);
            }
        }
        return false;
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

    #region Character Animation

    void Idle(Transform currentChar)
    {
        if(CharAnimationHandler.instance!=null)
        {
            CharAnimationHandler.instance.PlayIdleAnimation(currentChar);
        }
    }

    void Walk(Transform currentChar)
    {
        if(CharAnimationHandler.instance!=null)
        {
            CharAnimationHandler.instance.PlayWalkAnimation(currentChar);
        }
    }

    void Defensive(Transform currentChar)
    {
       if(CharAnimationHandler.instance!=null)
        {
            CharAnimationHandler.instance.PlayDefensiveAnimation(currentChar);
        } 
    }
    
    #endregion
}
