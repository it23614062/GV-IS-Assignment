using UnityEngine;

public class Coin : MonoBehaviour
{
    [Tooltip("How many points this coin is worth")]
    public int coinValue = 1;

    // OnTriggerEnter is called when another object enters this object's collider
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that touched the coin has the "Player" tag
        if (other.CompareTag("Player"))
        {
            // Add to the score
            ScoreManager.instance.AddScore(coinValue);

            // Destroy the coin game object so it disappears
            Destroy(gameObject);
        }
    }
}