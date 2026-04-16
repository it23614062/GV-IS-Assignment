using UnityEngine;

// ============================================================
// DEATH ZONE (Fall Off the Edge)
// ============================================================
// This script goes on a large INVISIBLE plane placed BELOW
// your game platforms. When the player falls through and
// hits this, they die.
//
// HOW TO SET IT UP IN UNITY:
//   1. Create a 3D Plane (GameObject > 3D Object > Plane)
//   2. Place it BELOW your main game platform
//      (e.g. if your game is at Y=0, put this at Y=-10)
//   3. Scale it very large (Scale: 50, 1, 50) to catch any fall
//   4. In the Mesh Renderer component, UNCHECK it to make it invisible
//   5. In the Mesh Collider, CHECK "Is Trigger"
//      OR: Add a Box Collider, check "Is Trigger", delete Mesh Collider
//   6. Drag this script onto it
// ============================================================

public class DeathZone : MonoBehaviour
{
    // ============================================================
    // OnTriggerEnter is called when something falls onto this plane.
    // ============================================================
    private void OnTriggerEnter(Collider other)
    {
        // Only care about the Player
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player fell off! Triggering death...");
            GameManager.Instance.PlayerDied();
        }
    }

    // ============================================================
    // Show a red zone in the editor so you can see it
    // ============================================================
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, new Vector3(1f, 0.05f, 1f));
        Gizmos.color = new Color(1f, 0f, 0f, 0.8f);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(1f, 0.05f, 1f));
    }
}