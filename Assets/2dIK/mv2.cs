using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mv2 : MonoBehaviour
{
    private int moveModel = 0;

    private float transformTime = 0.3f;
    private float transformTImeCounter = 0;

    private Collider2D coll;
    public Rigidbody2D rb;
    // private SpriteRenderer SR;
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
    [SerializeField] bool onGround = false;
    bool onWallSlide = false;
    [SerializeField] bool isJumping = false;
    [SerializeField] bool higherJump = false;

    [SerializeField] bool onWall = false;
    bool onRightWall = false;
    bool onLeftWall = false;

    
    bool canWalk = true;
    [SerializeField] public bool isDashing = false;

    float originalGravity;

    // Start is called before the first frame update
    void Start()
    {
        coll = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        // SR = GetComponent<SpriteRenderer>();
        spSize = coll.bounds.size;
        Debug.Log(spSize);
        MyInpulse = GetComponent<Cinemachine.CinemachineImpulseSource>();

        originalGravity = rb.gravityScale;
        Physics2D.queriesStartInColliders = false;  //避免射线从内部发射时检测到自己
        
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
        if (Input.GetButtonDown("Attack"))
        {
            transform.GetChild(0).gameObject.SetActive(true);
        }
        if (xRaw > 0){side = 1;transform.eulerAngles = new Vector3(0,0,0);}
        else if (xRaw < 0) {side = -1; transform.eulerAngles = new Vector3(0,180,0);}
        if (Input.GetKey(KeyCode.DownArrow)) crouch();
        else if (Input.GetKeyUp(KeyCode.DownArrow)) stopCrouch();

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
                    isJumping = true;
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
                    isJumping = true;
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
        StartCoroutine(stopMove(transformTime));
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

    void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.tag=="HurtArea")
        {
            Debug.Log("enter HurtArea!");
            MyInpulse.GenerateImpulse(new Vector3(2,5,0));
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
        //画的射线只在Scene窗口看得见
        Debug.DrawRay(new Vector3(transform.position.x - spSize.x/4, transform.position.y, transform.position.z), -transform.up * (spSize.y/2 - 0.2f), Color.red);
        Debug.DrawRay(new Vector3(transform.position.x + spSize.x/4, transform.position.y, transform.position.z), -transform.up * (spSize.y/2 - 0.2f), Color.red);
        RaycastHit2D groundHit1 = Physics2D.Raycast(new Vector3(transform.position.x - spSize.x/4, transform.position.y, transform.position.z), -transform.up, spSize.y/2 + 0.1f, groundLayer); 
        RaycastHit2D groundHit2 = Physics2D.Raycast(new Vector3(transform.position.x + spSize.x/4, transform.position.y, transform.position.z), -transform.up, spSize.y/2 + 0.1f, groundLayer);
        if ( groundHit1.collider != null || groundHit2.collider != null)
        {
            onGround = true;    
            isJumping = false;
            onWall = false;
            onWallSlide = false;
            jumpTimeCounter = jumpTime;
            graceTimeCounter = graceTime;
            airJumpCounter=1;
        }
        else
        {
            onGround = false;
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
        if (dir.x !=0 || dir.y != 0)rb.velocity = new Vector2(dir.x * speed, rb.velocity.y);    //即有方向输入才会walk，避免不输入时将速度归零
        else {if(onGround)rb.velocity=new Vector2(0,0);}    //地面，无方向输入，立即停下
    }

    void AirWalk(Vector2 dir)
    {
        if (rb.velocity.x < 11)
        {
            if (dir.x !=0 || dir.y != 0)rb.velocity = new Vector2(dir.x * airWalkSpeed, rb.velocity.y);    //即有方向输入才会walk，避免不输入时将速度归零
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
        isDashing = false;
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
        dashJumpPressed = false;
    }

    IEnumerator DashWait()
    {
        isDashing = true;
        canWalk = false;
        rb.gravityScale = 0;
        yield return new WaitForSeconds(.1f);
        if (isDashing)
        {
        rb.gravityScale = originalGravity;
        isDashing = false;
        rb.velocity = new Vector2(side*speed, rb.velocity.y);
        transform.localScale = new Vector3(0.8f,0.8f,1f);
        canWalk = true;
        }
    }

    IEnumerator stopMove(float time)
    {
        canWalk = false;
        yield return new WaitForSeconds(time);
        canWalk = true;
    }

    IEnumerator stopDash(float time)
    {
        hasDash = true;
        yield return new WaitForSeconds(time);
        hasDash = false;
    }
}
