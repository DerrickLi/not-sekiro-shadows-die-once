using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineOfSight : MonoBehaviour
{
    // called when something enters the trigger collider
    private void OnTriggerEnter2D(Collider2D coll)
    {
        // check if coll is the player
        if (coll.CompareTag("Player"))
        {
            GetComponentInParent<FinalBoss>().player = coll.transform;
            GetComponentInParent<FinalBoss>().hpSlider.gameObject.SetActive(true);
        }
    }
}
