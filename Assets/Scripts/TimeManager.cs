using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour
{
    public float timeLeft;
    public Text timeText;

    public float slowdownFactor = 0.05f;
    public float slowdownLength = 5f;

    public Slider timeSlider;
    public float maxTime;

    private float slowdownTimer = Mathf.Infinity;


    //Calling once on creations
    void Awake()
    {
        timeText.text = "Darkness: " + ((int)timeLeft).ToString();
        timeSlider.value = timeLeft / maxTime;
    }

    // Update is called once per frame
    void Update()
    {
        timeLeft -= Time.deltaTime;
        if (timeLeft < 0)
        {
            // find game manager and lose game
            GameObject gm = GameObject.FindWithTag("GameController");
            gm.GetComponent<GameManager>().LoseGame();
        }
        else
            timeText.text = "Darkness: " + ((int) timeLeft).ToString();

        timeSlider.value = timeLeft / maxTime;

        //Debug.Log(Time.timeScale);
    }

    void FixedUpdate()
    {
        slowdownTimer -= Time.unscaledDeltaTime;
        if (slowdownTimer < 0)
        {
            Time.timeScale = 1;
            Time.fixedDeltaTime = .02f;
        }
    }

    public void AddTime(float amount)
    {
        timeLeft += amount;
    }

    public void slowTime()
    {
        Time.timeScale = slowdownFactor;
        Time.fixedDeltaTime = Time.timeScale * .02f;
        slowdownTimer = slowdownLength;
    }
}
