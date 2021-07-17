using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sword : MonoBehaviour
{
    // Start is called before the first frame update
    private Collider2D coll;
    private GameObject Father;
    private Vector2 dir;
    private int lastAttackID = 0;

    private string attackState;

    private MySword ms;


    void Start()
    {
        coll = GetComponent<Collider2D>();
        Father = transform.parent.gameObject;
        ms = Father.GetComponent<Movement>().weapon;
        attackState = ms.attackState;
    }

    // Update is called once per frame
    void Update()
    {
        dir = new Vector2(Father.GetComponent<Movement>().side, 0);
        attackState = ms.attackState;
    }


    void checkHurt()
    {
     
    }

    void OnTriggerStay2D(Collider2D other) {
        //碰撞盒即使已经在区域内，由关闭到开启不会导致onTriggerEnter2D触发
        //所以使用Stay
        //或者用enter，关了以后就移开一点点，开的时候再移回来一点点，保证有移动，同时攻击范围不变？
        if (other.gameObject.tag == "Enemy")
        {
            // Debug.Log("砍到敌人了");
            other.gameObject.GetComponent<normalEnemy>().GetHurt(dir, attackState, 5);
        }        
    }

    

    //物体开启时的回调
    void OnEnable() {
        lastAttackID++;
        Debug.Log("刀光开启");
    }
}
