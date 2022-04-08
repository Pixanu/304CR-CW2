using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float runSpeed = 5;
    public float walkSpeed = 1.8f;
    public float rotationSpeed = 180;

    public bool IsStealth = false;
    public static bool IsHidden = false;
    public bool IsMoving = false;
    public bool IsDead = false;

    //Local 
    private bool isHidable = false;
   

    Animator animator;
    Rigidbody rb;
    CapsuleCollider myCollider;
    //mesh of the player
    GameObject graphics;

    // Start is called before the first frame update
    void Start()
    {
        graphics = transform.GetChild(0).gameObject;
        animator = transform.GetComponentInChildren<Animator>();
        rb= GetComponent<Rigidbody>();
        myCollider = GetComponent<CapsuleCollider>();
    }

    void Update()
    {
        if (IsDead)
            return;
        CheckHideSpot();

        if (!IsHidden)
        {
            CheckStealth();
            Move();
            Rotation();
        }
    }

    #region Methods

    public void KillPlayer()
    {
        IsDead = true;
        animator.SetBool("IsDeath",true);
    }
    void CheckHideSpot()
    {
        if(Input.GetKey(KeyCode.E) && isHidable)
        {
            IsHidden = !IsHidden;
            rb.useGravity = !IsHidden;
            myCollider.enabled = !IsHidden;
            graphics.SetActive(!IsHidden);

        }
    }


    void Move()
    {
        float verticalMove = Input.GetAxis("Vertical");

        if (IsStealth)
            transform.Translate(Vector3.forward * verticalMove * walkSpeed * Time.deltaTime, Space.Self);
        else
            transform.Translate(Vector3.forward * verticalMove * runSpeed * Time.deltaTime, Space.Self);

        animator.SetFloat("MoveSpeed",verticalMove);
        IsMoving = verticalMove !=0;
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
    #endregion

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Hidable")
            isHidable = true;
        
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag == "Hidable")
            isHidable = false;
    }
}
