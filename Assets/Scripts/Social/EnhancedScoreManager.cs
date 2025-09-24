using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Enhanced score manager with social features, leaderboards, and player profiles.
/// Extends the basic ScoreManager with online/local score persistence and comparison.
/// </summary>
public class EnhancedScoreManager : MonoBehaviour
{
    [Header("Score Settings")]
    [SerializeField] private bool enableOnlineScores = false;
    [SerializeField] private int maxLeaderboardEntries = 100;
    [SerializeField] private float scoreSubmissionCooldown = 1f;
    
    [Header("Local Leaderboards")]
    [SerializeField] private int maxLocalScores = 50;
    
    // Current session data
    private PlayerProfile currentPlayer;
    private int sessionScore = 0;
    private int sessionStars = 0;
    private int sessionRecipes = 0;
    private int sessionCustomers = 0;
    private float sessionStartTime;
    private float lastScoreSubmission = 0f;
    
    // Leaderboard data
    private List<LeaderboardEntry> allTimeLeaderboard = new List<LeaderboardEntry>();
    private List<LeaderboardEntry> weeklyLeaderboard = new List<LeaderboardEntry>();
    private List<LeaderboardEntry> monthlyLeaderboard = new List<LeaderboardEntry>();
    
    // Events
    public System.Action<PlayerProfile> OnPlayerProfileUpdated;
    public System.Action<LeaderboardEntry> OnNewHighScore;
    public System.Action<List<LeaderboardEntry>, LeaderboardType> OnLeaderboardUpdated;
    
    // References
    private ScoreManager originalScoreManager;
    
    public enum LeaderboardType
    {
        AllTime,
        Weekly,
        Monthly
    }
    
    // Properties
    public PlayerProfile CurrentPlayer => currentPlayer;
    public int SessionScore => sessionScore;
    
    void Start()
    {
        InitializeEnhancedScoreManager();
    }
    
    void Update()
    {
        // Track session time
        if (currentPlayer != null)
        {
            float sessionTime = Time.time - sessionStartTime;
            currentPlayer.AddPlayTime(Time.deltaTime);
        }
    }
    
    /// <summary>
    /// Initialize enhanced score manager
    /// </summary>
    private void InitializeEnhancedScoreManager()
    {
        originalScoreManager = GetComponent<ScoreManager>();
        
        // Load or create player profile
        currentPlayer = PlayerProfile.LoadFromPlayerPrefs();
        sessionStartTime = Time.time;
        
        // Load leaderboards
        LoadLeaderboards();
        
        // Subscribe to game events
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.OnScoreChanged += OnScoreChanged;
            gameManager.OnGameStateChanged += OnGameStateChanged;
        }
        
        RecipeManager recipeManager = FindFirstObjectByType<RecipeManager>();
        if (recipeManager != null)
        {
            recipeManager.OnRecipeCompleted += OnRecipeCompleted;
        }
        
        CustomerManager customerManager = FindFirstObjectByType<CustomerManager>();
        if (customerManager != null)
        {
            customerManager.OnOrderCompleted += OnCustomerOrderCompleted;
        }
        
