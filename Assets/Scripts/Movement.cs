using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//tilemap有缝隙，导致walljump有时会往下跳，使用整体的碰撞时则不会发生?????不是，整体的也会 当在跳跃后清除jumpbufferTimecounter计数时会发生
//上面是放屁，👴是傻狗

//只允许左右冲刺？八向冲刺？

//TODO:Dash
//TODO:jumpBufferTime需要改进
//靠墙长跳应当不受墙影响
//滑墙的判定还需要考虑，
//TODO:二段跳改为jump+airJump,不是jump+jump //done
//TODO:Dash时把碰撞盒缩小   //done 修改transform也会修改碰撞盒，所以直接改transform了，可能有问题
//TODO:Dash+Jump    会提前退出冲刺状态  //done
//上面都没怎么仔细测试，管他呢

//TODO：有时候wallJump会往下跳，不知道什么原因


//TODO：down+dash+jump  重要
//TODO：要让手感有apex的感觉，你懂吗？
//不懂，淦霖量,ALmovement

//TODO: 简单的攻击系统
//在做了

//TODO: 旋转跳跃
//？时间太久了，忘了自己写的啥，下次写清楚点


//TODO:上挑没做好，目前很拉跨，就是按键触发，给个向上的力，开个协程：一开始开bool过段时间关bool，打开伤害box；动画那边检测bool，播放动画，播放完关闭伤害box
//回复：目前，武器类写了，作为一个成员变量，会在初始化时赋值（不指明脚本时都是指本脚本）。每次update都会调用那边的MyUpdate（）
//其实就是把武器部分的逻辑写到了武器哪里，感觉不太行
//然后没用协程了，用了自己写的TimeCounter就是Length+Counter，集成了一下，方便一点。
//传到武器类那边的有StateManager，Rigidbody2D，武器的hitbox的游戏对象。
//感觉这个状态机需要做个单例模式？没有多线程吧？

//TODO：摄像机震动时会导致敌人闪烁，估计是z轴的震荡

//不同的招式，敌人不同的受击，上挑肯定要向上击飞敌人，
//那么原本的，武器调用敌人的受击函数，还需要传更多的参数
//动作优先级（霸体）

[RequireComponent(typeof(Rigidbody2D))]
public class Movement : MonoBehaviour
{
    public int health = 10;

    private TimePoint hurtTP;

    private int moveModel = 0;
    public StateManager SM;
    public MySword weapon;
    private float transformTime = 0.3f;
    private float transformTImeCounter = 0;

    private Collider2D coll;
    public Rigidbody2D rb;
    private SpriteRenderer SR;
    private Cinemachine.CinemachineImpulseSource MyInpulse;

    public LayerMask groundLayer;

    private Vector2 spSize;
    public int side = 1;   //根据输入决定的side**


    public int speed = 5;
    public int airWalkSpeed = 5;
    public int jumpForce = 7;
    public int dashSpeed = 10;
    public int dashJumpForce = 10;
    public int airJumpForce = 10;
    public float wallSlideSpeed = 5;
    public float wallJumpSpeed = 10;
    
    public float jumpTime = 0.3f;
    [SerializeField] private float jumpTimeCounter;

    public float graceTime = 0.1f;  //允许的土狼时间
    [SerializeField] private float graceTimeCounter; //土狼时间计时

    public float jumpBufferTime = 0.1f; //跳跃的输入缓冲时间
    [SerializeField] private float jumpBufferTimeCounter; //跳跃的输入缓冲计时
    
    [SerializeField] bool downPressed = false;

    [SerializeField] bool jumpPressed = false;
    [SerializeField] int airJumpCounter = 1;   //允许跳跃次数
    [SerializeField] bool airJumpPressed = false;   //空中跳跃
    [SerializeField] bool wallJumpPressed = false;
    [SerializeField] bool hasDash = false;
    [SerializeField] bool dashJumpPressed = false;
    bool dashPressed = false;


    //状态
    [SerializeField] bool onGround = false;
    [SerializeField] bool onAir = false;
    bool onWallSlide = false;
    [SerializeField] bool isJumping = false;
    [SerializeField] bool higherJump = false;

