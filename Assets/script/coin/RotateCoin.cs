using UnityEngine;

public class ObjectRotator : MonoBehaviour
{
    [Tooltip("Speed of rotation in degrees per second.")]
    public float rotationSpeed = 90f;

    [Tooltip("The axis to rotate around. (0, 1, 0) is the Y axis.")]
    public Vector3 rotationAxis = Vector3.up;

    void Update()
    {
        // transform.Rotate takes a Vector3. 
        // Multiplying by Time.deltaTime ensures the rotation is smooth and independent of framerate.
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
    }
}