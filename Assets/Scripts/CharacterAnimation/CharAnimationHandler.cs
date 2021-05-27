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

    private Animation anim;

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
        anim.Play();
    }

    public void PlayDefensiveAnimation(Transform t)
    {
        anim=t.GetComponent<Animation>();
        anim.clip=defensiveAnim;
        anim.Play();
    }

    #endregion
}