    public bool isUpChoping;
    [SerializeField] bool onWall = false;
    bool onRightWall = false;
    bool onLeftWall = false;

    bool canWalk = true;
    [SerializeField] public bool isDashing = false;

    float originalGravity;


    void Awake() 
    {
        
    }
    // Start is called before the first frame update
    void Start()
    {
        coll = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        SR = GetComponent<SpriteRenderer>();
        spSize = GetComponent<SpriteRenderer>().bounds.size;
        MyInpulse = GetComponent<Cinemachine.CinemachineImpulseSource>();

        originalGravity = rb.gravityScale;
        Physics2D.queriesStartInColliders = false;  //避免射线从内部发射时检测到自己
        string[] allState = {"onGround", "onAir", "isDashing", "isUpChoping", "isDashChoping"};
        SM = new StateManager();
        SM.Insert(allState);
        weapon = new MySword(transform.GetChild(0).gameObject, rb, SM);
        // TimeCounter hurtTC = new TimeCounter(1f);
        hurtTP = new TimePoint(1f);
        
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log(rb.velocity);
        /*
        GetButtonDown，按下返回true，并在下一帧重置，同时除非松开按键重按，否则无法触发下一次返回
        所以不能放fixupdate，可能按了但是又在下一帧被重置了导致等于没按
        得放在update避免被重置，并设置bool值传递给fixupdate
        */
        float xRaw = Input.GetAxisRaw("Horizontal");    
        float yRaw = Input.GetAxisRaw("Vertical");
        if (Input.GetButtonDown("MM1"))
        {
            changeMoveModel(0);
            Debug.Log("change move model to model_0");
        }
        else if (Input.GetButtonDown("MM2"))
        {
            changeMoveModel(1);
            Debug.Log("change move model to model_1");
        }
        if (yRaw == -1) crouch();
        else stopCrouch();
        // else if (Input.GetKeyUp(KeyCode.DownArrow)) stopCrouch();

        //旧攻击模块，迁移到武器了
        // if (Input.GetButtonDown("Attack"))
        // {
        //     if (onGround)
        //     {
        //         if (Input.GetKey(KeyCode.UpArrow))
        //         {
        //             //TODO:上挑
        //             upChop();
        //         }
        //         else
        //         {
        //             chop();
        //         }
        //     }
        //     else 
        //     {
        //         chop();
        //     }
            
        // }
        if (xRaw > 0){side = 1;transform.eulerAngles = new Vector3(0,0,0);}
        else if (xRaw < 0) {side = -1; transform.eulerAngles = new Vector3(0,180,0);}

        chekGround();
        if (!onGround)checkWall();

        if (Input.GetButtonDown("Jump")){jumpBufferTimeCounter = jumpBufferTime;}

        
        if (onWall)
        {
            // (Input.GetButtonDown("Jump") && xRaw>0 && onRightWall)   需要 xRaw>0吗？ 需要用输入缓冲吗？
            if ((jumpBufferTimeCounter > 0 && onRightWall) 
                || (jumpBufferTimeCounter > 0 && onLeftWall))
                {
                    jumpBufferTimeCounter=0;
                    wallJumpPressed = true;
                }    //并非滑墙时蹬墙跳
            if (rb.velocity.y < 0)onWallSlide = true;   //靠墙下落时才滑墙，避免跳到墙上马上就滑墙
            // if (onWallSlide && jumpBufferTimeCounter > 0)
            // {   
            //     wallJumpPressed = true;
            //     jumpBufferTimeCounter=0;
            // }  //
        }
        else{onWallSlide=false;}

        
        if(jumpBufferTimeCounter > 0)   //跳跃缓冲生效
        {
            if (isDashing)  //冲刺途中跳跃
            {
                if (graceTimeCounter>0) //触地，则dashJump
                {
                    // jumpPressed = true;  //TODO:
                    dashJumpPressed = true;
                    isJumping = true;SM.SetTrue("isJumping");
                    jumpBufferTimeCounter=0;
                    graceTimeCounter=0;              
                }
                else if (airJumpCounter>0)   //不触地，且airJumpCounter>0
                {                           //冲刺中airJump和普通的一样
                    airJumpCounter--;
                    airJumpPressed = true;
                }
            }
            else
            {
                if (graceTimeCounter>0) //触地，则Jump
                {
                    jumpPressed = true;
                    isJumping = true;SM.SetTrue("isJumping");
                    higherJump = true;
                    jumpBufferTimeCounter=0;
                    graceTimeCounter=0;
                }
                else if (airJumpCounter>0)   //不触地，且airJumpCounter>0
                {
                    airJumpCounter--;
                    airJumpPressed = true;
                }
            }
        }
        if (higherJump)
        {
            if (Input.GetButtonUp("Jump"))higherJump = false;   //只要松开按键立刻停止继续上升
            else    //没有松开按键
            {
                if (jumpTimeCounter > 0)   //如果jtc>0就长跳
                {
                    jumpPressed = true;
                    jumpTimeCounter -= Time.deltaTime;
                }
                else   //如果jtc<=0，即长跳结束
                {
                    higherJump = false;
                }
            }
        }
        

        if(Input.GetButtonDown("Dash") && !hasDash)
        {
            dashPressed = true;
            StartCoroutine(stopDash(1.0f));
        }
        jumpBufferTimeCounter -= Time.deltaTime;

        weapon.MyUpdate();
    }

