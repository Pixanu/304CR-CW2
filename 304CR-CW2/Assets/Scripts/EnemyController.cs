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
    Shoot,
    GoingBackToPatrol
}


public class EnemyController : MonoBehaviour
{
    public AIState actualState = AIState.Idle;

    [Header("Movement")]
   
    public GameObject[] checkPoints;

    [Header("Walk Curve")]
    [SerializeField] private AnimationCurve walkCurve;
    private float walkSpeed;
    private float walkTime;

    [Header("Run Curve")]
    [SerializeField] private AnimationCurve runCurve;
    private float runSpeed;
    private float runTime;

    [Header("Chase Curve")]
    [SerializeField] private AnimationCurve chaseStopCurve;
    private float stopChase;
    private float chaseTime;

    public float waitTime = 5;

    [Header("Senses")]
    public float hearRange = 4;
    public float sightRange = 5;
    [Range(0, 360)]
    public float sightAngle = 120;

    //Local
    Animator anim;
    private float stateTimer = 0;
    private int NrCheckpoint = 0;
    private NavMeshAgent agent;
    PlayerController player;
    Vector3 soundSource;
    Ray ray;
    RaycastHit hit;
    
    void Start()
    {
        walkTime = 0f;
        walkSpeed = walkCurve.Evaluate(walkTime);

        runTime = 0f;
        runSpeed = runCurve.Evaluate(runTime);

        chaseTime = 0f;
        stopChase = chaseStopCurve.Evaluate(chaseTime);
        
        player = FindObjectOfType<PlayerController>();
        agent = GetComponent<NavMeshAgent>();
        anim= transform.GetComponentInChildren<Animator>();
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
                //Actions
                MoveToCheckpoint();

                //Decisions
                if (Destination())
                {
                    NextCheckpoint();
                    ChangeState(AIState.Wait);
                }
                if(PlayerIsHeard())
                    ChangeState(AIState.Alert);
                if(PlayerInSight())
                    ChangeState(AIState.Chase);
                break;
            case AIState.Wait:
                if(WaitTime(waitTime))
                    ChangeState(AIState.Patrol);
                if (PlayerIsHeard())
                    ChangeState(AIState.Alert);
                if (PlayerInSight())
                    ChangeState(AIState.Chase);
                break;
            case AIState.Alert:
                //Actions
                MoveToSound();

                //Decisions
                if (Destination())
                    ChangeState(AIState.Wait);
                if(PlayerIsHeard())
                    ChangeState(AIState.Alert);
                if (PlayerInSight())
                    ChangeState(AIState.Chase);
                break;
            case AIState.Chase:
                //Actions
                MoveToPlayer();

                //Decisiions
                if (PlayerInSight() && WaitTime(0.05f))
                    ChangeState(AIState.Shoot);
                if(!PlayerAlive())
                    ChangeState(AIState.Wait);
                if(PlayerTooFar())
                    ChangeState(AIState.GoingBackToPatrol);
                break;
            case AIState.Shoot:
                player.KillPlayer();
                ChangeState(AIState.Wait);
                break;
            case AIState.GoingBackToPatrol:
                //Action
                //Debug.Log("I've stopped chasing, going back to patrol");
                ChangeState(AIState.Patrol);
                //Decision
                break;

        }
    }

    void ChangeState(AIState newState)
    {
        actualState = newState;
        stateTimer = 0;

        UpdateAnimation();
    }

    void UpdateAnimation()
    {
        walkTime += Time.deltaTime;
        walkSpeed = walkCurve.Evaluate(walkTime);

        runTime +=Time.deltaTime;
        runSpeed = runCurve.Evaluate(runTime);

        chaseTime +=Time.deltaTime;
        stopChase = chaseStopCurve.Evaluate(chaseTime);



        switch (actualState)
        {
            case AIState.Idle:
                agent.speed = 0;
                agent.isStopped = true;
                anim.SetBool("IsMoving", false);
                anim.SetBool("IsAlert", false);
                anim.SetBool("IsShooting", false);
                break;
            case AIState.Patrol:
                agent.speed = walkSpeed;
                anim.SetBool("IsMoving", true);
                anim.SetBool("IsAlert", false);
                anim.SetBool("IsShooting", false);
                break;
            case AIState.Wait:
                agent.speed = 0;
                agent.isStopped = true;
                anim.SetBool("IsMoving", false);
                anim.SetBool("IsAlert", false);
                anim.SetBool("IsShooting", false);
                break;
            case AIState.Alert:
                agent.speed = walkSpeed;
                anim.SetBool("IsMoving", true);
                anim.SetBool("IsAlert", false);
                anim.SetBool("IsShooting", false);
                break;
            case AIState.Chase:
                agent.speed = runSpeed;
                anim.SetBool("IsMoving", true);
                anim.SetBool("IsAlert", true);
                anim.SetBool("IsShooting", false);
                break;
            case AIState.Shoot:
                agent.speed = 0;
                agent.isStopped = true;
                anim.SetBool("IsMoving", false);
                anim.SetBool("IsAlert", false);
                anim.SetBool("IsShooting", true);
                break;
            case AIState.GoingBackToPatrol:
                agent.speed = walkSpeed;
                anim.SetBool("IsMoving", true);
                anim.SetBool("IsAlert", false);
                anim.SetBool("IsShooting", false);
                break;
        }

    }


    #region Actions

    void MoveToPlayer()
    {
        agent.destination = player.transform.position;
        agent.isStopped = false;
    }

   

    void MoveToSound()
    {
        agent.destination = soundSource;
        agent.isStopped = false;
    }

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

    bool PlayerAlive()
    {
        return !player.IsDead;
    }
    bool Destination()
    {
        return agent.remainingDistance < agent.stoppingDistance && !agent.pathPending;
    }
    bool PlayerTooFar()
    { 
        return agent.remainingDistance > stopChase;
    }

    bool WaitTime(float timeToWait)
    {
        return stateTimer > timeToWait;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, hearRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }

    bool PlayerIsHeard()
    {
        if(player.IsDead)
            return false;

        float distance = Vector3.Distance(transform.position, player.transform.position);
        bool result = !player.IsStealth && player.IsMoving && distance < hearRange;

        if (result)
            soundSource = player.transform.position;
        return result;

    }

    bool PlayerInSight()
    {
        if (player.IsDead)
            return false;
        //in range
        float distanceToPlayer = Vector3.Distance(transform.position,player.transform.position);   
        if(distanceToPlayer < sightRange)
        {
            //in angle
            Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            if(angleToPlayer < sightAngle / 2)
            {
                Debug.DrawLine(transform.position, player.transform.position);
                //in line
                Vector3 startPos;
                if(player.IsStealth)
                    startPos = transform.position + Vector3.up * 0.7f;
                else
                    startPos = transform.position + Vector3.up * 1.4f;

                ray = new Ray(startPos, directionToPlayer);
                if(Physics.Raycast(ray,out hit))
                {
                    //is player
                    if(hit.transform.gameObject.tag == "Player")
                    {
                        return true;
                    }
                }
            }
        }

        return false;

    }
    #endregion
}
