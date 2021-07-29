using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CutSceneAnimationHandler : MonoBehaviour
{
    public static CutSceneAnimationHandler instance;

    public Vector3 defenderPos=Vector3.zero;
    public Vector3 attackerPos=Vector3.zero;
    public Vector3 attackingPos=Vector3.zero;

    private Transform defender=null;
    private Transform attacker=null;

    private float walkSpeed=1f;

    public CharAnimationHandler animHandler;

    [Header("Object To Hide")]
    public GameObject[] obToHideForCutScene;

    [Header("Screen Fader")]
    public GameObject screnFadeHolder;
    public Image screenFader;
    private float fadeAmount=0;

    public bool fadeIn=false;
    public bool fadeOut=false;
    public float fadeSpeed=2f;

    private bool isStarted=false;

    private int dId=0;
    private int aId=0;
    private int animID=0;

    [Header("Background Color")]
    public Image bgImage;


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
       HideCharacters();
       screnFadeHolder.SetActive(false);
    }
    
    public void StartCutSceneAnimation(int defenderID,int attackerID,int animationId)
    {
        screnFadeHolder.SetActive(true);
        fadeAmount=0f;
        dId=defenderID;
        aId=attackerID;
        animID=animationId;
        isStarted=true;
        fadeIn=true;
        bgImage.color=new Color(0f,0f,0f,1f);
    }

    void Update()
    {
        if(fadeIn)
        {
            if(fadeAmount<1f)
            {
                fadeAmount+=Time.deltaTime*fadeSpeed;
            }
            else
            {
                HandleCutScene();
                fadeIn=false;
            }
        }
        else if(fadeOut)
        {
            if(fadeAmount>0f)
            {
                fadeAmount-=Time.deltaTime*fadeSpeed;
            }
            else
            {
                fadeOut=false;
            }
        }
        screenFader.color=new Color(0f,0f,0f,fadeAmount);
    }

    void HandleCutScene()
    {
        if(isStarted)
        {
            fadeOut=true;
            HandleCutSceneEssentials(false);
            UpdateCutSceneInfo(dId,aId);
            isStarted=false;
        }
        else
        {
            HideCharacters();
            bgImage.color=new Color(1f,1f,1f,1f);
            HandleCutSceneEssentials(true);
            fadeOut=true;
        }
    }

    void UpdateCutSceneInfo(int defenderIndex,int attackerIndex)
    {
        defender=transform.GetChild(defenderIndex);
        attacker=transform.GetChild(attackerIndex);
        defender.position=defenderPos;
        attacker.position=attackerPos;
        defender.gameObject.SetActive(true);
        attacker.gameObject.SetActive(true);
        //make defender idle
        animHandler.PlayIdleAnimation(defender);
        MoveAttacker();
    }
    
    void MoveAttacker()
    {
        animHandler.PlayWalkAnimation(attacker);
        walkSpeed=GameController.instance.coinMoveSpeed;
        iTween.MoveTo(attacker.gameObject, iTween.Hash("position",attackingPos, 
        "speed", walkSpeed, 
        "easetype", iTween.EaseType.linear,
        "oncomplete", "StartAttacking", 
        "oncompletetarget", this.gameObject
        ));
    }

    void StartAttacking()
    {
        animHandler.PlayKillAnimation(attacker);
        if(animID==0)
        {
            //from here we will check either it is for defending or killing
            StartCoroutine(KillDefender());
        }
        else
        {
            StartCoroutine(DefendTheAttack());
        }
       
    }

    //for killing the defender
    IEnumerator KillDefender()
    {
        if(GameAudioHandler.instance!=null)
        {
            GameAudioHandler.instance.PlayCharacterAttackSound();
        }
        yield return new WaitForSeconds(1f);
        animHandler.PlayDeathAnimation(defender);
        yield return new WaitForSeconds(1f);
        fadeIn=true;
        if(GameController.instance!=null)
        {
            GameController.instance.ResetCutCoin();
        }
    }

    //for defending the attacker
    IEnumerator DefendTheAttack()
    {
        if(GameAudioHandler.instance!=null)
        {
            GameAudioHandler.instance.PlayCharacterDefendSound();
        }
        animHandler. PlayDefendAttackAnimation(defender);
        yield return new WaitForSeconds(1f);
        fadeIn=true;
    }

   void HideCharacters()
   {
        for(int i=0;i<transform.childCount;i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
   }

   void HandleCutSceneEssentials(bool activationValue)
   {
       for(int i=0;i<obToHideForCutScene.Length;i++)
       {
           obToHideForCutScene[i].SetActive(activationValue);
       }
   }
}