    //固定间隔调用该函数，与Rigidbody的操作应该放在这里
    void FixedUpdate()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        float xRaw = Input.GetAxisRaw("Horizontal");    
        float yRaw = Input.GetAxisRaw("Vertical");
        
        Vector2 dir = new Vector2(xRaw, yRaw).normalized;
        // if (downPressed) crouch();
        // else stopCrouch();
        if (!isDashing && canWalk)
        {
            if (onGround)   Walk(dir);
            else AirWalk(dir);
        }
        if (dashPressed || isDashing) Dash();
        if (onWallSlide) wallSlide();
        if (wallJumpPressed) wallJump();
        if (dashJumpPressed) dashJump();
        else if (jumpPressed)Jump(dir);
        else if (airJumpPressed) airJump(dir);
        
        
    }

    void changeMoveModel(int MMIndex)
    {
        if (this.moveModel == MMIndex) return;
        this.moveModel = MMIndex;
        transformTImeCounter = transformTime;
        StartCoroutine(stopMove(transformTime));    //转变模式的时候不给动？
    }

    //TODO:蹲下的触发，维系和解除，我觉得这样写不行
    //TODO:v2
    void crouch()
    {
        transform.localScale = new Vector3(0.8f,0.8f/4*3,1f);
        downPressed = true;
    }

    void stopCrouch()
    {
        transform.localScale = new Vector3(0.8f,0.8f,1f);
        downPressed = false;
    }

    void OnTriggerStay2D(Collider2D other) {
        //不在无敌时间内的话
        if (!this.hurtTP.BeforeTime())
        {
            if(other.gameObject.tag=="HurtArea")
            {
                Debug.Log("enter HurtArea!");
                health--;
                hurtTP.MarkPoint();
                MyInpulse.GenerateImpulse(new Vector3(2,5,0));
            }  
        }

    }     

    void wallJump()
    {
        side = onRightWall? -1:1;
        Vector2 dir = new Vector2(side, 2).normalized;
        // Debug.Log(dir);
        rb.velocity = dir * wallJumpSpeed;
        wallJumpPressed = false;
        StartCoroutine(stopMove(0.1f));
    }

    // void OnCollisionEnter2D(Collision2D other) {
    //     Debug.Log(other.gameObject.name);    
    // }

    void wallSlide()
    {
        
        rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
    }

    void chekGround()
    {
        // RaycastHit2D groundHit1 = Physics2D.Raycast(new Vector3(transform.position.x - spSize.x/4, transform.position.y, transform.position.z), -transform.up, spSize.y/2 + 0.1f, groundLayer); 
        // RaycastHit2D groundHit2 = Physics2D.Raycast(new Vector3(transform.position.x + spSize.x/4, transform.position.y, transform.position.z), -transform.up, spSize.y/2 + 0.1f, groundLayer);
        MyRaycast rc1 = new MyRaycast(new Vector3(transform.position.x - spSize.x/4, transform.position.y, transform.position.z), -transform.up, spSize.y/2 + 0.1f, groundLayer);
        MyRaycast rc2 = new MyRaycast(new Vector3(transform.position.x + spSize.x/4, transform.position.y, transform.position.z), -transform.up, spSize.y/2 + 0.1f, groundLayer);
        RaycastHit2D groundHit1 = rc1.GetRaycastHit2D();
        RaycastHit2D groundHit2 = rc2.GetRaycastHit2D();
        // rc1.DrawRaycast2D(Color.red);
        // rc2.DrawRaycast2D(Color.red);
        //画的射线只在Scene窗口看得见
        if ( groundHit1.collider != null || groundHit2.collider != null)
        {
            onGround = true;SM.SetTrue("onGround");
            isJumping = false;  
            onAir = false;SM.SetFalse("onAir");
            onWall = false;
            onWallSlide = false;
            jumpTimeCounter = jumpTime;
            graceTimeCounter = graceTime;
            airJumpCounter=1;
        }
        else
        {
            onGround = false;SM.SetFalse("onGround");
            onAir = true;SM.SetTrue("onAir");
            graceTimeCounter -= Time.deltaTime;
        }
        
    }   

    void checkWall()
    {   
        //onwall需要方向键输入吗？
        if (onRightWall = Physics2D.Raycast(new Vector3(transform.position.x, transform.position.y + spSize.y/4, transform.position.z), Vector3.right, spSize.x/2 + 0.1f, groundLayer)
            ||  Physics2D.Raycast(new Vector3(transform.position.x, transform.position.y - spSize.y/4, transform.position.z), Vector3.right, spSize.x/2 + 0.1f, groundLayer)){}
        else
        {
            onLeftWall = Physics2D.Raycast(new Vector3(transform.position.x, transform.position.y + spSize.y/4, transform.position.z), Vector3.left, spSize.x/2 + 0.1f, groundLayer)
            ||  Physics2D.Raycast(new Vector3(transform.position.x, transform.position.y- spSize.y/4, transform.position.z), Vector3.left, spSize.x/2 + 0.1f, groundLayer);
        }
        // Debug.DrawRay(new Vector3(transform.position.x, transform.position.y + spSize.y/4, transform.position.z), Vector3.left*(spSize.x/2 + 0.1f), Color.red);
        // Debug.DrawRay(new Vector3(transform.position.x, transform.position.y- spSize.y/4, transform.position.z), Vector3.left*(spSize.x/2 + 0.1f), Color.red);
        // Debug.DrawRay(new Vector3(transform.position.x, transform.position.y + spSize.y/4, transform.position.z), Vector3.right*(spSize.x/2 + 0.1f), Color.red);
        // Debug.DrawRay(new Vector3(transform.position.x, transform.position.y- spSize.y/4, transform.position.z), Vector3.right*(spSize.x/2 + 0.1f), Color.red);
        if (onWall = onRightWall || onLeftWall)airJumpCounter=1;
        // {higherJump = false;
        // isJumping = false;}
    }

    void Walk(Vector2 dir)
    {
        // if (dir.x !=0 || dir.y != 0)rb.velocity = new Vector2(dir.x * speed, rb.velocity.y);    //即有方向输入才会walk，避免不输入时将速度归零
        if (dir.x !=0 || dir.y != 0)rb.velocity = new Vector2(dir.x * speed, rb.velocity.y);    //即有方向输入才会walk，避免不输入时将速度归零
        else {if(onGround)rb.velocity=new Vector2(0,0);}    //地面，无方向输入，立即停下
    }

    void AirWalk(Vector2 dir)
    {
        if (rb.velocity.x < 11)
        {
            if (dir.x !=0)rb.velocity = new Vector2(dir.x * airWalkSpeed, rb.velocity.y);    //即有方向输入才会walk，避免不输入时将速度归零
        }
    }


    /*
    跳跃
    跳跃条件判断
    跳跃动作
    跳跃条件重置
    */
    void Jump(Vector2 dir)
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        jumpPressed = false;
    }

    //目前的原理
    //dashPressed或者isDashing都会进入Dash
    //dashPressed进入时令dashPressed为false，然后开启0.1f的协程保证isDash=true，
    //两者都会设置高的速度值
    void Dash()
    {       
        // rb.velocity = dir * dashSpeed;
        if (dashPressed)
        {
            dashPressed = false;
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y/4*3,transform.localScale.z);
            StartCoroutine(DashWait());
        }
        rb.velocity = new Vector2(side*dashSpeed,0);  
    }

    void airJump(Vector2 dir)
    {
        rb.velocity = new Vector2(rb.velocity.x, airJumpForce);
        airJumpPressed=false;
    }

    void dashJump()
    {
        rb.gravityScale = originalGravity;
        isDashing = false;SM.SetFalse("isDashing");
        rb.velocity = new Vector2(side*speed, rb.velocity.y);
        transform.localScale = new Vector3(0.8f,0.8f,1f);
        if (downPressed)
        {
            rb.velocity = new Vector2(dashSpeed*side/5*4, dashJumpForce);
        }
        else
        {
            rb.velocity = new Vector2(dashSpeed*side/5*3, dashJumpForce);
        }
        StartCoroutine(stopMove(0.3f));   //目前的dashJump一小段时间不允许通过方向键对速度调节
                                        //TODO:最好是调节能力逐步回复到正常
        // StartCoroutine(airMoveLimit(0.3f));  
        //两个线性插值可能可以？
        //如果一冲就撞墙，这时候却还是冲刺时的受损空中移速
        //所以要做出现意外要直接将airWalkSpeed正常的判断。或者缩短受损到达正常的速度？
        //意外：受击？撞墙？
        //或者不要冲刺跳跃了
        dashJumpPressed = false;
    }

    void chop()
    {
        // transform.GetChild(0).gameObject.SetActive(true);
    }

    // void upChop()
    // {
    //     Debug.Log("上挑");
    //     transform.GetChild(0).gameObject.SetActive(true);
    //     StartCoroutine(upChopWait(0.25f));
    //     rb.velocity = new Vector2(0, 20f);
    // }


    IEnumerator DashWait()  //冲刺期间
    {
        isDashing = true;SM.SetTrue("isDashing");
        canWalk = false;
        rb.gravityScale = 0;
        yield return new WaitForSeconds(.1f);
        if (isDashing)
        {
        rb.gravityScale = originalGravity;
        isDashing = false;SM.SetFalse("isDashing");
        rb.velocity = new Vector2(side*speed, rb.velocity.y);
        transform.localScale = new Vector3(0.8f,0.8f,1f);
        canWalk = true;
        }
    }

    //  IEnumerator airMoveLimit(float time)    //受损空移恢复
    // {
    //     int temp = this.airWalkSpeed;
    //     this.airWalkSpeed = Mathf.Lerp(0, temp, Mathf.lerp(0, 1));
    //     yield return new WaitForSeconds(time);
    // }

    IEnumerator stopMove(float time)    //不准移动
    {
        canWalk = false;
        yield return new WaitForSeconds(time);
        canWalk = true;
    }

    IEnumerator stopDash(float time)    //不准冲刺
    {
        hasDash = true;
        yield return new WaitForSeconds(time);
        hasDash = false;
    }

    // IEnumerator upChopWait(float time)
    // {
    //     isUpChoping = true;
    //     yield return new WaitForSeconds(time);
    //     isUpChoping = false;
    // }

    void OnGUI()  
    {  
        GUI.TextArea(new Rect(0, 0, 250, 40), "Health " + health);//使用GUI在屏幕上面实时打印当前按下的按键  
    }  
}
