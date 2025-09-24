using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages level progression, difficulty scaling, and level completion.
/// Handles level loading, objective tracking, and progression unlocking.
/// </summary>
public class LevelManager : MonoBehaviour
{
    [Header("Level Settings")]
    [SerializeField] private List<Level> availableLevels = new List<Level>();
    [SerializeField] private Level currentLevel;
    [SerializeField] private bool autoLoadNextLevel = true;
    [SerializeField] private float levelCompletionDelay = 2f;
    
    [Header("Progression")]
    [SerializeField] private int maxUnlockedLevel = 1;
    [SerializeField] private bool unlockAllLevels = false; // For testing
    
    // Current state tracking
    private int recipesCompleted = 0;
    private int ordersCompleted = 0;
    private int currentStars = 0;
    private bool levelCompleted = false;
    private bool levelFailed = false;
    
    // Events
    public System.Action<Level> OnLevelStarted;
    public System.Action<Level, int, bool> OnLevelCompleted; // level, stars, success
    public System.Action<Level> OnLevelFailed;
    public System.Action<int> OnStarsEarned;
    public System.Action<Level> OnLevelUnlocked;
    
    // References
    private GameManager gameManager;
    private RecipeManager recipeManager;
    private CustomerManager customerManager;
    
    // Properties
    public Level CurrentLevel => currentLevel;
    public int MaxUnlockedLevel => maxUnlockedLevel;
    public bool IsLevelCompleted => levelCompleted;
    public bool IsLevelFailed => levelFailed;
    
    // Save data keys
    private const string MAX_UNLOCKED_LEVEL_KEY = "MaxUnlockedLevel";
    private const string LEVEL_STARS_KEY_PREFIX = "Level_";
    private const string TOTAL_STARS_KEY = "TotalStars";
    
    void Start()
    {
        InitializeLevelManager();
    }
    
    void Update()
    {
        if (currentLevel != null && !levelCompleted && !levelFailed)
        {
            CheckLevelCompletion();
        }
    }
    
    /// <summary>
    /// Initialize level manager and find references
    /// </summary>
    private void InitializeLevelManager()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        recipeManager = FindFirstObjectByType<RecipeManager>();
        customerManager = FindFirstObjectByType<CustomerManager>();
        
        // Load progression data
        LoadProgressionData();
        
        // Subscribe to game events
        if (gameManager != null)
        {
            gameManager.OnGameStateChanged += OnGameStateChanged;
        }
        
        if (recipeManager != null)
        {
            recipeManager.OnRecipeCompleted += OnRecipeCompleted;
        }
        
        if (customerManager != null)
        {
            customerManager.OnOrderCompleted += OnCustomerOrderCompleted;
        }
        
