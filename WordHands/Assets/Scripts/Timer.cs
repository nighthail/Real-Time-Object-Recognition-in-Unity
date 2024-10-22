using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public float timeRemaining = 10f;
    public bool timerIsRunning = false;
    public TMP_Text CounterText;

    private void Start()
    {
        // Starts the timer automatically
        timerIsRunning = true;
        // Format the float to a string with two decimal places
        CounterText.text = "Time: " + timeRemaining.ToString("F2");

    }

    void Update()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                // Update the displayed time with two decimal places
                CounterText.text = "Time: " + timeRemaining.ToString("F2");

                // Change text color when timeRemaining is less than 9 seconds
                if (timeRemaining <= 10)
                {
                    CounterText.color = Color.red;
                }
            }
            else
            {
                // Handle the case when the time runs out
                Debug.Log("Time has run out!");
                timeRemaining = 0;
                timerIsRunning = false;
                SceneManager.LoadScene(sceneName: "GameOver");
            }
        }
    }
}
