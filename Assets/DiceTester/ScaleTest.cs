using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleTest : MonoBehaviour
{
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            iTween.ScaleTo(gameObject, iTween.Hash("x", 2, "y", 2,"z",2, "time", 0.6f));
        }
    }

    void updateScale(float scaleValue)
    {
        transform.localScale=new Vector3(scaleValue,scaleValue,scaleValue);
    }
}
