using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimePack : MonoBehaviour
{
    public float timeAmount;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            GameObject.FindWithTag("Timer").GetComponent<TimerScript>().AddTime(timeAmount);
            Destroy(this.gameObject);
        }
    }
}
