using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialPoint : MonoBehaviour
{
    public GameObject text;
    private GameObject obj;

    public void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.CompareTag("Player"))
        {
            obj = Instantiate(text, transform.position + Vector3.up * 2, Quaternion.identity);
        }
    }

    public void OnTriggerExit2D(Collider2D coll)
    {
        if (coll.CompareTag("Player"))
        {
            Destroy(obj);
        }
    }
}
