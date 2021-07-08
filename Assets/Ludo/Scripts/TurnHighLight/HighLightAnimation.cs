using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighLightAnimation : MonoBehaviour
{
   private Material baseMat;

   [SerializeField]private Color initialColor;
   [SerializeField]private Color highlightedColor;


   void Start()
   {
       baseMat=GetComponent<Renderer>().material;
   }

   public void HighLight()
   {
        iTween.ValueTo(gameObject, iTween.Hash("name", "sp1","from", initialColor, "to", highlightedColor,"onupdate", 
        "tweenOnUpdateCallBack","loopType", iTween.LoopType.pingPong, "easetype", iTween.EaseType.linear, "time", .4f, "delay", 0.2f));
   }

    public void tweenOnUpdateCallBack(Color newColorValue)
    {
        baseMat.color = newColorValue;
    }

    public void StopAnimation()
    {
        iTween.StopByName(gameObject, "sp1");
        iTween.ValueTo(gameObject, iTween.Hash("name", "sp1",
           "from", baseMat.color, "to", initialColor,
           "onupdate", "tweenOnUpdateCallBack",
           "easetype", iTween.EaseType.easeOutSine, "time", 0.4f));
    }

}
