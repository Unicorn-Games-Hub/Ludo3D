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

    //keeping track of bonus turn for getting 6, reaching home and cutting the opponent coin
    private int bonusTurnCounter=0;


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
        public Transform charPrefab;
        public playerType player;
        public int initialPosIndex=0;
        public Color coinColor;
        public List<Transform> outCoins=new List<Transform>();
        public int noOfRoundsWithoutCoinOut=0;
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
    public bool autoBringSingleCoinOut=true;
    public bool allowDiceMoveTowardsPlayers=false;

    //clickable layerMask
    public LayerMask coinLayer;
    private Vector3 newCoinPos=Vector3.zero;

    //changing the coin move speed
    [Range(1f,5f)]
    public float coinMoveSpeed=1f;

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

    [Header("Indicator Essentials")]
    private List<Coin> allOutCoinList=new List<Coin>();
    private List<Coin> coinsAtCurrentLocation=new List<Coin>();
    public Material originalPathMat;
    public Material tempSafeZoneMat;

    //for playing coin or character
    public enum modelType
    {
        character,
        coin
    }
    public modelType playerModel;




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

    #region player initialization
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
        Transform tempPlayer=null;
        Vector3 tempPlayerPos=Vector3.zero;
        Quaternion initialPlayerRot=Quaternion.Euler(0f,0f,0f);

        for(int i=0;i<players.Count;i++)
        {
           for(int j=0;j<noOfCoins;j++)
           {
               if(players[i].player==playerType.Human||players[i].player==playerType.Bot)
               {
                   if(playerModel==modelType.character)
                    {
                        tempPlayer=players[i].charPrefab;
                        tempPlayerPos=coinContainer.GetChild(i).GetChild(j).position;
                        initialPlayerRot=Quaternion.Euler(0f,startRotations[i].y,0f);
                    }
                    else  if(playerModel==modelType.coin)
                    {
                        tempPlayer=players[i].coinPrefab;  
                        tempPlayerPos=new Vector3(coinContainer.GetChild(i).GetChild(j).position.x,0.2f,coinContainer.GetChild(i).GetChild(j).position.z);
                        initialPlayerRot=players[i].coinPrefab.transform.rotation;
                    }
                
                    // Transform newCoin=Instantiate(players[i].coinPrefab,coinContainer.GetChild(i).GetChild(j).position,players[i].coinPrefab.transform.rotation);
                    //newCoin.transform.localRotation=Quaternion.Euler(0f,startRotations[i].y,0f);
                    // newCoin.transform.localRotation=Quaternion.Euler(0f,initialYrot,0f);

                    Transform newCoin=Instantiate(tempPlayer,tempPlayerPos,tempPlayer.rotation);
                    newCoin.SetParent(generatedCoinsHolder.GetChild(i).transform);
                    newCoin.GetComponent<Coin>().HandleCoinInfo(players[i].playerID,newCoin.transform.localPosition);
                    newCoin.transform.localRotation=initialPlayerRot;
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
    #endregion

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
            if(bonusTurnCounter==0)
            {
                curTurn++;
            }
            else
            {
                bonusTurnCounter--;
            }

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
                       if(players[turnCounter].outCoins.Count==0&&autoBringSingleCoinOut)
                       {
                           List<Coin> tempBaseCoinList=new List<Coin>();

                           for(int i=0;i<generatedCoinsHolder.GetChild(turnCounter).childCount;i++)
                           {
                               Coin tempBaseCoin=generatedCoinsHolder.GetChild(turnCounter).GetChild(i).GetComponent<Coin>();
                               if(tempBaseCoin.atBase&&!tempBaseCoin.atHome)
                               {
                                   tempBaseCoinList.Add(tempBaseCoin);
                               }
                           }

                            int randomCoinIndex=Random.Range(0,tempBaseCoinList.Count);
                            Coin coinToTakeOut=tempBaseCoinList[randomCoinIndex];
                            if(!players[turnCounter].outCoins.Contains(coinToTakeOut.gameObject.transform))
                            {
                                players[turnCounter].outCoins.Add(coinToTakeOut.gameObject.transform);
                            }
                            StartCoroutine(UpdateCoinPosition(coinToTakeOut));
                       }
                       else
                       {
                           foreach(Coin c in clickableCoinsList)
                            {
                                c.SetClickable(true);
                            }
                       }
                    }
                    else
                    {
                        UpdateTurn();
                    }
                }
                
                //for increasing the probablity of 6
                players[turnCounter].noOfRoundsWithoutCoinOut=0;
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

                //for increasing the probablity of 6
                if(players[turnCounter].outCoins.Count==0)
                {
                    players[turnCounter].noOfRoundsWithoutCoinOut++;
                }
            }   

            if(players[turnCounter].player==playerType.Bot)
            {
                // StartCoroutine(HandleBotBehaviour());
                StartCoroutine(HandleAIBehaviour());
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
        //handling 6 probablity from here
        Dice.instance.UpdateDiceProbablity(players[turnCounter].noOfRoundsWithoutCoinOut);
        
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
            float tempYpos=0f;
            if(playerModel==modelType.character)
            {
                tempYpos=-0.01f;
            }
            else if(playerModel==modelType.coin)
            {
                tempYpos=0.135f;
            }

            int startingIndex=players[turnCounter].initialPosIndex;
            iTween.MoveTo(coin.gameObject, iTween.Hash("position", new Vector3(coinPathContainer.GetChild(startingIndex).position.x,tempYpos,coinPathContainer.GetChild(startingIndex).position.z), 
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

           //Reseting counter and indicator only for public lane coins
           if(!coin.onWayToHome)
           {
               StartCoroutine(ResetPreviousBlock(initialTargetCount));
           }
           
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
                    HandlePlayerNumIndicator(coin);
                }
            }
            else if(!coin.atHome)
            {
                UpdateTurn();
                HandlePlayerNumIndicator(coin);
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

        //for counter
        HandlePlayerNumIndicator(charFromHome.GetComponent<Coin>());
    }

    private bool isStepCompleted=false;

    void TargetReached()
    {
        isStepCompleted=true;
    }

   
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
                    int samePosCoinCounter=0;
                    Transform curPathBlock=coinPathContainer.GetChild(tempMyCharPosIndex).transform;
                    Vector3 curCoinPosition=new Vector3(curPathBlock.position.x,0.2f,curPathBlock.position.z);
                    Collider[] hitColliders = Physics.OverlapSphere(curCoinPosition, 0.1f);

                    foreach (var v in hitColliders)
                    {
                        if(v.transform.gameObject.layer == LayerMask.NameToLayer("Coin")||v.transform.gameObject.layer == LayerMask.NameToLayer("OppCoin"))
                        {
                            samePosCoinCounter++;
                        }
                    }

                    if(samePosCoinCounter==2)
                    {
                        lastCuttedCoin=cuttableCoins[i];
                        if(playerModel==modelType.character)
                        {
                            if(showCutSceneAnimation)
                            {
                                if(CutSceneAnimationHandler.instance!=null)
                                {
                                    CutSceneAnimationHandler.instance.StartCutSceneAnimation(lastCuttedCoin.id,myChar.id,0);
                                }
                            }
                            else
                            {
                                MoveCutCoin(lastCuttedCoin);
                                // iTween.MoveTo(cuttableCoins[i].gameObject, iTween.Hash("position", 
                                // cuttableCoins[i].initialPosInfo, 
                                // "speed", coinMoveSpeed, 
                                // "easetype", iTween.EaseType.linear,
                                // "oncomplete", "ResetCutCoin", 
                                // "oncompletetarget", this.gameObject
                                // ));
                            }
                        }
                        else if(playerModel==modelType.coin)
                        {
                            if(showCutSceneAnimation)
                            {
                                MoveCutCoin(lastCuttedCoin);
                            }
                            else
                            {
                                ResetThisCoin(lastCuttedCoin);
                            }
                        }
                        return true; 
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
        return false;
    }

    void MoveCutCoin(Coin recentCutCoin)
    {
        iTween.MoveTo(recentCutCoin.gameObject, iTween.Hash("position", 
        recentCutCoin.initialPosInfo, 
        "speed", coinMoveSpeed, 
        "easetype", iTween.EaseType.linear,
        "oncomplete", "ResetCutCoin", 
        "oncompletetarget", this.gameObject
        )); 
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

        //lets check if cut happened due to 6 or not
        if(currentDiceValue==6)
        {
            bonusTurnCounter++;
            Debug.Log("Cut due to 6");
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

        //only reset rotation of characters
        if(playerModel==modelType.character)
        {
            coinToReset.transform.localRotation=Quaternion.Euler(0f,startRotations[coinToReset.id].y,0f);
        }
       
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
        //remove the coins from out coins
        players[turnCounter].outCoins.Remove(coin.transform);

        //lets check if home reached by getting 6 or not
        if(currentDiceValue==6)
        {
            bonusTurnCounter++;
            Debug.Log("home reached due to 6");
        }

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

        UpdateDicePositionOnBoard();
    }

    void ShowPlayerRank()
    {
        Debug.Log("Congratulations ,"+players[winnersList[0]].colorName+" won the game");
    }
    #endregion
    
    #region New AI Behaviour
    IEnumerator HandleBotTurn()
    {
        //lets reset the weightage of each coins
        for(int i=0;i<generatedCoinsHolder.GetChild(turnCounter).childCount;i++)
        {
            generatedCoinsHolder.GetChild(turnCounter).GetChild(i).GetComponent<Coin>().ResetPriorityValues();
        }
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(Dice.instance.RolltheDice());
    }

    private enum BotPriority {cut=0,takeOut=1,safezone=2,home=3,move=4,nothing=5};

    IEnumerator HandleAIBehaviour()
    {
        movableAICoins.Clear();

        yield return new WaitForEndOfFrame();
        BotPriority priority=AiPriorityResolver();
        switch(priority)
        {
            case BotPriority.cut:
            for(int i=0;i<coinThatCutsOpponent.Count;i++)
            {
                if(!movableAICoins.Contains(coinThatCutsOpponent[i]))
                {
                    movableAICoins.Add(coinThatCutsOpponent[i]);
                }
            }
            Debug.Log("There is high probablity that current coin can cut the opponent coin");
            break;

            case BotPriority.takeOut:
            int tempCoinSelector=Random.Range(0,coinsInsideBase.Count);
            Coin baseCoinToMove=coinsInsideBase[tempCoinSelector];
            if(!players[turnCounter].outCoins.Contains(baseCoinToMove.transform))
            {
                players[turnCounter].outCoins.Add(baseCoinToMove.transform);
            }
            StartCoroutine(UpdateCoinPosition(baseCoinToMove));

            break;

            case BotPriority.safezone:
            for(int i=0;i<possibleSafeZoneCoins.Count;i++)
            {
                if(!movableAICoins.Contains(possibleSafeZoneCoins[i]))
                {
                    movableAICoins.Add(possibleSafeZoneCoins[i]);
                }
            }

            Debug.Log("There is high probablity that current coin can be moved to safe zone");
            break;

            case BotPriority.home:
            for(int i=0;i<possibleAIHomeCoins.Count;i++)
            {
                if(!movableAICoins.Contains(possibleAIHomeCoins[i]))
                {
                    movableAICoins.Add(possibleAIHomeCoins[i]);
                }
            }
            Debug.Log("There is high probablity that current coin can reach home");
            break;

            case BotPriority.move:
            Debug.Log("Coins avaliable for movement");
            break;

            case BotPriority.nothing:
            UpdateTurn();
            break;
        }

        yield return new WaitForEndOfFrame();
        if(priority!=BotPriority.nothing&&priority!=BotPriority.takeOut)
        {
            StartCoroutine(ChooseAICoinForMovement());
        }
    }

    BotPriority AiPriorityResolver()
    {
        if(CanAICutOpponentsCoin())
        {
            return BotPriority.cut;
        }
        if(CanAITakeOutCoin())
        {
            return BotPriority.takeOut;
        }
        if(CanAICoinReachSafeZone())
        {
            return BotPriority.safezone;
        }
        if(CanAICoinReachHome())
        {
            return BotPriority.home;
        }
        if(CanAIMoveCoin())
        {
            return BotPriority.move;
        }   
        return BotPriority.nothing;
    }

    //cutting opponents coin
    private List<Coin> unsafeOpponentCoins=new List<Coin>();
    private List<Coin> coinThatCutsOpponent=new List<Coin>();
    private List<Coin> totalOpponentOutCoins=new List<Coin>();

    private bool CanAICutOpponentsCoin()
    {
        unsafeOpponentCoins.Clear();
        coinThatCutsOpponent.Clear();
        totalOpponentOutCoins.Clear();

        for(int i=0;i<gamePlayersList.Count;i++)
        {
            if(gamePlayersList[i]!=turnCounter)
            {
                for(int j=0;j<players[gamePlayersList[i]].outCoins.Count;j++)
                {
                    Coin playerOutCoin=players[gamePlayersList[i]].outCoins[j].GetComponent<Coin>();

                    if(!playerOutCoin.isSafe&&!playerOutCoin.onWayToHome)
                    {
                        if(!unsafeOpponentCoins.Contains(playerOutCoin))
                        {
                            unsafeOpponentCoins.Add(playerOutCoin);
                        }
                    }

                    //keeping track of oppoent coins which are out side the base
                    if(!playerOutCoin.onWayToHome)
                    {
                        if(!totalOpponentOutCoins.Contains(playerOutCoin))
                        {
                            totalOpponentOutCoins.Add(playerOutCoin);
                        }   
                    }
                }
            }
        }

        //
        for(int i=0;i<players[turnCounter].outCoins.Count;i++)
        {
            for(int j=0;j<unsafeOpponentCoins.Count;j++)
            {
                Coin pCoin=players[turnCounter].outCoins[i].GetComponent<Coin>();
                Coin oCoin=unsafeOpponentCoins[j];
                
                int pCoinNewPosIndex=pCoin.stepCounter+players[pCoin.id].initialPosIndex+currentDiceValue;
                int oCoinPosIndex=oCoin.stepCounter+players[oCoin.id].initialPosIndex;

                
                if(pCoinNewPosIndex>coinPathContainer.childCount-1)
                {
                    pCoinNewPosIndex=pCoinNewPosIndex-coinPathContainer.childCount;
                }

                if(oCoinPosIndex>coinPathContainer.childCount-1)
                {
                    oCoinPosIndex=oCoinPosIndex-coinPathContainer.childCount;
                }

                if(pCoinNewPosIndex==oCoinPosIndex&&!pCoin.onWayToHome)
                {
                    //cut coin is avaliable
                    float cuttingWeight=GetWeights(3);
                    float disatnceTravelled=((float)pCoin.stepCounter/(float)(coinPathContainer.childCount-1))*100f;
                    int scoreForCutting=75;
                    if(disatnceTravelled>75)
                    {
                       scoreForCutting=100; 
                    }
                    pCoin.ComputeWeightage(scoreForCutting,cuttingWeight,3);
                    if(!coinThatCutsOpponent.Contains(pCoin))
                    {
                        coinThatCutsOpponent.Add(pCoin);
                    }
                }
            }
        }

        if(coinThatCutsOpponent.Count>0)
        {
            return true;
        }
        return false;
    }

    //taking coin out of the base
    List<Coin> coinsInsideBase=new List<Coin>();
    private bool CanAITakeOutCoin()
    {
        coinsInsideBase.Clear();
        if(currentDiceValue==coinOutAt)
        {
            for(int i=0;i<generatedCoinsHolder.GetChild(turnCounter).childCount;i++)
            {
                Coin myTempCoin=generatedCoinsHolder.GetChild(turnCounter).GetChild(i).GetComponent<Coin>();
                if(myTempCoin.atBase)
                {
                    if(!coinsInsideBase.Contains(myTempCoin))
                    {
                        coinsInsideBase.Add(myTempCoin);
                    }
                }
            }
            if(coinsInsideBase.Count>0)
            {
                return true;
            }
        }
        return false;
    }

    //checking for possible safe zone coin
    private List<Coin> possibleSafeZoneCoins=new List<Coin>();
    private bool CanAICoinReachSafeZone()
    {
        possibleSafeZoneCoins.Clear();
        for(int i=0;i<players[turnCounter].outCoins.Count;i++)
        {
            Coin possibleSafeCoin=players[turnCounter].outCoins[i].GetComponent<Coin>();
            if(!possibleSafeCoin.onWayToHome)
            {
                int phomeCoinIndex=possibleSafeCoin.stepCounter+currentDiceValue;
                if(phomeCoinIndex>coinPathContainer.childCount-1)
                {
                    phomeCoinIndex=phomeCoinIndex-coinPathContainer.childCount;
                }

                if(safePosIndexList.Contains(phomeCoinIndex))
                {
                    float safeZoneWeight=GetWeights(6);
                    possibleSafeCoin.ComputeWeightage(100,safeZoneWeight,6);
                    //
                    if(!possibleSafeZoneCoins.Contains(possibleSafeCoin))
                    {
                        possibleSafeZoneCoins.Add(possibleSafeCoin);
                    }
                }
            }
        }

        if(possibleSafeZoneCoins.Count>0)
        {
            return true;
        }
        return false;
    }

    //checking for movable coins
    List<Coin> movableAICoins=new List<Coin>();
    private bool CanAIMoveCoin()
    {
        movableAICoins.Clear();
        for(int i=0;i<players[turnCounter].outCoins.Count;i++)
        {
            Coin tempOutAiCoin=players[turnCounter].outCoins[i].GetComponent<Coin>();
            if(tempOutAiCoin.onWayToHome)
            {
                if((tempOutAiCoin.stepCounter+currentDiceValue)<6)
                {
                    if(!movableAICoins.Contains(tempOutAiCoin))
                    {
                        movableAICoins.Add(tempOutAiCoin);
                    }
                }
            }
            else
            {
               if(!movableAICoins.Contains(tempOutAiCoin))
                {
                    movableAICoins.Add(tempOutAiCoin);
                }
            }
        }
        if(movableAICoins.Count>0)
        {
            return true;
        }
        return false;
    }

    //checking if home coin avaliable
    List<Coin> possibleAIHomeCoins=new List<Coin>();
    private bool CanAICoinReachHome()
    {
        possibleAIHomeCoins.Clear();
        for(int i=0;i<players[turnCounter].outCoins.Count;i++)
        {
            Coin coinInsideHomeLane=players[turnCounter].outCoins[i].GetComponent<Coin>();
            if(coinInsideHomeLane.onWayToHome)
            {
                if((coinInsideHomeLane.stepCounter+currentDiceValue)==5)
                {
                    float homwLaneWeight=GetWeights(7);
                    coinInsideHomeLane.ComputeWeightage(100,homwLaneWeight,7);
                    possibleAIHomeCoins.Add(coinInsideHomeLane);
                }
            }
        }
        if(possibleAIHomeCoins.Count>0)
        {
            return true;
        }
        return false;
    }

    //
    public class opponentsCoinsInfo
    {
        public Coin curTurnCoin;
        public List<Coin> possibleOpponentCoins=new List<Coin>();
    }
    private List<opponentsCoinsInfo> coinsBehindMe =new List<opponentsCoinsInfo>();
    private List<opponentsCoinsInfo> coinsInfrontOfMe =new List<opponentsCoinsInfo>();

    IEnumerator ChooseAICoinForMovement()
    {
        coinsBehindMe.Clear();
        coinsInfrontOfMe.Clear();

        //totalOpponentOutCoins give information about all the opponents coins which are outside the base and which are not inside home lane

        for(int i=0;i<movableAICoins.Count;i++)
        {
            //lets find out percentage of present saftey, future saftey,future kill potential
            for(int j=0;j<totalOpponentOutCoins.Count;j++)
            {
                //lets get current position index of my current coin and opponent coin
                int myCoinTempIndex=movableAICoins[i].stepCounter+players[movableAICoins[i].id].initialPosIndex;
                int opponentCoinTempIndex=totalOpponentOutCoins[j].stepCounter+players[totalOpponentOutCoins[j].id].initialPosIndex;

                if(myCoinTempIndex>coinPathContainer.childCount-1)
                {
                    myCoinTempIndex=myCoinTempIndex-coinPathContainer.childCount;
                }
                if(opponentCoinTempIndex>coinPathContainer.childCount-1)
                {
                    opponentCoinTempIndex=opponentCoinTempIndex-coinPathContainer.childCount;
                }

                /*
                Debug.Log("My Index : "+myCoinTempIndex);
                Debug.Log("Opponents Index : "+opponentCoinTempIndex);
                */

                if(myCoinTempIndex>opponentCoinTempIndex)
                {
                    //some of the opponents coins are behind me
                    if((myCoinTempIndex-opponentCoinTempIndex)<6)
                    {
                        //there is high possiblity that opponent coin can cut my coin
                        opponentsCoinsInfo tempBehindCoins= new opponentsCoinsInfo();
                        tempBehindCoins.curTurnCoin=movableAICoins[i];
                        tempBehindCoins.possibleOpponentCoins.Add(totalOpponentOutCoins[j]);
                        coinsBehindMe.Add(tempBehindCoins);
                    }
                }
                else if(opponentCoinTempIndex>myCoinTempIndex)
                {
                    //some of the opponents coins are infront of me
                    if((opponentCoinTempIndex-myCoinTempIndex)<6)
                    {
                        //there is high possiblity that my coin can cut opponents coin
                        opponentsCoinsInfo tempfrontCoins= new opponentsCoinsInfo();
                        tempfrontCoins.curTurnCoin=movableAICoins[i];
                        tempfrontCoins.possibleOpponentCoins.Add(totalOpponentOutCoins[j]);
                        coinsInfrontOfMe.Add(tempfrontCoins);
                    }
                }
            }

            //lets compute the journey percentage
            float totalDisatanceTravelled=((float)movableAICoins[i].stepCounter/(float)(coinPathContainer.childCount-1))*100f;
            int journeyScore=GetJourneyScores(totalDisatanceTravelled);
            float journeyWeight=GetWeights(4);
            movableAICoins[i].ComputeWeightage(journeyScore,journeyWeight,4);

            //possiblity of going inside home lane
            if(!movableAICoins[i].onWayToHome)
            {
                int nearHomeIndex=movableAICoins[i].stepCounter+currentDiceValue;
                if(nearHomeIndex>coinPathContainer.childCount-2)
                {
                    //have chance to go inside home lane
                    float nearHomeWeight=GetWeights(5);
                    movableAICoins[i].ComputeWeightage(100,nearHomeWeight,5);
                }
            }
        }

        yield return new WaitForEndOfFrame();

        //lets assigh weightage to the coins accordingly
        for(int i=0;i<coinsBehindMe.Count;i++)
        {
            int psafteyScore=GetLikelyToDiePoints(coinsBehindMe[i].possibleOpponentCoins.Count);
            float psafteyWeight=GetWeights(0);
            coinsBehindMe[i].curTurnCoin.ComputeWeightage(psafteyScore,psafteyWeight,0);
        }

        for(int i=0;i<coinsInfrontOfMe.Count;i++)
        {
            int cutPossiblityScore=GetLikelyToKillPoints(coinsInfrontOfMe[i].possibleOpponentCoins.Count);
            float cutPossiblityWeight=GetWeights(1);
            coinsInfrontOfMe[i].curTurnCoin.ComputeWeightage(100,cutPossiblityWeight,1);
        }

        Coin highestPriorityCoin=movableAICoins[0];
        for(int i=0;i<movableAICoins.Count;i++)
        {
           if(movableAICoins[i].GetMaxWeightPercentage()>highestPriorityCoin.GetMaxWeightPercentage())
            {
                highestPriorityCoin=movableAICoins[i];
            }
        }

        /*
        Debug.Log("Coins infront of me : "+coinsInfrontOfMe.Count);
        Debug.Log("Coins behind me : "+coinsBehindMe.Count);
        Debug.Log("Movable coins of mine : "+movableAICoins.Count);
        */

        yield return new WaitForEndOfFrame();
        StartCoroutine(UpdateCoinPosition(highestPriorityCoin));
    }
    #endregion

    #region Weights
    int GetJourneyScores(float dt)
    {
        int score=1;
        if(dt>10f&&dt<=25f)
        {
            score=50;
        }
        else if(dt>25f&&dt<=35f)
        {
            score=60;
        }
        else if(dt>35f&&dt<=50f)
        {
            score=70;
        }
        else if(dt>50f&&dt<=60f)
        {
            score=75;
        }
        else if(dt>60f&&dt<=75f)
        {
            score=80;
        }
        else if(dt>75f)
        {
            score=25;
        }
        return score;
    }

    float GetWeights(int weightIndex)
    {
        float[] weightValueNormal={15f,15f,10f,20f,15f,7.5f,7.5f,10f};
        float[] weightValueEasy={20f,20f,5f,10f,15f,10f,10f,10f};
        float[] weightValueAggressive={10f,10f,20f,25f,10f,7.5f,7.5f,10f};

        if(gameBotType==BotType.Normal)
        {
            return weightValueNormal[weightIndex];
        }
        else if(gameBotType==BotType.Easy)
        {
            return weightValueEasy[weightIndex];
        }
        else 
        {
            return weightValueAggressive[weightIndex];
        }
    }

    int GetLikelyToDiePoints(int numOfCoins)
    {
        int dangerAheadScore=0;
        if(numOfCoins==1)
        {
            dangerAheadScore=100;
        }else if(numOfCoins==2)
        {
            dangerAheadScore=50;
        }else if(numOfCoins==3)
        {
           dangerAheadScore=30;
        }else if(numOfCoins>=4)
        {
            dangerAheadScore=10;
        }
        return dangerAheadScore;
    }

    int GetLikelyToKillPoints(int coinNum)
    {
        int tempScore=1;
        if(coinNum==1)
        {
            tempScore=10;
        }else if(coinNum==2)
        {
            tempScore=30;
        }
        else if(coinNum==3)
        {
            tempScore=50;
        }else if(coinNum>=4)
        {
            tempScore=100;
        }
        return tempScore;
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
        if(allowDiceMoveTowardsPlayers)
        {
            moveDice=true;
        }
        else
        {
            ludoDice.transform.position=newDicePosition;
            //StartCoroutine(ShrinkAndExpandDice());
        }

        //here we will now change the layer of opponent coins to another layer name
        for(int i=0;i<gamePlayersList.Count;i++)
        {
            for(int j=0;j<generatedCoinsHolder.GetChild(i).childCount;j++)
            {
               if(i==turnCounter)
               {
                   //coins of current turn player
                   generatedCoinsHolder.GetChild(i).GetChild(j).gameObject.layer=LayerMask.NameToLayer("Coin");
               }
               else
               {
                   //coins of player whose turn is not the current turn
                   //opp coin stands for opponents coin
                    generatedCoinsHolder.GetChild(i).GetChild(j).gameObject.layer=LayerMask.NameToLayer("OppCoin");
               }
            }
        }
    }

    IEnumerator ShrinkAndExpandDice()
    {
        ludoDice.transform.localScale=new Vector3(0f,0f,0f);
        yield return new WaitForSeconds(0.1f);
        ludoDice.transform.localScale=new Vector3(0.8f,0.8f,0.8f);
    }
    #endregion

    #region Character Rotation
    void UpdateCharacterRotation(Transform ludoChar)
    {
        if(playerModel==modelType.character)
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
    }
    #endregion
    
    #region Player Indicator
   
    
    void HandlePlayerNumIndicator(Coin recentlyMovedCoin)
    {
        GameObject center=null;
        if(!recentlyMovedCoin.onWayToHome)
        {
            int blockIndex=recentlyMovedCoin.stepCounter+players[recentlyMovedCoin.id].initialPosIndex;
            if(blockIndex>coinPathContainer.childCount-1)
            {
                blockIndex=blockIndex-coinPathContainer.childCount;
            }
            center=coinPathContainer.GetChild(blockIndex).gameObject;
        }
        else
        {
            //disable for home lane coin
            //center=coinPathContainer.GetChild(recentlyMovedCoin.stepCounter).gameObject;
        }

        Vector3 centerPos=new Vector3(center.transform.position.x,0.2f,center.transform.position.z);
        Collider[] hitColliders = Physics.OverlapSphere(centerPos, 0.1f);
        List<Coin> coinsAtSimilarPosition=new List<Coin>();

        //
        List<Coin> diffCoins=new List<Coin>();

        foreach (var h in hitColliders)
        {
            if(h.transform.gameObject.layer == LayerMask.NameToLayer("Coin")||h.transform.gameObject.layer == LayerMask.NameToLayer("OppCoin"))
            {
                Coin nc=h.transform.GetComponent<Coin>();
                coinsAtSimilarPosition.Add(nc);
                if(nc!=recentlyMovedCoin&&nc.id!=recentlyMovedCoin.id)
                {
                    diffCoins.Add(nc);
                }
            }
        }
        
        Debug.Log("Total coins at location : "+center.name+" is "+coinsAtSimilarPosition.Count);

        if(diffCoins.Count>0)
        {
            if(playerModel==modelType.character)
            {
                if(showCutSceneAnimation)
                {
                    if(CutSceneAnimationHandler.instance!=null)
                    {
                        int diffCoinIndex=Random.Range(0,diffCoins.Count);
                        CutSceneAnimationHandler.instance.StartCutSceneAnimation(diffCoins[diffCoinIndex].id,recentlyMovedCoin.id,1);
                    }
                }
            }
        }

        if(!safePosIndexList.Contains(recentlyMovedCoin.stepCounter))
        {
            if(coinsAtSimilarPosition.Count>1)
            {
                UpdateTemporarySafeZone(center,tempSafeZoneMat);
            }
            else
            {
                UpdateTemporarySafeZone(center,originalPathMat);
            }
        } 
    }

    void UpdateTemporarySafeZone(GameObject previousSafeZone,Material tempMat)
    {
        previousSafeZone.GetComponent<MeshRenderer>().material=tempMat;
    }

    IEnumerator ResetPreviousBlock(int prevBlockIndex)
    {
        if(prevBlockIndex>coinPathContainer.childCount-1)
        {
            prevBlockIndex=prevBlockIndex-coinPathContainer.childCount;
        }
        yield return new WaitForSeconds(0.5f);
        Transform newTempPosition=coinPathContainer.GetChild(prevBlockIndex).transform;
        Vector3 newTempPos=new Vector3(newTempPosition.position.x,0.2f,newTempPosition.position.z);

        Collider[] hitColliders = Physics.OverlapSphere(newTempPos, 0.1f);
        int coinsAtThisPosCount=0;

        foreach (var h in hitColliders)
        {
            if(h.transform.gameObject.layer == LayerMask.NameToLayer("Coin")||h.transform.gameObject.layer== LayerMask.NameToLayer("OppCoin"))
            {
                coinsAtThisPosCount++;
            }
        }

        if(!safePosIndexList.Contains(prevBlockIndex))
        {
            if(coinsAtThisPosCount<2)
            {
                UpdateTemporarySafeZone(newTempPosition.gameObject,originalPathMat);
                Debug.Log("Reset Successful..");
            }
        }
    }
    #endregion

    #region Character Animation
    void Idle(Transform currentChar)
    {
        if(playerModel==modelType.character)
        {
            if(CharAnimationHandler.instance!=null)
            {
                CharAnimationHandler.instance.PlayIdleAnimation(currentChar);
            }
        }
    }

    void Walk(Transform currentChar)
    {
        if(playerModel==modelType.character)
        {
            if(CharAnimationHandler.instance!=null)
            {
                CharAnimationHandler.instance.PlayWalkAnimation(currentChar);
            }
        }
    }

    void Defensive(Transform currentChar)
    {
        if(playerModel==modelType.character)
        {
            if(CharAnimationHandler.instance!=null)
            {
                CharAnimationHandler.instance.PlayDefensiveAnimation(currentChar);
            }
        }
    }
    #endregion
}
