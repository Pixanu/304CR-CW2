using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum AIState
{
    Idle,
    Patrol,
    Wait,
    Alert,
    Chase,
    Shoot
}


public class EnemyController : MonoBehaviour
{
    public AIState actualState =AIState.Idle;
    public GameObject[] checkPoints;

    //Local
    private float stateTimer = 0;
    private int NrCheckpoint = 0;
    private NavMeshAgent agent;
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

   
    void Update()
    {
        stateTimer += Time.deltaTime; 

        switch (actualState)
        {
            case AIState.Idle:
                //Actions
                ChangeState(AIState.Patrol);
                //Decisions

                break;
            case AIState.Patrol:
                MoveToCheckpoint();

                break;
            case AIState.Wait:
                break;
            case AIState.Alert:
                break;
            case AIState.Chase:
                break;
            case AIState.Shoot:
                break;
        }
    }

    void ChangeState(AIState newState)
    {
        actualState = newState;
        stateTimer = 0;
    }


    #region Actions

    void MoveToCheckpoint()
    {
        agent.destination = checkPoints[NrCheckpoint].transform.position;
        agent.isStopped = false;
    }

    void NextCheckpoint()
    {
        NrCheckpoint++;
        if (NrCheckpoint >= checkPoints.Length)
            NrCheckpoint = 0;
    }

    #endregion

    #region Decisions

    #endregion
}
