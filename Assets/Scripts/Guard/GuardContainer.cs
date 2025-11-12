using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardContainer : MonoBehaviour
{
    private List<GuardStateMachine> guards = new List<GuardStateMachine>();

    private void Awake()
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            guards.Add(transform.GetChild(i).GetComponent<GuardStateMachine>());
        }
    }

    public bool IsLastPursuer()
    {
        int i = 0;

        foreach(GuardStateMachine guard in guards)
        {
            if(guard.GetCurrentGuardState().Equals(GuardStateMachine.GuardState.Chasing))
            {
                i++;
            }
        }

        return i < 2;
    }
}
