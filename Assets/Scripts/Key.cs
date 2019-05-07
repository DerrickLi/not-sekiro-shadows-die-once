using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{
    public int keyNum;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            collision.transform.GetComponentInParent<PlayerController>().AddKey(keyNum);
            Destroy(this.gameObject);
        }
    }

    public void SetNum(int num)
    {
        keyNum = num;
    }
}
