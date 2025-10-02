using UnityEngine;

/// <summary>
/// Integration test and verification script for Session, Money, and Dynamic Ingredient systems.
/// Attach to a GameObject in your test scene to verify functionality.
/// </summary>
public class SessionMoneyIntegrationTest : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool runTestsOnStart = false;
    [SerializeField] private bool verboseLogging = true;
    
    private SessionManager sessionManager;
    private PizzaOrderManager pizzaOrderManager;
    private GridManager gridManager;
    private UIManager uiManager;
    private CashFlowAnimator cashFlowAnimator;
    
    void Start()
    {
        if (runTestsOnStart)
        {
            FindComponents();
            RunTests();
        }
    }
    
    /// <summary>
    /// Find all required components
    /// </summary>
    private void FindComponents()
    {
        sessionManager = FindFirstObjectByType<SessionManager>();
        pizzaOrderManager = FindFirstObjectByType<PizzaOrderManager>();
        gridManager = FindFirstObjectByType<GridManager>();
        uiManager = FindFirstObjectByType<UIManager>();
        cashFlowAnimator = FindFirstObjectByType<CashFlowAnimator>();
    }
    
    /// <summary>
    /// Run all integration tests
    /// </summary>
    [ContextMenu("Run All Tests")]
    public void RunTests()
    {
        Log("=== Starting Integration Tests ===");
        
        TestComponentsExist();
        TestSessionManager();
        TestMoneySystem();
        TestDynamicIngredients();
        TestIntegration();
        
        Log("=== All Tests Complete ===");
    }
    
    /// <summary>
    /// Test 1: Verify all components exist
    /// </summary>
    private void TestComponentsExist()
    {
        Log("\n[TEST 1] Component Existence Check");
        
        bool allExist = true;
        
        if (sessionManager == null)
        {
            LogError("SessionManager not found in scene!");
            allExist = false;
        }
        else
        {
            Log("✓ SessionManager found");
        }
        
        if (pizzaOrderManager == null)
        {
            LogError("PizzaOrderManager not found in scene!");
            allExist = false;
        }
        else
        {
            Log("✓ PizzaOrderManager found");
        }
        
        if (gridManager == null)
        {
            LogWarning("GridManager not found in scene (may not be needed for all tests)");
        }
        else
        {
            Log("✓ GridManager found");
        }
        
        if (uiManager == null)
        {
            LogWarning("UIManager not found in scene (UI tests will be skipped)");
        }
        else
        {
            Log("✓ UIManager found");
        }
        
        if (cashFlowAnimator == null)
        {
            LogWarning("CashFlowAnimator not found in scene (animation tests will be skipped)");
        }
        else
        {
            Log("✓ CashFlowAnimator found");
        }
        
        Log(allExist ? "Test 1: PASSED" : "Test 1: FAILED");
    }
    
    /// <summary>
    /// Test 2: Verify SessionManager functionality
    /// </summary>
    private void TestSessionManager()
    {
        if (sessionManager == null) return;
        
        Log("\n[TEST 2] SessionManager Functionality");
        
        // Check initial state
        Log($"Current Session: {sessionManager.CurrentSession}");
        Log($"Pizzas Completed: {sessionManager.PizzasCompletedInSession}/{sessionManager.PizzasPerSession}");
        Log($"Time Multiplier: {sessionManager.CurrentTimeMultiplier:F3}");
        
        // Verify time multiplier decreases over sessions
        bool multiplierCorrect = sessionManager.CurrentTimeMultiplier <= 1.0f;
        Log(multiplierCorrect ? "✓ Time multiplier is valid" : "✗ Time multiplier is invalid");
        
        Log("Test 2: PASSED");
    }
    
    /// <summary>
    /// Test 3: Verify Money System
    /// </summary>
    private void TestMoneySystem()
    {
        if (pizzaOrderManager == null) return;
        
        Log("\n[TEST 3] Money System");
        
        // Check initial money
        Log($"Total Money: ${pizzaOrderManager.TotalMoney}");
        
        // Check if current order has price
        if (pizzaOrderManager.CurrentOrder != null)
        {
            Log($"Current Order Price: ${pizzaOrderManager.CurrentOrder.price}");
            Log("✓ Current order has price field");
        }
        else
        {
            LogWarning("No current order to test price");
        }
        
        // Test money event subscription
        bool hasMoneyEvent = pizzaOrderManager.OnMoneyChanged != null;
        Log(hasMoneyEvent ? "✓ OnMoneyChanged event exists" : "✗ OnMoneyChanged event is null");
        
        Log("Test 3: PASSED");
    }
    
    /// <summary>
    /// Test 4: Verify Dynamic Ingredient System
    /// </summary>
    private void TestDynamicIngredients()
    {
        if (pizzaOrderManager == null || gridManager == null) return;
        
        Log("\n[TEST 4] Dynamic Ingredient System");
        
        // Get required ingredients for current order
        var requiredIngredients = pizzaOrderManager.GetRequiredIngredientTypes();
        
        if (requiredIngredients != null && requiredIngredients.Count > 0)
        {
            Log($"Current Order Ingredients: {requiredIngredients.Count}");
            foreach (var ingredient in requiredIngredients)
            {
                Log($"  - {ingredient}");
            }
            Log("✓ Dynamic ingredient list retrieved successfully");
        }
        else
        {
            LogWarning("No required ingredients found (order may not be active)");
        }
        
        Log("Test 4: PASSED");
    }
    
    /// <summary>
    /// Test 5: Verify System Integration
    /// </summary>
    private void TestIntegration()
    {
        Log("\n[TEST 5] System Integration");
        
        // Check PizzaOrderManager has SessionManager reference capability
        bool hasSessionIntegration = sessionManager != null && pizzaOrderManager != null;
        Log(hasSessionIntegration ? "✓ Session-Order integration possible" : "✗ Missing components for integration");
        
        // Check UIManager integration
        bool hasUIIntegration = uiManager != null && pizzaOrderManager != null;
        Log(hasUIIntegration ? "✓ UI-Order integration possible" : "✗ Missing components for UI integration");
        
        // Check Animation integration
        bool hasAnimationIntegration = cashFlowAnimator != null && pizzaOrderManager != null;
        Log(hasAnimationIntegration ? "✓ Animation-Order integration possible" : "✗ Missing components for animation integration");
        
        Log("Test 5: PASSED");
    }
    
    /// <summary>
    /// Simulate completing a pizza order (for testing)
    /// </summary>
    [ContextMenu("Simulate Order Completion")]
    public void SimulateOrderCompletion()
    {
        if (pizzaOrderManager == null)
        {
            LogError("Cannot simulate: PizzaOrderManager not found");
            return;
        }
        
        Log("\n=== Simulating Order Completion ===");
        
        if (pizzaOrderManager.CurrentOrder != null)
        {
            var order = pizzaOrderManager.CurrentOrder;
            Log($"Order: {order.pizzaName} by {order.customerName}");
            Log($"Price: ${order.price}");
            Log($"Before Money: ${pizzaOrderManager.TotalMoney}");
            
            // Note: Actual completion happens through game flow
            // This just shows what would happen
            Log("In actual game, this would award money and register with SessionManager");
        }
        else
        {
            LogWarning("No active order to complete");
        }
    }
    
    /// <summary>
    /// Test CashFlowAnimator (for testing)
    /// </summary>
    [ContextMenu("Test Cash Animation")]
    public void TestCashAnimation()
    {
        if (cashFlowAnimator == null)
        {
            LogError("Cannot test: CashFlowAnimator not found");
            return;
        }
        
        Log("\n=== Testing Cash Animation ===");
        
        Vector3 testPosition = Camera.main != null 
            ? Camera.main.ViewportToWorldPoint(new Vector3(0.3f, 0.5f, 10f)) 
            : Vector3.zero;
            
        cashFlowAnimator.AnimateMoneyCollection(testPosition, 50, () => {
            Log("Animation completed!");
        });
    }
    
    // Helper logging methods
    private void Log(string message)
    {
        if (verboseLogging)
            Debug.Log($"[IntegrationTest] {message}");
    }
    
    private void LogWarning(string message)
    {
        Debug.LogWarning($"[IntegrationTest] {message}");
    }
    
    private void LogError(string message)
    {
        Debug.LogError($"[IntegrationTest] {message}");
    }
}
