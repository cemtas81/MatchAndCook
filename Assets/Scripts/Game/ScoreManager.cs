using UnityEngine;

/// <summary>
/// Manages score tracking, high scores, and score-related functionality.
/// Provides simple score persistence for mobile gaming sessions.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    [Header("Score Settings")]
    [SerializeField] private bool saveHighScore = true;
    
    // Score tracking
    private int currentScore = 0;
    private int highScore = 0;
    private int sessionBestScore = 0;
    
    // Score breakdown
    private int totalTilesCleared = 0;
    private int totalCombos = 0;
    private int longestCombo = 0;
    
    // Events
    public System.Action<int> OnScoreUpdated;
    public System.Action<int> OnHighScoreBeaten;
    
    // Properties
    public int CurrentScore => currentScore;
    public int HighScore => highScore;
    public int SessionBestScore => sessionBestScore;
    public int TotalTilesCleared => totalTilesCleared;
    public int TotalCombos => totalCombos;
    public int LongestCombo => longestCombo;
    
    void Start()
    {
        LoadHighScore();
    }
    
    /// <summary>
    /// Update the current score
    /// </summary>
    public void UpdateScore(int newScore)
    {
        currentScore = newScore;
        OnScoreUpdated?.Invoke(currentScore);
        
        // Check for session best
        if (currentScore > sessionBestScore)
        {
            sessionBestScore = currentScore;
        }
        
        // Check for high score
        if (currentScore > highScore)
        {
            highScore = currentScore;
            OnHighScoreBeaten?.Invoke(highScore);
            
            if (saveHighScore)
            {
                SaveHighScore();
            }
            
            Debug.Log($"NEW HIGH SCORE: {highScore}!");
        }
    }
    
    /// <summary>
    /// Add points to the current score
    /// </summary>
    public void AddScore(int points)
    {
        UpdateScore(currentScore + points);
    }
    
    /// <summary>
    /// Record tiles cleared for statistics
    /// </summary>
    public void RecordTilesCleared(int tilesCleared)
    {
        totalTilesCleared += tilesCleared;
    }
    
    /// <summary>
    /// Record a combo for statistics
    /// </summary>
    public void RecordCombo(int comboLength)
    {
        totalCombos++;
        
        if (comboLength > longestCombo)
        {
            longestCombo = comboLength;
        }
    }
    
    /// <summary>
    /// Reset current session scores
    /// </summary>
    public void ResetCurrentScore()
    {
        currentScore = 0;
        totalTilesCleared = 0;
        totalCombos = 0;
        longestCombo = 0;
        
        OnScoreUpdated?.Invoke(currentScore);
    }
    
    /// <summary>
    /// Get formatted score string
    /// </summary>
    public string GetFormattedScore()
    {
        return FormatScore(currentScore);
    }
    
    /// <summary>
    /// Get formatted high score string
    /// </summary>
    public string GetFormattedHighScore()
    {
        return FormatScore(highScore);
    }
    
    /// <summary>
    /// Format score with proper number formatting
    /// </summary>
    private string FormatScore(int score)
    {
        if (score >= 1000000)
        {
            return $"{(score / 1000000f):F1}M";
        }
        else if (score >= 1000)
        {
            return $"{(score / 1000f):F1}K";
        }
        else
        {
            return score.ToString();
        }
    }
    
    /// <summary>
    /// Calculate score multiplier based on performance
    /// </summary>
    public float GetScoreMultiplier(int comboLength, int tilesCleared)
    {
        float baseMultiplier = 1f;
        
        // Combo bonus
        if (comboLength > 1)
        {
            baseMultiplier += (comboLength - 1) * 0.5f;
        }
        
        // Large clear bonus
        if (tilesCleared >= 5)
        {
            baseMultiplier += (tilesCleared - 4) * 0.2f;
        }
        
        return Mathf.Min(baseMultiplier, 5f); // Cap at 5x multiplier
    }
    
    /// <summary>
    /// Get game statistics summary
    /// </summary>
    public string GetStatsString()
    {
        return $"Current: {GetFormattedScore()} | Best: {GetFormattedHighScore()}\n" +
               $"Tiles Cleared: {totalTilesCleared} | Combos: {totalCombos} | Longest: {longestCombo}";
    }
    
    /// <summary>
    /// Save high score to PlayerPrefs
    /// </summary>
    private void SaveHighScore()
    {
        PlayerPrefs.SetInt("MatchAndCook_HighScore", highScore);
        PlayerPrefs.Save();
        Debug.Log($"High score saved: {highScore}");
    }
    
    /// <summary>
    /// Load high score from PlayerPrefs
    /// </summary>
    private void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt("MatchAndCook_HighScore", 0);
        Debug.Log($"High score loaded: {highScore}");
    }
    
    /// <summary>
    /// Reset high score (for testing or player request)
    /// </summary>
    public void ResetHighScore()
    {
        highScore = 0;
        PlayerPrefs.DeleteKey("MatchAndCook_HighScore");
        PlayerPrefs.Save();
        Debug.Log("High score reset");
    }
    
    /// <summary>
    /// Check if current score is a new record
    /// </summary>
    public bool IsNewHighScore()
    {
        return currentScore > 0 && currentScore == highScore;
    }
    
    /// <summary>
    /// Get percentage progress towards next milestone
    /// </summary>
    public float GetMilestoneProgress(int milestone)
    {
        return Mathf.Clamp01((float)currentScore / milestone);
    }
    
    /// <summary>
    /// Award bonus points for special achievements
    /// </summary>
    public void AwardBonus(int bonusPoints, string reason)
    {
        AddScore(bonusPoints);
        Debug.Log($"Bonus awarded: +{bonusPoints} points for {reason}");
    }
}