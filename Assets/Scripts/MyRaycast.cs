using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyRaycast
{
    private RaycastHit2D myRH2D;
    private Vector3 origin;
    private Vector3 dir;
    private float distance;
    private int layerMask;

    public RaycastHit2D GetRaycastHit2D(){return this.myRH2D;}
    public MyRaycast(Vector3 origin, Vector3 dir, float distance, int layerMask)
    {
        myRH2D = Physics2D.Raycast(origin, dir, distance, layerMask);
        this.origin = origin;
        this.dir = dir;
        this.distance = distance;
        this.layerMask = layerMask;
    }

    public void DrawRaycast2D(Color lineColor)
    {
        Debug.DrawRay(this.origin,this.dir * this.distance, lineColor);
    }

    
}
