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
    public float diceThrowForce=50f;
    private bool startDiceAnimation=false;
    private float x;
    private float diceRollMultiplier=10f;

    [Header("Dice Roll Controller")]
    public bool canRollDice=false;

   //array for dice face rotation
   private Vector2[] diceFaceArray={new Vector2(180f,0f),new Vector2(270f,0f),new Vector2(0f,90f),new Vector2(0f,270f),new Vector2(90f,0f),new Vector2(0f,0f)};
   [Range(1,6)]
   public int currentDiceValue=1;

   [Header("Dice")]
   private Material diceMat;
   private Color normalColor;

   public Color[] diceNormalColor;

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
        diceMat=GetComponent<MeshRenderer>().material;
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
                differenceVector=finalDicePos-initialDicePos;
                if(differenceVector.magnitude>0f)
                {
                    rb.AddForce(differenceVector.normalized * diceThrowForce, ForceMode.Impulse);
                }
                StartCoroutine(RolltheDice());
                isDragging=false;
                diceStates=states.rolling;
            }
       }
      
        if(isDragging)
        {
            curMousePos=Camera.main.ScreenToViewportPoint(Input.mousePosition);
            tempPos=GetMousePosAsWorldPos()+offset;
            finalDicePos=tempPos;
            tempPos.x=Mathf.Clamp(tempPos.x,-2f,2f);
            tempPos.z=Mathf.Clamp(tempPos.z,-2.5f,3f);
            transform.position=Vector3.Lerp(transform.position,new Vector3(tempPos.x,transform.position.y,tempPos.z),Time.deltaTime*dragSensitivity);
        }

        if(startDiceAnimation)
        {
            x+=diceRollMultiplier;
           transform.rotation=Quaternion.Euler(x,0f,x);
        }
   }

   public IEnumerator RolltheDice()
   {
       if(GameAudioHandler.instance!=null)
       {
           GameAudioHandler.instance.PlayDiceRollSound();
       }
       //from here we will stop blinking animation
       GameController.instance.StopBlinkingAnimation();
       currentDiceValue=GetRandomDiceValue(currentAttempts);
        startDiceAnimation=true;
        yield return new WaitForSeconds(1f);
        startDiceAnimation=false;
        yield return new WaitForEndOfFrame();
        RotateDiceToCorrectFace();
        diceStates=states.idle;
        rb.drag=40f;
        canRollDice=false;
        GameController.instance.HandleObtainedDiceValue(currentDiceValue);
   }

   void RotateDiceToCorrectFace()
   {
        transform.rotation=Quaternion.Euler(diceFaceArray[currentDiceValue-1].x,0f,diceFaceArray[currentDiceValue-1].y);
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

        iTween.ValueTo(gameObject, iTween.Hash("name", "scaleAnim","from", 0.9f, "to", 0.8f,"onupdate", 
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
           "from", 0.9f, "to", 0.8f,
           "onupdate", "tweenScaleCallback",
           "easetype", iTween.EaseType.easeOutSine, "time", 0.4f));
    }

    void tweenScaleCallback(float tempScale)
    {
        transform.localScale=new Vector3(tempScale,tempScale,tempScale);
    }
    #endregion
}
