using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathPlatform : MonoBehaviour
{
    public void OnCollisionEnter2D(Collision2D coll)
    {
        // find game manager and lose game
        if (coll.gameObject.CompareTag("Player"))
        {
            //GameObject gm = GameObject.FindWithTag("GameController");
            //gm.GetComponent<GameManager>().LoseGame();
            coll.gameObject.GetComponentInParent<PlayerController>().TakeDamage(5);
            coll.gameObject.GetComponentInParent<PlayerController>().Respawn();
        }
    }
}
