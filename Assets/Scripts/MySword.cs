using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO:冲刺攻击的话，应该把攻击范围拉宽一点（拉到人的后面），不然可能会冲刺经过了，但是实际碰撞盒没碰到。
public class MySword
{
    private GameObject hitbox;

    private Rigidbody2D rb;

    private StateManager SM;

    private Hashtable TCs;

    public string attackState = "normalAttack";
    public MySword(GameObject hitbox, Rigidbody2D rb, StateManager SM)
    {
        this.hitbox = hitbox;
        this.rb = rb;
        this.SM = SM;
        TCs = new Hashtable();
        TimeCounter TC1 = new TimeCounter(0.25f);
        TimeCounter TC2 = new TimeCounter(0.4f);
        TCs.Add("isUpChoping", TC1);
        TCs.Add("isDashChoping", TC2);
    }
    public void MyUpdate()
    {
        if (Input.GetButtonDown("Attack"))
        {
            // Debug.Log("按了攻击");
            if (SM.GetState("isDashing"))
            {
                dashChop();
            }
            else
            {
                if (SM.GetState("onGround"))
                {
                    if (Input.GetAxisRaw("Vertical") == 1)
                    {
                        upChop();
                    }
                    else
                    {
                        normalAttack();
                    }
                }
                else if (SM.GetState("onAir"))
                {
                    airAttack();
                }
            }
            
        }
        //判定计时器当前状态
        //每个计时器减去每一帧的耗时
        foreach (DictionaryEntry temp in TCs)
        {
            TimeCounter TC = (TimeCounter)temp.Value;
            SM.SetState((string)temp.Key, TC.InTime());
            // SM[sn] = (TimeCounter)(temp.Value).IsIsTime();
            TC.OneFrame();
        }
    }

    void dashChop()
    {
        Debug.Log("冲刺攻击");
        TimeCounter TC = (TimeCounter)TCs["isDashChoping"];
        TC.StartCount();
        hitbox.SetActive(true);
        attackState = "dashChop";
    }

    void normalAttack()
    {
        Debug.Log("普通攻击");
        hitbox.SetActive(true);
        attackState = "normalAttack";
    }

    void airAttack()
    {
        Debug.Log("空中攻击");
        hitbox.SetActive(true);
        attackState = "airAttack";
    }

    void upChop()
    {
        Debug.Log("上挑");
        TimeCounter TC = (TimeCounter)TCs["isUpChoping"];
        TC.StartCount();
        // (TimeCounter)(TCs["isUpChoping"]).StartCount();
        // StartCoroutine(upChopWait(0.25f));
        rb.velocity = new Vector2(0, 20f);
        hitbox.SetActive(true);
        attackState = "upChop";
    }

    // IEnumerator upChopWait(float time)
    // {
    //     SM.SetTrue("isUpChoping");
    //     yield return new WaitForSeconds(time);
    //     SM.SetFalse("isUpChoping");
    // }

}