        // Start with current level if available
        if (currentLevel == null && availableLevels.Count > 0)
        {
            LoadLevel(1); // Start with level 1
        }
        else if (currentLevel != null)
        {
            StartCurrentLevel();
        }
    }
    
    /// <summary>
    /// Load progression data from PlayerPrefs
    /// </summary>
    private void LoadProgressionData()
    {
        maxUnlockedLevel = PlayerPrefs.GetInt(MAX_UNLOCKED_LEVEL_KEY, 1);
        
        if (unlockAllLevels)
        {
            maxUnlockedLevel = availableLevels.Count;
        }
    }
    
    /// <summary>
    /// Save progression data to PlayerPrefs
    /// </summary>
    private void SaveProgressionData()
    {
        PlayerPrefs.SetInt(MAX_UNLOCKED_LEVEL_KEY, maxUnlockedLevel);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Load a specific level by number
    /// </summary>
    public bool LoadLevel(int levelNumber)
    {
        Level targetLevel = GetLevelByNumber(levelNumber);
        if (targetLevel == null)
        {
            Debug.LogWarning($"Level {levelNumber} not found!");
            return false;
        }
        
        if (levelNumber > maxUnlockedLevel && !unlockAllLevels)
        {
            Debug.LogWarning($"Level {levelNumber} is locked!");
            return false;
        }
        
        currentLevel = targetLevel;
        StartCurrentLevel();
        return true;
    }
    
    /// <summary>
    /// Start the current level
    /// </summary>
    private void StartCurrentLevel()
    {
        if (currentLevel == null) return;
        
        // Reset tracking variables
        recipesCompleted = 0;
        ordersCompleted = 0;
        currentStars = 0;
        levelCompleted = false;
        levelFailed = false;
        
        // Configure game systems for this level
        ConfigureGameForLevel();
        
        OnLevelStarted?.Invoke(currentLevel);
        
        Debug.Log($"Started Level {currentLevel.levelNumber}: {currentLevel.levelName}");
    }
    
    /// <summary>
    /// Configure game systems based on current level settings
    /// </summary>
    private void ConfigureGameForLevel()
    {
        if (currentLevel == null) return;
        
        // Configure GameManager
        if (gameManager != null)
        {
            // Update target score and moves through reflection or exposed methods
            Debug.Log($"Level configured: Target={currentLevel.targetScore}, Moves={currentLevel.movesLimit}");
        }
        
        // Configure CustomerManager
        if (customerManager != null)
        {
            customerManager.SetCurrentLevel(currentLevel.levelNumber);
            
            // Add level-specific orders
            foreach (var order in currentLevel.expectedOrders)
            {
                customerManager.AddAvailableOrder(order);
            }
        }
        
        // Configure RecipeManager
        if (recipeManager != null)
        {
            // Add level-specific recipes
            foreach (var recipe in currentLevel.requiredRecipes)
            {
                recipeManager.AddAvailableRecipe(recipe);
            }
        }
    }
    
    /// <summary>
    /// Check if level completion conditions are met
    /// </summary>
    private void CheckLevelCompletion()
    {
        if (gameManager == null || currentLevel == null) return;
        
        int currentScore = gameManager.CurrentScore;
        
        // Check if objectives are complete
        if (currentLevel.AreObjectivesComplete(currentScore, recipesCompleted, ordersCompleted))
        {
            CompleteLevel(true);
        }
        // Check if level failed (no moves left and objectives not met)
        else if (gameManager.MovesRemaining <= 0 && currentScore < currentLevel.targetScore)
        {
            CompleteLevel(false);
        }
    }
    
    /// <summary>
    /// Complete the current level
    /// </summary>
    private void CompleteLevel(bool success)
    {
        if (levelCompleted || levelFailed || currentLevel == null) return;
        
        if (success)
        {
            levelCompleted = true;
            int finalScore = gameManager != null ? gameManager.CurrentScore : 0;
            int starsEarned = currentLevel.CalculateStarsEarned(finalScore);
            
            // Save level completion
            SaveLevelCompletion(currentLevel.levelNumber, starsEarned);
            
            // Unlock next level
            UnlockNextLevel();
            
            OnLevelCompleted?.Invoke(currentLevel, starsEarned, true);
            OnStarsEarned?.Invoke(starsEarned);
            
            Debug.Log($"Level {currentLevel.levelNumber} completed! Stars: {starsEarned}");
            
            // Auto-load next level after delay
            if (autoLoadNextLevel)
            {
                Invoke(nameof(LoadNextLevel), levelCompletionDelay);
            }
        }
        else
        {
            levelFailed = true;
            OnLevelFailed?.Invoke(currentLevel);
            OnLevelCompleted?.Invoke(currentLevel, 0, false);
            
            Debug.Log($"Level {currentLevel.levelNumber} failed!");
        }
    }
    
    /// <summary>
    /// Save level completion data
    /// </summary>
    private void SaveLevelCompletion(int levelNumber, int starsEarned)
    {
        string levelKey = LEVEL_STARS_KEY_PREFIX + levelNumber;
        int previousStars = PlayerPrefs.GetInt(levelKey, 0);
        
        // Only save if we earned more stars than before
        if (starsEarned > previousStars)
        {
            PlayerPrefs.SetInt(levelKey, starsEarned);
            
            // Update total stars
            int totalStars = PlayerPrefs.GetInt(TOTAL_STARS_KEY, 0);
            totalStars += (starsEarned - previousStars);
            PlayerPrefs.SetInt(TOTAL_STARS_KEY, totalStars);
        }
        
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Unlock the next level
    /// </summary>
    private void UnlockNextLevel()
    {
        int nextLevelNumber = currentLevel.levelNumber + 1;
        if (nextLevelNumber > maxUnlockedLevel && nextLevelNumber <= availableLevels.Count)
        {
            maxUnlockedLevel = nextLevelNumber;
            SaveProgressionData();
            
            Level nextLevel = GetLevelByNumber(nextLevelNumber);
            if (nextLevel != null)
            {
                OnLevelUnlocked?.Invoke(nextLevel);
            }
        }
    }
    
    /// <summary>
    /// Load the next level
    /// </summary>
    private void LoadNextLevel()
    {
        int nextLevelNumber = currentLevel.levelNumber + 1;
        if (!LoadLevel(nextLevelNumber))
        {
            // No more levels, go to level select or main menu
            Debug.Log("All levels completed! Returning to main menu.");
            SceneManager.LoadScene("MainMenu");
        }
    }
    
    /// <summary>
    /// Restart the current level
    /// </summary>
    public void RestartLevel()
    {
        if (currentLevel != null)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    
    /// <summary>
    /// Handle recipe completion
    /// </summary>
    private void OnRecipeCompleted(Recipe recipe, int reward)
    {
        recipesCompleted++;
        Debug.Log($"Recipe completed! Total: {recipesCompleted}");
    }
    
    /// <summary>
    /// Handle customer order completion
    /// </summary>
    private void OnCustomerOrderCompleted(ActiveCustomerOrder order, bool success)
    {
        if (success)
        {
            ordersCompleted++;
            Debug.Log($"Order completed! Total: {ordersCompleted}");
        }
    }
    
    /// <summary>
    /// Handle game state changes
    /// </summary>
    private void OnGameStateChanged(GameManager.GameState newState)
    {
        if (newState == GameManager.GameState.Won)
        {
            CompleteLevel(true);
        }
        else if (newState == GameManager.GameState.Lost)
        {
            CompleteLevel(false);
        }
    }
    
    /// <summary>
    /// Get level by number
    /// </summary>
    private Level GetLevelByNumber(int levelNumber)
    {
        foreach (var level in availableLevels)
        {
            if (level.levelNumber == levelNumber)
            {
                return level;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Get stars earned for a specific level
    /// </summary>
    public int GetLevelStars(int levelNumber)
    {
        string levelKey = LEVEL_STARS_KEY_PREFIX + levelNumber;
        return PlayerPrefs.GetInt(levelKey, 0);
    }
    
    /// <summary>
    /// Get total stars earned across all levels
    /// </summary>
    public int GetTotalStars()
    {
        return PlayerPrefs.GetInt(TOTAL_STARS_KEY, 0);
    }
    
    /// <summary>
    /// Check if a level is unlocked
    /// </summary>
    public bool IsLevelUnlocked(int levelNumber)
    {
        return levelNumber <= maxUnlockedLevel || unlockAllLevels;
    }
    
    /// <summary>
    /// Get level completion progress
    /// </summary>
    public float GetLevelProgress()
    {
        if (currentLevel == null || gameManager == null) return 0f;
        
        return currentLevel.GetCompletionProgress(
            gameManager.CurrentScore,
            recipesCompleted,
            ordersCompleted
        );
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (gameManager != null)
        {
            gameManager.OnGameStateChanged -= OnGameStateChanged;
        }
        
        if (recipeManager != null)
        {
            recipeManager.OnRecipeCompleted -= OnRecipeCompleted;
        }
        
        if (customerManager != null)
        {
            customerManager.OnOrderCompleted -= OnCustomerOrderCompleted;
        }
    }
}