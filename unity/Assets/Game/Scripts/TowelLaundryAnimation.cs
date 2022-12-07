using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowelLaundryAnimation : MonoBehaviour
{
    //kid animation
    public GameObject kid;
    private Animator anim;

    public GameObject[] towel; //different towel states

    private float animationTime;
    private float waitTime; //waiting time after the animation ends
    private float timer;

    private Vector3 t1_init_pos; //starting position for towel 1

    void Start()
    {
        anim = kid.GetComponent<Animator>();

        animationTime = 3.667f;
        waitTime = 4.0f;
        timer = 0.0f;

        t1_init_pos = towel[1].transform.position;

        restartAnimation();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer > animationTime + waitTime)
        {
            restartAnimation();
        }


        if (timer>1.95  && towel[0].activeInHierarchy == true)
        {
            changeActivation(false, true, false);
        }


        if (towel[1].activeInHierarchy == true)
        {
            //move towel 1 up
            towel[1].transform.position = towel[1].transform.position + Vector3.up * Time.deltaTime*0.8f;
        }


        if (timer > 2.85  && towel[1].activeInHierarchy == true)
        {
            changeActivation(false, false, true);
        }
    }

    void restartAnimation()
    {
        timer = 0.0f;
        changeActivation(true, false, false);
        towel[1].transform.position = t1_init_pos;
      
        anim.Rebind(); //restart kid animation
    }

    //enable - disable towel states
    void changeActivation(bool a, bool b, bool c)
    {
        towel[0].SetActive(a);
        towel[1].SetActive(b);
        towel[2].SetActive(c);
    }

}
