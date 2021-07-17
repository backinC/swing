using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class swordAnimation : MonoBehaviour
{
    private Animator anim;
    private Movement move;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        move = GetComponentInParent<Movement>();
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetBool("isUpChoping", move.SM.GetState("isUpChoping"));
        anim.SetBool("isDashChoping", move.SM.GetState("isDashChoping"));
    }
}
