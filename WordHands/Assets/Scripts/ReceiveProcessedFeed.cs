using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Required for RawImage

public class ReceiveProcessedFeed : MonoBehaviour
{
    public int port = 8001;  // Port to listen for incoming data
    private UdpClient udpClient;
    private Texture2D texture;
    private List<byte> dataBuffer = new List<byte>();

    public RawImage rawImage; // Reference to RawImage component

    void Start()
    {
        udpClient = new UdpClient(port);
        texture = new Texture2D(2, 2, TextureFormat.RGB24, false);
        
        if (rawImage != null)
        {
            rawImage.texture = texture; // Set the texture on the RawImage
        }
        else
        {
            Debug.LogError("RawImage component is not assigned.");
        }
    }

    void Update()
    {
        if (udpClient.Available > 0)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
            byte[] receivedData = udpClient.Receive(ref endPoint);

            if (receivedData != null)
            {
                // Append received data to buffer
                dataBuffer.AddRange(receivedData);

                // Try to parse image from the buffered data
                if (TryParseImageFromBuffer(out byte[] imageBytes))
                {
                    texture.LoadImage(imageBytes);

                    if (rawImage != null)
                    {
                        rawImage.texture = texture; // Update the RawImage with the new texture
                    }
                }
            }
        }
    }

    private bool TryParseImageFromBuffer(out byte[] imageBytes)
    {
        // Try to load image from the buffer (considering image might be split into chunks)
        // Here you can implement your logic to handle complete image reconstruction from chunks
        // For simplicity, let's assume that if the buffer length is greater than a threshold, it's a complete image

        const int minimumImageSize = 1024;  // Adjust this based on your needs
        if (dataBuffer.Count >= minimumImageSize)
        {
            imageBytes = dataBuffer.ToArray();
            dataBuffer.Clear();  // Clear buffer after processing
            return true;
        }

        imageBytes = null;
        return false;
    }

    private void OnApplicationQuit()
    {
        udpClient.Close();
    }
}
