using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class RacketReceiver : MonoBehaviour
{
    UdpClient udpClient;
    Thread receiveThread;
    public int port = 8002;  // Same port as in the Python script

    public RectTransform rawImageTransform;  // The RectTransform of the RawImage
    public RectTransform spriteTransform;    // The RectTransform of the sprite to move

    // Resolution of the feed from the Python script (320x240)
    private float pythonWidth = 320f;
    private float pythonHeight = 240f;

    // Resolution of the Unity RawImage (640x480)
    private float unityWidth = 640f;
    private float unityHeight = 480f;

    Vector2 ballPosition;  // Stores the ball's position

    void Start()
    {
        udpClient = new UdpClient(port);
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();

        // Flip the RawImage horizontally (to mirror the webcam feed)
        rawImageTransform.localScale = new Vector3(-1, 1, 1);
    }

    void ReceiveData()
    {
        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, port);
        while (true)
        {
            byte[] data = udpClient.Receive(ref ipEndPoint);
            string message = Encoding.UTF8.GetString(data);
            string[] positionData = message.Split(',');
            float x = float.Parse(positionData[0]);
            float y = float.Parse(positionData[1]);

            // Store ball position
            ballPosition = new Vector2(x, y);
        }
    }

    void Update()
    {
        // 1. Flip the X-axis coordinate
        float mirroredX = pythonWidth - ballPosition.x;

        // 2. Flip the Y-axis (since OpenCV's Y-axis starts at the top, and Unity's starts at the bottom)
        float flippedY = pythonHeight - ballPosition.y;

        // 3. Scale to match the RawImage resolution (Unity: 640x480)
        float scaledX = (mirroredX / pythonWidth) * unityWidth;
        float scaledY = (flippedY / pythonHeight) * unityHeight;

        // 4. Adjust for the RawImage origin (set origin to bottom-left)
        float adjustedX = scaledX - (unityWidth / 2);
        float adjustedY = scaledY - (unityHeight / 2);

        // 5. Set the new position based on the adjusted values
        spriteTransform.anchoredPosition = new Vector2(adjustedX, adjustedY);
    }

    private void OnApplicationQuit()
    {
        udpClient.Close();
        receiveThread.Abort();
    }
}
