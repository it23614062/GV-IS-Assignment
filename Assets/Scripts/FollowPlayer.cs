using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [Header("Target Settings")]
    public GameObject player;
    public Vector3 offset = new Vector3(0, 5, -7);

    [Header("Camera Feel")]
    public float smoothSpeed = 10f; // Controls how quickly the camera catches up

    // Use LateUpdate for cameras instead of Update
    void LateUpdate()
    {
        if (player == null) return;

        // 1. Calculate the new position based on the player's rotation
        // TransformPoint converts your offset into the player's local space
        Vector3 targetPosition = player.transform.TransformPoint(offset);

        // 2. Move the camera to that position smoothly
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

        // 3. Force the camera to look directly at the player
        transform.LookAt(player.transform.position + Vector3.up * 1.5f);
    }
}