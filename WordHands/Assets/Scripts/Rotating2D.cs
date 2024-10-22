using UnityEngine;

public class Rotating2D : MonoBehaviour
{
    public float rotationSpeed; // Speed of rotation

    void Update()
    {
        // Rotate around the Z-axis
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}