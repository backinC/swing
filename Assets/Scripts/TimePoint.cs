using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimePoint
{
    private float timeLength;

    private float timePoint;

    //构造函数
    //timeLength：持续时间
    public TimePoint(float timeLength)
    {
        this.timeLength = timeLength;
        this.timePoint = 0f;
    }

    //标记结束时间点
    public void MarkPoint()
    {
        timePoint = Time.time + timeLength;
    }

    //是否在时间点之前
    public bool BeforeTime()
    {
        if (Time.time < timePoint)  return true;
        else return false;
    }
}
