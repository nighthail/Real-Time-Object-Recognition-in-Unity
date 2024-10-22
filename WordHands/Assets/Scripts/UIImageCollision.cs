using UnityEngine;
using UnityEngine.UI;  // Or use TMPro if you are using TextMeshPro
using TMPro; // Include if using TextMeshPro

public class UIImageCollision : MonoBehaviour
{
    [Header("Cursor Image")]
    public RectTransform cursorImage;

    [Header("Word RectTransforms")]
    public RectTransform[] wordImages;  // Array to store RectTransforms for word texts

    void Update()
    {
        // Loop through each wordImage and check for overlap
        for (int i = 0; i < wordImages.Length; i++)
        {
            if (IsOverlapping(cursorImage, wordImages[i]))
            {
                // Make the word image invisible (set alpha to 0)
                MakeImageInvisible(wordImages[i]);
            }
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

    // Function to make an image invisible (or text)
    void MakeImageInvisible(RectTransform rectTransform)
    {
        // Change the image's color to make it fully transparent (alpha = 0)
        CanvasGroup canvasGroup = rectTransform.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = rectTransform.gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 0f; // Set alpha to 0 to make it invisible
    }
}
