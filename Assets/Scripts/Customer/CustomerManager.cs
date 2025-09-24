using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages customer orders, timing, and satisfaction levels.
/// Creates time pressure and manages multiple concurrent orders.
/// </summary>
public class CustomerManager : MonoBehaviour
{
    [Header("Customer Settings")]
    [SerializeField] private List<CustomerOrder> availableOrders = new List<CustomerOrder>();
    [SerializeField] private int maxConcurrentOrders = 2;
    [SerializeField] private float orderSpawnInterval = 30f;
    [SerializeField] private float orderSpawnVariation = 10f;
    
    [Header("Difficulty Scaling")]
    [SerializeField] private float difficultyScaleRate = 0.1f;
    [SerializeField] private float minOrderTime = 20f;
    [SerializeField] private float maxOrderTime = 120f;
    
    // Current state
    private List<ActiveCustomerOrder> activeOrders = new List<ActiveCustomerOrder>();
    private float nextOrderSpawnTime;
    private int currentLevel = 1;
    private float gameTime = 0f;
    
    // Events
    public System.Action<ActiveCustomerOrder> OnOrderStarted;
    public System.Action<ActiveCustomerOrder, bool> OnOrderCompleted; // bool: success
    public System.Action<ActiveCustomerOrder> OnOrderExpired;
    public System.Action<ActiveCustomerOrder, float> OnOrderTimeUpdated;
    
    // References
    private RecipeManager recipeManager;
    private GameManager gameManager;
    
    // Properties
    public List<ActiveCustomerOrder> ActiveOrders => activeOrders;
    public int CurrentLevel => currentLevel;
    
    void Start()
    {
        InitializeCustomerManager();
    }
    
    void Update()
    {
        gameTime += Time.deltaTime;
        
        UpdateActiveOrders();
        CheckForNewOrders();
    }
    
    /// <summary>
    /// Initialize customer manager and find references
    /// </summary>
    private void InitializeCustomerManager()
    {
        recipeManager = FindFirstObjectByType<RecipeManager>();
        gameManager = FindFirstObjectByType<GameManager>();
        
        if (recipeManager != null)
        {
            recipeManager.OnRecipeCompleted += OnRecipeCompleted;
        }
        
        // Schedule first order
        ScheduleNextOrder();
    }
    
    /// <summary>
    /// Update all active orders
    /// </summary>
    private void UpdateActiveOrders()
    {
        for (int i = activeOrders.Count - 1; i >= 0; i--)
        {
            var order = activeOrders[i];
            order.remainingTime -= Time.deltaTime;
            
            OnOrderTimeUpdated?.Invoke(order, order.remainingTime);
            
            // Check if order expired
            if (order.remainingTime <= 0f)
            {
                ExpireOrder(order);
            }
        }
    }
    
    /// <summary>
    /// Check if it's time to spawn new orders
    /// </summary>
    private void CheckForNewOrders()
    {
        if (gameTime >= nextOrderSpawnTime && activeOrders.Count < maxConcurrentOrders)
        {
            SpawnRandomOrder();
            ScheduleNextOrder();
        }
    }
    
    /// <summary>
    /// Schedule the next order spawn time
    /// </summary>
    private void ScheduleNextOrder()
    {
        float variation = Random.Range(-orderSpawnVariation, orderSpawnVariation);
        nextOrderSpawnTime = gameTime + orderSpawnInterval + variation;
    }
    
    /// <summary>
    /// Spawn a random customer order
    /// </summary>
    public void SpawnRandomOrder()
    {
        if (availableOrders.Count == 0) return;
        
        // Filter orders by level requirement
        List<CustomerOrder> eligibleOrders = new List<CustomerOrder>();
        foreach (var order in availableOrders)
        {
            if (order.minimumLevel <= currentLevel)
            {
                eligibleOrders.Add(order);
            }
        }
        
        if (eligibleOrders.Count == 0)
        {
            eligibleOrders = availableOrders; // Fallback to all orders
        }
        
        CustomerOrder selectedOrder = eligibleOrders[Random.Range(0, eligibleOrders.Count)];
        StartOrder(selectedOrder);
    }
    
