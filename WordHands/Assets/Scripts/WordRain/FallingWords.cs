using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FallingWords : MonoBehaviour
{
    [Header("Cursor Prefab")]
    public RectTransform cursorImage;

    [Header("Good Word Prefab")]
    public GameObject GoodWordPrefab;

    [Header("Bad Word Prefab")]
    public GameObject BadWordPrefab;

    [Header("RawImage for Boundaries")]
    public RectTransform rawImageRect; // Reference to the RawImage's RectTransform

    [Header("Spawn Settings")]
    public float spawnInterval = 2.0f;  // Time between word spawns
    public float fallSpeed = 100f;      // Speed at which the words fall
    
    private float timeSinceLastSpawn;

    void Start()
    {
        timeSinceLastSpawn = 0f;
    }

    void Update()
    {
        // Spawn new word at regular intervals
        timeSinceLastSpawn += Time.deltaTime;
        if (timeSinceLastSpawn >= spawnInterval)
        {
            SpawnWord();
            timeSinceLastSpawn = 0f;
        }

        // Move existing words down
        MoveWordsDown();
    }

    // Function to spawn a new word object within the RawImage boundaries
    void SpawnWord()
    {
        // Randomly decide whether to spawn a good word or a bad word
        GameObject wordPrefab = (Random.value > 0.5f) ? GoodWordPrefab : BadWordPrefab;

        // Get the raw image rect's size and position
        Vector3[] rawImageCorners = new Vector3[4];
        rawImageRect.GetWorldCorners(rawImageCorners);

        // Convert world corners to local coordinates relative to the canvas
        Vector2 topLeft = rawImageRect.InverseTransformPoint(rawImageCorners[1]); // Top left corner
        Vector2 bottomRight = rawImageRect.InverseTransformPoint(rawImageCorners[3]); // Bottom right corner

        // Random spawn position within the RawImage boundaries
        float spawnX = Random.Range(topLeft.x, bottomRight.x);
        float spawnY = topLeft.y + 50f;  // Slightly above the top of the RawImage

        // Instantiate the word prefab as a child of the canvas
        GameObject newWord = Instantiate(wordPrefab, rawImageRect.parent); 
        RectTransform wordRect = newWord.GetComponent<RectTransform>();

        // Set the initial anchored position within the RawImage
        wordRect.anchoredPosition = new Vector2(spawnX, spawnY);
        wordRect.localScale = Vector3.one;  // Ensure correct scale

        // Ensure this word prefab is drawn on top of the RawImage
        newWord.transform.SetAsLastSibling();

        // Get the TMP_Text component from the newly instantiated object
        TMP_Text wordText = newWord.GetComponentInChildren<TMP_Text>();

        if (wordText != null)
        {
            if (wordPrefab == GoodWordPrefab)
            {
                wordText.text = GetRandomGoodWord();
            }
            else
            {
                wordText.text = GetRandomBadWord();
            }
        }
        else
        {
            Debug.LogError("TMP_Text component not found on the wordPrefab or its children!");
        }
    }

    // Function to move all word objects down
    void MoveWordsDown()
    {
        foreach (Transform word in transform)
        {
            RectTransform wordRect = word.GetComponent<RectTransform>();
            if (wordRect != null)
            {
                // Move the word down based on fall speed
                wordRect.anchoredPosition += Vector2.down * fallSpeed * Time.deltaTime;

                // Check if the word has collided with the cursor
                if (IsOverlapping(cursorImage, wordRect))
                {
                    Destroy(word.gameObject);  // Destroy the word object on collision with the cursor
                }

                // Destroy words that fall off the bottom of the screen
                if (wordRect.anchoredPosition.y < -100f)
                {
                    Destroy(word.gameObject);
                }
            }
        }
    }

    // Collision detection between cursor and word
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

    // Example function to get a random word
    string GetRandomGoodWord()
    {
        string[] words = { "Kitty", "Cute", "Best", "Woo", "Wow!", "Love" };
        return words[Random.Range(0, words.Length)];
    }

    string GetRandomBadWord()
    {
        string[] words = { "Bad", "Awful", "Shiet", "Bob", "Damn", "Noo" };
        return words[Random.Range(0, words.Length)];
    }
}
