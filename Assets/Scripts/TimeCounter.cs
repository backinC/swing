using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//计时器
//时间值，计数器
//初始化时间值，计数器为0
//OneFram()表示一帧，计数器减去的时间
//StartCount() 开始计时，timeCounter=timeLength
//InTime() 返回是否在时间内

//TODO:为什么不是length + time 时间值 + 时间点
//这样就不用每一帧去更新counter，只有判定的时候取Time.time对比
//计数器的话，重置只要赋值，更新需要加减，判定和0取大小
//时间点的话，重置要加减，更新不用操作，判定取time,比大小
public class TimeCounter
{
    private float timeLength;
    private float timeCounter;

    public TimeCounter(float timeLength)
    {
        this.timeLength = timeLength;
        timeCounter = 0;
    }

    public void OneFrame(float time)
    {
        timeCounter -= time;
    }

    public void OneFrame()
    {
        timeCounter -= Time.deltaTime;
    }

    public void StartCount()
    {
        timeCounter = timeLength;
    }

    public bool InTime()
    {
        if (timeCounter > 0) return true;
        else return false;
    }
}
