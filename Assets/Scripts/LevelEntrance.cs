using UnityEngine;

// ============================================================
// LEVEL ENTRANCE TRIGGER
// ============================================================
// Put this script on an INVISIBLE trigger object that sits at
// the entrance to each level zone.
//
// HOW TO SET IT UP IN UNITY:
//   1. Create a 3D Cube (GameObject > 3D Object > Cube)
//   2. SCALE it to be a wide, flat gate/doorway shape
//      (e.g. Scale: 5, 3, 0.5)
//   3. In its Collider component, CHECK the "Is Trigger" box
//   4. In its Mesh Renderer, uncheck it (so it's invisible) 
//      OR use a transparent material
//   5. Drag this script onto it
//   6. Set "This Level Number":
//        - Level 2 entrance → set to 2
//        - Level 3 entrance → set to 3
//   7. ALSO: Create a Cylinder at each level's start for the flag visual.
//      Make the cylinder a CHILD of this trigger object for easy placement.
//
// IMPORTANT NOTE about the Win Trigger at the end of Level 3:
//   Set "This Level Number" to 3 AND check "Is Final Win Trigger"
// ============================================================

public class LevelEntrance : MonoBehaviour
{
    [Header("--- SETTINGS ---")]
    [Tooltip("What level does the player enter when they walk through here?\n2 = entrance to level 2\n3 = entrance to level 3")]
    public int thisLevelNumber = 2;

    [Tooltip("Check this ONLY on the trigger at the very end of Level 3 (the winning zone)")]
    public bool isFinalWinTrigger = false;

    // ============================================================
    // OnTriggerEnter is called automatically by Unity when
    // another collider ENTERS this trigger zone.
    // 'other' = the thing that just entered.
    // ============================================================
    private void OnTriggerEnter(Collider other)
    {
        // Check if it's the Player that entered (using the "Player" tag)
        // You must set the player's Tag to "Player" in Unity!
        if (other.CompareTag("Player"))
        {
            if (isFinalWinTrigger)
            {
                // Tell the GameManager the player won!
                GameManager.Instance.PlayerWon();
            }
            else
            {
                // Tell the GameManager which level we just entered
                GameManager.Instance.PlayerEnteredLevel(thisLevelNumber);
            }
        }
    }

    // ============================================================
    // OnDrawGizmos draws a visible colored box in the Scene view
    // so you can see where your triggers are (only visible to you
    // in the editor, never in the game).
    // ============================================================
    private void OnDrawGizmos()
    {
        if (isFinalWinTrigger)
            Gizmos.color = new Color(1f, 0.8f, 0f, 0.4f); // Gold for win zone
        else
            Gizmos.color = new Color(0f, 1f, 0f, 0.4f); // Green for level entrance

        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, Vector3.one);
        Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 1f);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
}