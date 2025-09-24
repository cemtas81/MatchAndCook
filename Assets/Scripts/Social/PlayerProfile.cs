using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a player profile with statistics, achievements, and social information.
/// Handles local and online player data management.
/// </summary>
[System.Serializable]
public class PlayerProfile
{
    [Header("Basic Info")]
    public string playerName;
    public string playerId;
    public Sprite profilePicture;
    public int playerLevel;
    public int totalExperience;
    
    [Header("Game Statistics")]
    public int totalScore;
    public int highScore;
    public int totalStars;
    public int levelsCompleted;
    public int recipesCompleted;
    public int customersServed;
    public int powerUpsUsed;
    
    [Header("Time Statistics")]
    public float totalPlayTime; // in seconds
    public DateTime lastPlayedDate;
    public DateTime accountCreatedDate;
    public int daysPlayed;
    
    [Header("Social")]
    public List<string> friendIds = new List<string>();
    public bool shareScores = true;
    public bool receiveNotifications = true;
    
    // Weekly/Monthly stats for leaderboards
    [Header("Weekly Stats")]
    public int weeklyScore;
    public int weeklyStars;
    public DateTime weekStartDate;
    
    [Header("Monthly Stats")]
    public int monthlyScore;
    public int monthlyStars;
    public DateTime monthStartDate;
    
    /// <summary>
    /// Default constructor
    /// </summary>
    public PlayerProfile()
    {
        playerName = "Player";
        playerId = System.Guid.NewGuid().ToString();
        accountCreatedDate = DateTime.Now;
        lastPlayedDate = DateTime.Now;
        weekStartDate = GetWeekStart(DateTime.Now);
        monthStartDate = GetMonthStart(DateTime.Now);
    }
    
    /// <summary>
    /// Constructor with player name
    /// </summary>
    public PlayerProfile(string name) : this()
    {
        playerName = name;
    }
    
    /// <summary>
    /// Update player statistics
    /// </summary>
    public void UpdateStats(int scoreGained, int starsGained, int recipesCount, int customersCount)
    {
        totalScore += scoreGained;
        totalStars += starsGained;
        recipesCompleted += recipesCount;
        customersServed += customersCount;
        
        if (scoreGained > highScore)
        {
            highScore = scoreGained;
        }
        
        UpdateWeeklyStats(scoreGained, starsGained);
        UpdateMonthlyStats(scoreGained, starsGained);
        UpdateExperience(scoreGained);
        
        lastPlayedDate = DateTime.Now;
    }
    
    /// <summary>
    /// Update weekly statistics
    /// </summary>
    private void UpdateWeeklyStats(int scoreGained, int starsGained)
    {
        DateTime currentWeekStart = GetWeekStart(DateTime.Now);
        
        // Reset weekly stats if new week
        if (currentWeekStart > weekStartDate)
        {
            weeklyScore = 0;
            weeklyStars = 0;
            weekStartDate = currentWeekStart;
        }
        
        weeklyScore += scoreGained;
        weeklyStars += starsGained;
    }
    
    /// <summary>
    /// Update monthly statistics
    /// </summary>
    private void UpdateMonthlyStats(int scoreGained, int starsGained)
    {
        DateTime currentMonthStart = GetMonthStart(DateTime.Now);
        
        // Reset monthly stats if new month
        if (currentMonthStart > monthStartDate)
        {
            monthlyScore = 0;
            monthlyStars = 0;
            monthStartDate = currentMonthStart;
        }
        
        monthlyScore += scoreGained;
        monthlyStars += starsGained;
    }
    
    /// <summary>
    /// Update player experience and level
    /// </summary>
    private void UpdateExperience(int scoreGained)
    {
        int experienceGained = scoreGained / 10; // 1 XP per 10 points
        totalExperience += experienceGained;
        
        // Calculate level based on experience (simple formula)
        int newLevel = Mathf.FloorToInt(Mathf.Sqrt(totalExperience / 100f)) + 1;
        
        if (newLevel > playerLevel)
        {
            playerLevel = newLevel;
            Debug.Log($"Level up! Now level {playerLevel}");
        }
    }
    
