using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour
{
    public int gateNum;
    // Start is called before the first frame update

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            if (collision.transform.GetComponentInParent<PlayerController>().CheckKey(gateNum))
            {
                Destroy(this.gameObject);
            }
        }
    }
}
