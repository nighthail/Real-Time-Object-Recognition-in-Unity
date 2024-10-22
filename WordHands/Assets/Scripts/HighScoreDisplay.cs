using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HighScoreDisplay : MonoBehaviour
{
    public TMP_Text WrongWords;
    public TMP_Text RightWords;

    void Start()
    {
        // Load the right and wrong answers from PlayerPrefs
        int rightAnswers = PlayerPrefs.GetInt("rightAnswers", 0); // Default to 0 if the key doesn't exist
        int wrongAnswers = PlayerPrefs.GetInt("wrongAnswers", 0); // Default to 0 if the key doesn't exist

        // Display the values in the text fields
        WrongWords.text = wrongAnswers.ToString(); // Show wrong answers
        RightWords.text = rightAnswers.ToString(); // Show right answers
    }

    void Update()
    {
        // No need to update anything in this case
    }
}
