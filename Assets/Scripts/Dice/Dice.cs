using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dice : MonoBehaviour
{
    public static Dice instance;
   private Rigidbody rb;
   private RaycastHit hit;

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

   public enum states
   {
       idle,
       dragging,
       rolling
   }
   public states diceStates;

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
        currentDiceValue=GetRandomDiceValue();
        RotateDiceToCorrectFace();
        diceStates=states.idle;
   }

   void Update()
   {
       if(diceStates!=states.rolling)
       {
            if(Input.GetMouseButtonDown(0)&&canRollDice)
            {
                Ray ray=Camera.main.ScreenPointToRay(Input.mousePosition);
                if(Physics.Raycast(ray,out hit))
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
                rb.AddForce(differenceVector.normalized * diceThrowForce, ForceMode.Impulse);
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
       currentDiceValue=GetRandomDiceValue();
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

   int GetRandomDiceValue()
   {
        return Random.Range(1,7);
      
        //for testing only
        //return Random.Range(1,2);
        // if(Random.Range(0,2)==0)
        // {
        //     return 6;
        // }
        // else 
        // {
        //     return 5;
        // }
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
}
