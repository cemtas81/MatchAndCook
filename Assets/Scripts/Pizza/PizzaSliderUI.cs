using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// UI component for displaying pizza order progress as a circular slider/progress bar.
/// Shows the completion status of the current pizza order at the top of the screen.
/// </summary>
public class PizzaSliderUI : MonoBehaviour
{
    [Header("Pizza Progress Display")]
    [SerializeField] private Image pizzaProgressRing; // Circular progress indicator
    [SerializeField] private Image pizzaIcon; // Center pizza icon
    [SerializeField] private TextMeshProUGUI progressText; // "3/5" style text
    [SerializeField] private TextMeshProUGUI pizzaNameText; // Name of current pizza order
    
    [Header("Customer Display")]
    [SerializeField] private Image customerAvatar; // Customer image in top-right
    [SerializeField] private Transform customerContainer; // Container for customer UI
    [SerializeField] private TextMeshProUGUI customerNameText; // Customer name
    [SerializeField] private Image customerMoodIndicator; // Color indicator for customer mood
    
    [Header("Timer Display")]
    [SerializeField] private TextMeshProUGUI timerText; // Remaining time display
    [SerializeField] private Image timerBackground; // Background for timer
    [SerializeField] private Color normalTimeColor = Color.white;
    [SerializeField] private Color urgentTimeColor = Color.red;
    [SerializeField] private float urgentTimeThreshold = 30f; // Seconds when timer turns red
    
    [Header("Animation Settings")]
    [SerializeField] private float progressAnimationDuration = 0.5f;
    [SerializeField] private float completionCelebrationDuration = 1f;
    [SerializeField] private GameObject completionEffect; // Particle effect for completion
    
    // Internal state
    private PizzaOrderManager pizzaOrderManager;
    private float currentProgress = 0f;
    private float targetProgress = 0f;
    private bool isAnimatingProgress = false;
    
    void Start()
    {
        InitializePizzaSliderUI();
    }
    
    /// <summary>
    /// Initialize the pizza slider UI and connect to pizza order manager
    /// </summary>
    private void InitializePizzaSliderUI()
    {
        pizzaOrderManager = FindFirstObjectByType<PizzaOrderManager>();
        
        if (pizzaOrderManager != null)
        {
            // Subscribe to pizza order events
            pizzaOrderManager.OnOrderStarted += OnOrderStarted;
            pizzaOrderManager.OnOrderCompleted += OnOrderCompleted;
            pizzaOrderManager.OnOrderProgressChanged += OnProgressChanged;
            pizzaOrderManager.OnOrderTimeChanged += OnTimeChanged;
            pizzaOrderManager.OnIngredientCollected += OnIngredientCollected;
        }
        
        // Initialize UI elements
        ResetProgressDisplay();
        
        // Position customer container to top-right
        if (customerContainer != null)
        {
            PositionCustomerUI();
        }
    }
    
