using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndicatorRotator : MonoBehaviour
{
    private float rotationSpeed=100f;
    private float rotAngle=0f;

    // Update is called once per frame
    void Update()
    {
        rotAngle+=Time.deltaTime*rotationSpeed;
        transform.rotation=Quaternion.Euler(-90f,rotAngle,0f); 
    }
}
