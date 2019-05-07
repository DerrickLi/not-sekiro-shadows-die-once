using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewLineOfSight : MonoBehaviour
{
    // called when something enters the trigger collider
    private void OnTriggerEnter2D(Collider2D coll)
    {
        // check if coll is the player
        if (coll.CompareTag("Player"))
        {
            GetComponentInParent<NewEnemy>().player = coll.transform;

            Debug.Log("SEE PLAYER RUN AT PLAYER");
        }
    }
}
