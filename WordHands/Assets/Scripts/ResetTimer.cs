using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class ResetTimer : MonoBehaviour
{
    public float timeRemaining = 10f;
    public bool timerIsRunning = false;
    public TMP_Text ResetText;

    private void Start()
    {
        // Starts the timer automatically
        timerIsRunning = true;
        // Format the float to a string with two decimal places
        ResetText.text = "Resets in: " + timeRemaining.ToString("F2") + " seconds";
    }

    void Update()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                // Update the displayed time with two decimal places
                ResetText.text = "Resets in: " + timeRemaining.ToString("F2") + " seconds";

            }
            else
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                Debug.Log("Game restart");
                timeRemaining = 0;
                timerIsRunning = false;
                SceneManager.LoadScene(sceneName: "GameStart");
            }
        }
    }
}
