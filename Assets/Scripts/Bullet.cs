using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Vector3 dir;
    public float flyTime = 1;
    private float currTime = 0;
    public float damage = 3;
    public float speed = 20;

    void FixedUpdate()
    {
        transform.position += (Vector3)(dir) * speed * Time.deltaTime;
        if (currTime >= flyTime)
            Destroy(gameObject);
        currTime += Time.deltaTime;
    }

    void OnTriggerEnter2D (Collider2D coll)
    {
        if (coll.gameObject.CompareTag("Enemy"))
        {
            coll.gameObject.GetComponent<EnemyAI>().TakeDamage(damage);
            Destroy(this.gameObject);
        }
        else if (coll.gameObject.CompareTag("Boss"))
        {
            coll.gameObject.GetComponent<FinalBoss>().TakeDamage(damage);
            Destroy(this.gameObject);
        }
    }
}
