using System.Collections.Generic;  // Required for using List<>
using System.Linq;                 // Required for LINQ methods like ElementAt, OrderBy
using UnityEngine;                 // Required for Unity-specific functionality
using TMPro;                       // Required for TextMeshProUGUI
using UnityEngine.UI;
using UnityEngine.SceneManagement;  // Required for SceneManager

public class GameModeStart : MonoBehaviour
{
    public RectTransform cursorImage;  // Reference to the cursor image RectTransform
    public RectTransform FiveStarMode; // Reference to the FiveStarMode RectTransform
    public RectTransform WordRainMode; // Reference to the WordRainMode RectTransform

    // Declare sprite variables but don't initialize them yet
    Sprite WordRainImage;
    Sprite WordRainImageH;
    Sprite FiveStarImage;
    Sprite FiveStarImageH;
    // Variables for countdown timer
    private bool countDown = false;
    private float delayBeforeSwitch = 2f; // 2-second delay

    void Start()
    {
        // Load resources in the Start method
        WordRainImage = Resources.Load<Sprite>("WordRainImage");
        WordRainImageH = Resources.Load<Sprite>("WordRainImage_h");
        FiveStarImage = Resources.Load<Sprite>("FiveStarImage");
        FiveStarImageH = Resources.Load<Sprite>("FiveStarImage_h");
    }

    void Update()
    {
        bool isHoveringFiveStar = IsOverlapping(cursorImage, FiveStarMode);
        bool isHoveringWordRain = IsOverlapping(cursorImage, WordRainMode);

        // Handle hover state for FiveStarMode
        if (isHoveringFiveStar)
        {
            countDown = true; // Start countdown if hovering FiveStarMode
            FiveStarMode.GetComponent<Image>().sprite = FiveStarImageH;
            Debug.Log("You hover FiveStarMode");

            // Countdown logic for scene switch
            if (countDown)
            {
                delayBeforeSwitch -= Time.deltaTime;
                if (delayBeforeSwitch <= 0)
                {
                    SceneManager.LoadScene("Five_Star");
                }
            }
        }
        else
        {
            // Reset the countdown if not hovering
            countDown = false;
            delayBeforeSwitch = 2f;  // Reset the delay timer
            FiveStarMode.GetComponent<Image>().sprite = FiveStarImage;
        }

        // Handle hover state for WordRainMode
        if (isHoveringWordRain)
        {
            WordRainMode.GetComponent<Image>().sprite = WordRainImageH;
            Debug.Log("You hover WordRainMode");
        }
        else
        {
            WordRainMode.GetComponent<Image>().sprite = WordRainImage;
        }

        // Log "Nothing hovered" only if neither mode is being hovered
        if (!isHoveringFiveStar && !isHoveringWordRain)
        {
            Debug.Log("Nothing hovered");
        }
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
