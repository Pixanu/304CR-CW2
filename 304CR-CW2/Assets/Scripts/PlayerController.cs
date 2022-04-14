using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    //Health
    public int maxHealth = 100;
    public int currentHealth;
    public HealthBar healthBar; 

    //Movement
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

    //Mesh of the player
    GameObject graphics;

    // Start is called before the first frame update
    void Start()
    {
        //Set health
        healthBar.SetMaxHealth(maxHealth);
        currentHealth = maxHealth;

        //Get necessary components
        graphics = transform.GetChild(0).gameObject;
        animator = transform.GetComponentInChildren<Animator>();
        rb= GetComponent<Rigidbody>();
        myCollider = GetComponent<CapsuleCollider>();
    }

    //Update function for the Player Controller
    void Update()
    {
        if (IsDead)
            return;
        CheckHideSpot();

        //If not hidden check for the other functions
        if (!IsHidden)
        {
            CheckStealth();
            Move();
            Rotation();
        }

      
    }

    #region Methods

    //If the player dies form the damage taken play spefic animation and relod the scene afteer couple of seconds
    public void KillPlayer()
    {
        TakeDamage(20);
        if(currentHealth <= 0 )
        {
            IsDead = true;
            animator.SetBool("IsDeath", true);
            StartCoroutine(RelodScene());
        }
       
    }
    //When pressing E set the following values to inverted values
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

    //Take damage function 
    void TakeDamage(int damage)
    {
        currentHealth -= damage;

        healthBar.SetHealth(currentHealth);
    }

    //Movement on the Vertical Axis based on each state of the player (running/stealth))
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

    //Rotation on Horizontal axis for player to look around using "A" & "D" keys
    void Rotation()
    {
        float horizontalMove = Input.GetAxis("Horizontal");
        transform.Rotate(Vector3.up, horizontalMove * rotationSpeed * Time.deltaTime);
    }

    //Check if the player is in Stealth Mode
    void CheckStealth()
    {
        if(Input.GetKeyDown(KeyCode.C))
        {
            IsStealth = !IsStealth;
            animator.SetBool("IsStealth", IsStealth);

        }
    }
    #endregion


    //Check if other collider is equal to the tag and set isHidable to either true/false
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

    IEnumerator RelodScene()
    {
        yield return new WaitForSeconds(8f);
        SceneManager.LoadScene("Testing");
    }
}
