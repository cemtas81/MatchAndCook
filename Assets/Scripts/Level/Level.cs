using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a level configuration with objectives, difficulty, and progression settings.
/// Used to create structured gameplay progression with escalating challenges.
/// </summary>
[CreateAssetMenu(fileName = "New Level", menuName = "Match & Cook/Level")]
public class Level : ScriptableObject
{
    [Header("Level Info")]
    public int levelNumber;
    public string levelName;
    [TextArea(2, 4)]
    public string levelDescription;
    public Sprite levelIcon;
    
    [Header("Objectives")]
    public List<Recipe> requiredRecipes = new List<Recipe>();
    public List<CustomerOrder> expectedOrders = new List<CustomerOrder>();
    public int targetScore = 1000;
    public int movesLimit = 30;
    public float timeLimit = 300f; // 5 minutes default
    
    [Header("Difficulty Settings")]
    public LevelDifficulty difficulty = LevelDifficulty.Easy;
    public int maxConcurrentOrders = 1;
    public float orderSpawnRate = 60f; // Seconds between orders
    public bool hasObstacles = false;
    public bool hasTutorial = false;
    
    [Header("Rewards")]
    public int baseStars = 1;
    public int scoreForTwoStars = 1500;
    public int scoreForThreeStars = 2000;
    public int coinReward = 50;
    public List<PowerUpManager.PowerUpType> powerUpRewards = new List<PowerUpManager.PowerUpType>();
    
    [Header("Obstacles")]
    public List<ObstacleType> obstacleTypes = new List<ObstacleType>();
    public float obstacleSpawnChance = 0.1f;
    
    public enum LevelDifficulty
    {
        Tutorial,
        Easy,
        Medium,
        Hard,
        Expert,
        Master
    }
    
    public enum ObstacleType
    {
        Ice,        // Frozen tiles that need multiple hits
        Stone,      // Unmovable blocking tiles
        Honey,      // Tiles that slow down falling
        Chain,      // Chained tiles that move together
        Lock        // Tiles that require keys to unlock
    }
    
    /// <summary>
    /// Calculate stars earned based on score
    /// </summary>
    public int CalculateStarsEarned(int finalScore)
    {
        if (finalScore >= scoreForThreeStars)
            return 3;
        else if (finalScore >= scoreForTwoStars)
            return 2;
        else if (finalScore >= targetScore)
            return 1;
        else
            return 0;
    }
    
    /// <summary>
    /// Check if level objectives are completed
    /// </summary>
    public bool AreObjectivesComplete(int currentScore, int recipesCompleted, int ordersCompleted)
    {
        // Basic completion: reach target score
        bool scoreReached = currentScore >= targetScore;
        
        // Optional: check if minimum recipes/orders are completed
        bool recipesComplete = requiredRecipes.Count == 0 || recipesCompleted >= requiredRecipes.Count;
        bool ordersComplete = expectedOrders.Count == 0 || ordersCompleted >= expectedOrders.Count;
        
        return scoreReached && recipesComplete && ordersComplete;
    }
    
    /// <summary>
    /// Get completion progress as percentage
    /// </summary>
    public float GetCompletionProgress(int currentScore, int recipesCompleted, int ordersCompleted)
    {
        float scoreProgress = Mathf.Clamp01((float)currentScore / targetScore);
        
        float recipeProgress = requiredRecipes.Count > 0 ? 
            Mathf.Clamp01((float)recipesCompleted / requiredRecipes.Count) : 1f;
            
        float orderProgress = expectedOrders.Count > 0 ? 
            Mathf.Clamp01((float)ordersCompleted / expectedOrders.Count) : 1f;
        
        // Weight score progress more heavily
        return (scoreProgress * 0.6f) + (recipeProgress * 0.2f) + (orderProgress * 0.2f);
    }
    
    /// <summary>
    /// Get difficulty multiplier for scoring
    /// </summary>
    public float GetDifficultyMultiplier()
    {
        switch (difficulty)
        {
            case LevelDifficulty.Tutorial: return 0.5f;
            case LevelDifficulty.Easy: return 1f;
            case LevelDifficulty.Medium: return 1.2f;
            case LevelDifficulty.Hard: return 1.5f;
            case LevelDifficulty.Expert: return 2f;
            case LevelDifficulty.Master: return 3f;
            default: return 1f;
        }
    }
    
    /// <summary>
    /// Get recommended power-ups for this level
    /// </summary>
    public List<PowerUpManager.PowerUpType> GetRecommendedPowerUps()
    {
        List<PowerUpManager.PowerUpType> recommended = new List<PowerUpManager.PowerUpType>();
        
        // Recommend power-ups based on level characteristics
        if (hasObstacles)
        {
            recommended.Add(PowerUpManager.PowerUpType.Bomb);
        }
        
        if (maxConcurrentOrders > 1)
        {
            recommended.Add(PowerUpManager.PowerUpType.TimeAdd);
            recommended.Add(PowerUpManager.PowerUpType.IngredientSelector);
        }
        
        if (movesLimit < 25)
        {
            recommended.Add(PowerUpManager.PowerUpType.ExtraMove);
        }
        
        if (difficulty >= LevelDifficulty.Hard)
        {
            recommended.Add(PowerUpManager.PowerUpType.DoubleScore);
            recommended.Add(PowerUpManager.PowerUpType.Rainbow);
        }
        
        return recommended;
    }
}