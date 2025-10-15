using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PizzaOrderManager : MonoBehaviour
{
    [Header("Order Settings")]
    [SerializeField] private List<PizzaOrder> availablePizzaOrders = new List<PizzaOrder>();
    [SerializeField] private PizzaOrder currentOrder;
    [SerializeField] private PizzaOrder nextOrder; // preview (like Tetris)

    [Header("Level Progression")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int maxLevel = 50;
    [SerializeField] private float timeScalingFactor = 0.9f;

    [Header("Money System")]
    [SerializeField] private int totalMoney = 0;

    [Header("Ingredient Tracking")]
    [SerializeField] private Dictionary<Tile.TileType, int> collectedIngredients = new Dictionary<Tile.TileType, int>();

    // State
    private float orderStartTime;
    private float remainingTime;
    private bool isOrderActive = false;
    private bool orderCompleted = false;
    private bool isWaitingForIngredients = false;

    // Star system: +1 star after every 2 consecutive successes
    private int successSinceLastStar = 0;

    // Events
    public System.Action<PizzaOrder> OnOrderStarted;
    public System.Action<PizzaOrder, bool> OnOrderCompleted;
    public System.Action<float> OnOrderProgressChanged;
    public System.Action<float> OnOrderTimeChanged;
    public System.Action<Tile.TileType, int> OnIngredientCollected;
    public System.Action<int> OnLevelChanged;
    public System.Action<int> OnMoneyChanged;
    public System.Action<PizzaOrder> OnNextOrderPrepared;

    // References
    private GridManager gridManager;
    private GameManager gameManager;
    private SessionManager sessionManager;
    private CashFlowAnimator cashFlowAnimator;
    private MaterialStockManager stockManager;
    private RestaurantStarManager starManager;
    private IngredientWarningTimer warningTimer;
    private IngredientWarningPanel warningPanel;

    // Properties
    public PizzaOrder CurrentOrder => currentOrder;
    public PizzaOrder NextOrder => nextOrder;
    public int CurrentLevel => currentLevel;
    public float RemainingTime => remainingTime;
    public bool IsOrderActive => isOrderActive;
    public Dictionary<Tile.TileType, int> CollectedIngredients => collectedIngredients;
    public int TotalMoney => totalMoney;

    [SerializeField] private GameObject blockInputPanel; // Inspector'dan atayın

    void Start()
    {
        Initialize();
    }

    void Update()
    {
        UpdateOrderTimer();
        CheckAndResolveWarning();
    }

    private void Initialize()
    {
        gridManager = FindFirstObjectByType<GridManager>();
        gameManager = FindFirstObjectByType<GameManager>();
        sessionManager = FindFirstObjectByType<SessionManager>();
        cashFlowAnimator = FindFirstObjectByType<CashFlowAnimator>();
        stockManager = FindFirstObjectByType<MaterialStockManager>();
        starManager = FindFirstObjectByType<RestaurantStarManager>();
        warningTimer = FindFirstObjectByType<IngredientWarningTimer>();
        warningPanel = FindFirstObjectByType<IngredientWarningPanel>();

        //if (availablePizzaOrders.Count == 0)
        //    availablePizzaOrders = SamplePizzaOrders.GetAllSampleOrders();

        if (gridManager != null)
            gridManager.OnTilesCleared += OnTilesCleared;

        if (warningTimer != null)
        {
            warningTimer.OnWarningExpired += OnIngredientWarningExpired;
            warningTimer.OnWarningResolved += OnIngredientWarningResolved;
        }

        InitIngredientDictionary();
        PrepareInitialOrders();
    }

    private void InitIngredientDictionary()
    {
        collectedIngredients.Clear();
        System.Array ingredientTypes = System.Enum.GetValues(typeof(Tile.TileType));
        foreach (Tile.TileType t in ingredientTypes)
        {
            if (!Tile.IsSpecial(t))
                collectedIngredients[t] = 0;
        }
    }

    private List<PizzaOrder> GetLevelSuitable()
    {
        List<PizzaOrder> list = new List<PizzaOrder>();
        foreach (var o in availablePizzaOrders)
            if (o.difficultyLevel <= currentLevel)
                list.Add(o);
        if (list.Count == 0) list = availablePizzaOrders;
        return list;
    }

    private PizzaOrder PickRandom(List<PizzaOrder> pool)
    {
        if (pool == null || pool.Count == 0) return null;
        return pool[Random.Range(0, pool.Count)];
    }

    private void PrepareInitialOrders()
    {
        var suitable = GetLevelSuitable();
        currentOrder = PickRandom(suitable);
        nextOrder = PickRandom(suitable);
        StartPizzaOrder(currentOrder);
        OnNextOrderPrepared?.Invoke(nextOrder);
    }

    private void PromoteAndPrepareNext()
    {
        var suitable = GetLevelSuitable();

        if (nextOrder != null)
        {
            currentOrder = nextOrder;
        }
        else
        {
            currentOrder = PickRandom(suitable);
        }

        nextOrder = PickRandom(suitable);
        OnNextOrderPrepared?.Invoke(nextOrder);

        if (currentOrder != null)
            StartPizzaOrder(currentOrder);
    }

    public void StartPizzaOrder(PizzaOrder order)
    {
        if (order == null) return;

        currentOrder = order;
        orderStartTime = Time.time;

        float timeMultiplier = sessionManager != null
            ? sessionManager.CurrentTimeMultiplier
            : Mathf.Pow(timeScalingFactor, currentLevel - 1);

        remainingTime = Mathf.Max(30f, order.timeLimit * timeMultiplier);
        isOrderActive = true;
        orderCompleted = false;
        isWaitingForIngredients = false;
        InitIngredientDictionary();

        gridManager?.ResetGridForNewOrder();

        // Check if ingredients are available
        if (stockManager != null && !stockManager.CanFulfillOrder(order))
        {
            // Start warning timer for ingredient gathering
            isWaitingForIngredients = true;
            warningTimer?.StartWarningTimer();
            warningPanel?.ShowWarning("Malzemeler eksik! Lütfen topla!");

            // Dokunma engel panelini aç
            if (blockInputPanel != null)
                blockInputPanel.SetActive(true);

            Debug.Log($"Order started with insufficient ingredients: {order.pizzaName}. Warning timer activated.");
        }
        else
        {
            // Normal order start
            OnOrderStarted?.Invoke(currentOrder);
            OnOrderProgressChanged?.Invoke(0f);
            Debug.Log($"New order: {order.pizzaName}");
        }
    }

    private void UpdateOrderTimer()
    {
        if (!isOrderActive || orderCompleted) return;

        // If waiting for ingredients, don't decrease main timer
        if (isWaitingForIngredients)
        {
            // Update warning panel with timer
            if (warningTimer != null && warningPanel != null)
            {
                warningPanel.UpdateWarningTimer(warningTimer.RemainingWarningTime);
            }
            return;
        }

        remainingTime -= Time.deltaTime;
        OnOrderTimeChanged?.Invoke(remainingTime);

        if (remainingTime <= 0f)
            FailCurrentOrder();
    }

    private void OnTilesCleared(int tilesCleared)
    {
        if (!isOrderActive || currentOrder == null || orderCompleted) return;
        CollectIngredientsFromMatch();
    }

    private void CollectIngredientsFromMatch()
    {
        if (currentOrder == null || currentOrder.requiredIngredients.Count == 0) return;

        PizzaIngredientRequirement mostNeeded = null;
        int highestNeed = 0;
        foreach (var req in currentOrder.requiredIngredients)
        {
            int collected = collectedIngredients.ContainsKey(req.ingredientType)
                ? collectedIngredients[req.ingredientType] : 0;
            int need = req.requiredAmount - collected;
            if (need > highestNeed)
            {
                highestNeed = need;
                mostNeeded = req;
            }
        }

        if (mostNeeded != null && highestNeed > 0)
        {
            int amount = Random.Range(1, Mathf.Min(4, highestNeed + 1));
            CollectIngredient(mostNeeded.ingredientType, amount);
        }
    }

    public void CollectIngredient(Tile.TileType ingredientType, int amount)
    {
        if (!isOrderActive || currentOrder == null || orderCompleted) return;

        bool needed = false;
        foreach (var req in currentOrder.requiredIngredients)
            if (req.ingredientType == ingredientType) { needed = true; break; }
        if (!needed) return;

        if (collectedIngredients.ContainsKey(ingredientType))
            collectedIngredients[ingredientType] += amount;
        else
            collectedIngredients[ingredientType] = amount;

        OnIngredientCollected?.Invoke(ingredientType, amount);

        float progress = currentOrder.GetCompletionProgress(collectedIngredients);
        OnOrderProgressChanged?.Invoke(progress);

        if (currentOrder.IsCompleted(collectedIngredients))
            CompleteCurrentOrder();
    }

    private void CompleteCurrentOrder()
    {
        if (!isOrderActive || currentOrder == null || orderCompleted) return;

        orderCompleted = true;
        isOrderActive = false;

        int moneyEarned = currentOrder.price;
        AddMoney(moneyEarned);

        stockManager?.ConsumeIngredients(currentOrder);

        successSinceLastStar++;
        if (successSinceLastStar >= 2)
        {
            starManager?.OnPizzaSuccess();
            successSinceLastStar = 0;
        }

        sessionManager?.RegisterPizzaCompleted(true);
        OnOrderCompleted?.Invoke(currentOrder, true);

        Debug.Log($"Order completed successfully: {currentOrder.pizzaName}");

        StartCoroutine(AdvanceToNextLevel());
    }

    private void FailCurrentOrder()
    {
        if (!isOrderActive || currentOrder == null || orderCompleted) return;

        orderCompleted = true;
        isOrderActive = false;

        starManager?.OnPizzaFailed();
        successSinceLastStar = 0;

        sessionManager?.RegisterPizzaCompleted(false);
        OnOrderCompleted?.Invoke(currentOrder, false);

        Debug.Log($"Order failed: {currentOrder.pizzaName}");

        StartCoroutine(RetryCurrentLevel());
    }

    private IEnumerator AdvanceToNextLevel()
    {
        yield return new WaitForSeconds(1.5f);
        currentLevel++;
        OnLevelChanged?.Invoke(currentLevel);
        if (currentLevel <= maxLevel)
            PromoteAndPrepareNext();
    }

    private IEnumerator RetryCurrentLevel()
    {
        yield return new WaitForSeconds(1.2f);
        PromoteAndPrepareNext();
    }

    public List<Tile.TileType> GetRequiredIngredientTypes()
    {
        List<Tile.TileType> requiredTypes = new List<Tile.TileType>();
        if (currentOrder != null && currentOrder.requiredIngredients != null)
        {
            foreach (var r in currentOrder.requiredIngredients)
                if (!requiredTypes.Contains(r.ingredientType))
                    requiredTypes.Add(r.ingredientType);
        }
        return requiredTypes;
    }

    public float GetCurrentProgress()
    {
        if (currentOrder == null) return 0f;
        return currentOrder.GetCompletionProgress(collectedIngredients);
    }

    public void AddAvailablePizzaOrder(PizzaOrder order)
    {
        if (order != null && !availablePizzaOrders.Contains(order))
            availablePizzaOrders.Add(order);
    }

    public void AddExtraTime(float extraSeconds)
    {
        if (isOrderActive && !orderCompleted)
            remainingTime += extraSeconds;
    }

    private void AddMoney(int amount)
    {
        totalMoney += amount;
        OnMoneyChanged?.Invoke(totalMoney);
        if (cashFlowAnimator != null)
        {
            Vector3 pos = Camera.main != null
                ? Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f))
                : Vector3.zero;
            cashFlowAnimator.AnimateMoneyCollection(pos, amount);
        }
    }

    public void SpendMoney(int amount)
    {
        totalMoney -= amount;
        OnMoneyChanged?.Invoke(totalMoney);
    }

    /// <summary>
    /// Handle warning timer expiration - fail the order
    /// </summary>
    private void OnIngredientWarningExpired()
    {
        Debug.Log("Ingredient warning expired - order failed");
        warningPanel?.HideWarning();
        FailCurrentOrder();
    }

    /// <summary>
    /// Handle warning resolution - ingredients gathered, continue normally
    /// </summary>
    private void OnIngredientWarningResolved()
    {
        Debug.Log("Ingredients gathered - order proceeding normally");
        isWaitingForIngredients = false;
        warningPanel?.HideWarning();

        // Siparişi ve ana timerı tekrar başlat
        if (currentOrder != null)
        {
            orderStartTime = Time.time;
            float timeMultiplier = sessionManager != null
                ? sessionManager.CurrentTimeMultiplier
                : Mathf.Pow(timeScalingFactor, currentLevel - 1);

            remainingTime = Mathf.Max(30f, currentOrder.timeLimit * timeMultiplier);
            isOrderActive = true;
            orderCompleted = false;

            OnOrderStarted?.Invoke(currentOrder);
            OnOrderProgressChanged?.Invoke(0f);
            Debug.Log($"Order resumed: {currentOrder.pizzaName}");
        }
    }

    /// <summary>
    /// Check if ingredients became available and resolve warning
    /// </summary>
    private void CheckAndResolveWarning()
    {
        if (!isWaitingForIngredients || currentOrder == null) return;

        if (stockManager != null && stockManager.CanFulfillOrder(currentOrder))
        {
            warningTimer?.ResolveWarning();
        }
    }

    void OnDestroy()
    {
        if (gridManager != null)
            gridManager.OnTilesCleared -= OnTilesCleared;

        if (warningTimer != null)
        {
            warningTimer.OnWarningExpired -= OnIngredientWarningExpired;
            warningTimer.OnWarningResolved -= OnIngredientWarningResolved;
        }
    }
}