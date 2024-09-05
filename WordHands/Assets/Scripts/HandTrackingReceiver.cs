using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class HandTrackingReceiver : MonoBehaviour
{
    public int handPort = 8002;   // Port for hand position data
    public int videoPort = 8001;  // Port for video feed data
    public RawImage rawImage;     // Reference to the RawImage UI element
    public RectTransform buttonRectTransform; // Reference to a UI button's RectTransform

    private UdpClient handClient;
    private UdpClient videoClient;
    private Vector2 handPosition;
    private Texture2D texture;

    void Start()
    {
        // Initialize UDP clients
        handClient = new UdpClient(handPort);
        videoClient = new UdpClient(videoPort);

        // Initialize texture with appropriate size
        texture = new Texture2D(640, 480); 
        rawImage.texture = texture;
    }

    void Update()
    {
        ReceiveVideoFeed();
        ReceiveHandPosition();
        CheckHandInteraction();
    }

    private void ReceiveVideoFeed()
    {
        if (videoClient.Available > 0)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, videoPort);
            byte[] receivedData = videoClient.Receive(ref endPoint);

            // Load video data into texture
            texture.LoadImage(receivedData);
            rawImage.texture = texture;
        }
    }

    private void ReceiveHandPosition()
    {
        if (handClient.Available > 0)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, handPort);
            byte[] receivedData = handClient.Receive(ref endPoint);

            if (receivedData != null)
            {
                string dataString = System.Text.Encoding.UTF8.GetString(receivedData);
                string[] positionStrings = dataString.Split(',');
                if (positionStrings.Length == 2)
                {
                    float x = float.Parse(positionStrings[0]) * Screen.width;
                    float y = (1 - float.Parse(positionStrings[1])) * Screen.height; // Flip y-axis
                    handPosition = new Vector2(x, y);
                }
            }
        }
    }

    private void CheckHandInteraction()
    {
        // Check if hand is over the UI button
        if (buttonRectTransform != null && RectTransformUtility.RectangleContainsScreenPoint(buttonRectTransform, handPosition, null))
        {
            Debug.Log("Hand is over the button!");
            // Example interaction: Change button color
            buttonRectTransform.GetComponent<Image>().color = Color.red;
        }
        else
        {
            // Reset button color if not over
            buttonRectTransform.GetComponent<Image>().color = Color.white;
        }
    }

    private void OnApplicationQuit()
    {
        handClient.Close();
        videoClient.Close();
    }
}
