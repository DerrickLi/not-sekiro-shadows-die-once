using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    #region movement_variables
    public float moveSpeed;
    public float dashSpeed;
    float x_input;
    float moveModifier;
    float y_input;
    Vector2 currDirection = Vector2.right;

    public GameObject[] checkpoints;
    #endregion

    #region jump_variables
    //public float gravity;
    public float jumpHeight;
    public bool feetContact;
    //[SerializeField] private Collider2D m_FeetCollider;
    public float jumpAnimationTiming;
    Vector3 m_Velocity = Vector3.zero;
    [Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;
    #endregion

    #region dash_variables
    public float dashAnimationTiming;
    public float dashCooldown = 0.5f;
    private float dashTimer;
    private bool canDash = true;
    #endregion

    #region crouch_variables
    private bool isCrouch = false;
    [Range(0, 1)] [SerializeField] private float m_CrouchSpeed = .36f;			// Amount of maxSpeed applied to crouching movement. 1 = 100%
    [SerializeField] private LayerMask m_WhatIsGround;							// A mask determining what is ground to the character
    [SerializeField] private Transform m_CeilingCheck;							// A position marking where to check for ceilings
    [SerializeField] private Collider2D m_CrouchDisableCollider;				// A collider that will be disabled when crouching
    const float k_CeilingRadius = .2f;                                          // Radius of the overlap circle to determine if the player can stand up
    #endregion

    #region attack_variables
    public float damage;
    public float attackSpeed;
    float attackTimer;
    public float hitboxTiming;
    public float attackAnimationTiming;
    bool isAttacking;
    public float attackCost;
    public float shootCost;
    public GameObject projectile;
    #endregion

    #region health_variables
    public float maxHealth;
    public Slider hpSlider;
    float currHealth;
    #endregion

    #region stamina_variables
    public float maxStamina;
    public Slider staminaSlider;
    public float regenRate;           //Amount regen per second;
    float currStamina;
    #endregion

    #region physics_components
    Rigidbody2D playerRB;
    #endregion

    #region animation_components
    Animator anim;
    #endregion

    #region time_variables
    public TimeManager timeManager;
    #endregion

    #region block_variables
    private bool isDeflecting = false;
    private bool isBlocking = false;
    #endregion

    [SerializeField] bool[] keys;


    //Calling once on creations
    void Awake()
    {
        playerRB = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        moveModifier = 1;

        attackTimer = 0;

        currHealth = maxHealth;
        hpSlider.value = currHealth / maxHealth;

        currStamina = maxStamina;
        staminaSlider.value = currStamina / maxStamina;

        dashTimer = dashCooldown;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isBlocking && !isDeflecting)
        {
            currStamina += regenRate * Time.deltaTime;
            currStamina = Mathf.Min(currStamina, maxStamina);
            staminaSlider.value = currStamina / maxStamina;
        }
        

        if (!canDash)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer < 0)
            {
                canDash = true;
                dashTimer = dashCooldown;
            }
        }

        if (isAttacking)
            return;

        //access our input values
        x_input = Input.GetAxisRaw("Horizontal") * moveModifier;
        y_input = Input.GetAxisRaw("Vertical");
        Move();

        // set the player's direction for dashing
        if (x_input > 0)
            currDirection = Vector2.right;
        else if (x_input < 0)
            currDirection = Vector2.left;

        if ((Input.GetKeyDown(KeyCode.JoystickButton5) || Input.GetKeyDown(KeyCode.K)) && attackTimer <= 0)
            Attack();
        else
            attackTimer -= Time.deltaTime;

        if ((Input.GetKeyDown(KeyCode.JoystickButton1) || Input.GetKeyDown(KeyCode.Space)) && canJump() && !(y_input < 0))
            Jump();
        if (Input.GetKeyDown(KeyCode.JoystickButton2) || Input.GetKeyDown(KeyCode.J))
            Dash();

        if (Input.GetKeyDown(KeyCode.JoystickButton6) || Input.GetKeyDown(KeyCode.L))
            Shoot();

        if (y_input < 0)
            Crouch();
        else
            isCrouch = false;

        //if (Input.GetKeyDown(KeyCode.Joystick1Button6) || Input.GetKeyDown(KeyCode.LeftControl))
        //{
        //    timeManager.slowTime();
        //}

        if (Input.GetKeyDown(KeyCode.Joystick1Button4) || Input.GetKeyDown(KeyCode.F))
        {
            Deflect();
        } else if (Input.GetKeyUp(KeyCode.JoystickButton4) || Input.GetKeyUp(KeyCode.F))
        {
            Invoke("Unblock", 0.2F);
        }

    }

    void FixedUpdate()
    {
        if (playerRB.velocity.y > 0 && !Input.GetButton("Jump"))
            playerRB.velocity += Vector2.up * Physics2D.gravity.y * (2f - 1) * Time.fixedDeltaTime * Time.timeScale;

        playerRB.velocity += Vector2.up * Physics2D.gravity.y * Time.fixedDeltaTime;
    }

    #region move_functions
    private void Move()
    {
        anim.SetBool("Moving", true);
        Vector3 targetVelocity = new Vector2(x_input * moveSpeed, playerRB.velocity.y);
        // Smoothing it out and applying it to the character
        playerRB.velocity = Vector3.SmoothDamp(playerRB.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);
        if (x_input == 0)
            anim.SetBool("Moving", false);
        if (x_input < 0 && currDirection == Vector2.right)
            Flip();
        if (x_input > 0 && currDirection == Vector2.left)
            Flip();

        // If crouching, check to see if the character can stand up
        if (!isCrouch)
        {
            // If the character has a ceiling preventing them from standing up, keep them crouching, otherwise...
            if (!Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
            {
                // Enable the collider when not crouching
                if (m_CrouchDisableCollider != null)
                    m_CrouchDisableCollider.enabled = true;
                moveModifier = 1;
                anim.SetBool("Crouch", false);
            }
        }

        anim.SetFloat("DirX", currDirection.x);
    }
    //Flip both player animation and colliders
    private void Flip()
    {
        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
    #endregion

    #region jump_functions
    private void Jump()
    {
        playerRB.velocity = new Vector2(playerRB.velocity.x, jumpHeight);
        StartCoroutine(JumpRoutine());
    }
    bool canJump()
    {
        if (feetContact)
            return true;
        Debug.Log("cant jump");
        return false;
    }
    IEnumerator JumpRoutine()
    {
        anim.SetTrigger("Jump");
        yield return new WaitForSeconds(jumpAnimationTiming);
    }
    #endregion

    #region dash_functions
    private void Dash()
    {
        if (canDash)
        {
            playerRB.velocity = new Vector2(currDirection.x * dashSpeed, playerRB.velocity.y);
            canDash = false;
        }
    }
    IEnumerator DashRoutine()
    {
        anim.SetTrigger("Dash");
        yield return new WaitForSeconds(dashAnimationTiming);
    }
    #endregion

    #region crouch_functions
    private void Crouch()
    {
        anim.SetBool("Crouch", true);
        isCrouch = true;

        // Reduce the speed by the crouchSpeed multiplier
        moveModifier = m_CrouchSpeed;

        // Disable one of the colliders when crouching
        if (m_CrouchDisableCollider != null)
            m_CrouchDisableCollider.enabled = false;
    }

    #endregion

    #region attack_functions
    private void Attack()
    {
        if (currStamina < attackCost)
            return;

        UseMana(attackCost);

        //Handles all attack animations and calculates hitboxes
        StartCoroutine(AttackRoutine());

        attackTimer = attackSpeed;
    }

    private void Shoot()
    {
        if (currStamina < shootCost)
            return;
        UseMana(shootCost);

        //Handles all attack animations and calculates hitboxes
        StartCoroutine(ShootRoutine());

        attackTimer = attackSpeed;
    }

    IEnumerator ShootRoutine()
    {
        FindObjectOfType<AudioManager>().Play("PlayerRange");

        yield return new WaitForSeconds(attackAnimationTiming);
        GameObject fired = Instantiate(projectile, transform.position, transform.rotation);
        fired.GetComponent<Bullet>().dir = currDirection;
    }

    IEnumerator AttackRoutine()
    {
        FindObjectOfType<AudioManager>().Play("PlayerMelee");

        isAttacking = true;
        //Start animation
        anim.SetTrigger("Attack");
        //Brief pause before we calculate the hitbox
        yield return new WaitForSeconds(hitboxTiming);
        //Create hitbox
        RaycastHit2D[] hits = Physics2D.BoxCastAll(playerRB.position + currDirection, Vector2.one, 0f, Vector2.zero, 0);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.transform.CompareTag("Enemy"))
            {
                hit.transform.GetComponent<EnemyAI>().TakeDamage(damage);
            } else if (hit.transform.CompareTag("Boss"))
            {
                hit.transform.GetComponent<FinalBoss>().TakeDamage(damage);
            }
        }
        
        yield return new WaitForSeconds(attackAnimationTiming);

        //Re enables movement for the player after attacking
        isAttacking = false;
    }

    #endregion

    #region block_functions
    private void Deflect()
    {
        isDeflecting = true;
        Debug.Log(isDeflecting);
        Invoke("Block", 1F);
    }

    private void Block()
    {
        if (isDeflecting)
        {
            isDeflecting = false;
            isBlocking = true;
            Debug.Log(isDeflecting);
        }
    }

    private void Unblock()
    {
        isBlocking = false;
        isDeflecting = false;
    }
    #endregion
    #region health_functions

    public bool TakeDamage(float value) // return true if some type of damage successful, false if deflected
    {
        if (isDeflecting)
        {
            // reverse the bullet 
            return false;
        } else if (isBlocking)
        {
            if (currStamina - value <= 0)
            {
                // break guard
                Unblock();
                TakeDamage(value - currStamina);
            }
            else
            {
                currStamina -= value;
                staminaSlider.value = currStamina / maxStamina;
            }
        } else
        {
            FindObjectOfType<AudioManager>().Play("PlayerDamaged");

            //Decrement health
            currHealth -= value;
            hpSlider.value = currHealth / maxHealth;
            //Check for death
            if (currHealth <= 0)
            {
                //Die
                Die();
            }
        }
        return true;
    }

    public void Heal(float value)
    {
        //Increment Health
        currHealth += value;
        currHealth = Mathf.Min(currHealth, maxHealth);
        hpSlider.value = currHealth / maxHealth;
    }

    private void Die()
    {
        StartCoroutine("DieDelay");
        //Destroy Gameobject
        // Destroy(this.gameObject);

        //Trigger anything we need to end the game, find game manager and lose game
        GameObject gm = GameObject.FindWithTag("GameController");
        gm.GetComponent<GameManager>().LoseGame();
    }

    public void Respawn()
    {
        playerRB.velocity = Vector2.zero;
        for (int i = checkpoints.Length - 1; i >= 0; i--)
        {
            if (checkpoints[i].GetComponent<Checkpoint>().isVisited())
            {
                playerRB.position = checkpoints[i].GetComponent<Transform>().position;
                return; // stop execution
            }
        }
        Die();
    }
    IEnumerator DieDelay()
    {
        yield return new WaitForSeconds(2f);
    }
    #endregion

    #region mana_functions
    public void UseMana(float value)
    {
        //Decrement mana
        currStamina -= value;
        staminaSlider.value = currStamina / maxStamina;
    }
    #endregion

    public void AddKey(int keyNum)
    {
        keys[keyNum] = true;
    } 

    public bool CheckKey(int keyNum)
    {
        return keys[keyNum];
    }
}
