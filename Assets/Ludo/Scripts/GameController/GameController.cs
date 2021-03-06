using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    public Transform coinContainer;
    public Transform coinPathContainer;

    [Header("Transform to hold generated coins")]
    public Transform generatedCoinsHolder;

    [Header("Home Path")]
    public Transform[] homePaths;

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
        public float winningChance=0f;
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

    [Header("Ingame player indicator")]
    public Transform playerIndicators;

    [Header("Dice Position During Roll")]
    public Transform ludoDice;
    public Vector3[] diceInitialPos;
    private float diceMoveSpeed=25f;
    private Vector3 newDicePosition=Vector3.zero;
    private bool moveDice=false;

    [Header("Highlight Animation")]
    public HighLightAnimation[] highLights;

    //animation state
    public enum statusType
    {
        None,
        Cut,
        Defend
    }
    [HideInInspector]
    public statusType animState; 

    [Header("Indicator Essentials")]
    private List<Coin> allOutCoinList=new List<Coin>();
    private List<Coin> coinsAtCurrentLocation=new List<Coin>();
    public Material originalPathMat;
    public Material tempSafeZoneMat;

    public Sprite[] indicatorSpriteContainer;


    [Header("Confetti Particles")]
    public ParticleSystem smallConfitti=null;
    public ParticleSystem largeConfitti=null;

    [Header("Spawn Particles")]
    public ParticleSystem[] spawnParticles;

    [Header("Death Particles")]
    public ParticleSystem[] deathParticles;

    //for playing coin or character
    public enum modelType
    {
        character,
        coin
    }
    public modelType playerModel;

    [Header("Materials for metallic art style")]
    public Material boardMat;
    [Range(0f,1f)]
    public float metallicValue=0f;
    //for selecting board art style
    public enum boardStyle
    {
        board_default,
        board_metallic
    }
    public boardStyle boardArtStyle;

    //keeping track of human
    private List<int> totalHumanPlayers=new List<int>();

    //0 for default and 1 for metallic
    public Material[] charBodyMat;


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
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        
        HandleGameData();
        if(GameDataHolder.instance!=null)
        {
            for(int i=0;i<GameDataHolder.instance.playerIndex.Length;i++)
            {
                if(GameDataHolder.instance.playerIndex[i]==0)
                {
                    players[i].player=playerType.Human;
                    if(!totalHumanPlayers.Contains(i))
                    {
                        totalHumanPlayers.Add(i);
                    }
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

        //changing art style
        if(PlayerPrefs.GetInt("ludo_board_artStyle")==0)
        {
            boardArtStyle=boardStyle.board_default;
        }
        else
        {
            boardArtStyle=boardStyle.board_metallic;
        }
        HandleGameArtStyle();
    }

    void HandleGameData()
    {
        //player type
        if(PlayerPrefs.GetInt("LudoPlayer-Type")==0)
        {
            playerModel=modelType.character;
        }
        else
        {
            playerModel=modelType.coin;
        }

        //cut scene animation
        if(PlayerPrefs.GetInt("Ludo-CutScene")==0)
        {
            showCutSceneAnimation=true;
        }
        else
        {
            showCutSceneAnimation=false;
        }

        //home only on cut?
        if(PlayerPrefs.GetInt("home_on_cut_only")==0)
        {
            enterHomeWithOutCutting=true;
        }
        else
        {
            enterHomeWithOutCutting=false;
        }

        //punish on consecutive roll
        if(PlayerPrefs.GetInt("punish_on_consecutive_roll")==0)
        {
            punishPlayerOnConsecutiveRoll=false;
        }
        else
        {
            punishPlayerOnConsecutiveRoll=true;
        }

        //auto bring first coin out
        if(PlayerPrefs.GetInt("auto_bring_firstcoin")==0)
        {
            autoBringSingleCoinOut=false;
        }
        else
        {
            autoBringSingleCoinOut=true;
        }

        //smooth dice movement
        if(PlayerPrefs.GetInt("smooth_dice_movement")==0)
        {
            allowDiceMoveTowardsPlayers=false;
        }
        else
        {
            allowDiceMoveTowardsPlayers=true;
        }
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
                    //updating indicator sprite
                    newCoin.GetComponent<Coin>().indicator.GetComponent<SpriteRenderer>().sprite=indicatorSpriteContainer[players[i].playerID];
                    //
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
        StartCoroutine(HandleDiceRoll(turnCounter));
       UpdateDicePositionOnBoard();
    }

    void UpdateTurn()
    {
        StartCoroutine(WaitBeforeUpdatingTurn());
    }
    
    IEnumerator WaitBeforeUpdatingTurn()
    {
        highLights[turnCounter].StopAnimation();
        yield return new WaitForSeconds(0.3f);
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
        yield return new WaitForSeconds(0.2f);
        StartCoroutine(HandleDiceRoll(turnCounter));
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
                        if(!curTempCoin.onWayToHome&&!curTempCoin.atHome)
                        {
                            clickableCoinsList.Add(curTempCoin);
                        }
                        else
                        {
                            if(curTempCoin.stepCounter<homePaths[turnCounter].childCount&&!curTempCoin.atHome)
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
                               if(tempBaseCoin.atBase)
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
                                ActivateIndicator(c);
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
                    if(!singleOutCoin.onWayToHome&&!singleOutCoin.atHome)
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
                        if(!tempOutCoin.onWayToHome&&!tempOutCoin.atHome)
                        {
                            coinThatisMovable=tempOutCoin;
                            tempOutCoin.SetClickable(true);
                            tempCoinCounter++;
                            ActivateIndicator(tempOutCoin);
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
                                    ActivateIndicator(tempOutCoin);
                                }
                            }
                        }
                    }

                    if(tempCoinCounter==1)
                    {
                        HideCoinIndicator();
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

    //for activating indicator coin
    void ActivateIndicator(Coin clickableTempCoin)
    {
        if(clickableTempCoin.indicator!=null)
        {
            clickableTempCoin.indicator.SetActive(true);
        }
        clickableTempCoin.StartScaleAnimation();
    }

    IEnumerator HandleDiceRoll(int turnValue)
    {
        //only check if coins are out of the base
        if(players[turnCounter].outCoins.Count>0)
        {
            List<int> possibleRollList=FindOutPossibleCutOutComes();
            yield return new WaitForEndOfFrame();
            if(possibleRollList.Count>0)
            {
                Dice.instance.UpdateCutProbablity(possibleRollList);
            }
        }
        //handling 6 probablity from here
        Dice.instance.UpdateDiceProbablity(players[turnCounter].noOfRoundsWithoutCoinOut);

        if(players[turnValue].player==playerType.Human)
        {
            Dice.instance.canRollDice=true;
            highLights[turnCounter].HighLight();
            Dice.instance.StartDiceHighlights(players[turnCounter].coinColor,turnCounter);
        }
        else if(players[turnValue].player==playerType.Bot)
        {
            Dice.instance.canRollDice=false;
            StartCoroutine(HandleBotTurn());
        }
        Dice.instance.UpdateDiceMaterial(players[turnCounter].coinColor);
    }

    public void StopBlinkingAnimation()
    {
        highLights[turnCounter].StopAnimation();
        Dice.instance.StopDiceHighLight();
    }
    #endregion

    //for decreasing the chance of cutting opponent coin
    List<int> FindOutPossibleCutOutComes()
    {
        List<Coin> tempUnsafeCoins=new List<Coin>();
        List<int> possibleCutPoint=new List<int>();

        for(int i=0;i<gamePlayersList.Count;i++)
        {
            if(gamePlayersList[i]!=turnCounter)
            {
                for(int j=0;j<players[gamePlayersList[i]].outCoins.Count;j++)
                {
                    Coin playerOutCoin=players[gamePlayersList[i]].outCoins[j].GetComponent<Coin>();

                    if(!playerOutCoin.isSafe&&!playerOutCoin.onWayToHome)
                    {
                        if(!tempUnsafeCoins.Contains(playerOutCoin))
                        {
                            tempUnsafeCoins.Add(playerOutCoin);
                        }
                    }
                }
            }
        }

        for(int i=0;i<players[turnCounter].outCoins.Count;i++)
        {
            for(int j=0;j<tempUnsafeCoins.Count;j++)
            {
                Coin pCoin=players[turnCounter].outCoins[i].GetComponent<Coin>();
                Coin oCoin=tempUnsafeCoins[j];
                
                int pCoinNewPosIndex=pCoin.stepCounter+players[pCoin.id].initialPosIndex;
                int oCoinPosIndex=oCoin.stepCounter+players[oCoin.id].initialPosIndex;

                if(pCoinNewPosIndex>coinPathContainer.childCount-1)
                {
                    pCoinNewPosIndex=pCoinNewPosIndex-coinPathContainer.childCount;
                }

                if(oCoinPosIndex>coinPathContainer.childCount-1)
                {
                    oCoinPosIndex=oCoinPosIndex-coinPathContainer.childCount;
                }

                int tempRollNum=oCoinPosIndex-pCoinNewPosIndex;

                if(tempRollNum>0&&tempRollNum<=6)
                {
                    if(!possibleCutPoint.Contains(tempRollNum))
                    {
                        possibleCutPoint.Add(tempRollNum);
                    }
                }
            }
        }
        return possibleCutPoint;
    }

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
                        HideCoinIndicator();
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

    void HideCoinIndicator()
    {
        for(int i=0;i<generatedCoinsHolder.GetChild(turnCounter).childCount;i++)
        {
            generatedCoinsHolder.GetChild(turnCounter).GetChild(i).GetComponent<Coin>().HideIndicator();
            // generatedCoinsHolder.GetChild(turnCounter).GetChild(i).GetComponent<Coin>().isClickable=false;
            // generatedCoinsHolder.GetChild(turnCounter).GetChild(i).GetComponent<Coin>().indicator.SetActive(false);
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
            ComputeWinPercentage(coin);

            if(GameAudioHandler.instance!=null)
            {
                GameAudioHandler.instance.PlayOutOfBaseSound();
            }
        }
        else
        {
            // int initialTargetCount=coin.stepCounter+players[turnCounter].initialPosIndex;
            // int newTargetCount=initialTargetCount+currentDiceValue;
            // int tempStepCount=0;

            //lets check if coin can be cut or not
            if(!coin.onWayToHome)
            {
                CheckForCutAndDefend(coin);
            }
        
            yield return new WaitForEndOfFrame();

            //new conditions
            int initialTargetCount=0;
            int newTargetCount=0;
            int tempStepCount=0;

            if(animState!=statusType.None&&playerModel==modelType.character&&!showCutSceneAnimation&&!coin.onWayToHome)
            {
                initialTargetCount=coin.stepCounter+players[turnCounter].initialPosIndex;
                newTargetCount=(initialTargetCount+currentDiceValue)-1;
            }
            else
            {
                initialTargetCount=coin.stepCounter+players[turnCounter].initialPosIndex;
                newTargetCount=initialTargetCount+currentDiceValue;
            }
          
           //Reseting counter and indicator only for public lane coins
        //    if(!coin.onWayToHome)
        //    {
        //        StartCoroutine(ResetPreviousBlock(initialTargetCount));
        //    }
           
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

                coin.stepsMovedTillNow++;

                yield return new WaitUntil(() => isStepCompleted);
                yield return new WaitForSeconds(0.1f);
                isStepCompleted = false;
            }

            //lets check if the current coin is safe or not
            coin.isSafe=IsCoinSafe(coin);

            if(coin.isSafe)
            {
                Defensive(coin.transform);

                if(GameAudioHandler.instance!=null)
                {
                    GameAudioHandler.instance.PlaySafeZoneReachedSound();
                }
            }
            else
            {
                Idle(coin.transform);
            }

            //checking if coin is safe or can be cut
            if(!coin.onWayToHome)
            {
                if(playerModel!=modelType.character||showCutSceneAnimation)
                {
                    //we will check new conditions for coin from here
                    if(CutTheCoin(coin))
                    {
                        // HandleDiceRoll(turnCounter);
                    }
                    else
                    {
                        UpdateTurn();
                        HandlePlayerNumIndicator(coin);
                    }
                }
                else
                {
                    if(animState==statusType.Cut)
                    {
                        StartCoroutine(TimeToCutOpponentCoin(coin,ocToCut));
                    }
                    else if(animState==statusType.Defend)
                    {
                        StartCoroutine(TimeToDefendAttack(coin,ocToDefend));
                    }
                    else
                    {
                        UpdateTurn();
                        HandlePlayerNumIndicator(coin);
                    }
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

            ////-------------------------
            ComputeWinPercentage(coin);
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

    #region Move Character one step front
    IEnumerator TimeToCutOpponentCoin(Coin coin1,Coin coin2)
    {
        yield return new WaitForSeconds(0.5f);
        Attack(coin1.transform);
        if(GameAudioHandler.instance!=null)
        {
            GameAudioHandler.instance.PlayCharacterAttackSound();
        }
        yield return new WaitForSeconds(2f);
        //playing death particke effect
        deathParticles[coin2.id].transform.position=new Vector3(coin2.transform.position.x,0.14f,coin2.transform.position.z);
        deathParticles[coin2.id].Play();
        //reset opponent coin
        StartCoroutine(ResetThisCoin(coin2));
       
        int tempCoinNewStepCount=coin1.stepCounter+players[coin1.id].initialPosIndex+1;
        if(tempCoinNewStepCount>coinPathContainer.childCount-1)
        {
            tempCoinNewStepCount=tempCoinNewStepCount-coinPathContainer.childCount;
        }
        Vector3 oneStepFrontPos=new Vector3(coinPathContainer.GetChild(tempCoinNewStepCount).position.x,coin1.transform.position.y,coinPathContainer.GetChild(tempCoinNewStepCount).position.z);
       
        iTween.MoveTo(coin1.transform.gameObject, iTween.Hash("position",oneStepFrontPos, 
            "speed", coinMoveSpeed, 
            "easetype", iTween.EaseType.linear,
            "oncomplete", "OneStepCompleted", 
            "oncompletetarget", this.gameObject
        ));

        //lets increment the step counter of coin by 1
        coin1.stepCounter++;
        coin1.stepsMovedTillNow++;
        //rotating character
        UpdateCharacterRotation(coin1.transform);
        Walk(coin1.transform);
        yield return new WaitForSeconds(0.3f);
        Idle(coin1.transform);
    }

    IEnumerator TimeToDefendAttack(Coin attackerCoin,Coin defenderCoin)
    {
        Quaternion opCoinOriginalRot=defenderCoin.transform.rotation;
        defenderCoin.transform.LookAt(attackerCoin.transform);
        DefendTheAttack(defenderCoin.transform);
        yield return new WaitForSeconds(0.5f);
        Attack(attackerCoin.transform);
        if(GameAudioHandler.instance!=null)
        {
            GameAudioHandler.instance.PlayCharacterDefendSound();
        }
        yield return new WaitForSeconds(2f);
        int tempCoinNewStepCount1=attackerCoin.stepCounter+players[attackerCoin.id].initialPosIndex+1;
        if(tempCoinNewStepCount1>coinPathContainer.childCount-1)
        {
            tempCoinNewStepCount1=tempCoinNewStepCount1-coinPathContainer.childCount;
        }
        Vector3 newstepPos=new Vector3(coinPathContainer.GetChild(tempCoinNewStepCount1).position.x,attackerCoin.transform.position.y,coinPathContainer.GetChild(tempCoinNewStepCount1).position.z);

        iTween.MoveTo(attackerCoin.transform.gameObject, iTween.Hash("position",newstepPos, 
            "speed", coinMoveSpeed, 
            "easetype", iTween.EaseType.linear,
            "oncomplete", "OneStepCompleted", 
            "oncompletetarget", this.gameObject
        ));

        //lets increment the step counter of coin by 1
        attackerCoin.stepCounter++;
        attackerCoin.stepsMovedTillNow++;
        //rotating character
        UpdateCharacterRotation(attackerCoin.transform);
        Walk(attackerCoin.transform);
        yield return new WaitForSeconds(0.3f);
        defenderCoin.transform.rotation=opCoinOriginalRot;
        Idle(defenderCoin.transform);
        Idle(attackerCoin.transform);
    }

    void OneStepCompleted()
    {
        StartCoroutine(HandleDiceRoll(turnCounter));
    }

    private Coin ocToCut=null;
    private Coin ocToDefend=null;
    void  CheckForCutAndDefend(Coin newC1)
    {
        ocToCut=null;
        ocToDefend=null;
        List<Coin> avaliableOpponentCoins=new List<Coin>();
        List<Coin> opCoinsAtTargetPos=new List<Coin>();

        List<Coin> tempDefendableCoins=new List<Coin>();
        List<Coin> tempMultiCoins=new List<Coin>();

        for(int i=0;i<gamePlayersList.Count;i++)
        {
            for(int j=0;j<players[gamePlayersList[i]].outCoins.Count;j++)
            {
                if(newC1.id!=players[gamePlayersList[i]].playerID)
                {
                    Coin tempCoin1=players[gamePlayersList[i]].outCoins[j].GetComponent<Coin>();
                    if(!tempCoin1.onWayToHome&&!tempCoin1.atHome)
                    {
                        if(!tempCoin1.isSafe)
                        {
                            if(!avaliableOpponentCoins.Contains(tempCoin1))
                            {
                                avaliableOpponentCoins.Add(tempCoin1);
                            }
                        }
                        //store every opponent coin
                        if(!tempDefendableCoins.Contains(tempCoin1))
                        {
                            tempDefendableCoins.Add(tempCoin1);
                        }
                    }
                }
            }
        }

        //now lets check how many opponents coins are there at current coins target position
        for(int i=0;i<avaliableOpponentCoins.Count;i++)
        {
            int opCoinPosIndex=avaliableOpponentCoins[i].stepCounter+players[avaliableOpponentCoins[i].id].initialPosIndex;
            int newC1PosIndex=newC1.stepCounter+players[newC1.id].initialPosIndex+currentDiceValue;
            if(opCoinPosIndex>coinPathContainer.childCount-1)
            {
                opCoinPosIndex=opCoinPosIndex-coinPathContainer.childCount;
            }

            if(newC1PosIndex>coinPathContainer.childCount-1)
            {
                newC1PosIndex=newC1PosIndex-coinPathContainer.childCount;
            }

            if(newC1PosIndex==opCoinPosIndex)
            {
                if(!opCoinsAtTargetPos.Contains(avaliableOpponentCoins[i]))
                {
                    opCoinsAtTargetPos.Add(avaliableOpponentCoins[i]);
                }
            }
        }

        for(int i=0;i<tempDefendableCoins.Count;i++)
        {
            int tempOpPosIndex=tempDefendableCoins[i].stepCounter+players[tempDefendableCoins[i].id].initialPosIndex;
            int curCoinTempPos=newC1.stepCounter+players[newC1.id].initialPosIndex+currentDiceValue;
            if(tempOpPosIndex>coinPathContainer.childCount-1)
            {
                tempOpPosIndex=tempOpPosIndex-coinPathContainer.childCount;
            }

            if(curCoinTempPos>coinPathContainer.childCount-1)
            {
                curCoinTempPos=curCoinTempPos-coinPathContainer.childCount;
            }

            if(tempOpPosIndex==curCoinTempPos)
            {
                if(!tempMultiCoins.Contains(tempDefendableCoins[i]))
                {
                    tempMultiCoins.Add(tempDefendableCoins[i]);
                }
            }
        }

        if(opCoinsAtTargetPos.Count==1)
        {
            ocToCut=opCoinsAtTargetPos[0];
            animState=statusType.Cut;
        }
        else if(tempMultiCoins.Count>0)
        {
            ocToDefend=tempMultiCoins[Random.Range(0,tempMultiCoins.Count)];
            animState=statusType.Defend;
        }
        else
        {
            animState=statusType.None;
        }
    }
    #endregion character cuttting opponent coins ends from here

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
                                StartCoroutine(ResetThisCoin(lastCuttedCoin));
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
        StartCoroutine(ResetThisCoin(lastCuttedCoin));
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
            // Debug.Log("Cut due to 6");
        }
        StartCoroutine(UpdateTurnAfterCutting());
    }

    IEnumerator ResetThisCoin(Coin coinToReset)
    {
        coinToReset.gameObject.SetActive(false);
        yield return new WaitForSeconds(1f);
        //spawn particle effect
        spawnParticles[coinToReset.id].transform.position=new Vector3(coinToReset.initialPosInfo.x,0.21f,coinToReset.initialPosInfo.z);
        spawnParticles[coinToReset.id].Play();

        if(GameAudioHandler.instance!=null)
        {
            GameAudioHandler.instance.PlayPartShatterSound();
        }

        coinToReset.gameObject.SetActive(true);
        //
        coinToReset.stepCounter=0;
        coinToReset.stepsMovedTillNow=0;
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

        if(GameAudioHandler.instance!=null)
        {
            GameAudioHandler.instance.PlayReSpawnSound();
        }
    }

    IEnumerator UpdateTurnAfterCutting()
    {
        yield return new WaitForSeconds(1f);
        StartCoroutine(HandleDiceRoll(turnCounter));
    }
    #endregion

    #region Win lose
    private List<int> winnersList=new List<int>();
    void CheckForWinner(Coin coin)
    {
        //confitti on entering home
        if(smallConfitti!=null)
        {
            smallConfitti.Play();
        }

        if(GameAudioHandler.instance!=null)
        {
            GameAudioHandler.instance.PlayConfettiExplosionSound();
        }
        
        //remove the coins from out coins
        players[turnCounter].outCoins.Remove(coin.transform);

        //lets check if home reached by getting 6 or not
        if(currentDiceValue==6)
        {
            bonusTurnCounter++;
            // Debug.Log("home reached due to 6");
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
            //lets play confitti on winning
            if(largeConfitti!=null)
            {
                largeConfitti.Play();
            }
            winnersList.Add(coin.id);

            if(GameAudioHandler.instance!=null)
            {
                GameAudioHandler.instance.PlayConfettiExplosionSound();
            }

            //lets find out winner and otehr players rank
            FindPlayersRank();
           
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

            //****************************************************
            if(totalHumanPlayers.Count==1)
            {
                //there is only one human player
                if(coin.id==totalHumanPlayers[0])
                {
                    //human won the game lets show winner ui
                    // Debug.Log("Human won the game lets show game end ui and compute completed percentage of each bots");
                    ShowLudoLeaderboard(1);
                    if(GameAudioHandler.instance!=null)
                    {
                        GameAudioHandler.instance.PlayVictorySound();
                    }
                }
                else
                {
                    //bot won lets show the leaderboard
                    // Debug.Log("Bot won the game lets show leaderboard ui");
                    ShowLudoLeaderboard(0);
                    if(GameAudioHandler.instance!=null)
                    {
                        GameAudioHandler.instance.PlayDefeatSound();
                    }
                }
            }
            else
            {
                //there are multiple human players
                if(gamePlayersList.Count>1)
                {
                    int remainingHuman=0;
                    for(int i=0;i<gamePlayersList.Count;i++)
                    {
                        if(players[gamePlayersList[i]].player==playerType.Human)
                        {
                            remainingHuman++;
                        }
                    }
                    //lets see if all human player completed or not
                    if(remainingHuman==0)
                    {
                        ShowLudoLeaderboard(1);
                    }
                    else
                    {
                        ShowLudoLeaderboard(0);
                    }
                }
                else
                {
                    ShowLudoLeaderboard(1);
                }
            }
            //****************************************************
        }
        else
        {
            StartCoroutine(HandleDiceRoll(turnCounter));
        }
        UpdateDicePositionOnBoard();
    }


    int p1,p2,p3,p4=0;
    void FindPlayersRank()
    {
        List<infoHolder> SortedList = players.OrderByDescending(o=>o.winningChance).ToList();
        if(LeaderboardHandler.instance!=null)
        {
            if(SortedList[0].player==playerType.Bot)
            {
                p1=1;
            }
            if(SortedList[1].player==playerType.Bot)
            {
                p2=1;
            }
            if(SortedList[2].player==playerType.Bot)
            {
                p3=1;
            }
            if(SortedList[3].player==playerType.Bot)
            {
                p4=1;
            }
            LeaderboardHandler.instance.UpdateFirstRank(SortedList[0].playerID,SortedList[0].colorName,p1,SortedList[0].winningChance);
            LeaderboardHandler.instance.UpdateSecondRank(SortedList[1].playerID,SortedList[1].colorName,p2,SortedList[1].winningChance);
            LeaderboardHandler.instance.UpdateThirdRank(SortedList[1].playerID,SortedList[2].colorName,p3,SortedList[2].winningChance);
            LeaderboardHandler.instance.UpdateFourthRank(SortedList[2].playerID,SortedList[3].colorName,p4,SortedList[3].winningChance);
        }
    }

    void ShowPlayerRank()
    {
        ShowLudoLeaderboard(1);
    }

    void ShowLudoLeaderboard(int lbValue)
    {
        if(LeaderboardHandler.instance!=null)
        {
            LeaderboardHandler.instance.ShowLeaderBoardUI(lbValue);
        }
    }

    #endregion

    #region  Calculations for leaderboard
    void ComputeWinPercentage(Coin coinToCompute)
    {
        float tp=0f;
        for(int i=0;i<generatedCoinsHolder.GetChild(coinToCompute.id).childCount;i++)
        {
            Coin gpCoin=generatedCoinsHolder.GetChild(coinToCompute.id).GetChild(i).GetComponent<Coin>();
            if(!gpCoin.atBase)
            {
                tp+=GetTravelledPercentage(gpCoin);
            }
        }
        tp=tp/generatedCoinsHolder.GetChild(coinToCompute.id).childCount;
        players[coinToCompute.id].winningChance=tp;
        // Debug.Log("winning percentage of "+players[coinToCompute.id].colorName+" is :"+tp.ToString("F2")+"%");
    }

    float GetTravelledPercentage(Coin tempCurCoin)
    {
        //56 is the total number of step a coin has to move to its entire life to reach home from base
        int tempTotalSetp=tempCurCoin.stepsMovedTillNow;
        float travelPercentage=0f;
        travelPercentage=(((float)tempTotalSetp/56f)*90f)+10f;
        return travelPercentage;
    }
  
    #endregion
    
    #region New AI Behaviour
    IEnumerator HandleBotTurn()
    {
        for(int i=0;i<generatedCoinsHolder.childCount;i++)
        {
            generatedCoinsHolder.GetChild(turnCounter).GetChild(i).GetComponent<Coin>().ResetPriorityValues();
        }
        yield return new WaitForSeconds(0.3f);
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
            // Debug.Log("There is high probablity that current coin can cut the opponent coin");
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

            // Debug.Log("There is high probablity that current coin can be moved to safe zone");
            break;

            case BotPriority.home:
            for(int i=0;i<possibleAIHomeCoins.Count;i++)
            {
                if(!movableAICoins.Contains(possibleAIHomeCoins[i]))
                {
                    movableAICoins.Add(possibleAIHomeCoins[i]);
                }
            }
            // Debug.Log("There is high probablity that current coin can reach home");
            break;

            case BotPriority.move:
            // Debug.Log("Coins avaliable for movement");
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

        Coin highestMovedCoin=movableAICoins[0];
        int totalCloseRangecoins=0;

        yield return new WaitForEndOfFrame();

        //totalOpponentOutCoins give information about all the opponents coins which are outside the base and which are not inside home lane

        for(int i=0;i<movableAICoins.Count;i++)
        {
            //lets get current position index of my coin
            int myCoinTempIndex=movableAICoins[i].stepCounter+players[movableAICoins[i].id].initialPosIndex;
            if(myCoinTempIndex>coinPathContainer.childCount-1)
            {
                myCoinTempIndex=myCoinTempIndex-coinPathContainer.childCount;
            }
            
            //lets find out percentage of present saftey, future saftey,future kill potential
            for(int j=0;j<totalOpponentOutCoins.Count;j++)
            {
                //lets get current position index of opponent coin
                int opponentCoinTempIndex=totalOpponentOutCoins[j].stepCounter+players[totalOpponentOutCoins[j].id].initialPosIndex;

                if(opponentCoinTempIndex>coinPathContainer.childCount-1)
                {
                    opponentCoinTempIndex=opponentCoinTempIndex-coinPathContainer.childCount;
                }

                if(myCoinTempIndex>opponentCoinTempIndex)
                {
                    //some of the opponents coins are behind me
                    if((myCoinTempIndex-opponentCoinTempIndex)<6)
                    {
                        if(!movableAICoins[i].isSafe)
                        {
                            //there is high possiblity that opponent coin can cut my coin
                            opponentsCoinsInfo tempBehindCoins= new opponentsCoinsInfo();
                            tempBehindCoins.curTurnCoin=movableAICoins[i];
                            tempBehindCoins.possibleOpponentCoins.Add(totalOpponentOutCoins[j]);
                            coinsBehindMe.Add(tempBehindCoins);
                            totalCloseRangecoins++;
                        }
                    }
                }
                else if(opponentCoinTempIndex>myCoinTempIndex)
                {
                    //some of the opponents coins are infront of me
                    if((opponentCoinTempIndex-myCoinTempIndex)<6)
                    {
                        if(!movableAICoins[i].isSafe)
                        {
                            //there is high possiblity that my coin can cut opponents coin
                            opponentsCoinsInfo tempfrontCoins= new opponentsCoinsInfo();
                            tempfrontCoins.curTurnCoin=movableAICoins[i];
                            tempfrontCoins.possibleOpponentCoins.Add(totalOpponentOutCoins[j]);
                            coinsInfrontOfMe.Add(tempfrontCoins);
                            totalCloseRangecoins++;
                        }
                    }
                }
            }

            if(highestMovedCoin.stepCounter<movableAICoins[i].stepCounter)
            {
                highestMovedCoin=movableAICoins[i];
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

        //calculation for the new highest priority coin
        Coin highestPriorityCoin=movableAICoins[0];
        
        if(totalCloseRangecoins==0)
        {
            if(highestMovedCoin.isSafe&&movableAICoins.Count>1)
            {   
                movableAICoins.Remove(highestMovedCoin);
            }
            highestPriorityCoin=GetNewAiCoinForMovement();
        }
        else
        {
            highestPriorityCoin=GetHighPriorityCoin();
        }

        yield return new WaitForEndOfFrame();
        StartCoroutine(UpdateCoinPosition(highestPriorityCoin));
    }

    Coin GetHighPriorityCoin()
    {
        Coin tempHighCoin=movableAICoins[0];
        for(int i=0;i<movableAICoins.Count;i++)
        {
           if(movableAICoins[i].GetMaxWeightPercentage()>tempHighCoin.GetMaxWeightPercentage())
            {
                tempHighCoin=movableAICoins[i];
            }
        }
        return tempHighCoin;
    }

    Coin GetNewAiCoinForMovement()
    {
        Coin tempNewCoin=movableAICoins[0];
        List<Coin> unsafeMovableCoins=new List<Coin>();
        for(int i=0;i<movableAICoins.Count;i++)
        {
            if(!movableAICoins[i].isSafe)
            {
                unsafeMovableCoins.Add(movableAICoins[i]);
            }
        }

        if(unsafeMovableCoins.Count>0)
        {
            Coin unsafeHighCoin=unsafeMovableCoins[0];
            for(int i=0;i<unsafeMovableCoins.Count;i++)
            {   
                if(unsafeMovableCoins[i].GetMaxWeightPercentage()>unsafeHighCoin.GetMaxWeightPercentage())
                {
                    unsafeHighCoin=unsafeMovableCoins[i];
                }
            }
            tempNewCoin=unsafeHighCoin;
        }
        else
        {
            tempNewCoin=movableAICoins[Random.Range(0,movableAICoins.Count)];
        }
        return tempNewCoin;
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
                            StartCoroutine(ResetThisCoin(maxTravelledCoin));
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
       
        Vector3 centerPos=new Vector3(center.transform.position.x,0.2f,center.transform.position.z);
        Collider[] hitColliders = Physics.OverlapSphere(centerPos, 0.1f);
        List<Coin> coinsAtSimilarPosition=new List<Coin>();

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
                // Debug.Log("Reset Successful..");
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
    
            if(GameAudioHandler.instance!=null)
            {
                GameAudioHandler.instance.PlayCharacterWalkSound();
            }
        }
        else
        {
            if(GameAudioHandler.instance!=null)
            {
                GameAudioHandler.instance.PlayCoinMoveSound();
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

    void Attack(Transform currentChar)
    {
        if(playerModel==modelType.character)
        {
            if(CharAnimationHandler.instance!=null)
            {
                CharAnimationHandler.instance.PlayAttackAnimation(currentChar);
            }
        }
    }


    void DefendTheAttack(Transform currentChar)
    {
        if(playerModel==modelType.character)
        {
            if(CharAnimationHandler.instance!=null)
            {
                CharAnimationHandler.instance.PlayDefendAttackAnimation(currentChar);
            }
        }
    }

    #endregion

    #region art style
    void HandleGameArtStyle()
    {
        if(boardArtStyle==boardStyle.board_default)
        {
            UpdateCharSmoothness(0);
            boardMat.SetFloat ("_Glossiness", 0f);
            boardMat.SetFloat ("_Metallic", 0f);
        }
        else
        {
            UpdateCharSmoothness(1);
            boardMat.SetFloat ("_Glossiness", 0.3f);
            boardMat.SetFloat ("_Metallic", 0.4f);
        }
    }

    void UpdateCharSmoothness(int metallic_ID)
    {
        if(playerModel==modelType.character)
        {
            for(int i=0;i<charBodyMat.Length;i++)
            {
                if(metallic_ID==1)
                {
                    charBodyMat[i].SetFloat ("_Glossiness",0.35f);
                    charBodyMat[i].SetFloat ("_Metallic", 0.7f);
                }
                else
                {
                    charBodyMat[i].SetFloat ("_Glossiness",0f);
                    charBodyMat[i].SetFloat ("_Metallic",0f);
                }
            }
        }
    }
    #endregion
}
