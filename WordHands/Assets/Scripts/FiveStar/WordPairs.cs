using System.Collections.Generic;  // Required for using List<>
using System.Linq;                 // Required for LINQ methods like ElementAt, OrderBy
using UnityEngine;                 // Required for Unity-specific functionality
using TMPro;                       // Required for TextMeshProUGUI
using System.Collections;           // Required for IEnumerator

public class WordPairs : MonoBehaviour
{
    private FiveStarDictionary fiveStarDictionary; // Reference to the FiveStarDictionary

    private List<string> translations; 
    private string correctTranslation; 

    public TextMeshProUGUI centerWordText;   
    public List<TextMeshProUGUI> choiceTexts; 
    public RectTransform cursorImage;  // Reference to the cursor image RectTransform

    private int rightAnswer = 0;
    private int wrongAnswer = 0;
    private Color correctColor = Color.green; // Color for correct answer
    private Color incorrectColor = Color.red; // Color for incorrect answer
    private Color defaultColor = Color.white; // Default color

    private bool canInteract = true;  // Flag to determine if we can interact with words
    private bool newWordsLoaded = false; // Flag to check if new words have been loaded
    private bool isLoadingNewWords = false; // Flag to indicate if new words are being loaded

    void Start()
    {
        fiveStarDictionary = new FiveStarDictionary(); // Initialize the FiveStarDictionary
        LoadNewWord();
    }

    void Update()
    {
        PlayerPrefs.SetInt("rightAnswers", rightAnswer);
        PlayerPrefs.SetInt("wrongAnswers", wrongAnswer);
        PlayerPrefs.Save();
        
        // Check hover over each word only if interaction is allowed
        if (canInteract && !isLoadingNewWords) // Check if new words are loading
        {
            for (int i = 0; i < choiceTexts.Count; i++)
            {
                if (IsOverlapping(cursorImage, choiceTexts[i].rectTransform))
                {
                    // If new words have been loaded, don't allow interaction yet
                    if (newWordsLoaded)
                    {
                        // Reset color to default if hovering over a new word
                        choiceTexts[i].color = defaultColor;
                        return; // Exit early
                    }

                    // Change color based on whether the word is correct or incorrect
                    if (choiceTexts[i].text == correctTranslation)
                    {
                        choiceTexts[i].color = correctColor;
                        rightAnswer++;
                        StartCoroutine(ShowResultAndLoadNewWord(true, choiceTexts[i].transform)); // Pass the selected choice's transform
                    }
                    else
                    {
                        choiceTexts[i].color = incorrectColor;
                        wrongAnswer++;
                        StartCoroutine(ShowResultAndLoadNewWord(false, choiceTexts[i].transform)); // Pass the selected choice's transform
                    }

                    // Disable interaction after hovering
                    canInteract = false;
                    newWordsLoaded = true; // Set flag to indicate new words are loaded
                    isLoadingNewWords = true; // Set loading flag
                    break; // Exit the loop since we already processed a hover
                }
                else
                {
                    // Reset color to default if not hovering
                    choiceTexts[i].color = defaultColor;
                }
            }
        }
        else
        {
            // If we can't interact, check if the cursor has unhovered all words
            bool allWordsUnhovered = true;
            for (int i = 0; i < choiceTexts.Count; i++)
            {
                if (IsOverlapping(cursorImage, choiceTexts[i].rectTransform))
                {
                    allWordsUnhovered = false; // Still hovering over a word
                    break;
                }
            }

            // If all words have been unhovered, allow interaction again
            if (allWordsUnhovered)
            {
                canInteract = true; // Enable interaction again
                if (newWordsLoaded)
                {
                    isLoadingNewWords = false; // Reset loading flag
                }
                newWordsLoaded = false; // Reset new words loaded flag
            }
        }
    }

    public void LoadNewWord()
    {
        Debug.Log("right: " + rightAnswer + " wrong: " + wrongAnswer);

        // Use ElementAt to get a random word from the dictionary
        int randomIndex = Random.Range(0, fiveStarDictionary.WordPairs.Count);
        var randomWord = fiveStarDictionary.WordPairs.ElementAt(randomIndex);

        centerWordText.text = randomWord.Key;
        correctTranslation = randomWord.Value;

        // Create a new list for translations and add the correct translation
        translations = new List<string> { correctTranslation };

        // Get all other translations
        var allTranslations = new List<string>(fiveStarDictionary.WordPairs.Values);
        allTranslations.Remove(correctTranslation); // Remove the correct answer

        // Shuffle the remaining translations
        allTranslations = allTranslations.OrderBy(x => Random.value).ToList();

        // Select the first few translations to fill up to the required number of choices
        int numberOfChoices = choiceTexts.Count - 1; // Subtract one for the correct answer
        translations.AddRange(allTranslations.Take(numberOfChoices));

        // Shuffle the final list to randomize positions
        translations = translations.OrderBy(x => Random.value).ToList();

        // Assign text to UI elements
        for (int i = 0; i < choiceTexts.Count; i++)
        {
            choiceTexts[i].text = translations[i];
        }
    }

    private IEnumerator ShowResultAndLoadNewWord(bool isCorrect, Transform selectedChoiceTransform)
    {
        // Find the icon based on correctness
        GameObject resultIcon = selectedChoiceTransform.Find(isCorrect ? "RightIcon" : "WrongIcon").gameObject;
        resultIcon.SetActive(true); // Show the icon

        // Wait for one second
        yield return new WaitForSeconds(1f);

        // Hide the icon after one second
        resultIcon.SetActive(false);

        LoadNewWord(); // Load new words after showing the result
        canInteract = true; // Allow interaction again
        isLoadingNewWords = false; // Reset loading flag after words have loaded
    }

    // Function to check if two RectTransforms are overlapping
    bool IsOverlapping(RectTransform rect1, RectTransform rect2)
    {
        // Get world corners of the RectTransforms
        Vector3[] corners1 = new Vector3[4];
        Vector3[] corners2 = new Vector3[4];
        rect1.GetWorldCorners(corners1);
        rect2.GetWorldCorners(corners2);

        // Create bounds for both RectTransforms
        Rect rectA = new Rect(corners1[0], corners1[2] - corners1[0]);
        Rect rectB = new Rect(corners2[0], corners2[2] - corners2[0]);

        // Check if the two rectangles intersect
        return rectA.Overlaps(rectB);
    }
}
