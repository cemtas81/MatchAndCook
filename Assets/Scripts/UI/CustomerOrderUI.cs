using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// UI component for displaying customer orders in the corner of the screen.
/// Shows customer avatar, order details, timer, and satisfaction level.
/// </summary>
public class CustomerOrderUI : MonoBehaviour
{
    [Header("Customer Order Panel")]
    [SerializeField] private Transform orderContainer;
    [SerializeField] private GameObject customerOrderPrefab;
    [SerializeField] private int maxVisibleOrders = 3;
    
    [Header("Animation Settings")]
    [SerializeField] private float orderAppearDuration = 0.5f;
    [SerializeField] private float orderDisappearDuration = 0.3f;
    [SerializeField] private float urgentPulseDuration = 1f;
    
    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color urgentColor = Color.red;
    [SerializeField] private Color completedColor = Color.green;
    
    // Current state
    private List<CustomerOrderItemUI> orderItems = new List<CustomerOrderItemUI>();
    private CustomerManager customerManager;
    
    void Start()
    {
        InitializeCustomerOrderUI();
    }
    
    /// <summary>
    /// Initialize customer order UI and find references
    /// </summary>
    private void InitializeCustomerOrderUI()
    {
        customerManager = FindFirstObjectByType<CustomerManager>();
        
        if (customerManager != null)
        {
            customerManager.OnOrderStarted += OnOrderStarted;
            customerManager.OnOrderCompleted += OnOrderCompleted;
            customerManager.OnOrderExpired += OnOrderExpired;
            customerManager.OnOrderTimeUpdated += OnOrderTimeUpdated;
        }
    }
    
    /// <summary>
    /// Handle new order started
    /// </summary>
    private void OnOrderStarted(ActiveCustomerOrder order)
    {
        CreateOrderItem(order);
    }
    
    /// <summary>
    /// Handle order completed
    /// </summary>
    private void OnOrderCompleted(ActiveCustomerOrder order, bool success)
    {
        CustomerOrderItemUI item = FindOrderItem(order);
        if (item != null)
        {
            if (success)
            {
                item.PlayCompletionAnimation(() => RemoveOrderItem(item));
            }
            else
            {
                item.PlayFailureAnimation(() => RemoveOrderItem(item));
            }
        }
    }
    
    /// <summary>
    /// Handle order expired
    /// </summary>
    private void OnOrderExpired(ActiveCustomerOrder order)
    {
        // OnOrderCompleted will be called with success=false, so we don't need additional handling here
    }
    
    /// <summary>
    /// Handle order time updated
    /// </summary>
    private void OnOrderTimeUpdated(ActiveCustomerOrder order, float remainingTime)
    {
        CustomerOrderItemUI item = FindOrderItem(order);
        if (item != null)
        {
            item.UpdateTimer(remainingTime, order.orderData.timeLimit);
            
            // Check if order is urgent
            if (order.IsUrgent())
            {
                item.SetUrgent(true);
            }
        }
    }
    
    /// <summary>
    /// Create a new order item UI
    /// </summary>
    private void CreateOrderItem(ActiveCustomerOrder order)
    {
        if (customerOrderPrefab == null || orderContainer == null) return;
        
        // Limit visible orders
        if (orderItems.Count >= maxVisibleOrders)
        {
            // Remove oldest order item
            if (orderItems.Count > 0)
            {
                RemoveOrderItem(orderItems[0]);
            }
        }
        
        GameObject itemObj = Instantiate(customerOrderPrefab, orderContainer);
        CustomerOrderItemUI itemUI = itemObj.GetComponent<CustomerOrderItemUI>();
        
        if (itemUI == null)
        {
            itemUI = itemObj.AddComponent<CustomerOrderItemUI>();
        }
        
        itemUI.Initialize(order, normalColor, urgentColor, completedColor);
        orderItems.Add(itemUI);
        
        // Animate appearance
        itemUI.PlayAppearAnimation(orderAppearDuration);
    }
    
    /// <summary>
    /// Remove an order item UI
    /// </summary>
    private void RemoveOrderItem(CustomerOrderItemUI item)
    {
        if (item == null) return;
        
        orderItems.Remove(item);
        Destroy(item.gameObject);
    }
    
    /// <summary>
    /// Find order item UI for a specific order
    /// </summary>
    private CustomerOrderItemUI FindOrderItem(ActiveCustomerOrder order)
    {
        foreach (var item in orderItems)
        {
            if (item.Order == order)
            {
                return item;
            }
        }
        return null;
    }
    
    void OnDestroy()
    {
        // Clean up events
        if (customerManager != null)
        {
            customerManager.OnOrderStarted -= OnOrderStarted;
            customerManager.OnOrderCompleted -= OnOrderCompleted;
            customerManager.OnOrderExpired -= OnOrderExpired;
            customerManager.OnOrderTimeUpdated -= OnOrderTimeUpdated;
        }
    }
}

/// <summary>
/// UI component for individual customer order items
/// </summary>
public class CustomerOrderItemUI : MonoBehaviour
{
    [Header("Customer Order Elements")]
    [SerializeField] private Image customerAvatar;
    [SerializeField] private TextMeshProUGUI customerNameText;
    [SerializeField] private TextMeshProUGUI orderDescriptionText;
    [SerializeField] private Image recipeIcon;
    [SerializeField] private Slider timerSlider;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image satisfactionIndicator;
    