    /// <summary>
    /// Add play time
    /// </summary>
    public void AddPlayTime(float sessionTime)
    {
        totalPlayTime += sessionTime;
        
        // Update days played
        if (lastPlayedDate.Date != DateTime.Now.Date)
        {
            daysPlayed++;
        }
        
        lastPlayedDate = DateTime.Now;
    }
    
    /// <summary>
    /// Add a friend
    /// </summary>
    public bool AddFriend(string friendId)
    {
        if (string.IsNullOrEmpty(friendId) || friendIds.Contains(friendId))
            return false;
        
        friendIds.Add(friendId);
        return true;
    }
    
    /// <summary>
    /// Remove a friend
    /// </summary>
    public bool RemoveFriend(string friendId)
    {
        return friendIds.Remove(friendId);
    }
    
    /// <summary>
    /// Get formatted play time
    /// </summary>
    public string GetFormattedPlayTime()
    {
        TimeSpan time = TimeSpan.FromSeconds(totalPlayTime);
        
        if (time.TotalDays >= 1)
            return $"{time.Days}d {time.Hours}h {time.Minutes}m";
        else if (time.TotalHours >= 1)
            return $"{time.Hours}h {time.Minutes}m";
        else
            return $"{time.Minutes}m {time.Seconds}s";
    }
    
    /// <summary>
    /// Get player rank based on total score
    /// </summary>
    public string GetPlayerRank()
    {
        if (totalScore >= 100000) return "Master Chef";
        if (totalScore >= 50000) return "Expert Cook";
        if (totalScore >= 25000) return "Skilled Cook";
        if (totalScore >= 10000) return "Junior Cook";
        if (totalScore >= 5000) return "Apprentice";
        return "Novice";
    }
    
    /// <summary>
    /// Get experience needed for next level
    /// </summary>
    public int GetExperienceForNextLevel()
    {
        int nextLevelRequirement = (playerLevel * playerLevel) * 100;
        return Mathf.Max(0, nextLevelRequirement - totalExperience);
    }
    
    /// <summary>
    /// Get progress to next level (0-1)
    /// </summary>
    public float GetLevelProgress()
    {
        int currentLevelRequirement = ((playerLevel - 1) * (playerLevel - 1)) * 100;
        int nextLevelRequirement = (playerLevel * playerLevel) * 100;
        int progressInCurrentLevel = totalExperience - currentLevelRequirement;
        int experienceNeededForLevel = nextLevelRequirement - currentLevelRequirement;
        
        return Mathf.Clamp01((float)progressInCurrentLevel / experienceNeededForLevel);
    }
    
    /// <summary>
    /// Get statistics summary
    /// </summary>
    public Dictionary<string, object> GetStatsSummary()
    {
        return new Dictionary<string, object>
        {
            {"Total Score", totalScore},
            {"High Score", highScore},
            {"Total Stars", totalStars},
            {"Levels Completed", levelsCompleted},
            {"Recipes Completed", recipesCompleted},
            {"Customers Served", customersServed},
            {"Play Time", GetFormattedPlayTime()},
            {"Player Level", playerLevel},
            {"Player Rank", GetPlayerRank()},
            {"Days Played", daysPlayed}
        };
    }
    
    /// <summary>
    /// Save profile to PlayerPrefs
    /// </summary>
    public void SaveToPlayerPrefs()
    {
        string profileJson = JsonUtility.ToJson(this);
        PlayerPrefs.SetString("PlayerProfile", profileJson);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Load profile from PlayerPrefs
    /// </summary>
    public static PlayerProfile LoadFromPlayerPrefs()
    {
        string profileJson = PlayerPrefs.GetString("PlayerProfile", "");
        
        if (string.IsNullOrEmpty(profileJson))
        {
            return new PlayerProfile();
        }
        
        try
        {
            return JsonUtility.FromJson<PlayerProfile>(profileJson);
        }
        catch
        {
            Debug.LogWarning("Failed to load player profile, creating new one");
            return new PlayerProfile();
        }
    }
    
    /// <summary>
    /// Get start of week for a given date
    /// </summary>
    private DateTime GetWeekStart(DateTime date)
    {
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-1 * diff).Date;
    }
    
    /// <summary>
    /// Get start of month for a given date
    /// </summary>
    private DateTime GetMonthStart(DateTime date)
    {
        return new DateTime(date.Year, date.Month, 1);
    }
}