using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyAI : MonoBehaviour
{
    #region movement_variables
    public bool onlyMelee;
    public float setSpeed;
    private float moveSpeed;
    bool movingRight;
    public Transform groundDetection;
    public float groundRayDistance;
    #endregion

    #region patrol_variables
    public Transform[] patrolPoints;
    Transform currentPatrolPoint;
    int currentPatrolIndex;
    #endregion

    #region chase_variables
    public float chaseRange;
    public float chaseBonus;
    public Transform lineOfSightEnd;
    public float detectionAngle;
    public float searchingTime;
    float searchTimer = 0;
    bool seen;
    #endregion

    #region melee_variables
    public float meleeRange;
    public int meleeDamage;
    public float attackSpeed;
    float attackTimer = 0;
    #endregion

    #region ranged_variables
    public GameObject projectileType;
    public float shootRange;
    public float rangeDamage;
    public float timeToReload;
    public float fireSpeed;
    float reloadTimer = 0;
    public Transform lineOfSight;
    #endregion

    #region health_variables
    public float maxHealth;
    public Slider hpSlider;
    float currHealth;
    #endregion

    #region targeting_variables
    public Transform target;
    float distanceToTarget;
    Vector2 targetDir;
    #endregion

    #region physics_components
    Rigidbody2D enemyRB;
    #endregion

    #region animation_components
    Animator anim;
    #endregion

    #region drop_components
    public GameObject healthPack;
    public GameObject timePack;
    #endregion

    private void Awake()
    {
        enemyRB = GetComponent<Rigidbody2D>();

        currHealth = maxHealth;
        hpSlider.value = currHealth / maxHealth;

        currentPatrolIndex = 0;
        currentPatrolPoint = patrolPoints[currentPatrolIndex];
        moveSpeed = setSpeed;
        movingRight = false;

        distanceToTarget = 0;
    }

    // Update is called once per frame
    private void Update()
    {
        targetDir = target.transform.position - transform.position;

        distanceToTarget = Vector2.Distance(transform.position, target.transform.position);
        
        if (!CanPlayerBeSeen())
        {
            //if player got out of sight, keep looking at last seen direction for a couple seconds
            if (seen)
            {
                if (searchTimer == 0)
                    StartCoroutine(SearchRoutine());

                if (searchTimer > searchingTime)
                    searchTimer = 0;
                else
                {
                    searchTimer += Time.deltaTime;
                    return;
                }
            }

            Patrol();
            seen = false;

            //Find player if too close
            if (distanceToTarget <= meleeRange)
                CloseRotate();
        }
        else
        {
            seen = true;

            //Melee only enemy
            if (onlyMelee)
            {
                //Chase
                if (distanceToTarget > meleeRange - 1.2 && IsInRange())
                    Chase();
                //Melee
                if (distanceToTarget <= meleeRange)
                {
                    if (attackTimer > attackSpeed)
                    {
                        attackTimer = 0;
                        Melee();
                    }
                    else
                        attackTimer += Time.deltaTime;
                }
            }
            else
            {
                //Chase
                if (distanceToTarget > shootRange && IsInRange())
                    Chase();

                //Range
                if (distanceToTarget > meleeRange && distanceToTarget <= shootRange)
                {
                    if (reloadTimer > timeToReload)
                    {
                        reloadTimer = 0;
                        Range();
                    }
                    else
                        reloadTimer += Time.deltaTime;
                }

                //Melee
                if (distanceToTarget <= meleeRange)
                {
                    if (attackTimer > attackSpeed)
                    {
                        attackTimer = 0;
                        Melee();
                    }
                    else
                        attackTimer += Time.deltaTime;
                }
            }
            
        }

    }

    #region patrol_functions
    private void Patrol()
    {
        moveSpeed = setSpeed;

        // Check if reach patrol point
        if (Vector2.Distance(transform.position, currentPatrolPoint.position) < 1f)
        {
            //Check for more patrol points, if not go to beginning
            if (currentPatrolIndex + 1 < patrolPoints.Length)
                currentPatrolIndex++;
            else
                currentPatrolIndex = 0;
            currentPatrolPoint = patrolPoints[currentPatrolIndex];
        }

        // Turn to face the current patrol point
        Vector2 patrolPointDir = currentPatrolPoint.position - transform.position;
        
        // Figure out if patrol point is left or right of enemy
        if (patrolPointDir.x < 0f)
        {
            transform.Translate(-transform.right * Time.deltaTime * moveSpeed);
            movingRight = false;
            Flip();
        }
        if (patrolPointDir.x > 0f)
        {
            transform.Translate(transform.right * Time.deltaTime * moveSpeed);
            movingRight = true;
            Flip();
        }
    }

    private void Flip()
    {
        Quaternion newRotation = Quaternion.Euler(0f, 0f, 0f);
        if (movingRight)
            newRotation = Quaternion.Euler(0f, 180f, 0f);
        transform.rotation = newRotation;
    }
    #endregion

    #region chase_functions
    private void Chase()
    {
        moveSpeed = setSpeed + chaseBonus;
        
        //Detect ground
        RaycastHit2D groundInfo = Physics2D.Raycast(groundDetection.position, Vector2.down, groundRayDistance);
        if (groundInfo.collider == false)
            return;


        if (targetDir.x < 0f && !movingRight)
        {
            transform.Translate(-transform.right * Time.deltaTime * moveSpeed);
            movingRight = false;
        }
        if (targetDir.x > 0f && movingRight)
        {
            transform.Translate(transform.right * Time.deltaTime * moveSpeed);
            movingRight = true;
        }  
    }

    private void CloseRotate()
    {
        if (targetDir.x < 0f)
        {
            movingRight = false;
            Flip();
        }
        if (targetDir.x > 0f)
        {
            movingRight = true;
            Flip();
        }
    }

    IEnumerator SearchRoutine()
    {
        yield return new WaitForSeconds(searchingTime / 3);

        if (CanPlayerBeSeen())
            yield break;

        movingRight = !movingRight;
        Flip();
        yield return new WaitForSeconds(searchingTime / 3);

        if (CanPlayerBeSeen())
            yield break;

        movingRight = !movingRight;
        Flip();
    }
    #endregion

    #region detection_functions
    bool PlayerInFieldOfView()
    {
        // check if the player is within the enemy's field of view
        // this is only checked if the player is within the enemy's sight range

        // find the angle between the enemy's 'forward' direction and the player's location and return true if it's within 65 degrees (for 130 degree field of view)

        Vector2 directionToPlayer = target.position - lineOfSight.position; // represents the direction from the enemy to the player    
        Debug.DrawLine(lineOfSight.position, target.position, Color.magenta); // a line drawn in the Scene window equivalent to directionToPlayer

        Vector2 lineOfSightDir = lineOfSightEnd.position - lineOfSight.position; // the centre of the enemy's field of view, the direction of looking directly ahead
        Debug.DrawLine(lineOfSight.position, lineOfSightEnd.position, Color.yellow); // a line drawn in the Scene window equivalent to the enemy's field of view centre

        // calculate the angle formed between the player's position and the centre of the enemy's line of sight
        float angle = Vector2.Angle(directionToPlayer, lineOfSightDir);

        // if the player is within 65 degrees (either direction) of the enemy's centre of vision (i.e. within a 130 degree cone whose centre is directly ahead of the enemy) return true
        if (angle < detectionAngle)
            return true;
        else
            return false;
    }

    bool PlayerHiddenByObstacles()
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(lineOfSight.position, target.position - lineOfSight.position, distanceToTarget);
        Debug.DrawRay(lineOfSight.position, target.position - lineOfSight.position, Color.blue); // draw line in the Scene window to show where the raycast is looking
        List<float> distances = new List<float>();

        foreach (RaycastHit2D hit in hits)
        {
            // ignore the enemy's own colliders (and other enemies)
            if (hit.transform.tag == "Enemy")
                continue;

            // if anything other than the player is hit then it must be between the player and the enemy's eyes (since the player can only see as far as the player)
            if (hit.transform.tag != "Player")
            {
                return true;
            }
        }

        // if no objects were closer to the enemy than the player return false (player is not hidden by an object)
        return false;

    }

    bool CanPlayerBeSeen()
    {
        // we only need to check visibility if the player is within the enemy's visual range
        if (IsInRange())
        {
            if (PlayerInFieldOfView())
                return (!PlayerHiddenByObstacles());
            else
                return false;
        }
        else
        {
            // always false if the player is not within the enemy's range
            return false;
        }
        //return playerInRange;
    }

    bool IsInRange()
    {
        if (distanceToTarget <= chaseRange) 
            return true;
        return false;
    }

    #endregion

    #region attack_functions
    private void Melee()
    {
        //Create hitbox
        Vector2 currDirection = Vector2.left;
        if (movingRight)
        {
            currDirection = Vector2.right;
        }
        RaycastHit2D[] hits = Physics2D.BoxCastAll(GetComponent<Rigidbody2D>().position + currDirection, new Vector2(2, 1), 0f, Vector2.zero, 0);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.transform.CompareTag("Player"))
            {
                hit.transform.GetComponentInParent<PlayerController>().TakeDamage(meleeDamage);
                break;
            }
        }

    }

    /*
    IEnumerator MeleeRoutine()
    {
        //Start animation
        //anim.SetTrigger("Attack");
        //Brief pause before we calculate the hitbox
        yield return new WaitForSeconds(hitboxTiming);
        

        yield return new WaitForSeconds(attackAnimationTiming);
        
    } */

    private void Range()
    {
        GameObject fired = Instantiate(projectileType, lineOfSight.position, transform.rotation);
        fired.GetComponent<Projectile>().speed /= fireSpeed;
        fired.GetComponent<Projectile>().damage = rangeDamage;
        
        if (targetDir.x > 0f)
            fired.GetComponent<Projectile>().target = Vector2.right;
        else if (targetDir.x < 0f)
            fired.GetComponent<Projectile>().target = Vector2.left;

        //fired.GetComponent<Projectile>().target = targetDir;

        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), fired.GetComponent<Collider2D>());
    }
    
    #endregion

    #region health_functions
    //Enemy takes damage based on 'value' param
    public void TakeDamage(float value)
    {
        FindObjectOfType<AudioManager>().Play("EnemyDamaged");

        //Decrement health
        currHealth -= value;
        hpSlider.value = currHealth / maxHealth;
        //Check for death
        if (currHealth <= 0)
        {
            Die();
        }
    }

    //Destroys enemy object
    private void Die()
    {
        int val = Random.Range(0, 2);
        //Debug.Log(val);
        if (val == 0)
        {
            Instantiate(healthPack, transform.position + Vector3.up, transform.rotation);
        } else
        {
            Instantiate(timePack, transform.position + Vector3.up, transform.rotation);
        }
        Destroy(this.gameObject);
    }
    #endregion

    #region knockback_functions
    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.CompareTag("Player"))
        {
            bool knockToRight = true;

            //Debug.Log("detect");

            if (targetDir.x < 0f)
                knockToRight = false;

            target.GetComponentInParent<PlayerController>().CallKnockback(knockToRight);
        }
    }
    #endregion

}
