using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Main controller for the pizza-themed Match-3 game.
/// Coordinates between all pizza systems and provides easy setup for the complete game.
/// This script ensures all pizza systems work together seamlessly.
/// </summary>
public class PizzaGameController : MonoBehaviour
{
    [Header("Pizza Game Components")]
    [SerializeField] private PizzaOrderManager pizzaOrderManager;
    [SerializeField] private PizzaSliderUI pizzaSliderUI;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private PowerUpManager powerUpManager;
    
    [Header("Game Configuration")]
    [SerializeField] private bool autoFindComponents = true;
    [SerializeField] private bool initializeWithSampleOrders = true;
    [SerializeField] private int startingLevel = 1;
    
    [Header("Pizza Order Configuration")]
    [SerializeField] private List<PizzaOrder> customPizzaOrders = new List<PizzaOrder>();
    
    // Game state
    private bool isGameInitialized = false;
    
    void Start()
    {
        InitializePizzaGame();
    }
    
    /// <summary>
    /// Initialize the complete pizza match-3 game system
    /// </summary>
    private void InitializePizzaGame()
    {
        if (isGameInitialized) return;
        
        // Auto-find components if needed
        if (autoFindComponents)
        {
            FindGameComponents();
        }
        
        // Validate required components
        if (!ValidateComponents())
        {
            Debug.LogError("PizzaGameController: Missing required components! Game may not function properly.");
            return;
        }
        
        // Setup pizza orders
        SetupPizzaOrders();
        
        // Initialize systems in correct order
        InitializeGameSystems();
        
        isGameInitialized = true;
        Debug.Log("Pizza Match-3 game initialized successfully!");
    }
    
    /// <summary>
    /// Auto-find game components if not manually assigned
    /// </summary>
    private void FindGameComponents()
    {
        if (pizzaOrderManager == null)
            pizzaOrderManager = FindFirstObjectByType<PizzaOrderManager>();
            
        if (pizzaSliderUI == null)
            pizzaSliderUI = FindFirstObjectByType<PizzaSliderUI>();
            
        if (gridManager == null)
            gridManager = FindFirstObjectByType<GridManager>();
            
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();
            
        if (powerUpManager == null)
            powerUpManager = FindFirstObjectByType<PowerUpManager>();
    }
    
    /// <summary>
    /// Validate that all required components are present
    /// </summary>
    private bool ValidateComponents()
    {
        bool isValid = true;
        
        if (pizzaOrderManager == null)
        {
            Debug.LogError("PizzaOrderManager not found! This is required for the pizza game.");
            isValid = false;
        }
        
        if (gridManager == null)
        {
            Debug.LogError("GridManager not found! This is required for match-3 gameplay.");
            isValid = false;
        }
        
        if (gameManager == null)
        {
            Debug.LogWarning("GameManager not found! Some features may not work properly.");
        }
        
        if (pizzaSliderUI == null)
        {
            Debug.LogWarning("PizzaSliderUI not found! Pizza order progress won't be displayed.");
        }
        
        return isValid;
    }
    
    /// <summary>
    /// Setup pizza orders for the game
    /// </summary>
    private void SetupPizzaOrders()
    {
        if (pizzaOrderManager == null) return;
        
        // Add custom pizza orders if specified
        foreach (var order in customPizzaOrders)
        {
            if (order != null)
            {
                pizzaOrderManager.AddAvailablePizzaOrder(order);
            }
        }
        
        // If no custom orders and sample orders are enabled, this will be handled by PizzaOrderManager
        if (customPizzaOrders.Count == 0 && initializeWithSampleOrders)
        {
            Debug.Log("No custom pizza orders found. PizzaOrderManager will use sample orders.");
        }
    }
    
    /// <summary>
    /// Initialize all game systems in the correct order
    /// </summary>
    private void InitializeGameSystems()
    {
        // The systems will initialize themselves, but we can perform any additional setup here
        
        // Set starting level if different from default
        if (startingLevel > 1 && pizzaOrderManager != null)
        {
            // Note: This would require a SetCurrentLevel method in PizzaOrderManager
            Debug.Log($"Starting at level {startingLevel}");
        }
    }
    
    /// <summary>
    /// Add a new pizza order to the game at runtime
    /// </summary>
    public void AddPizzaOrder(PizzaOrder order)
    {
        if (order != null && pizzaOrderManager != null)
        {
            pizzaOrderManager.AddAvailablePizzaOrder(order);
            Debug.Log($"Added pizza order: {order.pizzaName}");
        }
    }
    
    /// <summary>
    /// Get the current game status for debugging
    /// </summary>
    public string GetGameStatus()
    {
        if (!isGameInitialized)
            return "Game not initialized";
            
        string status = "Pizza Match-3 Game Status:\n";
        
        if (pizzaOrderManager != null)
        {
            status += $"- Current Level: {pizzaOrderManager.CurrentLevel}\n";
            status += $"- Order Active: {pizzaOrderManager.IsOrderActive}\n";
            if (pizzaOrderManager.CurrentOrder != null)
            {
                status += $"- Current Pizza: {pizzaOrderManager.CurrentOrder.pizzaName}\n";
                status += $"- Time Remaining: {pizzaOrderManager.RemainingTime:F1}s\n";
            }
        }
        
        if (gameManager != null)
        {
            status += $"- Score: {gameManager.CurrentScore}\n";
            status += $"- Moves Remaining: {gameManager.MovesRemaining}\n";
        }
        
        return status;
    }
    
    /// <summary>
    /// Restart the pizza game
    /// </summary>
    public void RestartGame()
    {
        if (gameManager != null)
        {
            gameManager.RestartGame();
        }
        else
        {
            // Fallback restart
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
    
    /// <summary>
    /// Pause the pizza game
    /// </summary>
    public void PauseGame()
    {
        if (gameManager != null)
        {
            gameManager.PauseGame();
        }
    }
    
    /// <summary>
    /// Resume the pizza game
    /// </summary>
    public void ResumeGame()
    {
        if (gameManager != null)
        {
            gameManager.ResumeGame();
        }
    }
    
    /// <summary>
    /// Enable debug logging for pizza game systems
    /// </summary>
    [ContextMenu("Log Game Status")]
    public void LogGameStatus()
    {
        Debug.Log(GetGameStatus());
    }
    
    /// <summary>
    /// Create a quick test setup for the pizza game
    /// </summary>
    [ContextMenu("Quick Setup for Testing")]
    public void QuickSetupForTesting()
    {
        autoFindComponents = true;
        initializeWithSampleOrders = true;
        startingLevel = 1;
        
        if (!isGameInitialized)
        {
            InitializePizzaGame();
        }
        
        Debug.Log("Quick setup completed. Pizza game ready for testing!");
    }
}