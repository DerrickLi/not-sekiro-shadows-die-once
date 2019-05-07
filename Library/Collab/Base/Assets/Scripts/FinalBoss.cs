using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FinalBoss : MonoBehaviour
{
    #region movement_variables
    public float moveSpeed;
    public float dashSpeed;
    Vector2 currDirection = Vector2.left;
    public float jumpHeight;
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

    #region targeting_variables
    public Transform player;
    #endregion

    #region health_variables
    public float maxHealth;
    float currHealth;
    public Slider hpSlider;
    #endregion
    // Start is called before the first frame update
    #region physics_components
    Rigidbody2D bossRB;
    #endregion

    #region animation_components
    Animator anim;
    #endregion

    #region block_variables
    private bool isDeflecting = false;
    private bool isBlocking = false;
    #endregion

    void Awake()
    {
        bossRB = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        attackTimer = 0;

        currHealth = maxHealth;
        hpSlider.value = currHealth / maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null)
        {
            if (attackTimer > attackSpeed)
            {
                FacePlayer();

                int actions = Random.Range(0, 2);
                switch (actions)
                {
                    case 0:
                        Attack0();
                        break;
                    case 1:
                        Attack1();
                        break;
                    case 2:
                        Attack2();
                        break;
                }
                attackTimer = 0;

            }
            else
                attackTimer += Time.deltaTime;

            Debug.Log(attackTimer);
        }
    }

    // dash at the player then melee
    private void Attack0()
    {
        DashAtPlayer();
        Melee();
    }

    private void FacePlayer()
    {
        float res = transform.position.x - player.position.x;
        // Multiply the player's x local scale by -1.
        if (res < 0)
        {
            currDirection *= -1;
            Vector3 theScale = transform.localScale;
            theScale.x *= -1;
            transform.localScale = theScale;
        }
    }

    // dash backwards then shoot twice
    private void Attack1()
    {
        DashBackwards();
        Shoot();
        Shoot();
    }

    // jump up, dash at player, shoot then melee
    private void Attack2()
    {
        Jump();
        Shoot();
        DashAtPlayer();
        Melee();
    }

    private void Jump()
    {
        bossRB.velocity = new Vector2(bossRB.velocity.x, jumpHeight);
    }

    private void Dash()
    {
        bossRB.velocity = new Vector2(currDirection.x * dashSpeed, bossRB.velocity.y);
    }

    private void DashBackwards()
    {
        bossRB.velocity = new Vector2(currDirection.x * -1 * dashSpeed, bossRB.velocity.y);
    }

    private void Melee()
    {
        //Handles all attack animations and calculates hitboxes
        StartCoroutine(AttackRoutine());
        attackTimer = attackSpeed;
    }

    IEnumerator AttackRoutine()
    {
        //Start animation
        anim.SetTrigger("Attack");
        RaycastHit2D[] hits = Physics2D.BoxCastAll(bossRB.position + currDirection, Vector2.one, 0f, Vector2.zero, 0);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.transform.CompareTag("Player"))
                hit.transform.GetComponent<PlayerController>().TakeDamage(damage);
        }

        yield return new WaitForSeconds(attackAnimationTiming);
    }

    private void Shoot()
    {
        StartCoroutine(ShootRoutine());

        attackTimer = attackSpeed;
    }

    IEnumerator ShootRoutine()
    {
        yield return new WaitForSeconds(attackAnimationTiming);
        GameObject fired = Instantiate(projectile, transform.position, transform.rotation);
        fired.GetComponent<Projectile>().target = currDirection;
        fired.GetComponent<Projectile>().damage = damage;
    }

    private void DashAtPlayer()
    {
        Vector2 direction = player.position - transform.position;
        bossRB.velocity = direction.normalized * dashSpeed;
    }

    public bool TakeDamage(float value) // return true if some type of damage successful, false if deflected
    {
        if (isDeflecting)
        {
            // reverse the bullet 
            return false;
        }
        else
        {
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

    private void Die()
    {
        StartCoroutine("DieDelay");
        //Destroy Gameobject
        Destroy(this.gameObject);
    }

    IEnumerator DieDelay()
    {
        yield return new WaitForSeconds(2f);
    }
}
