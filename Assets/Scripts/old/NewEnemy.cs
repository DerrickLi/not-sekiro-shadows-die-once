using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewEnemy : MonoBehaviour
{
     #region movement_variables
    public float movespeed;
    #endregion

    #region physics_components
    Rigidbody2D enemyRB;
    #endregion

    #region targeting_variables
    public Transform player;
    #endregion

    #region attack_variables
    public float explosionDamage;
    public float explosionRadius;
    public GameObject explosionObj;
    private int currExplosion;
    public int explosionDelay;
    #endregion

    #region health_variables
    public float maxHealth;
    float currHealth;
    #endregion

    #region Unity_functions
    private void Awake()
    {
        enemyRB = GetComponent<Rigidbody2D>();

        currHealth = maxHealth;
        currExplosion = explosionDelay;

    }

    private void Update()
    {
        if (player == null)
        {
            return;
        }

        Move();
    }
    #endregion

    #region movement_functions
    private void Move()
    {
        Vector2 direction = player.position - transform.position;

        // Debug.Log("Transform: " + transform.position);
        // Debug.Log("Player: " + player.position);
        // Debug.Log("Distance: " + Vector2.Distance())

        if (Vector2.Distance(transform.position, player.position) > 1) {
            enemyRB.velocity = direction.normalized * movespeed;
        } else {
            enemyRB.velocity = Vector2.zero;
        }
    }
    #endregion

    #region attack_functions

    // raycasts box for player and causes damage, spawns explosion prefab
    private void Explode()
    {
        // explosion sound
        FindObjectOfType<AudioManager>().Play("Explosion");

        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, explosionRadius, Vector2.zero);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.transform.CompareTag("Player"))
            {
                // cause damage
                hit.transform.GetComponent<PlayerController>().TakeDamage(explosionDamage);

                Debug.Log("hit player with explosion");

                // spawn prefab explosion
                Instantiate(explosionObj, transform.position, transform.rotation);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            Explode();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            if (currExplosion == 0)
            {
                Explode();
                currExplosion = explosionDelay;
            }
            else
            {
                currExplosion -= 1;
            }
        }
    }
    #endregion

    #region health_functions
    // enemy takes damage based on value
    public void TakeDamage(float value)
    {
        // audio hurt
        FindObjectOfType<AudioManager>().Play("BatHurt");

        currHealth -= value;
        Debug.Log("health now " + currHealth.ToString());
        
        if (currHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(this.gameObject);
    }
    #endregion

}
