using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;

public class UnityWebCamClient : MonoBehaviour
{
    public RawImage rawImage; // For displaying the video feed
    public Button uiButton;   // The button we want to change color

    private UdpClient udpClientVideo; // UDP client for receiving video feed
    private UdpClient udpClientResolution; // UDP client for receiving resolution data
    private UdpClient udpClientLandmarks; // UDP client for receiving landmark data
    private IPEndPoint serverEndPointVideo;
    private IPEndPoint serverEndPointResolution;
    private IPEndPoint serverEndPointLandmarks;

    private Texture2D receivedTexture;
    private Thread receiveThread;
    private bool isRunning = true;

    private ConcurrentQueue<byte[]> videoDataQueue = new ConcurrentQueue<byte[]>(); // Queue for video data
    private ConcurrentQueue<string> resolutionQueue = new ConcurrentQueue<string>(); // Queue for resolution data
    private ConcurrentQueue<string> landmarksDataQueue = new ConcurrentQueue<string>(); // Queue for landmark data

    private int videoWidth = 640; // Default width
    private int videoHeight = 480; // Default height

    void Start()
    {
        // Initialize UDP clients
        udpClientVideo = new UdpClient(8001); // Unity receives from port 8001
        udpClientResolution = new UdpClient(8003); // Unity receives resolution from port 8003
        udpClientLandmarks = new UdpClient(8002); // Unity receives landmark data from port 8002
        serverEndPointVideo = new IPEndPoint(IPAddress.Any, 8001);
        serverEndPointResolution = new IPEndPoint(IPAddress.Any, 8003);
        serverEndPointLandmarks = new IPEndPoint(IPAddress.Any, 8002);

        // Initialize the texture to display received video
        receivedTexture = new Texture2D(2, 2);

        // Start receiving video and resolution data in a separate thread
        receiveThread = new Thread(ReceiveData);
        receiveThread.Start();
    }

    void Update()
    {
        // Process the video data received from the thread
        if (videoDataQueue.TryDequeue(out byte[] videoData))
        {
            receivedTexture.LoadImage(videoData); // Load the received JPEG data into the texture
            rawImage.texture = receivedTexture;   // Update the displayed texture on the RawImage
        }

        // Process the resolution data received from the thread
        if (resolutionQueue.TryDequeue(out string resolutionData))
        {
            string[] dimensions = resolutionData.Split(',');
            if (dimensions.Length == 2)
            {
                videoWidth = int.Parse(dimensions[0]);
                videoHeight = int.Parse(dimensions[1]);

                AdjustAspectRatio(); // Adjust the RawImage size based on the new resolution
            }
        }

        // Process the landmark data received from the thread
        if (landmarksDataQueue.TryDequeue(out string landmarksData))
        {
            // Parse the plain text data
            var handLandmarksStrings = landmarksData.Split('|');
            if (handLandmarksStrings.Length > 0)
            {
                // Use the first hand for this example, adjust if you want to support multiple hands
                var handLandmarksStr = handLandmarksStrings[0].Split(';');
                var handLandmarks = new List<Vector3>();

                foreach (var landmarkStr in handLandmarksStr)
                {
                    var coords = landmarkStr.Split(',');
                    if (coords.Length == 3)
                    {
                        // Parse x, y, z coordinates
                        float x = float.Parse(coords[0]);
                        float y = float.Parse(coords[1]);
                        float z = float.Parse(coords[2]);
                        handLandmarks.Add(new Vector3(x, y, z));
                    }
                }

                // Check if the tip of the index finger is over the button
                CheckIfHandOverButton(handLandmarks);
            }
        }
    }

    void AdjustAspectRatio()
    {
        // Adjust the RawImage's aspect ratio based on the received resolution
        float aspectRatio = (float)videoWidth / videoHeight;
        RectTransform rt = rawImage.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(rt.sizeDelta.y * aspectRatio, rt.sizeDelta.y); // Adjust width based on aspect ratio
    }

    void CheckIfHandOverButton(List<Vector3> handLandmarks)
    {
        // Check if there are enough landmarks in the list
        if (handLandmarks.Count > 8)
        {
            // Assuming index 8 is the tip of the index finger (you can change to whatever landmark you need)
            Vector3 indexFingerPos = handLandmarks[8]; // Index 8 is typically the tip of the index finger

            // Convert the normalized hand position to screen space coordinates
            Vector2 screenPos = new Vector2(indexFingerPos.x * Screen.width, (1 - indexFingerPos.y) * Screen.height);

            // Check if the index finger is over the button
            RectTransform buttonRectTransform = uiButton.GetComponent<RectTransform>();
            if (RectTransformUtility.RectangleContainsScreenPoint(buttonRectTransform, screenPos))
            {
                // Change button color when hand is over the button
                uiButton.image.color = Color.red;
            }
            else
            {
                // Reset button color when hand is not over the button
                uiButton.image.color = Color.white;
            }
        }
        else
        {
            // Debug message if the list does not contain enough landmarks
            Debug.LogWarning("Hand landmarks data is incomplete. Not enough landmarks found.");
        }
    }


    void ReceiveData()
    {
        while (isRunning)
        {
            try
            {
                // Receive video data from Python server
                byte[] videoData = udpClientVideo.Receive(ref serverEndPointVideo);
                videoDataQueue.Enqueue(videoData); // Enqueue the video data for processing in Update()

                // Receive resolution data from Python server
                byte[] resolutionData = udpClientResolution.Receive(ref serverEndPointResolution);
                string resolution = System.Text.Encoding.UTF8.GetString(resolutionData);
                resolutionQueue.Enqueue(resolution); // Enqueue the resolution data for processing in Update()

                // Receive landmark data from Python server
                byte[] landmarkData = udpClientLandmarks.Receive(ref serverEndPointLandmarks);
                string landmarks = System.Text.Encoding.UTF8.GetString(landmarkData);
                landmarksDataQueue.Enqueue(landmarks); // Enqueue the landmark data for processing in Update()
            }
            catch (SocketException e)
            {
                Debug.LogError("Socket exception: " + e.Message);
            }
        }
    }

    void OnDestroy()
    {
        // Clean up resources
        isRunning = false;
        if (receiveThread != null)
        {
            receiveThread.Join(); // Wait for the thread to finish
        }
        udpClientVideo.Close();
        udpClientResolution.Close();
        udpClientLandmarks.Close();
    }
}
