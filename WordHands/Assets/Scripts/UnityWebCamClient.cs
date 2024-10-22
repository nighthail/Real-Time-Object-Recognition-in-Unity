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
    private IPEndPoint serverEndPointVideo;

    private Texture2D receivedTexture;
    private Thread receiveThread;
    private bool isRunning = true;

    private ConcurrentQueue<byte[]> videoDataQueue = new ConcurrentQueue<byte[]>(); // Queue for video data

    void Start()
    {
        // Initialize UDP client
        udpClientVideo = new UdpClient(8001); // Unity receives from port 8001
        serverEndPointVideo = new IPEndPoint(IPAddress.Any, 8001);

        // Initialize the texture to display received video
        receivedTexture = new Texture2D(2, 2);

        // Start receiving video data in a separate thread
        receiveThread = new Thread(ReceiveData);
        receiveThread.Start();
    }

    void Update()
    {
        // Process the video data received from the thread
        if (videoDataQueue.TryDequeue(out byte[] videoData))
        {
            if (receivedTexture != null && videoData.Length > 0)
            {
                receivedTexture.LoadImage(videoData); // Load the received JPEG data into the texture
                rawImage.texture = receivedTexture;   // Update the displayed texture on the RawImage
            }
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
            }
            catch (SocketException e)
            {
                Debug.LogError("Socket exception: " + e.Message);
            }
        }
    }

    void OnDestroy()
    {
        // Safely stop the receiving thread
        isRunning = false;

        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Join();  // Wait for the thread to finish before shutting down
        }

        // Close the UDP client to avoid memory leaks
        if (udpClientVideo != null) udpClientVideo.Close();
    }
}
