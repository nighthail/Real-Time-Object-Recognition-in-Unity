using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Collections.Concurrent;

public class UnityWebCamClient : MonoBehaviour
{
    public RawImage rawImage; // For displaying the video feed

    private UdpClient udpClientVideo; // UDP client for receiving video feed
    private UdpClient udpClientResolution; // UDP client for receiving resolution data
    private IPEndPoint serverEndPointVideo;
    private IPEndPoint serverEndPointResolution;

    private Texture2D receivedTexture;
    private Thread receiveThread;
    private bool isRunning = true;

    private ConcurrentQueue<byte[]> videoDataQueue = new ConcurrentQueue<byte[]>(); // Queue for video data
    private ConcurrentQueue<string> resolutionQueue = new ConcurrentQueue<string>(); // Queue for resolution data

    private int videoWidth = 640; // Default width
    private int videoHeight = 480; // Default height

    void Start()
    {
        // Initialize UDP clients
        udpClientVideo = new UdpClient(8001); // Unity receives from port 8001
        udpClientResolution = new UdpClient(8003); // Unity receives resolution from port 8003
        serverEndPointVideo = new IPEndPoint(IPAddress.Any, 8001);
        serverEndPointResolution = new IPEndPoint(IPAddress.Any, 8003);

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
    }

    void AdjustAspectRatio()
    {
        // Adjust the RawImage's aspect ratio based on the received resolution
        float aspectRatio = (float)videoWidth / videoHeight;
        RectTransform rt = rawImage.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(rt.sizeDelta.y * aspectRatio, rt.sizeDelta.y); // Adjust width based on aspect ratio
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
    }
}
