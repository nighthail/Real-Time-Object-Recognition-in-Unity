using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FallingPrefabSpawner : MonoBehaviour
{
    [Header("Prefab to Spawn")]
    public GameObject wrongWord; 
    public GameObject rightWord; 

    [Header("Spawn Settings")]
    public float spawnInterval = 1.0f;  // Time between each spawn
    public float fallSpeed = 100f;       // Speed at which the prefab falls
    public int maxPrefabs = 20;          // Max number of prefabs on screen at once

    [Header("RawImage Boundaries")]
    public RectTransform rawImageRect;   // Reference to the RawImage's RectTransform

    [Header("Cursor for Collision")]
    public RectTransform cursorImage;    // Reference to the cursor's RectTransform for collision

    private float timeSinceLastSpawn = 0f;

    private string[] goodWords; // Array to hold good words
    private string[] badWords;  // Array to hold bad words

    private ThemeManager themeManager;
    private int correctAnswers;
    private int incorrectAnswers;

    void Start()
    {
        // Get the ThemeManager component
        themeManager = FindObjectOfType<ThemeManager>();

        // Load good and bad words from the currently selected theme
        goodWords = themeManager.GetGoodWords();
        badWords = themeManager.GetBadWords();
    }

    void Update()
    {
        timeSinceLastSpawn += Time.deltaTime;

        // Spawn prefabs at regular intervals
        if (timeSinceLastSpawn >= spawnInterval && transform.childCount < maxPrefabs)
        {
            SpawnPrefab();
            timeSinceLastSpawn = 0f;
        }

        // Move all spawned prefabs downward
        MovePrefabs();
    }

    // Function to spawn the prefab at a random position within the RawImage boundaries
    void SpawnPrefab()
    {
        // Randomly decide whether to spawn a good word or a bad word
        GameObject wordPrefab = (Random.value > 0.7f) ? rightWord : wrongWord;

        // Instantiate the word prefab but do not set the position yet
        GameObject newWord = Instantiate(wordPrefab, transform);
        RectTransform wordRect = newWord.GetComponent<RectTransform>();

        // Optionally, you can set the text of the prefab's TMP_Text here
        TMP_Text wordText = newWord.GetComponentInChildren<TMP_Text>();
        if (wordText != null)
        {
            if (wordPrefab == rightWord)
            {
                wordText.text = GetRandomWord(goodWords);
            }
            else
            {
                wordText.text = GetRandomWord(badWords);
            }
        }

        // Layout update to ensure the word's size is calculated based on the text content
        LayoutRebuilder.ForceRebuildLayoutImmediate(wordRect);

        // Now get the width of the instantiated prefab
        float prefabWidth = wordRect.rect.width;

        // Calculate the boundaries for spawning so that the entire word stays within the canvas
        float xMin = rawImageRect.rect.xMin + (prefabWidth / 2);
        float xMax = rawImageRect.rect.xMax - (prefabWidth / 2);

        // Generate a random X position within the adjusted range
        float randomX = Random.Range(xMin, xMax);

        // Set the spawn position above the RawImage
        Vector3 spawnPosition = new Vector3(randomX, rawImageRect.rect.yMax + 50f, 0);

        // Convert the local position to world position
        Vector3 worldSpawnPosition = rawImageRect.TransformPoint(spawnPosition);

        // Set the initial position in world space
        wordRect.position = worldSpawnPosition;
    }

    // Function to get a random word from the given array
    string GetRandomWord(string[] words)
    {
        if (words.Length > 0)
        {
            return words[Random.Range(0, words.Length)];
        }
        return "NoWord"; // Fallback if no words are available
    }

    // Function to move prefabs down the screen
    void MovePrefabs()
    {
        foreach (Transform child in transform)
        {
            RectTransform childRect = child.GetComponent<RectTransform>();

            if (childRect != null)
            {
                // Move the prefab downward
                childRect.anchoredPosition += Vector2.down * fallSpeed * Time.deltaTime;

                // Check if the prefab has collided with the cursor
              if (IsOverlapping(cursorImage, childRect))
            {
                // Check if it's a good word or a bad word
                if (child.CompareTag("RightWord"))
                {
                    correctAnswers = correctAnswers + 1;
                    PlayerPrefs.SetInt("rightAnswers", correctAnswers);
                    Debug.Log("Good Words: " + correctAnswers);
                }
                else if (child.CompareTag("WrongWord"))
                {
                    incorrectAnswers = incorrectAnswers + 1;
                    PlayerPrefs.SetInt("wrongAnswers", incorrectAnswers);
                    Debug.Log("Bad Words: " + incorrectAnswers);
                }

                Destroy(child.gameObject);  // Destroy the prefab on collision with the cursor
            }
            }
        }
    }

    // Collision detection between cursor and prefab
    bool IsOverlapping(RectTransform rect1, RectTransform rect2)
    {
        Vector3[] corners1 = new Vector3[4];
        Vector3[] corners2 = new Vector3[4];
        rect1.GetWorldCorners(corners1);
        rect2.GetWorldCorners(corners2);

        Rect rectA = new Rect(corners1[0], corners1[2] - corners1[0]);
        Rect rectB = new Rect(corners2[0], corners2[2] - corners2[0]);

        return rectA.Overlaps(rectB);
    }
}
