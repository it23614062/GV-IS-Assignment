using UnityEngine;
using UnityEngine.SceneManagement; // Required for reloading the scene

public class PlayerHitDetector : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Drag the Game Over Panel (containing text and button) here.")]
    public GameObject gameOverPanel;

    [Header("Settings")]
    public string spearTag = "Spear";

    private void Start()
    {
        // Ensure the game over screen is hidden when the game starts
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Make sure game time is running normally
        Time.timeScale = 1f;
    }

    // Use this if your Spear's collider is set to "Is Trigger"
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(spearTag))
        {
            TriggerGameOver();
        }
    }

    // Use this if your Spear uses standard physical collisions
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(spearTag))
        {
            TriggerGameOver();
        }
    }

    private void TriggerGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true); // Show the UI
        }

        Time.timeScale = 0f; // Freeze the game
    }

    // This function will be linked to your Restart Button
    public void RestartGame()
    {
        Time.timeScale = 1f; // Unfreeze time before reloading
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload the current scene
    }
}