    /// <summary>
    /// Position customer UI in the top-right corner
    /// </summary>
    private void PositionCustomerUI()
    {
        RectTransform rectTransform = customerContainer.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // Anchor to top-right
            rectTransform.anchorMin = new Vector2(1f, 1f);
            rectTransform.anchorMax = new Vector2(1f, 1f);
            rectTransform.anchoredPosition = new Vector2(-20f, -20f); // Small offset from corner
        }
    }
    
    /// <summary>
    /// Handle new pizza order started
    /// </summary>
    private void OnOrderStarted(PizzaOrder order)
    {
        if (order == null) return;
        
        // Update customer display
        if (customerAvatar != null && order.customerAvatar != null)
        {
            customerAvatar.sprite = order.customerAvatar;
            customerAvatar.gameObject.SetActive(true);
        }
        
        if (customerNameText != null)
        {
            customerNameText.text = order.customerName;
        }
        
        if (pizzaNameText != null)
        {
            pizzaNameText.text = order.pizzaName;
        }
        
        // Reset progress display
        ResetProgressDisplay();
        
        // Show UI with entrance animation
        ShowPizzaOrderUI();
        
        Debug.Log($"Pizza order UI updated for: {order.pizzaName}");
    }
    
    /// <summary>
    /// Handle pizza order completion
    /// </summary>
    private void OnOrderCompleted(PizzaOrder order, bool success)
    {
        if (success)
        {
            PlayCompletionCelebration();
        }
        else
        {
            PlayFailureAnimation();
        }
        
        // Hide UI after animation
        HidePizzaOrderUI();
    }
    
    /// <summary>
    /// Handle progress change in pizza order
    /// </summary>
    private void OnProgressChanged(float progress)
    {
        targetProgress = Mathf.Clamp01(progress);
        AnimateProgressChange();
    }
    
    /// <summary>
    /// Handle time change in pizza order
    /// </summary>
    private void OnTimeChanged(float remainingTime)
    {
        UpdateTimerDisplay(remainingTime);
    }
    
    /// <summary>
    /// Handle ingredient collection for visual feedback
    /// </summary>
    private void OnIngredientCollected(Tile.TileType ingredientType, int amount)
    {
        // Add visual feedback when ingredients are collected
        PlayIngredientCollectionEffect(ingredientType, amount);
    }
    
    /// <summary>
    /// Reset the progress display to initial state
    /// </summary>
    private void ResetProgressDisplay()
    {
        currentProgress = 0f;
        targetProgress = 0f;
        
        if (pizzaProgressRing != null)
        {
            pizzaProgressRing.fillAmount = 0f;
        }
        
        if (progressText != null)
        {
            progressText.text = "0%";
        }
    }
    
    /// <summary>
    /// Animate progress change smoothly
    /// </summary>
    private void AnimateProgressChange()
    {
        if (isAnimatingProgress) return;
        
        isAnimatingProgress = true;
        
        // Animate the circular progress ring
        if (pizzaProgressRing != null)
        {
            pizzaProgressRing.DOFillAmount(targetProgress, progressAnimationDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => isAnimatingProgress = false);
        }
        
        // Animate the progress text
        if (progressText != null)
        {
            DOTween.To(() => currentProgress, x => {
                currentProgress = x;
                progressText.text = $"{Mathf.RoundToInt(currentProgress * 100)}%";
            }, targetProgress, progressAnimationDuration)
            .SetEase(Ease.OutQuad);
        }
        
        // Add juice animation to pizza icon
        if (pizzaIcon != null)
        {
            pizzaIcon.transform.DOPunchScale(Vector3.one * 0.1f, 0.3f, 10, 1f);
        }
    }
    
    /// <summary>
    /// Update timer display with color coding
    /// </summary>
    private void UpdateTimerDisplay(float remainingTime)
    {
        if (timerText == null) return;
        
        // Format time as MM:SS
        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
        
        // Change color based on urgency
        Color timeColor = remainingTime <= urgentTimeThreshold ? urgentTimeColor : normalTimeColor;
        timerText.color = timeColor;
        
        // Add pulsing effect when time is urgent
        if (remainingTime <= urgentTimeThreshold && remainingTime > 0f)
        {
<<<<<<< Updated upstream
            if (!isAnimatingProgress)
=======
            if (timerText.transform.localScale==new Vector3(1,1,1))
>>>>>>> Stashed changes
            {
                timerText.transform.DOScale(1.1f, 0.5f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
        }
        else
        {
            timerText.transform.DOKill();
            timerText.transform.localScale = Vector3.one;
        }
    }
    
    /// <summary>
    /// Play completion celebration animation
    /// </summary>
    private void PlayCompletionCelebration()
    {
        // Scale up pizza icon with celebration effect
        if (pizzaIcon != null)
        {
            pizzaIcon.transform.DOScale(1.3f, completionCelebrationDuration * 0.5f)
                .SetEase(Ease.OutBack)
                .OnComplete(() => {
                    pizzaIcon.transform.DOScale(1f, completionCelebrationDuration * 0.5f)
                        .SetEase(Ease.InBack);
                });
        }
        
        // Show particle effect
        if (completionEffect != null)
        {
            GameObject effect = Instantiate(completionEffect, transform);
            Destroy(effect, completionCelebrationDuration);
        }
        
        // Flash progress ring green
        if (pizzaProgressRing != null)
        {
            Color originalColor = pizzaProgressRing.color;
            pizzaProgressRing.color = Color.green;
            pizzaProgressRing.DOColor(originalColor, completionCelebrationDuration);
        }
    }
    
    /// <summary>
    /// Play failure animation
    /// </summary>
    private void PlayFailureAnimation()
    {
        // Shake pizza icon
        if (pizzaIcon != null)
        {
            pizzaIcon.transform.DOShakePosition(1f, 10f, 10, 90f);
        }
        
        // Flash progress ring red
        if (pizzaProgressRing != null)
        {
            Color originalColor = pizzaProgressRing.color;
            pizzaProgressRing.color = Color.red;
            pizzaProgressRing.DOColor(originalColor, 1f);
        }
    }
    
    /// <summary>
    /// Play ingredient collection effect
    /// </summary>
    private void PlayIngredientCollectionEffect(Tile.TileType ingredientType, int amount)
    {
        // Small bounce animation on progress ring
        if (pizzaProgressRing != null)
        {
            pizzaProgressRing.transform.DOPunchScale(Vector3.one * 0.05f, 0.2f, 5, 1f);
        }
        
        // TODO: Add ingredient-specific visual feedback (flying ingredient icons, etc.)
    }
    
    /// <summary>
    /// Show pizza order UI with entrance animation
    /// </summary>
    private void ShowPizzaOrderUI()
    {
        gameObject.SetActive(true);
        
        // Entrance animation from top
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            Vector2 originalPosition = rectTransform.anchoredPosition;
            rectTransform.anchoredPosition = originalPosition + Vector2.up * 100f;
            rectTransform.DOAnchorPos(originalPosition, 0.5f).SetEase(Ease.OutBack);
        }
        
        // Scale in customer avatar
        if (customerAvatar != null)
        {
            customerAvatar.transform.localScale = Vector3.zero;
            customerAvatar.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack).SetDelay(0.2f);
        }
    }
    
    /// <summary>
    /// Hide pizza order UI with exit animation  
    /// </summary>
    private void HidePizzaOrderUI()
    {
        // Exit animation to top
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            Vector2 exitPosition = rectTransform.anchoredPosition + Vector2.up * 100f;
            rectTransform.DOAnchorPos(exitPosition, 0.5f)
                .SetEase(Ease.InBack)
                .SetDelay(2f) // Show result for a moment
                .OnComplete(() => gameObject.SetActive(false));
        }
    }
    
    void OnDestroy()
    {
        // Clean up event subscriptions
        if (pizzaOrderManager != null)
        {
            pizzaOrderManager.OnOrderStarted -= OnOrderStarted;
            pizzaOrderManager.OnOrderCompleted -= OnOrderCompleted;
            pizzaOrderManager.OnOrderProgressChanged -= OnProgressChanged;
            pizzaOrderManager.OnOrderTimeChanged -= OnTimeChanged;
            pizzaOrderManager.OnIngredientCollected -= OnIngredientCollected;
        }
        
        // Kill any active tweens
        transform.DOKill();
        if (pizzaIcon != null) pizzaIcon.transform.DOKill();
        if (pizzaProgressRing != null) pizzaProgressRing.transform.DOKill();
        if (timerText != null) timerText.transform.DOKill();
    }
}