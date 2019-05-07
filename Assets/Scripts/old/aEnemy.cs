using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class aEnemy : MonoBehaviour
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

        enemyRB.velocity = direction.normalized * movespeed;
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
                Destroy(this.gameObject);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            Explode();
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
