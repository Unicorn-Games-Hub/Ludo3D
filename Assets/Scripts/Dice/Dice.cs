using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dice : MonoBehaviour
{
   private Rigidbody rb;

   private Vector3 initialMousePos,curMousePos;
   private float zCoord;
   private Vector3 offset;
   private Vector3 tempPos;
   private bool isDragging=false;
   public float dragSensitivity=5f;

   //
   private Vector3 initialDicePos;
   private Vector3 finalDicePos;
   private Vector3 differenceVector;

   [Header("Dice Behaviour")]
   public float diceThrowForce=50f;

   //
   public bool startDiceAnimation=false;

   void Start()
   {
       rb=GetComponent<Rigidbody>();
   }

   void Update()
   {
       if(Input.GetMouseButtonDown(0))
        {
            initialMousePos=Camera.main.ScreenToWorldPoint(Input.mousePosition);
            zCoord=Camera.main.WorldToScreenPoint(transform.position).z;
            offset=transform.position-GetMousePosAsWorldPos();
            initialDicePos=GetMousePosAsWorldPos()+offset;
            isDragging=true;
        }
        else if(Input.GetMouseButtonUp(0)&&isDragging)
        {
            differenceVector=finalDicePos-initialDicePos;
            rb.AddForce(differenceVector.normalized * diceThrowForce, ForceMode.Impulse);
            startDiceAnimation=true;
            isDragging=false;
            StartCoroutine(CheckDiceVelocity());
        }

        if(isDragging)
        {
            curMousePos=Camera.main.ScreenToViewportPoint(Input.mousePosition);
            tempPos=GetMousePosAsWorldPos()+offset;
            finalDicePos=tempPos;
            tempPos.x=Mathf.Clamp(tempPos.x,-2f,2f);
            tempPos.z=Mathf.Clamp(tempPos.z,-3f,3f);
            transform.position=Vector3.Lerp(transform.position,new Vector3(tempPos.x,transform.position.y,tempPos.z),Time.deltaTime*dragSensitivity);
        }

        if(startDiceAnimation)
        {
           transform.Rotate(Vector3.up*20f*Time.deltaTime);
        }
   }

   IEnumerator CheckDiceVelocity()
   {
        yield return new WaitForSeconds(0.5f);
        while(rb.velocity.magnitude>0.5f)
        {
            yield return new WaitForSeconds(0.1f);
        }
        startDiceAnimation=false;
   }

    private Vector3 GetMousePosAsWorldPos()
    {
        Vector3 mousePos=Input.mousePosition;
        mousePos.z=zCoord;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }
}
