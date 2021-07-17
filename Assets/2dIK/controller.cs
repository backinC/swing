using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class controller : MonoBehaviour
{
    private Rigidbody2D rb;
    public float speed = 5f;
    private int side;
    public bool isRunning;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate() 
    {
        float xRaw = Input.GetAxisRaw("Horizontal");  
        if (isRunning)Run(new Vector2(xRaw, 0));
    }

    // Update is called once per frame
    void Update()
    {
        float xRaw = Input.GetAxisRaw("Horizontal");
        if (xRaw > 0){isRunning=true;side = 1;transform.eulerAngles = new Vector3(0,0,0);}
        else if (xRaw < 0) {isRunning=true;side = -1; transform.eulerAngles = new Vector3(0,180,0);}
        else {isRunning = false;}
    }

    void Run(Vector2 dir)
    {
        if (dir.x !=0 || dir.y != 0)rb.velocity = new Vector2(dir.x * speed, rb.velocity.y);    //即有方向输入才会walk，避免不输入时将速度归零
        else {rb.velocity=new Vector2(0,0);}    //地面，无方向输入，立即停下
    }
}
