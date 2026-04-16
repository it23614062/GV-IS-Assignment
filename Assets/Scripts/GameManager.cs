using UnityEngine;
using TMPro;

// ============================================================
// GAME MANAGER
// ============================================================
// Think of this as the "brain" of the whole game.
// It keeps track of:
//   - Which level the player is currently on (1, 2, or 3)
//   - Where the player should respawn when they die
//   - Triggering death and respawn
//
// HOW TO SET IT UP IN UNITY:
//   1. Create an empty GameObject in your scene.
//   2. Name it "GameManager".
//   3. Drag this script onto it.
//   4. Fill in all the slots you see in the Inspector.
// ============================================================

public class GameManager : MonoBehaviour
{
    // --- SINGLETON PATTERN ---
    // This lets ANY other script call GameManager.Instance.PlayerDied()
    // from anywhere without needing a reference. Very handy!
    public static GameManager Instance;

    [Header("--- SPAWN POINTS (one per level) ---")]
    [Tooltip("Where the player respawns if they die in Level 1")]
    public Transform level1SpawnPoint;
    [Tooltip("Where the player respawns if they die in Level 2")]
    public Transform level2SpawnPoint;
    [Tooltip("Where the player respawns if they die in Level 3")]
    public Transform level3SpawnPoint;

    [Header("--- SCENE OBJECTS ---")]
    [Tooltip("Drag your Player GameObject here")]
    public GameObject player;
    [Tooltip("Drag your AI Agent GameObject here")]
    public GameObject agent;

    [Header("--- UI PANELS ---")]
    [Tooltip("A Text object that shows 'Level 1', 'Level 2', etc.")]
    public TextMeshProUGUI levelDisplayText;
    [Tooltip("A Panel that appears when you die (can be null)")]
    public GameObject deathPanel;
    [Tooltip("A Panel that appears when you win (can be null)")]
    public GameObject winPanel;

    // --------------------------------------------------------
    // PRIVATE VARIABLES
    // These are the "memory" of the GameManager.
    // --------------------------------------------------------
    private int currentLevel = 1;       // Tracks which level we're on
    private bool isRespawning = false;  // Prevents dying twice at the same time
    private Vector3 agentStartPosition; // We remember where the agent starts

    // ============================================================
    // AWAKE is called before Start. Perfect for setting up the Singleton.
    // ============================================================
    void Awake()
    {
        // Set up the Singleton so other scripts can find this GameManager
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // If a duplicate exists, destroy it
        }
    }

    // ============================================================
    // START is called once when the game begins.
    // ============================================================
    void Start()
    {
        // Remember where the agent starts so we can reset it on death
        if (agent != null)
        {
            agentStartPosition = agent.transform.position;
        }

        // Hide the death/win panels at the start
        if (deathPanel != null) deathPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);

        // Show the level text
        UpdateLevelUI();
    }

    // ============================================================
    // PUBLIC: Called by LevelEntrance.cs when player walks
    // into a new level zone.
    // ============================================================
    public void PlayerEnteredLevel(int levelNumber)
    {
        // Only update if this is a NEW, higher level
        // (prevents going backwards accidentally)
        if (levelNumber > currentLevel)
        {
            currentLevel = levelNumber;
            Debug.Log("Now in Level " + currentLevel);
            UpdateLevelUI();
        }
    }

    // ============================================================
    // PUBLIC: Called by AgentCatcher.cs or DeathZone.cs
    // when the player should die.
    // ============================================================
    public void PlayerDied()
    {
        // isRespawning check prevents this from being called
        // multiple times in the same frame (e.g. agent touches + fall)
        if (isRespawning) return;
        isRespawning = true;

        Debug.Log("Player Died! Respawning at Level " + currentLevel + " spawn...");

        // Show the death panel if you have one
        if (deathPanel != null) deathPanel.SetActive(true);

        // Wait 2 seconds, then call RespawnPlayer
        // Invoke is Unity's built-in "wait then call a function" tool
        Invoke("RespawnPlayer", 2f);
    }

    // ============================================================
    // PUBLIC: Called when the player reaches the end of Level 3.
    // ============================================================
    public void PlayerWon()
    {
        Debug.Log("YOU WIN!");
        if (winPanel != null) winPanel.SetActive(true);

        // Disable player movement so they can't run around on the win screen
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null) pc.enabled = false;
    }

    // ============================================================
    // PRIVATE: The actual respawn logic. Called by Invoke above.
    // ============================================================
    private void RespawnPlayer()
    {
        // Hide the death panel
        if (deathPanel != null) deathPanel.SetActive(false);

        // --- STEP 1: Find the correct spawn point ---
        Transform spawnPoint = GetSpawnPointForCurrentLevel();
        if (spawnPoint == null)
        {
            Debug.LogError("No spawn point found! Did you assign all 3 spawn points in the Inspector?");
            isRespawning = false;
            return;
        }

        // --- STEP 2: Teleport the player ---
        // IMPORTANT: We must disable the CharacterController BEFORE moving the
        // player, otherwise Unity will ignore the position change!
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        player.transform.position = spawnPoint.position;
        if (cc != null) cc.enabled = true;

        // --- STEP 3: Reset the AI agent back to its starting position ---
        ResetAgent();

        // Allow dying again
        isRespawning = false;
    }

    // ============================================================
    // PRIVATE: Returns the right spawn point for the current level.
    // ============================================================
    private Transform GetSpawnPointForCurrentLevel()
    {
        switch (currentLevel)
        {
            case 1: return level1SpawnPoint;
            case 2: return level2SpawnPoint;
            case 3: return level3SpawnPoint;
            default: return level1SpawnPoint;
        }
    }

    // ============================================================
    // PRIVATE: Moves the agent back to where it started.
    // ============================================================
    private void ResetAgent()
    {
        if (agent == null) return;

        // Teleport the agent back to its original position
        agent.transform.position = agentStartPosition;

        // If the agent has a NavMeshAgent component, we need to also
        // reset that (warp = teleport for NavMeshAgent)
        UnityEngine.AI.NavMeshAgent navAgent = agent.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (navAgent != null)
        {
            navAgent.Warp(agentStartPosition);
        }
    }

    // ============================================================
    // PRIVATE: Updates the level text on screen.
    // ============================================================
    private void UpdateLevelUI()
    {
        if (levelDisplayText != null)
        {
            levelDisplayText.text = "Level " + currentLevel;
        }
    }

    // ============================================================
    // GETTER: Other scripts can ask "what level is the player on?"
    // ============================================================
    public int GetCurrentLevel()
    {
        return currentLevel;
    }
}