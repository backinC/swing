using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager
{
    private Hashtable stateTable;

    public StateManager()
    {
        stateTable = new Hashtable();
    }
    public void Insert(string stateName, bool curState){stateTable.Add(stateName, curState);}
    public void Insert(string stateName){stateTable.Add(stateName, false);}
    public void Insert(string[] allState)
    {
        for (int i = 0; i < allState.Length; i++)
        {
            this.Insert(allState[i]);
        }
    }

    public void SetState(string stateName, bool curState)
    {
        if (!stateTable.ContainsKey(stateName))
        {
            Debug.Log("没有这个状态:" + stateName);
            return;
        }
        else
        {
            stateTable[stateName]=curState;
        }
    }

    public bool GetState(string stateName)
    {
        return (bool)stateTable[stateName];
    }

    public Hashtable GetAllState()
    {
        return this.stateTable;
    }


    public void SetTrue(string stateName)
    {
        stateTable[stateName] = true;
    }

    public void SetFalse(string stateName)
    {
        stateTable[stateName] = false;
    }

    public bool Contains(string stateName)
    {
        return stateTable.ContainsKey(stateName);
    }
}
