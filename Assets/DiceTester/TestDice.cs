using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDice : MonoBehaviour
{
    private Vector3 initialMousePos,finalMousePos,initialDicePos,offset,curMousePos,finalDicePos,tempPos,differenceVector;
    private float zCoord;
    private bool isDragging=false;

    public float dragSensitivity=10f;

    public float diceThrowForce=50f;
    
    private Rigidbody rb;

     
    void Start()
    {
        rb=GetComponent<Rigidbody>();
    }

    void Update()
    {
        //lastFrameVelocity = rb.velocity;
        if(Input.GetMouseButtonDown(0))
        {
            HandleDrag(0f);
            initialMousePos=Camera.main.ScreenToWorldPoint(Input.mousePosition);
            zCoord=Camera.main.WorldToScreenPoint(transform.position).z;
            offset=transform.position-GetMousePosAsWorldPos();
            initialDicePos=GetMousePosAsWorldPos()+offset;
            isDragging=true;
        }
        else if(Input.GetMouseButtonUp(0)&&isDragging)
        {
            isDragging=false;
            differenceVector=finalDicePos-initialDicePos;  
            //rb.velocity=differenceVector.normalized*diceThrowForce;
            //rb.AddForce(differenceVector.normalized*diceThrowForce,ForceMode.Impulse);
            //rb.AddTorque(Vector3.Cross(finalDicePos, initialDicePos) * 1000f, ForceMode.Impulse);
            rb.AddForce ((differenceVector.normalized) * (Vector3.Distance (finalDicePos, initialDicePos)) *diceThrowForce* rb.mass);
            
        }

        if(isDragging)
        {
            curMousePos=Camera.main.ScreenToViewportPoint(Input.mousePosition);
            tempPos=GetMousePosAsWorldPos()+offset;
            finalDicePos=tempPos;
            tempPos.x=Mathf.Clamp(tempPos.x,-5f,5f);
            tempPos.z=Mathf.Clamp(tempPos.z,-8f,8f);
            transform.position=Vector3.Lerp(transform.position,new Vector3(tempPos.x,transform.position.y,tempPos.z),Time.deltaTime*dragSensitivity);
        }
    }

    private Vector3 GetMousePosAsWorldPos()
    {
        Vector3 mousePos=Input.mousePosition;
        mousePos.z=zCoord;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }

    void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.layer==LayerMask.NameToLayer("wall"))
        {
            HandleDrag(6f);
        }
    }

    void HandleDrag(float dragValue)
    {
        rb.drag=dragValue;
        rb.angularDrag=dragValue;
    }

    /*
    [SerializeField]
    [Tooltip("Just for debugging, adds some velocity during OnEnable")]
    private Vector3 initialVelocity;

    [SerializeField]
    private float minVelocity = 10f;

    private Vector3 lastFrameVelocity;


    private void OnCollisionEnter(Collision collision)
    {
        Bounce(collision.contacts[0].normal);
    }

    private void Bounce(Vector3 collisionNormal)
    {
        var speed = lastFrameVelocity.magnitude;
        var direction = Vector3.Reflect(lastFrameVelocity.normalized, collisionNormal);
        rb.velocity = direction * Mathf.Max(speed, minVelocity);
    }
    */
}
