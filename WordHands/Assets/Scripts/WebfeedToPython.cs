using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class WebfeedToPython : MonoBehaviour
{
    private WebCamTexture webcamTexture;
    private UdpClient udpClient;
    private int sendPort = 8000;
    private string serverIP = "127.0.0.1";  // Replace with Python server's IP if needed

    void Start()
    {
        // Initialize the webcam texture
        webcamTexture = new WebCamTexture();
        webcamTexture.Play();

        // Initialize UDP client
        udpClient = new UdpClient();
    }

    void Update()
    {
        if (webcamTexture.didUpdateThisFrame)
        {
            // Capture the current frame
            Texture2D texture = new Texture2D(webcamTexture.width, webcamTexture.height);
            texture.SetPixels(webcamTexture.GetPixels());
            texture.Apply();

            // Convert texture to PNG bytes
            byte[] imageBytes = texture.EncodeToJPG(); // Use JPG to reduce data size

            // Send bytes over UDP
            udpClient.Send(imageBytes, imageBytes.Length, serverIP, sendPort);
        }
    }

    void OnApplicationQuit()
    {
        webcamTexture.Stop();
        udpClient.Close();
    }
}
