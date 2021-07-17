using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    private Animator anim;
    private controller move;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        move = GetComponentInParent<controller>();
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetBool("isRunning",move.isRunning);
    }
}
