using UnityEngine;
using UnityEngine.UI;

public class WebcamDisplay : MonoBehaviour
{
    private WebCamTexture webcamTexture;
    public RawImage rawImage; // Assign this in the Unity Editor

    void Start()
    {
        // Check if there are any webcams available
        if (WebCamTexture.devices.Length > 0)
        {
            // Initialize the webcam texture
            webcamTexture = new WebCamTexture();

            // Start the webcam
            webcamTexture.Play();

            // Set the texture of the RawImage to the webcam feed
            rawImage.texture = webcamTexture;
            rawImage.material.mainTexture = webcamTexture;

            // Wait until the webcam texture is playing to get its dimensions
            StartCoroutine(AdjustAspectRatio());
        }
        else
        {
            Debug.LogWarning("No webcam found!");
        }
    }

    System.Collections.IEnumerator AdjustAspectRatio()
    {
        // Wait for the webcam to initialize (to get the correct width and height)
        while (!webcamTexture.isPlaying || webcamTexture.width == 16)
        {
            yield return null; // Wait for the next frame
        }

        // Calculate the aspect ratio
        float aspectRatio = (float)webcamTexture.width / webcamTexture.height;

        // Adjust the RawImage's aspect ratio
        RectTransform rt = rawImage.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(rt.sizeDelta.y * aspectRatio, rt.sizeDelta.y);
    }

    void OnDisable()
    {
        // Stop the webcam when the object is disabled or destroyed
        if (webcamTexture != null)
        {
            webcamTexture.Stop();
        }
    }
}
