using UnityEngine;
using TMPro; // Required to use TextMeshPro

public class ScoreManager : MonoBehaviour
{
    // This allows other scripts to access this one easily
    public static ScoreManager instance;

    [Tooltip("Drag your ScoreText UI element here")]
    public TextMeshProUGUI scoreText;

    private int score = 0;

    void Awake()
    {
        // Set up the singleton instance
        if (instance == null)
        {
            instance = this;
        }
    }

    // The coin script will call this function when touched
    public void AddScore(int amount)
    {
        score += amount;
        scoreText.text = "Coins: " + score.ToString() + "/5";
    }
}