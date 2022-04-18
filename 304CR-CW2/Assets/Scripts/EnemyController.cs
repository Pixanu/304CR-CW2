using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


//States
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
    AudioSource gun;
    Vector3 soundSource;
    Ray ray;
    RaycastHit hit;
    
    void Start()
    {
        gun = GetComponent<AudioSource>();

        //Evaluating the walk,run and chase aniamtion curves at the beginning of the curve.
        walkTime = 0f;
        walkSpeed = walkCurve.Evaluate(walkTime);

        runTime = 0f;
        runSpeed = runCurve.Evaluate(runTime);

        chaseTime = 0f;
        stopChase = chaseStopCurve.Evaluate(chaseTime);
        
        //Get necessary components
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
                //ACTIONS
                ChangeState(AIState.Patrol);
                //DECISIONS
                break;

            case AIState.Patrol:
                //ACTIONS
                MoveToCheckpoint();
                //DECISIONS
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
                //ACTIONS
                MoveToSound();
                //DECISIONS
                if (Destination())
                    ChangeState(AIState.Wait);
                if(PlayerIsHeard())
                    ChangeState(AIState.Alert);
                if (PlayerInSight())
                    ChangeState(AIState.Chase);
                break;

            case AIState.Chase:
                //ACTIONS
                PlayerTooFar();
                MoveToPlayer();
                //DECISIONS
                if (!PlayerAlive())
                    ChangeState(AIState.Wait);

                break;
            case AIState.Shoot:
                player.KillPlayer();
                ChangeState(AIState.Wait);
                break;

            case AIState.GoingBackToPatrol:
                //ACTIONS
                ChangeState(AIState.Patrol);
                //DECISIONS
                break;
        }
    }

    //Change State function
    void ChangeState(AIState newState)
    {
        actualState = newState;
        stateTimer = 0;

        //Call the update animation function
        UpdateAnimation();
    }

    void UpdateAnimation()
    {
        //re-evaluation of the curves based on the time that it passed.
        walkTime += Time.deltaTime;
        walkSpeed = walkCurve.Evaluate(walkTime);

        runTime += Time.deltaTime;
        runSpeed = runCurve.Evaluate(runTime);

        chaseTime += Time.deltaTime;
        stopChase = chaseStopCurve.Evaluate(chaseTime);

        //Based on the state taht the AI is in specific animation bool variables are true or false
        //Those are declared within the AI/ Enemy Animation Controller
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
                gun.Play();
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
    //Move towards the player
    void MoveToPlayer()
    {
        agent.destination = player.transform.position;
        agent.isStopped = false;
    }

    //Moves towards the Sound Source
    void MoveToSound()
    {
        agent.destination = soundSource;
        agent.isStopped = false;
    }

    //Move to one of the checkpoints
    void MoveToCheckpoint()
    {
        agent.destination = checkPoints[NrCheckpoint].transform.position;
        agent.isStopped = false;
    }

    //If one checkpoint is reached moved to the next one
    void NextCheckpoint()
    {
        NrCheckpoint++;
        if (NrCheckpoint >= checkPoints.Length)
            NrCheckpoint = 0;
    }
    #endregion


    #region Decisions

    //Check if player is alive
    bool PlayerAlive()
    {
        return !player.IsDead;
    }
    
    //Destination Check
    bool Destination()
    {
        return agent.remainingDistance < agent.stoppingDistance && !agent.pathPending;
    }

    //Fuzzy State MAchine or Fuzzy Logic
    void PlayerTooFar()
    {
        //Function only called in the Chase State
        //If the distance is bigger than the end of the animation curve stop chassing
        
        if (agent.remainingDistance > stopChase)
        {
            ChangeState(AIState.GoingBackToPatrol);
            Debug.Log("The Player escaped, going back to Patrol");
        }
        //If is less for example additional check to see if player is in sight
        //Change the state to Shoot
        else if (agent.remainingDistance < stopChase/2)
        {
            if (PlayerInSight() && WaitTime(0.5f))
                ChangeState(AIState.Shoot);
            Debug.Log("The Player is in sight and Shoot State Active");
        }
        else
        //If player not in sight durring chasing listen for sounds move to an Alert state
        {
            if(!PlayerInSight())
                ChangeState(AIState.Alert);
            Debug.Log("Player not in sight, AI is listening to sounds");
        }

    }
    //Wait/Delay before performing an action
    bool WaitTime(float timeToWait)
    {
        return stateTimer > timeToWait;
    }

    //Gizmos for hearing and sight
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, hearRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }

    //Hearing Sense
    bool PlayerIsHeard()
    {
        if(player.IsDead)
            return false;

        //If the player is not in Stealth Mode and is moving and the distance from the player to the Ai is less than its hearing range 
        //set the sound source ("footsteps") to the player position
        float distance = Vector3.Distance(transform.position, player.transform.position);
        bool result = !player.IsStealth && player.IsMoving && distance < hearRange;

        if (result)
            soundSource = player.transform.position;
        return result;

    }

    //Sight Sense
    bool PlayerInSight()
    {
        if (player.IsDead)
            return false;
        //In range
        float distanceToPlayer = Vector3.Distance(transform.position,player.transform.position);   
        if(distanceToPlayer < sightRange)
        {
            //In angle
            Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            if(angleToPlayer < sightAngle / 2)
            {

                Debug.DrawLine(transform.position, player.transform.position);
                //In line
                Vector3 startPos;

                //Based on the start postion of the raycast , if the player is in stealth Mode multiply by 0.7 if not by 1.4
                //If the player is hiding behind a low wall or not

                if (player.IsStealth)
                    startPos = transform.position + Vector3.up * 0.7f;
                else
                    startPos = transform.position + Vector3.up * 1.4f;

                ray = new Ray(startPos, directionToPlayer);
                if(Physics.Raycast(ray,out hit))
                {
                    //Is player
                    //Raycast hit to find the player tag
                    if(hit.transform.gameObject.tag == "Player")
                    {
                        return true;
                        //If all the checks return true, it means that the AI succesfully found the player 
                    }
                }
            }
        }

        return false;

    }
    #endregion
}
