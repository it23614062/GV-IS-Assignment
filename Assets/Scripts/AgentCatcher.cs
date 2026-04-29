using UnityEngine;

// ============================================================
// AGENT CATCHER
// ============================================================
// Attach this script to your AI Agent GameObject.
// When the agent's collider touches the player, the player dies.
//
// HOW TO SET IT UP IN UNITY:
//   1. Select your Agent GameObject in the Hierarchy
//   2. Make sure it has a Collider component (Capsule Collider works great)
//      - The "Is Trigger" box should be CHECKED on this collider
//      - If "Is Trigger" is unchecked, it will physically push the player
//        instead of detecting overlap. Either can work, but trigger is cleaner.
//   3. Drag this script onto the Agent GameObject
//
// NOTE: If your Agent has a child object for the visual body,
//       put the script and collider on the PARENT (root) object.
// ============================================================

public class AgentCatcher : MonoBehaviour
{
    [Header("--- SETTINGS ---")]
    [Tooltip("How close the agent must get before the player dies (in Unity units)")]
    public float catchDistance = 1.5f;

    [Tooltip("If checked, uses distance check every frame instead of trigger collider.\nUse this if the trigger collider approach isn't working.")]
    public bool useDistanceCheck = false;

    // Reference to the player - we find it automatically
    private Transform playerTransform;

    void Start()
    {
        // Find the player automatically using the "Player" tag
        // (Remember: you MUST set the player's tag to "Player" in Unity)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogError("AgentCatcher: No GameObject with tag 'Player' found! " +
                           "Please set your player's Tag to 'Player' in the Inspector.");
        }
    }

    void Update()
    {
        // --- Optional: Distance-based catch detection ---
        // Use this if the collider trigger approach isn't working for you
        if (useDistanceCheck && playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            if (distanceToPlayer <= catchDistance)
            {
                Debug.Log("Agent caught the player! (distance check)");
                GameManager.Instance.PlayerDied();
            }
        }
    }

    // OnTriggerEnter fires when the Agent's trigger collider
    // touches another collider.
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Agent caught the player! (trigger)");
            GameManager.Instance.PlayerDied();
        }
    }

    // Show the catch radius as a sphere in the editor
    // ============================================================
    private void OnDrawGizmos()
    {
        if (useDistanceCheck)
        {
            Gizmos.color = new Color(1f, 0.3f, 0f, 0.5f);
            Gizmos.DrawSphere(transform.position, catchDistance);
        }
    }
}