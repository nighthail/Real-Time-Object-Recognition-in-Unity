using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextInCorner : MonoBehaviour
{
    public TextMeshProUGUI scoreText;  // Use TextMeshProUGUI for UI Text
    public int scorePoints;

    void Start()
    {
        scorePoints = 2;
        UpdateScore();  // Set the initial text to display the score
    }

    public void UpdateScore()
    {
        // Update the text with the latest score
        scoreText.text = "Points: " + scorePoints;
    }

    // Update is called once per frame
    void Update()
    {
        // If you want continuous checks or additional logic here, you can add it
    }
}
