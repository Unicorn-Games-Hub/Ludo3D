using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dice : MonoBehaviour
{
    public static Dice instance;
    private Rigidbody rb;
    private RaycastHit hit;
    public LayerMask diceLayer;

    //dice dragging and throwing
   private Vector3 initialMousePos,curMousePos;
   private float zCoord;
   private Vector3 offset;
   private Vector3 tempPos;
   private bool isDragging=false;
   public float dragSensitivity=5f;
   private Vector3 initialDicePos;
   private Vector3 finalDicePos;
   private Vector3 differenceVector;

    [Header("Dice Behaviour")]
    [Range(5f,50f)]
    public float diceThrowForce=25f;
    private bool startDiceAnimation=false;
    private float x;
    private float diceRollMultiplier=10f;

    [Header("Dice Roll Controller")]
    public bool canRollDice=false;
    private Animation diceAnim;
    public AnimationClip rollAnim;
    private string rollAnimName;
    [Range(1f,5f)]
    public float rollSpeed=1f;


   //array for dice face rotation
   private Vector2[] diceFaceArray={new Vector2(180f,0f),new Vector2(270f,0f),new Vector2(0f,90f),new Vector2(0f,270f),new Vector2(90f,0f),new Vector2(0f,0f)};
   [HideInInspector]
   [Range(1,6)]
   public int currentDiceValue=1;

   [Header("Dice")]
   public Transform ludoDice;
   public Material diceMat;
   private Color diceMatCol;
   private Color normalColor;
   public Color[] diceNormalColor;
    private bool transparencyChanged=false;

   public enum states
   {
       idle,
       dragging,
       rolling
   }
   public states diceStates;

   [Header("Debug Mode")]
   public bool enableDebugging=false;
   public int minDebugVal;
   public int maxDebugVal;

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
        rb=GetComponent<Rigidbody>();
        currentDiceValue=Random.Range(1,7);
        RotateDiceToCorrectFace();
        diceStates=states.idle;
        diceAnim=ludoDice.GetComponent<Animation>();
        rollAnimName=rollAnim.name;
   }

   void Update()
   {
       if(diceStates!=states.rolling)
       {
            if(Input.GetMouseButtonDown(0)&&canRollDice)
            {
                Ray ray=Camera.main.ScreenPointToRay(Input.mousePosition);
                if(Physics.Raycast(ray,out hit,diceLayer))
                {
                    if(hit.collider!=null)
                    {   
                        if(hit.collider.gameObject.tag=="Dice")
                        {
                            initialMousePos=Camera.main.ScreenToWorldPoint(Input.mousePosition);
                            zCoord=Camera.main.WorldToScreenPoint(transform.position).z;
                            offset=transform.position-GetMousePosAsWorldPos();
                            initialDicePos=GetMousePosAsWorldPos()+offset;
                            isDragging=true;
                            rb.drag=0f;
                            diceStates=states.dragging;
                        }
                    }
                }
            }
            else if(Input.GetMouseButtonUp(0)&&isDragging)
            {
                rb.velocity=(finalDicePos-initialDicePos).normalized*diceThrowForce;
                //rb.AddForce (((finalDicePos-initialDicePos).normalized) * (Vector3.Distance (finalDicePos, initialDicePos)) * diceThrowForce * rb.mass);
                StartCoroutine(RolltheDice());
                isDragging=false;
                diceStates=states.rolling;
                UpdateDragForce();
            }
       }
      
        if(isDragging)
        {
            curMousePos=Camera.main.ScreenToViewportPoint(Input.mousePosition);
            tempPos=GetMousePosAsWorldPos()+offset;
            finalDicePos=tempPos;
            tempPos.x=Mathf.Clamp(tempPos.x,-2.4f,2.4f);
            tempPos.z=Mathf.Clamp(tempPos.z,-2.4f,2.4f);
            transform.position=Vector3.Lerp(transform.position,new Vector3(tempPos.x,transform.position.y,tempPos.z),Time.deltaTime*dragSensitivity);
        }

        if(startDiceAnimation)
        {
            //x+=diceRollMultiplier;
            // transform.rotation=Quaternion.Euler(x,0f,x);
        }        
   }

   void UpdateDragForce()
   {
        rb.drag=5f;
        rb.angularDrag=5f;
   }


   public IEnumerator RolltheDice()
   {
       if(GameAudioHandler.instance!=null)
       {
           GameAudioHandler.instance.PlayDiceRollSound();
       }
       //from here we will stop blinking animation
       GameController.instance.StopBlinkingAnimation();

       if(handleDiceProbablity)
       {
            switch(cutProbablity)
            {
                case difficultyType.low:
                currentDiceValue=GetLowCutProbablity();
                break;
                case difficultyType.medium:
                currentDiceValue=GetMediumCutProbablity();
                break;
                case difficultyType.high:
                currentDiceValue=GetHighCutProbablity();
                break;
            }
            handleDiceProbablity=false;
       }
       else
       {
           currentDiceValue=GetRandomDiceValue(currentAttempts);
       }
        diceAnim.clip=rollAnim;
        diceAnim[rollAnimName].speed=rollSpeed;
        diceAnim.Play();
        AnimateDiceScale(true);
        yield return new WaitForSeconds(0.6f);
        diceAnim.Stop();
        startDiceAnimation=false;
        yield return new WaitForEndOfFrame();
        RotateDiceToCorrectFace();
        AnimateDiceScale(false);
        diceStates=states.idle;
        transparencyChanged=false;
        canRollDice=false;
        GameController.instance.HandleObtainedDiceValue(currentDiceValue);
        currentAttempts=0;
   }

   void RotateDiceToCorrectFace()
   {
        rb.velocity=Vector3.zero;
        rb.angularVelocity=Vector3.zero;
        ludoDice.rotation=Quaternion.Euler(diceFaceArray[currentDiceValue-1].x,0f,diceFaceArray[currentDiceValue-1].y);
   }

    private Vector3 GetMousePosAsWorldPos()
    {
        Vector3 mousePos=Input.mousePosition;
        mousePos.z=zCoord;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }

    #region Probablity of gettting 6
    int currentAttempts =0;
    public void UpdateDiceProbablity(int receivedAttempts)
    {
       currentAttempts=receivedAttempts;
    }

    int GetRandomDiceValue(int attempts)
    {
       int numToReplace=0;
       if(attempts<2)
       {
            numToReplace=1;
       }
       else if(attempts>=2&&attempts<4)
       {
           numToReplace=2;
       }
       else
       {
            numToReplace=3;
       }

        int[] newArray=GetProbablityArray(numToReplace);
        int newIndex=Random.Range(0,newArray.Length);
        int newDiceValue=newArray[newIndex];
        return newDiceValue;
    }

    int[] GetProbablityArray(int noToReplace)
    {
        int[] outComeArray={1,2,3,4,5,6};
        while(noToReplace!=0)
        {
            int i0=Random.Range(0,5);
            int num0=outComeArray[i0];
            if(num0!=6)
            {
                outComeArray[i0]=6;
                noToReplace--;
            }
        }
        return outComeArray;
    }
    #endregion

    #region probablity of cutting
    public enum difficultyType
    {
        low,
        medium,
        high
    }
    public difficultyType cutProbablity;

    public List<int> defaultOutComeList=new List<int>();
    private List<int> outcomeList=new List<int>();

    private bool handleDiceProbablity=false;

    public void UpdateCutProbablity(List<int> tempOutList)
    {
        outcomeList=tempOutList;
        handleDiceProbablity=true;
    }

    int GetLowCutProbablity()
    {
        List<int> newProbablityList=defaultOutComeList;
        for(int i=0;i<outcomeList.Count;i++)
        {
            if(newProbablityList.Contains(outcomeList[i]))
            {
                newProbablityList.Remove(outcomeList[i]);
            }
        }

        int n0=Random.Range(0,newProbablityList.Count);
        int n0Value=newProbablityList[n0];
        return n0Value;
    }

    int GetMediumCutProbablity()
    {
        int n1=Random.Range(0,defaultOutComeList.Count);
        int n1Value=defaultOutComeList[n1];
        return n1Value;
    }

    int GetHighCutProbablity()
    {
        int n2=Random.Range(0,defaultOutComeList.Count);
        int n2Value=defaultOutComeList[n2];
        return n2Value;
    }
    #endregion

    #region HighLightAnimation
    private Color initialColor;
    private Color highlightedColor;
    public void UpdateDiceMaterial(Color dicecolor)
    {
        diceMat.color=dicecolor;
        initialColor=dicecolor;
    }

    public void tweenOnUpdateCallBack(Color newColorValue)
    {
        diceMat.color=newColorValue;
    }

    public void StartDiceHighlights(Color newcol,int pIndex)
    {
        normalColor=diceNormalColor[pIndex];
        iTween.ValueTo(gameObject, iTween.Hash("name", "sp1","from", newcol, "to", normalColor,"onupdate", 
        "tweenOnUpdateCallBack","loopType", iTween.LoopType.pingPong, "easetype", iTween.EaseType.linear, "time", .4f, "delay", 0.2f));

        iTween.ValueTo(gameObject, iTween.Hash("name", "scaleAnim","from", 1f, "to", 1.1f,"onupdate", 
        "tweenScaleCallback","loopType", iTween.LoopType.pingPong, "easetype", iTween.EaseType.linear, "time", .1f, "delay", 0.05f));
    }

    public void StopDiceHighLight()
    {
        iTween.StopByName(gameObject, "sp1");
        iTween.ValueTo(gameObject, iTween.Hash("name", "sp1",
           "from", diceMat.color, "to", initialColor,
           "onupdate", "tweenOnUpdateCallBack",
           "easetype", iTween.EaseType.easeOutSine, "time", 0.4f));

           iTween.StopByName(gameObject, "scaleAnim");
        iTween.ValueTo(gameObject, iTween.Hash("name", "scaleAnim",
           "from", 1.1f, "to", 1f,
           "onupdate", "tweenScaleCallback",
           "easetype", iTween.EaseType.easeOutSine, "time", 0.4f));
    }

    void tweenScaleCallback(float tempScale)
    {
        ludoDice.localScale=new Vector3(tempScale,tempScale,tempScale);
    }
    #endregion

    void AnimateDiceScale(bool value)
    {
        if(value)
        {
            iTween.ScaleTo(gameObject, iTween.Hash("x", 1.1, "y", 1.1,"z",1.1, "time", 0.5f));
        }
        else
        {
            iTween.ScaleTo(gameObject, iTween.Hash("x", 1, "y", 1,"z",1, "time", 0.5f));
        }
    }

    #region changing transparency of the dice
    void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.layer==LayerMask.NameToLayer("Coin")||col.gameObject.layer==LayerMask.NameToLayer("OppCoin"))
        {
            if(diceStates==states.idle)
            {
                FadeOutDiceColor();
            }
        }
    }

    void FadeOutDiceColor()
    {
        if(!transparencyChanged)
        {
            diceMatCol=diceMat.color;
            diceMat.color=new Color(diceMatCol.r,diceMatCol.g,diceMatCol.b,0.4f);
            transparencyChanged=true;
        }
    }
    #endregion
}