    // State
    private ActiveCustomerOrder order;
    private Color normalColor;
    private Color urgentColor;
    private Color completedColor;
    private bool isUrgent = false;
    private Tween currentTween;
    
    public ActiveCustomerOrder Order => order;
    
    /// <summary>
    /// Initialize order item with data
    /// </summary>
    public void Initialize(ActiveCustomerOrder customerOrder, Color normal, Color urgent, Color completed)
    {
        order = customerOrder;
        normalColor = normal;
        urgentColor = urgent;
        completedColor = completed;
        
        UpdateDisplay();
        InitializeTimer();
    }
    
    /// <summary>
    /// Update display with order data
    /// </summary>
    private void UpdateDisplay()
    {
        if (order?.orderData == null) return;
        
        // Update customer avatar
        if (customerAvatar != null && order.orderData.customerAvatar != null)
        {
            customerAvatar.sprite = order.orderData.customerAvatar;
        }
        
        // Update customer name
        if (customerNameText != null)
        {
            customerNameText.text = order.orderData.customerName;
        }
        
        // Update order description
        if (orderDescriptionText != null)
        {
            orderDescriptionText.text = order.orderData.orderDescription;
        }
        
        // Update recipe icon
        if (recipeIcon != null && order.orderData.requestedRecipe?.recipeIcon != null)
        {
            recipeIcon.sprite = order.orderData.requestedRecipe.recipeIcon;
        }
        
        // Update background color
        if (backgroundImage != null)
        {
            backgroundImage.color = normalColor;
        }
    }
    
    /// <summary>
    /// Initialize timer display
    /// </summary>
    private void InitializeTimer()
    {
        if (timerSlider != null)
        {
            timerSlider.minValue = 0f;
            timerSlider.maxValue = order.orderData.timeLimit;
            timerSlider.value = order.remainingTime;
        }
        
        UpdateTimerDisplay();
    }
    
    /// <summary>
    /// Update timer display
    /// </summary>
    public void UpdateTimer(float remainingTime, float totalTime)
    {
        // Update slider
        if (timerSlider != null)
        {
            timerSlider.value = remainingTime;
        }
        
        // Update timer text
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60f);
            int seconds = Mathf.FloorToInt(remainingTime % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
        
        // Update satisfaction indicator
        if (satisfactionIndicator != null)
        {
            float satisfaction = order.orderData.GetSatisfactionLevel(remainingTime);
            Color moodColor = order.orderData.GetCustomerMoodColor(remainingTime);
            satisfactionIndicator.color = moodColor;
        }
    }
    
    /// <summary>
    /// Update timer display (called from UpdateTimer)
    /// </summary>
    private void UpdateTimerDisplay()
    {
        UpdateTimer(order.remainingTime, order.orderData.timeLimit);
    }
    
    /// <summary>
    /// Set urgent state
    /// </summary>
    public void SetUrgent(bool urgent)
    {
        if (isUrgent == urgent) return;
        
        isUrgent = urgent;
        
        if (urgent)
        {
            StartUrgentAnimation();
        }
        else
        {
            StopUrgentAnimation();
        }
    }
    
    /// <summary>
    /// Start urgent pulsing animation
    /// </summary>
    private void StartUrgentAnimation()
    {
        if (backgroundImage == null) return;
        
        currentTween?.Kill();
        currentTween = backgroundImage.DOColor(urgentColor, 0.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }
    
    /// <summary>
    /// Stop urgent animation
    /// </summary>
    private void StopUrgentAnimation()
    {
        currentTween?.Kill();
        
        if (backgroundImage != null)
        {
            backgroundImage.color = normalColor;
        }
    }
    
    /// <summary>
    /// Play appearance animation
    /// </summary>
    public void PlayAppearAnimation(float duration)
    {
        if (transform == null) return;
        
        // Start small and grow
        transform.localScale = Vector3.zero;
        currentTween?.Kill();
        currentTween = transform.DOScale(Vector3.one, duration)
            .SetEase(Ease.OutBack);
    }
    
    /// <summary>
    /// Play completion animation
    /// </summary>
    public void PlayCompletionAnimation(System.Action onComplete)
    {
        if (backgroundImage != null)
        {
            backgroundImage.DOColor(completedColor, 0.2f)
                .OnComplete(() => {
                    transform.DOScale(Vector3.zero, 0.3f)
                        .SetEase(Ease.InBack)
                        .OnComplete(() => onComplete?.Invoke());
                });
        }
        else
        {
            transform.DOScale(Vector3.zero, 0.3f)
                .SetEase(Ease.InBack)
                .OnComplete(() => onComplete?.Invoke());
        }
    }
    
    /// <summary>
    /// Play failure animation
    /// </summary>
    public void PlayFailureAnimation(System.Action onComplete)
    {
        // Shake and fade out
        currentTween?.Kill();
        currentTween = transform.DOShakePosition(0.5f, 10f, 20)
            .OnComplete(() => {
                transform.DOScale(Vector3.zero, 0.3f)
                    .SetEase(Ease.InBack)
                    .OnComplete(() => onComplete?.Invoke());
            });
    }
    
    void OnDestroy()
    {
        currentTween?.Kill();
    }
}