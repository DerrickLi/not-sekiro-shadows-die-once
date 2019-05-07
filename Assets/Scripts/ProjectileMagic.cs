using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileMagic : Projectile
{
	private Vector2 dir;
	public float flyTime;
	private float currTime;
    public bool reversed;

	void Start()
	{
        dir = target.normalized;
       
        currTime = 0.0f;
        
	}

	private void FixedUpdate()
	{
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, 0.25F);

        if (hit.transform != null && !reversed && hit.transform.CompareTag("Player"))
        {
            bool success = hit.transform.GetComponentInParent<PlayerController>().TakeDamage(damage);

            if (success)
            {
                Destroy(this.gameObject);
            }
            else
            {
                this.dir *= -1; // reverse the bullet
                reversed = true;
            }
        }
        else if (hit.transform != null && hit.transform.CompareTag("Enemy") && reversed)
        {
            hit.transform.GetComponent<EnemyAI>().TakeDamage(damage);
            Destroy(this.gameObject);
        }
        //else if (hit.transform != null && hit.transform.CompareTag("Boss") && reversed)
        //{
        //    hit.transform.GetComponent<FinalBoss>().TakeDamage(damage);
        //    Destroy(this.gameObject);
        //}

        else
        {
            transform.position += (Vector3)(dir) * speed * Time.deltaTime;
            if (currTime >= flyTime)
                Destroy(gameObject);
            currTime += Time.deltaTime;
        }
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
        //Debug.Log(collision.gameObject.tag);

        if (collision.gameObject.CompareTag("Player") && !reversed)
		{
          
            //FindObjectOfType<AudioManager>().Play("PlayerDamaged");
            bool success = collision.gameObject.GetComponentInParent<PlayerController>().TakeDamage(damage);

            if (success)
            {
                Destroy(this.gameObject);
            } else
            {
                this.dir *= -1; // reverse the bullet
                reversed = true;
            }
            
        } else if (collision.gameObject.CompareTag("Enemy") && reversed)
        {
            collision.gameObject.GetComponent<EnemyAI>().TakeDamage(damage);
            Destroy(this.gameObject);
        }
        else if (collision.gameObject.CompareTag("Boss") && reversed)
        {
            collision.gameObject.GetComponent<FinalBoss>().TakeDamage(damage);
            Destroy(this.gameObject);
        }
    }
}


