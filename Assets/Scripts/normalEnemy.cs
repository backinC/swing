using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class normalEnemy : MonoBehaviour
{
    //TODO:状态转换，巡逻状态受到攻击变成受击状态
    // public float jituiSpeed = 5f;   //击退速度
    // public float jituiDistance = 3f;    //击退距离，直接改postition的
    private SpriteRenderer sp;
    private Vector2 spsize;

    public float hurtLength = 0.1f;
    private float hurtCounter;
    public LayerMask groundLayer;
    private int myside = 1;

    public float jituiForce = 5f;

    public float moveDistance = 5f;
    private Rigidbody2D rb;

    private int health;

    //后期想法，受击传ID，建立哈希表，key是ID，value是对该攻击的无敌时间点
    //每次受击，根据ID查找是否存在，
    //存在则查看时间与统一时间对比，时间内则不做反应，时间外则受击，
    //不存在则受击
    //TODO：那时间外，但是没有收到攻击，什么时候删除呢，不然就会有一堆无用的影响搜索速率？
    //会吗？一般来说哈希表搜索是O(1)，所以看冲突几率，所以可以从ID入手，即使有一堆无用的也不影响

    //以上做法是为了防止多次受伤的情况出现，同时保证一进入范围，就受到伤害。

    // private int lastHurtID = -1;
    private Hashtable hurtMap;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sp = GetComponent<SpriteRenderer>();
        spsize = sp.bounds.size;
        hurtMap = new Hashtable();
        health = 10;
        Physics2D.queriesStartInColliders = false;  //避免射线从内部发射时检测到自己
    }

        void Update()
    {
        if (myside > 0){transform.eulerAngles = new Vector3(0,0,0);}
        else if (myside < 0) {transform.eulerAngles = new Vector3(0,180,0);}
        if (hurtCounter < 0)
        {
            sp.material.SetFloat("_FlashAmount", 0);
            patrol();
        }
        else 
        {
            hurtCounter -= Time.deltaTime;
        }
        Death();
    }

    //dir：受击方向
    //attackState：攻击ID
    public void GetHurt(Vector2 dir, string attackState, int demage)
    {
        //还没受过这个伤害
        if (!hurtMap.ContainsKey(attackState))
        {
            turnRed();
            rb.AddForce(dir*jituiForce, ForceMode2D.Impulse);
            this.health -= demage;
            Debug.Log("受击，attackID:" + attackState);
            hurtMap.Add(attackState, Time.time + 0.5f);
        }
        else    //受过这个伤害，判断是否处于无敌时间内？不作为:受击并重置无敌时间
        {
            if (Time.time > (float)hurtMap[attackState])
            {
                turnRed();
                rb.AddForce(dir*jituiForce, ForceMode2D.Impulse);
                this.health -= demage;
                Debug.Log("受击，attackID:" + attackState);
                hurtMap[attackState] = Time.time + 0.5f;
            }
        }
        Death();
    }

    private void Death()
    {
        if (health > 0) return;
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void turnRed()
    {
        Debug.Log("一个敌人受伤");
        sp.material.SetFloat("_FlashAmount", 1);
        hurtCounter = hurtLength;
    }
    //简单的巡逻AI
    //自动左右走，如果碰到地板边缘，或者墙壁就会自动掉头
    //所以需要检测地板边缘和墙壁的函数（Raycast）
    private void patrol()
    {
        int pside = checkEndge();
        if (pside != 0)
        {
            myside = -myside;
        }
        transform.position =  new Vector3(transform.position.x + myside * moveDistance * Time.deltaTime, transform.position.y, transform.position.z);
        // rb.velocity = new Vecotr2(5f * myside, 0);
    }


    private int checkEndge()
    {
        float offset = 0.25f;
        Vector3 transCenter = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        
        transCenter.x += spsize.x/2 + offset;
        MyRaycast mrc1 = new MyRaycast(transCenter, -transform.up, spsize.y/2+0.5f, groundLayer);

        transCenter.x -= spsize.x + offset*2;
        MyRaycast mrc2 = new MyRaycast(transCenter, -transform.up, spsize.y/2+0.5f, groundLayer);

        transCenter.x = transform.position.x;
        MyRaycast mrc3 = new MyRaycast(transCenter, Vector3.right, spsize.x/2+0.2f, groundLayer);
        MyRaycast mrc4 = new MyRaycast(transCenter, Vector3.left, spsize.x/2+0.2f, groundLayer);

        RaycastHit2D rightFloorHit = mrc1.GetRaycastHit2D();
        RaycastHit2D leftFloorHit = mrc2.GetRaycastHit2D();
        RaycastHit2D rightWallHit = mrc3.GetRaycastHit2D();
        RaycastHit2D leftWallHit = mrc4.GetRaycastHit2D();

        // Dictionary<string, RaycastHit2D> Dic = new Dictionary<string,RaycastHit2D>();
        // Dic.Add( "rightFloorHit", rightFloorHit);
        // Dic.Add( "leftFloorHit", leftFloorHit);
        // Dic.Add( "rightWallHit", rightWallHit);
        // Dic.Add( "leftWallHit", leftWallHit);

        // foreach (var temp in Dic)
        // {
        //     if (temp.Value)
        //     {
        //         Debug.Log(temp.Key + "hit");
        //     }
        //     else 
        //     {
        //         Debug.Log(temp.Key + "do not hit");
        //     }
        // }

        mrc1.DrawRaycast2D(Color.red);
        mrc2.DrawRaycast2D(Color.blue);
        mrc3.DrawRaycast2D(Color.red);
        mrc4.DrawRaycast2D(Color.blue);
        
        
        if ((!rightFloorHit || rightWallHit) && myside == 1) //右侧探测到墙壁或者没有探测到地板
        {
            return -1;
        }
        else if ((!leftFloorHit || leftWallHit) && myside == -1)  //左侧探测到墙壁或者没有探测到地板
        {
            return 1;
        }
        else return 0;

        
    }

    // Start is called before the first frame update


    // Update is called once per frame



}
