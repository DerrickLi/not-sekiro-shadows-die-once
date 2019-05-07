using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    #region movement_variables
    public float setSpeed;
    private float moveSpeed;
    private bool movingRight = true;
    public Transform groundDetection;
    public float groundRayDistance;
    #endregion

    #region health_variables
    public float maxHealth;
    public Slider hpSlider;
    float currHealth;
    #endregion

    #region attack_variables
    public GameObject projectileType;
    public float timeToReload;
    public float fireSpeed;
    float reloadTimer = 0;
    FieldOfView fow;
    #endregion

    #region targeting_variables
    #endregion

    #region physics_components
    Rigidbody2D enemyRB;
    #endregion

    //Run once on creation
    private void Awake()
    {
        enemyRB = GetComponent<Rigidbody2D>();

        currHealth = maxHealth;
        hpSlider.value = currHealth / maxHealth;

        fow = GetComponentInParent<FieldOfView>();

        moveSpeed = setSpeed;

    }

    //Run every frame
    private void Update()
    {
        if (reloadTimer > timeToReload)
        {
            //Check to see if we know where player is
            if (fow.visibleTargets.Count > 0)
            {
                reloadTimer = 0;
                moveSpeed = 0;
                Fire();
            }
            else
                moveSpeed = setSpeed;
        }
        else
            reloadTimer += Time.deltaTime;

        Move();
        
    }

    #region movement_functions
    private void Move()
    {
        //Calculate the movement vector. Player pos - Enermy pos = Direction of player relative to enemy
        //Vector2 direction = new Vector2(targetingPlayer.position.x - transform.position.x, targetingPlayer.position.y);
        //enemyRB.velocity = direction.normalized * moveSpeed;

        //change to right if enemy face right at start
        transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);

        RaycastHit2D groundInfo = Physics2D.Raycast(groundDetection.position, Vector2.down, groundRayDistance);
        if (groundInfo.collider == false)
            if (movingRight)
            {
                transform.eulerAngles = new Vector3(0, -180, 0);
                movingRight = false;
            }
            else
            {
                transform.eulerAngles = new Vector3(0, 0, 0);
                movingRight = true;
            }


    }
    #endregion

    #region attack_functions
    private void Fire()
    {

        GameObject fired = Instantiate(projectileType, this.transform.position, Quaternion.identity);
        if (movingRight)
            fired.GetComponent<Projectile>().target = new Vector2(1,0);
        else
            fired.GetComponent<Projectile>().target = new Vector2(-1, 0);
        fired.GetComponent<Projectile>().speed /= fireSpeed;
        fired.GetComponent<Projectile>().damage /= 4.0f;

        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), fired.GetComponent<Collider2D>());

    }

    IEnumerator DelayFire()
    {
        //yield return new WaitForSeconds(enemyInfo.squadNum / 2.0f * enemyInfo.bCooldown);
        yield return new WaitForSeconds(0.5f);
        Fire();
    }
    #endregion

    #region health_functions
    //Enemy takes damage based on 'value' param
    public void TakeDamage(float value)
    {   
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
        Destroy(this.gameObject);
    }
    #endregion

}
