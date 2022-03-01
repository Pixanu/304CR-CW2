using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float runSpeed = 5;
    public float walkSpeed = 1.8f;
    public float rotationSpeed = 180;

    public static bool IsStealth = false;

    //Local 
    Animator animator;



    // Start is called before the first frame update
    void Start()
    {
        animator = transform.GetComponentInChildren<Animator>();
    }

    void Update()
    {
        CheckStealth();
        Move();
        Rotation();
    }

    void Move()
    {
        float verticalMove = Input.GetAxis("Vertical");

        if (IsStealth)
            transform.Translate(Vector3.forward * verticalMove * walkSpeed * Time.deltaTime, Space.Self);
        else
            transform.Translate(Vector3.forward * verticalMove * runSpeed * Time.deltaTime, Space.Self);

        animator.SetFloat("MoveSpeed",verticalMove);
    }

    void Rotation()
    {
        float horizontalMove = Input.GetAxis("Horizontal");
        transform.Rotate(Vector3.up, horizontalMove * rotationSpeed * Time.deltaTime);
    }

    void CheckStealth()
    {
        if(Input.GetKeyDown(KeyCode.C))
        {
            IsStealth =!IsStealth;
            animator.SetBool("IsStealth", IsStealth);

        }
    }
}
