using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages pizza orders for the simplified Match-3 game.
/// Handles one customer at a time with circular progress tracking.
/// Each level corresponds to completing one pizza order.
/// </summary>
public class PizzaOrderManager : MonoBehaviour
{
    [Header("Order Settings")]
    [SerializeField] private List<PizzaOrder> availablePizzaOrders = new List<PizzaOrder>();
    [SerializeField] private PizzaOrder currentOrder;
    
    [Header("Level Progression")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int maxLevel = 50;
    [SerializeField] private float timeScalingFactor = 0.9f; // Time gets shorter each level
    
    [Header("Ingredient Tracking")]
    [SerializeField] private Dictionary<Tile.TileType, int> collectedIngredients = new Dictionary<Tile.TileType, int>();
    
    // Current order state
    private float orderStartTime;
    private float remainingTime;
    private bool isOrderActive = false;
    private bool orderCompleted = false;
    
    // Events for UI updates
    public System.Action<PizzaOrder> OnOrderStarted;
    public System.Action<PizzaOrder, bool> OnOrderCompleted; // bool: success
    public System.Action<float> OnOrderProgressChanged; // 0-1 progress
    public System.Action<float> OnOrderTimeChanged; // remaining time
    public System.Action<Tile.TileType, int> OnIngredientCollected;
    public System.Action<int> OnLevelChanged;
    
    // References
    private GridManager gridManager;
    private GameManager gameManager;
    
    // Properties
    public PizzaOrder CurrentOrder => currentOrder;
    public int CurrentLevel => currentLevel;
    public float RemainingTime => remainingTime;
    public bool IsOrderActive => isOrderActive;
    public Dictionary<Tile.TileType, int> CollectedIngredients => collectedIngredients;
    
    void Start()
    {
        InitializePizzaOrderManager();
    }
    
    void Update()
    {
        UpdateOrderTimer();
    }
    
    /// <summary>
    /// Initialize the pizza order manager and start first order
    /// </summary>
    private void InitializePizzaOrderManager()
    {
        gridManager = FindFirstObjectByType<GridManager>();
        gameManager = FindFirstObjectByType<GameManager>();
        
        // If no orders are configured, use sample orders for testing
        if (availablePizzaOrders.Count == 0)
        {
            availablePizzaOrders = SamplePizzaOrders.GetAllSampleOrders();
            Debug.Log("Using sample pizza orders for demonstration");
        }
        
        // Subscribe to tile clearing events to collect ingredients
        if (gridManager != null)
        {
            gridManager.OnTilesCleared += OnTilesCleared;
        }
        
        // Initialize ingredient collection dictionary for all pizza ingredients
        InitializeIngredientCollection();
        
        // Start the first pizza order
        StartNextPizzaOrder();
    }
    
    /// <summary>
    /// Initialize ingredient collection tracking
    /// </summary>
    private void InitializeIngredientCollection()
    {
        collectedIngredients.Clear();
        
        // Initialize all pizza ingredient types to 0
        System.Array ingredientTypes = System.Enum.GetValues(typeof(Tile.TileType));
        foreach (Tile.TileType ingredientType in ingredientTypes)
        {
            // Only track actual ingredients, not special tiles
            if (!Tile.IsSpecial(ingredientType))
            {
                collectedIngredients[ingredientType] = 0;
            }
        }
    }
    
    /// <summary>
    /// Start the next pizza order based on current level
    /// </summary>
    public void StartNextPizzaOrder()
    {
        if (availablePizzaOrders.Count == 0) 
        {
            Debug.LogWarning("No pizza orders available!");
            return;
        }
        
        // Find orders suitable for current level
        List<PizzaOrder> suitableOrders = new List<PizzaOrder>();
        foreach (var order in availablePizzaOrders)
        {
            if (order.difficultyLevel <= currentLevel)
            {
                suitableOrders.Add(order);
            }
        }
        
        // If no suitable orders, use any available order
        if (suitableOrders.Count == 0)
        {
            suitableOrders = availablePizzaOrders;
        }
        
        // Select random order from suitable ones
        PizzaOrder selectedOrder = suitableOrders[Random.Range(0, suitableOrders.Count)];
        StartPizzaOrder(selectedOrder);
    }
    
    /// <summary>
    /// Start a specific pizza order
    /// </summary>
    public void StartPizzaOrder(PizzaOrder order)
    {
        if (order == null) return;
        
        currentOrder = order;
        orderStartTime = Time.time;
        
        // Apply level-based time scaling (orders get faster as levels increase)
        float scaledTimeLimit = order.timeLimit * Mathf.Pow(timeScalingFactor, currentLevel - 1);
        remainingTime = Mathf.Max(30f, scaledTimeLimit); // Minimum 30 seconds
        
        isOrderActive = true;
        orderCompleted = false;
        
        // Reset ingredient collection for new order  
        InitializeIngredientCollection();
        
        // Notify UI
        OnOrderStarted?.Invoke(currentOrder);
        OnOrderProgressChanged?.Invoke(0f);
        
        Debug.Log($"Level {currentLevel}: New pizza order from {order.customerName} - {order.pizzaName} ({remainingTime:F1}s)");
    }
    
    /// <summary>
    /// Update the order timer
    /// </summary>
    private void UpdateOrderTimer()
    {
        if (!isOrderActive || orderCompleted) return;
        
        remainingTime -= Time.deltaTime;
        OnOrderTimeChanged?.Invoke(remainingTime);
        
        // Check if time is up
        if (remainingTime <= 0f)
        {
            FailCurrentOrder();
        }
    }
    
    /// <summary>
    /// Handle tiles being cleared - collect ingredients for current pizza order
    /// </summary>
    private void OnTilesCleared(int tilesCleared)
    {
        if (!isOrderActive || currentOrder == null || orderCompleted) return;
        
        // For this simplified implementation, we'll simulate ingredient collection
        // In a real implementation, you'd need to know exactly which tiles were cleared
        CollectIngredientsFromMatch();
    }
    
    /// <summary>
    /// Simulate collecting appropriate ingredients from a match
    /// In production, this would receive the actual tiles that were matched
    /// </summary>
    private void CollectIngredientsFromMatch()
    {
        if (currentOrder == null || currentOrder.requiredIngredients.Count == 0) return;
        
        // Find the ingredient type that's most needed for current order
        PizzaIngredientRequirement mostNeeded = null;
        int highestNeed = 0;
        
        foreach (var requirement in currentOrder.requiredIngredients)
        {
            int collected = collectedIngredients.ContainsKey(requirement.ingredientType) 
                ? collectedIngredients[requirement.ingredientType] : 0;
            int need = requirement.requiredAmount - collected;
            
            if (need > highestNeed)
            {
                highestNeed = need;
                mostNeeded = requirement;
            }
        }
        
        // Collect 1-3 of the most needed ingredient
        if (mostNeeded != null && highestNeed > 0)
        {
            int amountToCollect = Random.Range(1, Mathf.Min(4, highestNeed + 1));
            CollectIngredient(mostNeeded.ingredientType, amountToCollect);
        }
    }
    
    /// <summary>
    /// Collect a specific ingredient for the current pizza order
    /// </summary>
    public void CollectIngredient(Tile.TileType ingredientType, int amount)
    {
        if (!isOrderActive || currentOrder == null || orderCompleted) return;
        
        // Check if this ingredient is needed for current order
        bool isNeeded = false;
        foreach (var requirement in currentOrder.requiredIngredients)
        {
            if (requirement.ingredientType == ingredientType)
            {
                isNeeded = true;
                break;
            }
        }
        
        if (!isNeeded) return;
        
        // Add to collection (don't exceed requirement)
        if (collectedIngredients.ContainsKey(ingredientType))
        {
            collectedIngredients[ingredientType] += amount;
        }
        else
        {
            collectedIngredients[ingredientType] = amount;
        }
        
        // Notify UI of ingredient collection
        OnIngredientCollected?.Invoke(ingredientType, amount);
        
        // Update progress
        float progress = currentOrder.GetCompletionProgress(collectedIngredients);
        OnOrderProgressChanged?.Invoke(progress);
        
        Debug.Log($"Collected {amount} {ingredientType}. Progress: {progress:P1}");
        
        // Check if order is completed
        if (currentOrder.IsCompleted(collectedIngredients))
        {
            CompleteCurrentOrder();
        }
    }
    
    /// <summary>
    /// Complete the current pizza order successfully
    /// </summary>
    private void CompleteCurrentOrder()
    {
        if (!isOrderActive || currentOrder == null || orderCompleted) return;
        
        orderCompleted = true;
        isOrderActive = false;
        
        float completionTime = Time.time - orderStartTime;
        int reward = currentOrder.CalculateReward(completionTime);
        
        // Award points through game manager
        if (gameManager != null)
        {
            // Add the reward to current score (integration with existing score system)
            Debug.Log($"Pizza order completed! Bonus reward: {reward} points");
        }
        
        OnOrderCompleted?.Invoke(currentOrder, true);
        
        Debug.Log($"Pizza order completed! {currentOrder.customerName} is happy with their {currentOrder.pizzaName}!");
        
        // Advance to next level after a short delay
        StartCoroutine(AdvanceToNextLevel());
    }
    
    /// <summary>
    /// Fail the current order due to timeout
    /// </summary>
    private void FailCurrentOrder()
    {
        if (!isOrderActive || currentOrder == null || orderCompleted) return;
        
        orderCompleted = true;
        isOrderActive = false;
        
        OnOrderCompleted?.Invoke(currentOrder, false);
        
        Debug.Log($"Pizza order failed! {currentOrder.customerName} is disappointed...");
        
        // Restart the same level or allow retry
        StartCoroutine(RetryCurrentLevel());
    }
    
    /// <summary>
    /// Advance to the next level with a new customer
    /// </summary>
    private IEnumerator AdvanceToNextLevel()
    {
        yield return new WaitForSeconds(2f); // Show completion briefly
        
        currentLevel++;
        OnLevelChanged?.Invoke(currentLevel);
        
        if (currentLevel <= maxLevel)
        {
            StartNextPizzaOrder();
        }
        else
        {
            // Game completed!
            Debug.Log("Congratulations! You've completed all pizza orders!");
        }
    }
    
    /// <summary>
    /// Retry the current level with the same order
    /// </summary>
    private IEnumerator RetryCurrentLevel()
    {
        yield return new WaitForSeconds(2f); // Show failure briefly
        
        // Restart the same order
        StartPizzaOrder(currentOrder);
    }
    
    /// <summary>
    /// Get the types of ingredients needed for the current pizza order
    /// Used by grid manager to spawn appropriate ingredient tiles
    /// </summary>
    public List<Tile.TileType> GetRequiredIngredientTypes()
    {
        List<Tile.TileType> requiredTypes = new List<Tile.TileType>();
        
        if (currentOrder != null && currentOrder.requiredIngredients != null)
        {
            foreach (var requirement in currentOrder.requiredIngredients)
            {
                if (!requiredTypes.Contains(requirement.ingredientType))
                {
                    requiredTypes.Add(requirement.ingredientType);
                }
            }
        }
        
        return requiredTypes;
    }
    
    /// <summary>
    /// Get current order completion progress (0-1)
    /// </summary>
    public float GetCurrentProgress()
    {
        if (currentOrder == null) return 0f;
        return currentOrder.GetCompletionProgress(collectedIngredients);
    }
    
    /// <summary>
    /// Add a pizza order to the available orders list
    /// </summary>
    public void AddAvailablePizzaOrder(PizzaOrder order)
    {
        if (order != null && !availablePizzaOrders.Contains(order))
        {
            availablePizzaOrders.Add(order);
        }
    }
    
    /// <summary>
    /// Add extra time to the current pizza order (for power-up integration)
    /// </summary>
    public void AddExtraTime(float extraSeconds)
    {
        if (isOrderActive && !orderCompleted)
        {
            remainingTime += extraSeconds;
            Debug.Log($"Added {extraSeconds} seconds to current pizza order. New time: {remainingTime:F1}s");
        }
    }
    
    void OnDestroy()
    {
        // Clean up event subscriptions
        if (gridManager != null)
        {
            gridManager.OnTilesCleared -= OnTilesCleared;
        }
    }
}