        Debug.Log($"Enhanced Score Manager initialized for player: {currentPlayer.playerName}");
    }
    
    /// <summary>
    /// Handle score changes during gameplay
    /// </summary>
    private void OnScoreChanged(int newScore)
    {
        sessionScore = newScore;
    }
    
    /// <summary>
    /// Handle game state changes
    /// </summary>
    private void OnGameStateChanged(GameManager.GameState newState)
    {
        if (newState == GameManager.GameState.Won || newState == GameManager.GameState.Lost)
        {
            EndSession(newState == GameManager.GameState.Won);
        }
    }
    
    /// <summary>
    /// Handle recipe completion
    /// </summary>
    private void OnRecipeCompleted(Recipe recipe, int reward)
    {
        sessionRecipes++;
    }
    
    /// <summary>
    /// Handle customer order completion
    /// </summary>
    private void OnCustomerOrderCompleted(ActiveCustomerOrder order, bool success)
    {
        if (success)
        {
            sessionCustomers++;
        }
    }
    
    /// <summary>
    /// End the current session and update player profile
    /// </summary>
    private void EndSession(bool won)
    {
        if (currentPlayer == null) return;
        
        // Calculate session stars (simplified - would be more complex in real game)
        sessionStars = won ? Mathf.Max(1, sessionScore / 500) : 0;
        
        // Update player profile
        currentPlayer.UpdateStats(sessionScore, sessionStars, sessionRecipes, sessionCustomers);
        currentPlayer.levelsCompleted += won ? 1 : 0;
        
        // Check for new high score
        bool isNewHighScore = sessionScore > currentPlayer.highScore;
        if (isNewHighScore)
        {
            currentPlayer.highScore = sessionScore;
        }
        
        // Save profile
        currentPlayer.SaveToPlayerPrefs();
        
        // Submit score to leaderboards
        SubmitScore(sessionScore, sessionStars);
        
        OnPlayerProfileUpdated?.Invoke(currentPlayer);
        
        if (isNewHighScore)
        {
            OnNewHighScore?.Invoke(new LeaderboardEntry(currentPlayer, sessionScore));
        }
        
        Debug.Log($"Session ended. Score: {sessionScore}, Stars: {sessionStars}, New High Score: {isNewHighScore}");
    }
    
    /// <summary>
    /// Submit score to leaderboards
    /// </summary>
    public void SubmitScore(int score, int stars)
    {
        if (Time.time - lastScoreSubmission < scoreSubmissionCooldown) return;
        
        lastScoreSubmission = Time.time;
        
        LeaderboardEntry entry = new LeaderboardEntry(currentPlayer, score, stars);
        
        // Add to all-time leaderboard
        AddToLeaderboard(allTimeLeaderboard, entry, LeaderboardType.AllTime);
        
        // Add to weekly leaderboard
        AddToLeaderboard(weeklyLeaderboard, entry, LeaderboardType.Weekly);
        
        // Add to monthly leaderboard
        AddToLeaderboard(monthlyLeaderboard, entry, LeaderboardType.Monthly);
        
        // Save leaderboards
        SaveLeaderboards();
        
        if (enableOnlineScores)
        {
            SubmitScoreOnline(entry);
        }
    }
    
    /// <summary>
    /// Add entry to a specific leaderboard
    /// </summary>
    private void AddToLeaderboard(List<LeaderboardEntry> leaderboard, LeaderboardEntry entry, LeaderboardType type)
    {
        // Remove existing entry for this player
        leaderboard.RemoveAll(e => e.playerId == entry.playerId);
        
        // Add new entry
        leaderboard.Add(entry);
        
        // Sort by score (descending)
        leaderboard.Sort((a, b) => b.score.CompareTo(a.score));
        
        // Limit entries
        if (leaderboard.Count > maxLeaderboardEntries)
        {
            leaderboard.RemoveRange(maxLeaderboardEntries, leaderboard.Count - maxLeaderboardEntries);
        }
        
        OnLeaderboardUpdated?.Invoke(leaderboard, type);
    }
    
    /// <summary>
    /// Submit score to online leaderboard (placeholder)
    /// </summary>
    private void SubmitScoreOnline(LeaderboardEntry entry)
    {
        // This would integrate with an online leaderboard service
        // like Google Play Games, GameCenter, or a custom backend
        Debug.Log($"Submitting score online: {entry.playerName} - {entry.score}");
    }
    
    /// <summary>
    /// Get leaderboard entries
    /// </summary>
    public List<LeaderboardEntry> GetLeaderboard(LeaderboardType type, int maxEntries = 10)
    {
        List<LeaderboardEntry> sourceLeaderboard;
        
        switch (type)
        {
            case LeaderboardType.Weekly:
                sourceLeaderboard = weeklyLeaderboard;
                break;
            case LeaderboardType.Monthly:
                sourceLeaderboard = monthlyLeaderboard;
                break;
            default:
                sourceLeaderboard = allTimeLeaderboard;
                break;
        }
        
        return sourceLeaderboard.Take(maxEntries).ToList();
    }
    
    /// <summary>
    /// Get player rank in leaderboard
    /// </summary>
    public int GetPlayerRank(LeaderboardType type)
    {
        List<LeaderboardEntry> leaderboard = GetLeaderboard(type, maxLeaderboardEntries);
        
        for (int i = 0; i < leaderboard.Count; i++)
        {
            if (leaderboard[i].playerId == currentPlayer.playerId)
            {
                return i + 1; // Rank is 1-based
            }
        }
        
        return -1; // Not found
    }
    
    /// <summary>
    /// Get friends' scores (placeholder - would need friend system integration)
    /// </summary>
    public List<LeaderboardEntry> GetFriendsScores(LeaderboardType type)
    {
        List<LeaderboardEntry> friendsScores = new List<LeaderboardEntry>();
        List<LeaderboardEntry> leaderboard = GetLeaderboard(type, maxLeaderboardEntries);
        
        foreach (var entry in leaderboard)
        {
            if (currentPlayer.friendIds.Contains(entry.playerId))
            {
                friendsScores.Add(entry);
            }
        }
        
        return friendsScores;
    }
    
    /// <summary>
    /// Load leaderboards from PlayerPrefs
    /// </summary>
    private void LoadLeaderboards()
    {
        string allTimeJson = PlayerPrefs.GetString("AllTimeLeaderboard", "");
        string weeklyJson = PlayerPrefs.GetString("WeeklyLeaderboard", "");
        string monthlyJson = PlayerPrefs.GetString("MonthlyLeaderboard", "");
        
        if (!string.IsNullOrEmpty(allTimeJson))
        {
            LeaderboardData data = JsonUtility.FromJson<LeaderboardData>(allTimeJson);
            allTimeLeaderboard = data.entries ?? new List<LeaderboardEntry>();
        }
        
        if (!string.IsNullOrEmpty(weeklyJson))
        {
            LeaderboardData data = JsonUtility.FromJson<LeaderboardData>(weeklyJson);
            weeklyLeaderboard = data.entries ?? new List<LeaderboardEntry>();
        }
        
        if (!string.IsNullOrEmpty(monthlyJson))
        {
            LeaderboardData data = JsonUtility.FromJson<LeaderboardData>(monthlyJson);
            monthlyLeaderboard = data.entries ?? new List<LeaderboardEntry>();
        }
    }
    
    /// <summary>
    /// Save leaderboards to PlayerPrefs
    /// </summary>
    private void SaveLeaderboards()
    {
        string allTimeJson = JsonUtility.ToJson(new LeaderboardData { entries = allTimeLeaderboard });
        string weeklyJson = JsonUtility.ToJson(new LeaderboardData { entries = weeklyLeaderboard });
        string monthlyJson = JsonUtility.ToJson(new LeaderboardData { entries = monthlyLeaderboard });
        
        PlayerPrefs.SetString("AllTimeLeaderboard", allTimeJson);
        PlayerPrefs.SetString("WeeklyLeaderboard", weeklyJson);
        PlayerPrefs.SetString("MonthlyLeaderboard", monthlyJson);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Clear all leaderboards (for testing)
    /// </summary>
    public void ClearLeaderboards()
    {
        allTimeLeaderboard.Clear();
        weeklyLeaderboard.Clear();
        monthlyLeaderboard.Clear();
        SaveLeaderboards();
    }
    
    /// <summary>
    /// Update player name
    /// </summary>
    public void UpdatePlayerName(string newName)
    {
        if (currentPlayer != null && !string.IsNullOrEmpty(newName))
        {
            currentPlayer.playerName = newName;
            currentPlayer.SaveToPlayerPrefs();
            OnPlayerProfileUpdated?.Invoke(currentPlayer);
        }
    }
    
    /// <summary>
    /// Get player statistics summary
    /// </summary>
    public Dictionary<string, object> GetPlayerStats()
    {
        return currentPlayer?.GetStatsSummary() ?? new Dictionary<string, object>();
    }
    
    void OnDestroy()
    {
        // Save any pending data
        if (currentPlayer != null)
        {
            currentPlayer.SaveToPlayerPrefs();
        }
        
        SaveLeaderboards();
    }
}

/// <summary>
/// Represents a leaderboard entry
/// </summary>
[System.Serializable]
public class LeaderboardEntry
{
    public string playerId;
    public string playerName;
    public int score;
    public int stars;
    public string rank;
    public System.DateTime submitTime;
    
    public LeaderboardEntry() { }
    
    public LeaderboardEntry(PlayerProfile player, int playerScore, int playerStars = 0)
    {
        playerId = player.playerId;
        playerName = player.playerName;
        score = playerScore;
        stars = playerStars;
        rank = player.GetPlayerRank();
        submitTime = System.DateTime.Now;
    }
}

/// <summary>
/// Container for leaderboard data serialization
/// </summary>
[System.Serializable]
public class LeaderboardData
{
    public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
}