    /// <summary>
    /// Start a specific customer order
    /// </summary>
    public void StartOrder(CustomerOrder orderData)
    {
        if (orderData == null || activeOrders.Count >= maxConcurrentOrders) return;
        
        // Apply difficulty scaling
        float scaledTimeLimit = orderData.timeLimit;
        scaledTimeLimit *= Mathf.Max(0.5f, 1f - (currentLevel - 1) * difficultyScaleRate);
        scaledTimeLimit = Mathf.Clamp(scaledTimeLimit, minOrderTime, maxOrderTime);
        
        ActiveCustomerOrder activeOrder = new ActiveCustomerOrder
        {
            orderData = orderData,
            remainingTime = scaledTimeLimit,
            startTime = gameTime,
            isActive = true
        };
        
        activeOrders.Add(activeOrder);
        OnOrderStarted?.Invoke(activeOrder);
        
        Debug.Log($"New order from {orderData.customerName}: {orderData.requestedRecipe.recipeName} ({scaledTimeLimit:F1}s)");
    }
    
    /// <summary>
    /// Handle recipe completion - check if it matches any active orders
    /// </summary>
    private void OnRecipeCompleted(Recipe completedRecipe, int recipeReward)
    {
        // Find matching active order
        ActiveCustomerOrder matchingOrder = null;
        foreach (var order in activeOrders)
        {
            if (order.orderData.requestedRecipe == completedRecipe)
            {
                matchingOrder = order;
                break;
            }
        }
        
        if (matchingOrder != null)
        {
            CompleteOrder(matchingOrder);
        }
    }
    
    /// <summary>
    /// Complete a customer order successfully
    /// </summary>
    private void CompleteOrder(ActiveCustomerOrder order)
    {
        if (!activeOrders.Contains(order)) return;
        
        float completionTime = gameTime - order.startTime;
        int orderReward = order.orderData.CalculateReward(completionTime);
        
        // Award additional points for completing customer order
        if (gameManager != null)
        {
            // This is a simplified integration - in a real system you'd want more direct score management
            Debug.Log($"Customer order bonus: {orderReward} points");
        }
        
        activeOrders.Remove(order);
        OnOrderCompleted?.Invoke(order, true);
        
        Debug.Log($"Order completed for {order.orderData.customerName}! Reward: {orderReward} points");
    }
    
    /// <summary>
    /// Expire a customer order due to timeout
    /// </summary>
    private void ExpireOrder(ActiveCustomerOrder order)
    {
        if (!activeOrders.Contains(order)) return;
        
        activeOrders.Remove(order);
        OnOrderExpired?.Invoke(order);
        OnOrderCompleted?.Invoke(order, false);
        
        Debug.Log($"Order expired for {order.orderData.customerName}! Customer is unhappy.");
    }
    
    /// <summary>
    /// Get customer satisfaction for all active orders
    /// </summary>
    public float GetOverallCustomerSatisfaction()
    {
        if (activeOrders.Count == 0) return 1f;
        
        float totalSatisfaction = 0f;
        foreach (var order in activeOrders)
        {
            totalSatisfaction += order.orderData.GetSatisfactionLevel(order.remainingTime);
        }
        
        return totalSatisfaction / activeOrders.Count;
    }
    
    /// <summary>
    /// Set current game level for difficulty scaling
    /// </summary>
    public void SetCurrentLevel(int level)
    {
        currentLevel = level;
    }
    
    /// <summary>
    /// Add a customer order to available orders
    /// </summary>
    public void AddAvailableOrder(CustomerOrder order)
    {
        if (order != null && !availableOrders.Contains(order))
        {
            availableOrders.Add(order);
        }
    }
    
    /// <summary>
    /// Remove a customer order from available orders
    /// </summary>
    public void RemoveAvailableOrder(CustomerOrder order)
    {
        if (order != null && availableOrders.Contains(order))
        {
            availableOrders.Remove(order);
        }
    }
    
    /// <summary>
    /// Force complete all active orders (for level completion, etc.)
    /// </summary>
    public void CompleteAllOrders()
    {
        for (int i = activeOrders.Count - 1; i >= 0; i--)
        {
            ExpireOrder(activeOrders[i]);
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (recipeManager != null)
        {
            recipeManager.OnRecipeCompleted -= OnRecipeCompleted;
        }
    }
}

/// <summary>
/// Represents an active customer order with timing information
/// </summary>
[System.Serializable]
public class ActiveCustomerOrder
{
    public CustomerOrder orderData;
    public float remainingTime;
    public float startTime;
    public bool isActive;
    
    /// <summary>
    /// Get elapsed time for this order
    /// </summary>
    public float GetElapsedTime()
    {
        return orderData.timeLimit - remainingTime;
    }
    
    /// <summary>
    /// Get completion progress (0-1)
    /// </summary>
    public float GetTimeProgress()
    {
        return Mathf.Clamp01((orderData.timeLimit - remainingTime) / orderData.timeLimit);
    }
    
    /// <summary>
    /// Check if order is about to expire
    /// </summary>
    public bool IsUrgent(float urgentThreshold = 10f)
    {
        return remainingTime <= urgentThreshold;
    }
}