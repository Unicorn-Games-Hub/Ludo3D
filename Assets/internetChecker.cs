using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class internetChecker : MonoBehaviour
{
    public GameObject rayBlocker,checkinternet;
    
    public float counter = 0.3f;
    // Update is called once per frame
    void Update()
    {
        if (myMannager.Instance.isMultiPlayer)
        {
            if (counter > 0)
            {
                counter -= Time.deltaTime * 0.1f;

            }
            else
            {

                counter = 1;

                if (!rayBlocker.activeInHierarchy && !checkinternet.activeInHierarchy)
                {
                    if (Application.internetReachability == NetworkReachability.NotReachable)
                    {

                        rayBlocker.SetActive(true);
                        checkinternet.SetActive(true);
                        checkinternet.GetComponent<DOTweenAnimation>().DORestart();
                    }
                }
                else
                {
                    if (Application.internetReachability != NetworkReachability.NotReachable)
                    {
                        rayBlocker.SetActive(false);
                        checkinternet.SetActive(false);
                    }
                }
            }
        }
    }

    public void retry()
    {
        if (Application.internetReachability != NetworkReachability.NotReachable) {


            rayBlocker.SetActive(false);
            checkinternet.SetActive(false);
        }
    }

}
