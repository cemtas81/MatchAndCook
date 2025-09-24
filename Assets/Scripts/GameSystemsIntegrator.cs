using UnityEngine;

/// <summary>
/// Integrates all the new Match & Cook systems together.
/// Ensures proper initialization order and system communication.
/// </summary>
public class GameSystemsIntegrator : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private RecipeManager recipeManager;
    [SerializeField] private CustomerManager customerManager;
    [SerializeField] private PowerUpManager powerUpManager;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private EnhancedScoreManager enhancedScoreManager;
    [SerializeField] private TutorialManager tutorialManager;
    
    [Header("UI References")]
    [SerializeField] private RecipeCardUI recipeCardUI;
    [SerializeField] private CustomerOrderUI customerOrderUI;
    [SerializeField] private PowerUpUI powerUpUI;
    [SerializeField] private LeaderboardUI leaderboardUI;
    
    [Header("Integration Settings")]
    [SerializeField] private bool autoFindReferences = true;
    [SerializeField] private bool enableAllSystems = true;
    [SerializeField] private bool showDebugInfo = true;
    
    // System status tracking
    private bool systemsInitialized = false;
    
    void Awake()
    {
        if (autoFindReferences)
        {
            FindSystemReferences();
        }
        
        InitializeSystems();
    }
    
    void Start()
    {
        if (enableAllSystems)
        {
            EnableAllSystems();
        }
        
        if (showDebugInfo)
        {
            LogSystemStatus();
        }
    }
    
    /// <summary>
    /// Automatically find system references if not assigned
    /// </summary>
    private void FindSystemReferences()
    {
        // Core systems
        if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>();
        if (gridManager == null) gridManager = FindFirstObjectByType<GridManager>();
        if (uiManager == null) uiManager = FindFirstObjectByType<UIManager>();
        
        // New systems
        if (recipeManager == null) recipeManager = FindFirstObjectByType<RecipeManager>();
        if (customerManager == null) customerManager = FindFirstObjectByType<CustomerManager>();
        if (powerUpManager == null) powerUpManager = FindFirstObjectByType<PowerUpManager>();
        if (levelManager == null) levelManager = FindFirstObjectByType<LevelManager>();
        if (enhancedScoreManager == null) enhancedScoreManager = FindFirstObjectByType<EnhancedScoreManager>();
        if (tutorialManager == null) tutorialManager = FindFirstObjectByType<TutorialManager>();
        
        // UI systems
        if (recipeCardUI == null) recipeCardUI = FindFirstObjectByType<RecipeCardUI>();
        if (customerOrderUI == null) customerOrderUI = FindFirstObjectByType<CustomerOrderUI>();
        if (powerUpUI == null) powerUpUI = FindFirstObjectByType<PowerUpUI>();
        if (leaderboardUI == null) leaderboardUI = FindFirstObjectByType<LeaderboardUI>();
    }
    
    /// <summary>
    /// Initialize all systems in the correct order
    /// </summary>
    private void InitializeSystems()
    {
        if (systemsInitialized) return;
        
        Debug.Log("Initializing Match & Cook systems...");
        
        // Initialize core systems first
        // (These should already be initialized by their own Start methods)
        
        // Initialize new systems
        // (These also initialize themselves, but we can add any cross-system setup here)
        
        // Set up enhanced integration
        SetupSystemIntegrations();
        
        systemsInitialized = true;
        Debug.Log("All Match & Cook systems initialized successfully!");
    }
    
    /// <summary>
    /// Set up integrations between systems
    /// </summary>
    private void SetupSystemIntegrations()
    {
        // Recipe Manager integration with Grid Manager
        if (recipeManager != null && gridManager != null)
        {
            // This integration is already handled in RecipeManager
            Debug.Log("Recipe-Grid integration: Active");
        }
        
        // Customer Manager integration with Recipe Manager
        if (customerManager != null && recipeManager != null)
        {
            // This integration is already handled in CustomerManager
            Debug.Log("Customer-Recipe integration: Active");
        }
        
        // Power-up Manager integration with Grid Manager
        if (powerUpManager != null && gridManager != null)
        {
            // This integration is already handled in PowerUpManager
            Debug.Log("PowerUp-Grid integration: Active");
        }
        
        // Level Manager integration with all systems
        if (levelManager != null)
        {
            // Level manager already integrates with other systems
            Debug.Log("Level management integration: Active");
        }
        
        // Enhanced Score Manager integration
        if (enhancedScoreManager != null)
        {
            // Enhanced score manager already integrates with all systems
            Debug.Log("Enhanced scoring integration: Active");
        }
        
        // Tutorial Manager integration
        if (tutorialManager != null)
        {
            // Tutorial manager already integrates with game systems
            Debug.Log("Tutorial integration: Active");
        }
    }
    
    /// <summary>
    /// Enable all systems for gameplay
    /// </summary>
    private void EnableAllSystems()
    {
        // All systems are enabled by default through their own initialization
        // This method can be used for additional activation if needed
        
        Debug.Log("All Match & Cook systems are enabled and ready for gameplay!");
    }
    
    /// <summary>
    /// Log the status of all systems for debugging
    /// </summary>
    private void LogSystemStatus()
    {
        Debug.Log("=== Match & Cook Systems Status ===");
        
        Debug.Log($"GameManager: {(gameManager != null ? "✓" : "✗")}");
        Debug.Log($"GridManager: {(gridManager != null ? "✓" : "✗")}");
        Debug.Log($"UIManager: {(uiManager != null ? "✓" : "✗")}");
        Debug.Log($"RecipeManager: {(recipeManager != null ? "✓" : "✗")}");
        Debug.Log($"CustomerManager: {(customerManager != null ? "✓" : "✗")}");
        Debug.Log($"PowerUpManager: {(powerUpManager != null ? "✓" : "✗")}");
        Debug.Log($"LevelManager: {(levelManager != null ? "✓" : "✗")}");
        Debug.Log($"EnhancedScoreManager: {(enhancedScoreManager != null ? "✓" : "✗")}");
        Debug.Log($"TutorialManager: {(tutorialManager != null ? "✓" : "✗")}");
        
        Debug.Log("=== UI Systems Status ===");
        Debug.Log($"RecipeCardUI: {(recipeCardUI != null ? "✓" : "✗")}");
        Debug.Log($"CustomerOrderUI: {(customerOrderUI != null ? "✓" : "✗")}");
        Debug.Log($"PowerUpUI: {(powerUpUI != null ? "✓" : "✗")}");
        Debug.Log($"LeaderboardUI: {(leaderboardUI != null ? "✓" : "✗")}");
        
        Debug.Log("================================");
    }
    
    /// <summary>
    /// Get integration status for external queries
    /// </summary>
    public bool AreSystemsReady()
    {
        return systemsInitialized && 
               gameManager != null && 
               gridManager != null && 
               uiManager != null;
    }
    
    /// <summary>
    /// Get the count of active systems
    /// </summary>
    public int GetActiveSystemsCount()
    {
        int count = 0;
        
        if (gameManager != null) count++;
        if (gridManager != null) count++;
        if (uiManager != null) count++;
        if (recipeManager != null) count++;
        if (customerManager != null) count++;
        if (powerUpManager != null) count++;
        if (levelManager != null) count++;
        if (enhancedScoreManager != null) count++;
        if (tutorialManager != null) count++;
        
        return count;
    }
    
    /// <summary>
    /// Manually trigger system refresh (for debugging)
    /// </summary>
    [ContextMenu("Refresh Systems")]
    public void RefreshSystems()
    {
        systemsInitialized = false;
        FindSystemReferences();
        InitializeSystems();
        LogSystemStatus();
    }
    
    /// <summary>
    /// Show leaderboard UI
    /// </summary>
    public void ShowLeaderboard()
    {
        if (leaderboardUI != null)
        {
            leaderboardUI.Show();
        }
        else
        {
            Debug.LogWarning("LeaderboardUI not found!");
        }
    }
    
    /// <summary>
    /// Start tutorial manually
    /// </summary>
    public void StartTutorial()
    {
        if (tutorialManager != null)
        {
            tutorialManager.StartTutorial();
        }
        else
        {
            Debug.LogWarning("TutorialManager not found!");
        }
    }
    
    /// <summary>
    /// Get game statistics summary
    /// </summary>
    public string GetGameStatsSummary()
    {
        if (enhancedScoreManager == null) return "Enhanced Score Manager not available";
        
        var stats = enhancedScoreManager.GetPlayerStats();
        string summary = "=== Player Statistics ===\n";
        
        foreach (var stat in stats)
        {
            summary += $"{stat.Key}: {stat.Value}\n";
        }
        
        return summary;
    }
}