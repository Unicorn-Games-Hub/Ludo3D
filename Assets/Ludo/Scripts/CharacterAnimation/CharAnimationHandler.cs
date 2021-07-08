using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharAnimationHandler : MonoBehaviour
{
    public static CharAnimationHandler instance;

    [Header("Character Animations")]
    public AnimationClip idleAnim;
    public AnimationClip walkAnim;
    public AnimationClip defensiveAnim;
    public AnimationClip jumpAnim;
    public AnimationClip attackAnim;
    public AnimationClip killAnim;
    public AnimationClip deathAnim;
    public AnimationClip defendAttackAnim;

    private Animation anim;

    string walkAnimName;
    string defendAttackAnimName;
    
    void Start()
    {
        if(instance!=null)
        {
            return;
        }
        else
        {
            instance=this;
        }

       walkAnimName=walkAnim.name;
       defendAttackAnimName=defendAttackAnim.name;
    }

    #region Character animations
    public void PlayIdleAnimation(Transform t)
    {
        anim=t.GetComponent<Animation>();
        anim.clip=idleAnim;
        anim.Play();
    }

    public void PlayWalkAnimation(Transform t)
    {
        anim=t.GetComponent<Animation>();
        anim.clip=walkAnim;
        anim[walkAnimName].speed=GameController.instance.coinMoveSpeed;
        anim.Play();
    }

    public void PlayDefensiveAnimation(Transform t)
    {
        anim=t.GetComponent<Animation>();
        anim.clip=defensiveAnim;
        anim.Play();
    }

    public void PlayAttackAnimation(Transform t)
    {
        anim=t.GetComponent<Animation>();
        anim.clip=attackAnim;
        anim.Play();
    }

    public void PlayKillAnimation(Transform t)
    {
        anim=t.GetComponent<Animation>();
        anim.clip=killAnim;
        anim.Play();
    }

    public void PlayDeathAnimation(Transform t)
    {
        anim=t.GetComponent<Animation>();
        anim.clip=deathAnim;
        anim.Play();
    }

    public void PlayDefendAttackAnimation(Transform t)
    {
        anim=t.GetComponent<Animation>();
        anim.clip=defendAttackAnim;
        anim[defendAttackAnimName].speed=2.2f;
        anim.Play();
    }

    #endregion
}
