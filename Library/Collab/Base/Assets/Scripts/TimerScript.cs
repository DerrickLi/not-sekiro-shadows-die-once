using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerScript : MonoBehaviour
{
    public float timeLeft;
    public Text timeText;

    //Calling once on creations
    void Awake()
    {
        timeText.text = "World of Darkness: " + ((int)timeLeft).ToString();
    }

    // Update is called once per frame
    void Update()
    {
        timeLeft -= Time.deltaTime;
        if (timeLeft < 0)
        {
            Debug.Log("GameOver");
        }
        else
            timeText.text = "World of Darkness: " + ((int) timeLeft).ToString();
    }
}
